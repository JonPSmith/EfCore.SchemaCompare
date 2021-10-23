// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace EfSchemaCompare.Internal
{
    internal enum MappingOptions { NotMapped, ToTable, ToView}
    
    internal static class CompareHelpers
    {
        /// <summary>
        /// This returns a string in the format "table" or "{schema}.{table}" that this entity is mapped to
        /// This also handles "ToView" entities, in which case it will map the 
        /// It it isn't mapped to a table it returns null
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static string FormSchemaTableFromModel(this IEntityType entityType)
        {
            var viewAnnotations = entityType.GetAnnotations()
                .Where(a => a.Name == RelationalAnnotationNames.TableName ||
                            a.Name == RelationalAnnotationNames.ViewSchema)
                .OrderBy(a =>a.Name)
                .Select(a => (string)a.Value)
                .ToList();

            return viewAnnotations.Any()
                ? FormSchemaTable(viewAnnotations.Last(), viewAnnotations.First())
                : entityType.GetTableName() == null
                    ? null
                    : FormSchemaTable(entityType.GetSchema(), entityType.GetTableName());
        }

        public static string FormSchemaTableFromDatabase(this DatabaseTable table, string defaultSchema)
        {
            //The DatabaseTable always provides a schema name, while the database Model provides null if default schema name.
            //This makes sure that name will match the EF Core Model format
            var schemaToUse = table.Schema == defaultSchema ? null : table.Schema;
            return FormSchemaTable(schemaToUse, table.Name);
        }
        

        /// <summary>
        /// Use this on Model side, where the schema is null for the default schema
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string FormSchemaTable(this string schema, string table)
        {
            return string.IsNullOrEmpty(schema)
                ? table
                : $"{schema}.{table}";
        }

        public static string NullableAsString(this bool isNullable)
        {
            return isNullable ? "NULL" : "NOT NULL";
        }

        public static StringComparison GetStringComparison(this StringComparer caseComparer)
        {
            return caseComparer.ComparerToComparison();
        }

        private static StringComparison ComparerToComparison(this StringComparer caseComparer)
        {
            if (caseComparer.Equals(StringComparer.CurrentCulture))
                return StringComparison.CurrentCulture;
            if (caseComparer.Equals(StringComparer.CurrentCultureIgnoreCase))
                return StringComparison.CurrentCultureIgnoreCase;
            if (caseComparer.Equals(StringComparer.InvariantCulture))
                return StringComparison.InvariantCulture;
            if (caseComparer.Equals(StringComparer.InvariantCultureIgnoreCase))
                return StringComparison.InvariantCultureIgnoreCase;
            if (caseComparer.Equals(StringComparer.Ordinal))
                return StringComparison.Ordinal;
            if (caseComparer.Equals(StringComparer.OrdinalIgnoreCase))
                return StringComparison.OrdinalIgnoreCase;

            throw new ArgumentOutOfRangeException(nameof(caseComparer));
        }

        //The scaffold does not set the correct ValueGenerated for a column that has a sql default value of a computed column
        //see https://github.com/aspnet/EntityFrameworkCore/issues/9323
        public static string ConvertNullableValueGenerated(this ValueGenerated? valGen, string computedColumnSql, string defaultValueSql)
        {
            if (valGen == null && defaultValueSql != null)
                return ValueGenerated.OnAdd.ToString();
            if (valGen == null && computedColumnSql != null)
                return ValueGenerated.OnAddOrUpdate.ToString();
            return valGen?.ToString() ?? ValueGenerated.Never.ToString();
        }

        public static string ConvertReferentialActionToDeleteBehavior(this ReferentialAction? refAct, DeleteBehavior entityBehavior)
        {
            if ((entityBehavior == DeleteBehavior.ClientSetNull || entityBehavior == DeleteBehavior.Restrict)
                && refAct == ReferentialAction.NoAction)
                //A no action constrait can map to either ClientSetNull or Restrict
                return entityBehavior.ToString();

            return refAct?.ToString() ?? ReferentialAction.NoAction.ToString();
        }

        public static string RemoveUnnecessaryBrackets(this string val)
        {
            if (val == null) return null;

            while (val.Length > 1 && val[0] == '(' && val[val.Length-1] == ')')
            {
                val = val.Substring(1, val.Length - 2);
            }

            return val;
        }
    }
}