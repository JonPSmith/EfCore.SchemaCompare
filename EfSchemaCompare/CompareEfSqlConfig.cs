// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EfSchemaCompare
{
    /// <summary>
    /// This class holds the configuration information for the CompareEfSql class
    /// </summary>
    public class CompareEfSqlConfig
    {
        private readonly List<CompareLog> _logsToIgnore = new List<CompareLog>();

        ///// <summary>
        ///// Set this to StringComparer.CurrentCultureIgnoreCase to change the 
        ///// This effects the table, schema, column, primary/index/foreignKey constraint names
        ///// </summary>
        //NOTE Turned off CaseComparer as doesn't work with EF Core 5
        //public StringComparer CaseComparer { get; set; } = StringComparer.CurrentCulture;

        /// <summary>
        /// This allows you to ignore tables that your EF Core context doesn't use. There are three settings
        /// 1. null - this will only check the tables that the DbContexts are mapped to.
        /// 2. "" - This will check all tables in the database against the entity classes in the DbContexts.
        /// 3. A comma delimited list of tables, with optional schema, to ignore. 
        ///    Typical format: "MyTable,MyOtherTable,MySchema.MyTableWithSchema"
        ///    (note: the schema/table match is case insensitive)
        /// </summary>
        public string TablesToIgnoreCommaDelimited { get; set; }

        /// <summary>
        /// This contains all the log types that should be ignored by the comparer
        /// </summary>
        public IReadOnlyList<CompareLog> LogsToIgnore => _logsToIgnore.ToImmutableList();

        /// <summary>
        /// Specifies the culture, case, and sort rules to be used
        /// </summary>
        public StringComparison? CaseComparison { get; set; }
        /// <summary>
        /// Represents a string comparison operation that uses specific case and culture-based or ordinal comparison rules. 
        /// </summary>
        public StringComparer CaseComparer { get; set; }

        /// <summary>
        /// This allows you to clip a set of errors strings and add them as ignore items
        /// </summary>
        /// <param name="textWithNewlineBetweenErrors"></param>
        public void IgnoreTheseErrors(string textWithNewlineBetweenErrors)
        {
            foreach (var errorString in textWithNewlineBetweenErrors.Split('\n'))
            {
                AddIgnoreCompareLog(CompareLog.DecodeCompareTextToCompareLog(errorString));
            }
        }

        /// <summary>
        /// This allows you to add a log with setting that will be used to decide if a log is ignored.
        /// The Type and State must be set, but any strings set to null with automatically match anything 
        /// and the Attribute has a MatchAnything setting too.
        /// </summary>
        /// <param name="logTypeToIgnore"></param>
        public void AddIgnoreCompareLog(CompareLog logTypeToIgnore)
        {
            if (logTypeToIgnore.State <= CompareState.Ok)
                throw new ArgumentException("You cannot ignore logs with a State of OK (or lower).");
            _logsToIgnore.Add(logTypeToIgnore);
        }
    }
}