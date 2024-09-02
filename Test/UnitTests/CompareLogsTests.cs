// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using EfSchemaCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class CompareLogsTests
    {
        private readonly ITestOutputHelper _output;

        public CompareLogsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DecodeStringToCompareLog()
        {
            //SETUP
            const string logOk = @"OK: DbContext 'BookContext'";
            const string logStr1 =
                @"NOT IN DATABASE: BookDetail->ForeignKey 'FK_Books_Books_BookSummaryId', constraint name. Expected = FK_Books_Books_BookSummaryId";
            const string logStr2 =
                @"DIFFERENT: BookSummary->Property 'BookSummaryId', value generated. Expected = OnAdd, found = Never";
            const string logStr3 =
                @"NOT IN DATABASE: BookDetail->ForeignKey 'FK_Books_Books_BookSummaryId', constraint name. Expected = FK_Books_Books_BookSummaryId";

            //ATTEMPT

            //VERIFY
            CompareLog.DecodeCompareTextToCompareLog(logOk).ToString().ShouldEqual(logOk);
            CompareLog.DecodeCompareTextToCompareLog(logStr1).ToString().ShouldEqual(logStr1.Replace("BookDetail->",""));
            CompareLog.DecodeCompareTextToCompareLog(logStr2).ToString().ShouldEqual(
                "DIFFERENT: Property 'BookSummaryId', value generated. Expected = OnAdd, found = <null>");
            CompareLog.DecodeCompareTextToCompareLog(logStr3).ToString().ShouldEqual(logStr3.Replace("BookDetail->", ""));
        }

        [Theory]
        [ClassData(typeof(CompareIgnoreLogs))]
        public void CheckIgnore(CompareLog ignoreItem, bool shouldIgnore)
        {
            //SETUP
            var list = new List<CompareLog>
            {
                ignoreItem
            };
            var log = new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnName,
                "Expected", "Found");

            //ATTEMPT
            var ignoreThis = log.ShouldIgnoreThisLog(list);

            //VERIFY
            ignoreThis.ShouldEqual(shouldIgnore);

        }

        private class CompareIgnoreLogs : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] {new CompareLog(CompareType.MatchAnything, CompareState.Different, null), true},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, null), true},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name"), true},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "DiffName"), false},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnName, "Expected"), true},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnType, "Expected"), false},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnType, "DiffExpected"), false},
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
