﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace EfSchemaCompare.Internal
{
    internal class Stage2Comparer
    {
        private readonly StringComparer _caseComparer;
        private readonly DatabaseModel _databaseModel;
        private readonly IReadOnlyList<CompareLog> _ignoreList;

        private readonly List<CompareLog> _logs = new List<CompareLog>();

        private bool _hasErrors;

        public Stage2Comparer(DatabaseModel databaseModel, CompareEfSqlConfig config = null)
        {
            _databaseModel = databaseModel;
            _ignoreList = config?.LogsToIgnore ?? new List<CompareLog>();
            _caseComparer = config?.CaseComparer ?? StringComparer.CurrentCulture;//Turned off CaseComparer as doesn't work with EF Core 5
        }

        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public bool CompareLogsToDatabase(IReadOnlyList<CompareLog> firstStageLogs)
        {
            var logger = new CompareLogger2(CompareType.Database, _databaseModel.DatabaseName, _logs, _ignoreList, () => _hasErrors = true);
            logger.MarkAsOk(null);
            LookForUnusedTables(firstStageLogs, _logs.Last());
            LookForUnusedColumns(firstStageLogs, _logs.Last());
            LookForUnusedIndexes(firstStageLogs, _logs.Last());

            return _hasErrors;
        }

        private void LookForUnusedTables(IReadOnlyList<CompareLog> firstStageLogs, CompareLog log)
        {
            var logger = new CompareLogger2(CompareType.Table, null, log.SubLogs, _ignoreList, () => _hasErrors = true);
            var databaseTableNames = _databaseModel.Tables.Select(x => x.FormSchemaTableFromDatabase(_databaseModel.DefaultSchema));
            var allEntityTableNames = firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity)
                .Select(p => p.Expected).OrderBy(p => p).Distinct().ToList();
            var tablesNotUsed = databaseTableNames.Where(p => !allEntityTableNames.Contains(p, _caseComparer));

            foreach (var tableName in tablesNotUsed)
            {
                logger.ExtraInDatabase(null, CompareAttributes.NotSet, tableName);
            }
        }

        private void LookForUnusedColumns(IReadOnlyList<CompareLog> firstStageLogs, CompareLog log)
        {
            var logger = new CompareLogger2(CompareType.Column, null, _logs, _ignoreList, () => _hasErrors = true);
            var tableDict = _databaseModel.Tables.ToDictionary(x => x.FormSchemaTableFromDatabase(_databaseModel.DefaultSchema), _caseComparer);
            //because of table splitting and TPH we need to groups properties by table name to correctly say what columns are missed
            var entityColsGrouped = firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity)
                .GroupBy(x => x.Expected, y => y.SubLogs
                    .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Property)
                    .Select(p => p.Expected));
            var entityColsByTableDict = entityColsGrouped.ToDictionary(x => x.Key, y => y.SelectMany(x => x.ToList()), _caseComparer);

            foreach (var entityLog in firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity))
            {
                if (tableDict.ContainsKey(entityLog.Expected))
                {
                    var dbColNames = tableDict[entityLog.Expected].Columns.Select(x => x.Name);
                    var colsNotUsed = dbColNames.Where(p => !entityColsByTableDict[entityLog.Expected].Contains(p, _caseComparer));
                    foreach (var colName in colsNotUsed)
                    {
                        logger.ExtraInDatabase(colName, CompareAttributes.ColumnName, entityLog.Expected);
                    }
                }               
            }
        }

        private void LookForUnusedIndexes(IReadOnlyList<CompareLog> firstStageLogs, CompareLog log)
        {
            var logger = new CompareLogger2(CompareType.Index, null, _logs, _ignoreList, () => _hasErrors = true);
            var tableDict = _databaseModel.Tables.ToDictionary(x => x.FormSchemaTableFromDatabase(_databaseModel.DefaultSchema), _caseComparer);
            foreach (var entityLog in firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity))
            {
                if (tableDict.ContainsKey(entityLog.Expected))
                {
                    var indexCol = tableDict[entityLog.Expected].Indexes.Select(x => x.Name);
                    var allEfIndexNames = entityLog.SubLogs
                        .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Index)
                        .Select(p => p.Expected).OrderBy(p => p).Distinct().ToList();
                    var indexesNotUsed = indexCol.Where(p => !allEfIndexNames.Contains(p, _caseComparer));
                    foreach (var indexName in indexesNotUsed)
                    {
                        logger.ExtraInDatabase(indexName, CompareAttributes.IndexConstraintName, entityLog.Expected);
                    }
                }
            }
        }
    }
}