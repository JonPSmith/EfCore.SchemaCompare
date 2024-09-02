// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace EfSchemaCompare.Internal
{

    internal class CompareLogger2
    {
        private readonly IList<CompareLog> _compareLogs;
        private readonly string _defaultName;
        private readonly IReadOnlyList<CompareLog> _ignoreList;
        private readonly Action _setErrorHasHappened;
        private readonly CompareType _type;

        public CompareLogger2(CompareType type, string defaultName, IList<CompareLog> compareLogs, IReadOnlyList<CompareLog> ignoreList, Action setErrorHasHappened)
        {
            _type = type;
            _defaultName = defaultName;
            _compareLogs = compareLogs;
            _ignoreList = ignoreList ?? new List<CompareLog>();
            _setErrorHasHappened = setErrorHasHappened;
        }

        public CompareLog MarkAsOk(string expected, string name = null)
        {
            var log = new CompareLog(_type, CompareState.Ok, name ?? _defaultName, CompareAttributes.NotSet, expected, null);
            _compareLogs.Add(log);
            return log;
        }

        public bool CheckDifferent(string expected, string found, CompareAttributes attribute,
            StringComparison caseComparison, string name = null)
        {
            if (!string.Equals(expected, found, caseComparison) && 
                !string.Equals(expected?.Replace(" ", ""), found?.Replace(" ", ""), caseComparison))
            {
                return AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.Different, name ?? _defaultName, attribute, expected, found));
            }
            return false;
        }

        public void NotInDatabase(string expected, CompareAttributes attribute = CompareAttributes.NotSet, string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.NotInDatabase, name ?? _defaultName, attribute, expected, null));
        }

        public void ExtraInDatabase(string found, CompareAttributes attribute, string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.ExtraInDatabase, name ?? _defaultName, attribute, null, found));
        }


        /// <summary>
        /// This is for adding a Not Checked log.
        /// </summary>
        /// <param name="errorMessage">What we don't check</param>
        /// <param name="found">entities that aren't checked</param>
        /// <param name="attribute"></param>
        public void MarkAsNotChecked(string errorMessage, string found, CompareAttributes attribute)
        {
            AddToLogsIfNotIgnored(new CompareLog(CompareType.Entity, CompareState.NotChecked, found, attribute, null, found));
        }

        //------------------------------------------------------
        //private methods

        /// <summary>
        /// Only adds the error if they aren't in the IgnoreTheseErrors
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private bool AddToLogsIfNotIgnored(CompareLog log)
        {
            if (!log.ShouldIgnoreThisLog(_ignoreList))
            {
                _compareLogs.Add(log);
                _setErrorHasHappened();
                return true;
            }

            return false;
        }
    }
}