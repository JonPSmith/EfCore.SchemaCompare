// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using EfSchemaCompare;
using EfSchemaCompare.Internal;
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
        public void DecodeCompareTextToCompareLog()
        {
            //SETUP
            const string logOk = @"OK: DbContext 'BookContext'";
            const string logStr1 =
                @"NOT IN DATABASE: BookDetail->ForeignKey 'FK_Books_Books_BookSummaryId', constraint name. Expected = FK_Books_Books_BookSummaryId";
            const string logStr2 =
                @"DIFFERENT: MyEntity->Property 'MyEntityId', value generated. Expected = Never, found = OnAdd";
                //@"DIFFERENT: BookSummary->Property 'BookSummaryId', value generated. Expected = OnAdd, found = Never";
            const string logStr3 =
                @"NOT IN DATABASE: BookDetail->ForeignKey 'FK_Books_Books_BookSummaryId', constraint name. Expected = FK_Books_Books_BookSummaryId";

            //ATTEMPT
            var resultOk = FindErrorsToIgnore.DecodeCompareTextToCompareLog(logOk).ToString();
            var result1 = FindErrorsToIgnore.DecodeCompareTextToCompareLog(logStr1).ToString();
            var result2 = FindErrorsToIgnore.DecodeCompareTextToCompareLog(logStr2).ToString();
            var result3 = FindErrorsToIgnore.DecodeCompareTextToCompareLog(logStr3).ToString();

            //VERIFY
            resultOk.ShouldEqual(logOk);
            result1.ShouldEqual(logStr1.Replace("BookDetail->", ""));
            result2.ShouldEqual(logStr2.Replace("MyEntity->", ""));
            result3.ShouldEqual(logStr3.Replace("BookDetail->", ""));
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
