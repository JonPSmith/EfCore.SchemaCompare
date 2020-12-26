// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

[assembly: InternalsVisibleTo("Test")]

namespace EfSchemaCompare.Internal
{
    internal class Stage1Comparer
    {
        private const string NoPrimaryKey = "- no primary key -";

        private readonly IModel _model;
        private readonly string _dbContextName;
        private readonly IRelationalTypeMappingSource _relationalTypeMapping;
        private readonly IReadOnlyList<CompareLog> _ignoreList;
        private readonly StringComparer _caseComparer;
        private readonly StringComparison _caseComparison;
        private readonly string _modelDefaultSchema;

        private string _databaseDefaultSchema;
        private Dictionary<string, DatabaseTable> _tableDict;
        private bool _hasErrors;

        private readonly List<CompareLog> _logs;
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public Stage1Comparer(DbContext context, CompareEfSqlConfig config = null, List<CompareLog> logs = null)
        {
            _model = context.Model;
            _dbContextName = context.GetType().Name;
            _relationalTypeMapping = context.GetService<IRelationalTypeMappingSource>();
            _logs = logs ?? new List<CompareLog>();
            _ignoreList = config?.LogsToIgnore ?? new List<CompareLog>();
            _caseComparer = StringComparer.CurrentCulture;          //Turned off CaseComparer as doesn't work with EF Core 5
            _caseComparison = _caseComparer.GetStringComparison();

            _modelDefaultSchema = (config ?? new CompareEfSqlConfig()).DefaultSchema;
        }


        public bool CompareModelToDatabase(DatabaseModel databaseModel)
        {
            _databaseDefaultSchema = databaseModel.DefaultSchema;

                
            var dbLogger = new CompareLogger2(CompareType.DbContext, _dbContextName, _logs, _ignoreList, () => _hasErrors = true);

            //Check things about the database, such as sequences
            dbLogger.MarkAsOk(_dbContextName);
            CheckDatabaseOk(_logs.Last(), _model, databaseModel);

            _tableDict = databaseModel.Tables.ToDictionary(x => x.FormSchemaTable(_databaseDefaultSchema), _caseComparer);
            var dbQueries = _model.GetEntityTypes().Where(x => x.FindPrimaryKey() == null).ToList();
            if (dbQueries.Any())
                dbLogger.Warning("EfSchemaCompare does not check read-only types", null, string.Join(", ", dbQueries.Select(x => x.ClrType.Name)));
            foreach (var entityType in _model.GetEntityTypes().Where(x => x.FindPrimaryKey() != null))
            {
                var logger = new CompareLogger2(CompareType.Entity, entityType.ClrType.Name, _logs.Last().SubLogs, _ignoreList, () => _hasErrors = true);
                if (_tableDict.ContainsKey(entityType.FormSchemaTable()))
                {
                    var databaseTable = _tableDict[entityType.FormSchemaTable()];
                    //Checks for table matching
                    var log = logger.MarkAsOk(entityType.FormSchemaTable());
                    logger.CheckDifferent(entityType.FindPrimaryKey()?.GetName() ?? NoPrimaryKey,
                        databaseTable.PrimaryKey?.Name ?? NoPrimaryKey,
                        CompareAttributes.ConstraintName, _caseComparison);
                    CompareColumns(log, entityType, databaseTable);
                    CompareForeignKeys(log, entityType, databaseTable);
                    CompareIndexes(log, entityType, databaseTable);
                }
                else
                {
                    logger.NotInDatabase(entityType.FormSchemaTable(), CompareAttributes.TableName);
                }
            }
            return _hasErrors;
        }

        private void CheckDatabaseOk(CompareLog log, IModel modelRel, DatabaseModel databaseModel)
        {
            //Check sequences
            //var logger = new CompareLogger2(CompareType.Sequence, <sequence name>, _logs);
        }


        private void CompareForeignKeys(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var fKeyDict = table.ForeignKeys.ToDictionary(x => x.Name, _caseComparer);

            foreach (var entityFKey in entityType.GetForeignKeys())
            {
                var entityFKeyprops = entityFKey.Properties;
                var constraintName = entityFKey.GetConstraintName();
                var logger = new CompareLogger2(CompareType.ForeignKey, constraintName, log.SubLogs, _ignoreList, () => _hasErrors = true);
                if (IgnoreForeignKeyIfInSameTable(entityType, entityFKey, table))
                    continue;
                if (fKeyDict.ContainsKey(constraintName))
                {
                    //Now check every foreign key
                    var error = false;
                    var thisKeyCols = fKeyDict[constraintName].Columns.ToDictionary(x => x.Name, _caseComparer);
                    foreach (var fKeyProp in entityFKeyprops)
                    {
                        var columnName = GetColumnNameTakingIntoAccountSchema( fKeyProp, table);
                        if (!thisKeyCols.ContainsKey(columnName))
                        {
                            logger.NotInDatabase(columnName);
                            error = true;
                        }
                    }
                    error |= logger.CheckDifferent(entityFKey.DeleteBehavior.ToString(),
                        fKeyDict[constraintName].OnDelete.ConvertReferentialActionToDeleteBehavior(entityFKey.DeleteBehavior),
                            CompareAttributes.DeleteBehaviour, _caseComparison);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.ConstraintName);
                }
            }
        }

        
        private bool IgnoreForeignKeyIfInSameTable(IEntityType entityType, IForeignKey entityFKey, DatabaseTable table)
        {
            //see https://github.com/aspnet/EntityFrameworkCore/issues/10345#issuecomment-345841191
            var fksPropsInOneTable = entityFKey.Properties.All(x =>
                string.Equals(x.DeclaringEntityType.FormSchemaTable(), table.FormSchemaTable(_databaseDefaultSchema), _caseComparison));
            var fksPropsColumnNames = entityFKey.Properties.Select(p => GetColumnNameTakingIntoAccountSchema(p, table));
            var pkPropsColumnNames =
                entityFKey.PrincipalKey.Properties.Select(p => GetColumnNameTakingIntoAccountSchema(p, 
                    _tableDict[p.DeclaringEntityType.FormSchemaTable()]));
            
            return fksPropsInOneTable && fksPropsColumnNames.SequenceEqual(pkPropsColumnNames);
        }

        private void CompareIndexes(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var indexDict = DatabaseIndexData.GetIndexesAndUniqueConstraints(table).ToDictionary(x => x.Name, _caseComparer);
            foreach (var entityIdx in entityType.GetIndexes())
            {
                var entityIdxprops = entityIdx.Properties;
                var allColumnNames = string.Join(",", entityIdxprops
                    .Select(x => GetColumnNameTakingIntoAccountSchema(x, table)));
                var logger = new CompareLogger2(CompareType.Index, allColumnNames, log.SubLogs, _ignoreList, () => _hasErrors = true);
                var constraintName = entityIdx.GetDatabaseName();
                if (indexDict.ContainsKey(constraintName))
                {
                    //Now check every column in an index
                    var error = false;
                    var thisIdxCols = indexDict[constraintName].Columns.ToDictionary(x => x.Name, _caseComparer);
                    foreach (var idxProp in entityIdxprops)
                    {
                        var columnName = GetColumnNameTakingIntoAccountSchema(idxProp, table);
                        if (!thisIdxCols.ContainsKey(columnName))
                        {
                            logger.NotInDatabase(columnName);
                            error = true;
                        }
                    }
                    error |= logger.CheckDifferent(entityIdx.IsUnique.ToString(),
                        indexDict[constraintName].IsUnique.ToString(), CompareAttributes.Unique, _caseComparison);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.IndexConstraintName);
                }
            }
        }

        /// <summary>
        /// This looks for
        /// 1. TPH
        /// 2. Nested Owned Types and table splitting where the class is optional
        /// If it finds one of these it returns a list of property names that aren't forces to be nullable (all other properties are forced to nullable types)
        /// Otherwise it returns null
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private List<string> ClassIsAddedToTable(IEntityType entityType, DatabaseTable table)
        {
            //Check for THP
            if (entityType.BaseType != null)
            {
                //We need to return the BaseType properties as not forces to nullable
                return entityType.BaseType.GetProperties()
                    .Select(x => GetColumnNameTakingIntoAccountSchema(x, table, false))
                    .ToList();
            }

            //Now check for Nested Owned Types and table splitting where the class is optional
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var fkColumnNames = foreignKey
                    .Properties.Select(x => GetColumnNameTakingIntoAccountSchema(x, table)).ToList();
                var pkPropsColumnNames =
                    foreignKey.PrincipalKey
                        .Properties.Select(x => GetColumnNameTakingIntoAccountSchema(x, table, false))
                        .ToList();
                if (pkPropsColumnNames.SequenceEqual(fkColumnNames))
                    return pkPropsColumnNames;
            }

            return null;
        }

        private void CompareColumns(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var primaryKeyDict = table.PrimaryKey?.Columns.ToDictionary(x => x.Name, _caseComparer)
                                 ?? new Dictionary<string, DatabaseColumn>();
            var efPKeyConstraintName = entityType.FindPrimaryKey().GetName();
            bool pKeyError = false;
            var pKeyLogger = new CompareLogger2(CompareType.PrimaryKey, efPKeyConstraintName, log.SubLogs, _ignoreList,
                () =>
                {
                    pKeyError = true;  //extra set of pKeyError
                    _hasErrors = true;
                });
            
            pKeyLogger.CheckDifferent(efPKeyConstraintName, table.PrimaryKey?.Name ?? NoPrimaryKey, 
                CompareAttributes.ConstraintName, _caseComparison);
            var columnDict = table.Columns.ToDictionary(x => x.Name, _caseComparer);
            
            //This finds all the Owned Types and THP
            var pksOfClassAddedToTable = ClassIsAddedToTable(entityType, table);
            foreach (var property in entityType.GetProperties())
            {
                var colLogger = new CompareLogger2(CompareType.Property, property.Name, log.SubLogs, _ignoreList, () => _hasErrors = true);
                var columnName = GetColumnNameTakingIntoAccountSchema(property, table);
                if (columnDict.ContainsKey(columnName))
                {
                    var isNullable = pksOfClassAddedToTable?.Contains(columnName) == false;
                    var error = ComparePropertyToColumn(colLogger, property,
                        columnDict[GetColumnNameTakingIntoAccountSchema(property, table)], isNullable);
                    //check for primary key
                    if (property.IsPrimaryKey() !=
                        primaryKeyDict.ContainsKey(GetColumnNameTakingIntoAccountSchema(property, table)))
                    {
                        if (!primaryKeyDict.ContainsKey(GetColumnNameTakingIntoAccountSchema(property, table)))
                        {
                            pKeyLogger.NotInDatabase(GetColumnNameTakingIntoAccountSchema(property, table),
                                CompareAttributes.ColumnName);
                            error = true;
                        }
                        else
                        {
                            pKeyLogger.ExtraInDatabase(GetColumnNameTakingIntoAccountSchema(property, table),
                                CompareAttributes.ColumnName,
                                table.PrimaryKey.Name);
                        }
                    }

                    if (!error)
                    {
                        //There were no errors noted, so we mark it as OK
                        colLogger.MarkAsOk(GetColumnNameTakingIntoAccountSchema(property, table));
                    }
                }
                else
                {
                    colLogger.NotInDatabase(GetColumnNameTakingIntoAccountSchema(property, table), CompareAttributes.ColumnName);
                }
            }
            if (!pKeyError)
                pKeyLogger.MarkAsOk(efPKeyConstraintName);
        }

        private bool ComparePropertyToColumn(CompareLogger2 logger, IProperty property, DatabaseColumn column, bool isNullable)
        {
            var error = logger.CheckDifferent(property.GetColumnType(), column.StoreType, CompareAttributes.ColumnType, _caseComparison);
            error |= logger.CheckDifferent((property.IsNullable || isNullable).NullableAsString(), 
                column.IsNullable.NullableAsString(), CompareAttributes.Nullability, _caseComparison);
            error |= logger.CheckDifferent(property.GetComputedColumnSql().RemoveUnnecessaryBrackets(),
                column.ComputedColumnSql.RemoveUnnecessaryBrackets(), CompareAttributes.ComputedColumnSql, _caseComparison);
            if (property.GetComputedColumnSql() != null)
                error |= logger.CheckDifferent(property.GetIsStored()?.ToString() ?? false.ToString()
                    , column.IsStored.ToString(),
                    CompareAttributes.PersistentComputedColumn, _caseComparison);
            var defaultValue = property.GetDefaultValue() != null
                ? _relationalTypeMapping.FindMapping(property.GetDefaultValue().GetType())
                    .GenerateSqlLiteral(property.GetDefaultValue())
                : property.GetDefaultValueSql().RemoveUnnecessaryBrackets();
            error |= logger.CheckDifferent(defaultValue,
                    column.DefaultValueSql.RemoveUnnecessaryBrackets(), CompareAttributes.DefaultValueSql, _caseComparison);
            error |= CheckValueGenerated(logger, property, column);
            return error;
        }

        //thanks to https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        private static HashSet<Type> IntegerTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
        };

        private bool CheckValueGenerated(CompareLogger2 logger, IProperty property, DatabaseColumn column)
        {
            var colValGen = column.ValueGenerated.ConvertNullableValueGenerated(column.ComputedColumnSql, column.DefaultValueSql);
            if (colValGen == ValueGenerated.Never.ToString()
                //There is a case where the property is part of the primary key and the key is not set in the database
                && property.ValueGenerated == ValueGenerated.OnAdd
                && property.IsKey()
                //We assume that a integer of some form should be provided by the database
                && !IntegerTypes.Contains(property.ClrType))
                return false;
            return logger.CheckDifferent(property.ValueGenerated.ToString(),
                colValGen, CompareAttributes.ValueGenerated, _caseComparison);
        }

        private string GetColumnNameTakingIntoAccountSchema(IProperty property, DatabaseTable table, bool throwExceptionOnNull = true)
        {
            var modelSchema = table.Schema == _modelDefaultSchema ? null : table.Schema;
            var columnName = property.GetColumnName(StoreObjectIdentifier.Table(table.Name, modelSchema));
            if (columnName == null && throwExceptionOnNull)
                throw new Exception("Column name is null");
            return columnName;
        }

    }
}