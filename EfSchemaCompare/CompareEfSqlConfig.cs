// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EfSchemaCompare.Internal;
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

        /// <summary>
        /// By default, Stage 2 only runs if: either there were no errors in Stage 1, or if you
        /// have suppressed all the errors in the first stage. But Stage 2 can be forced to run by either:
        /// 1. Set <see cref="AlwaysRunStage2"/> to true
        /// 2. Set <see cref="TablesToIgnoreCommaDelimited"/>  to "", which says you want to check all tables
        /// in the database against your DbContext(s).
        /// </summary>
        public bool AlwaysRunStage2 { get; set; } = false;

        /// <summary>
        /// This allows you to ignore tables that your EF Core context doesn't use. There are three settings
        /// 1. null - this will only check the tables that the DbContexts are mapped to. This is the default.
        /// 2. "" - This will check all tables in the database against the entity classes in the DbContexts.
        /// 3. A comma-delimited list of tables, with optional schema, to ignore. 
        ///    Typical format: "MyTable,MyOtherTable,MySchema.MyTableWithSchema"
        ///    (note: the schema/table match is case-insensitive)
        /// </summary>
        public string TablesToIgnoreCommaDelimited { get; set; }

        /// <summary>
        /// This contains all the log types that should be ignored by the comparer
        /// </summary>
        public IReadOnlyList<CompareLog> LogsToIgnore => _logsToIgnore.ToImmutableList();

        /// <summary>
        /// This allows you to clip a set of errors strings and add them as ignore items
        /// </summary>
        /// <param name="textWithNewlineBetweenErrors"></param>
        public void IgnoreTheseErrors(string textWithNewlineBetweenErrors)
        {
            foreach (var errorString in textWithNewlineBetweenErrors.Split('\n'))
            {
                AddIgnoreCompareLog(FindErrorsToIgnore.DecodeCompareTextToCompareLog(errorString));
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