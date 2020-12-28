// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.TablePerType;
using EfSchemaCompare;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestTablePerType
    {
        [Fact]
        public void CompareTptContext()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<TptDbContext>();
            using var context = new TptDbContext(options);
            context.Database.EnsureClean();

            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }        
        
        [Fact]
        public void CompareTptContextIgnoreViaIgnoreTheseErrors()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<TptDbContext>();
            using var context = new TptDbContext(options);
            context.Database.EnsureClean();

            var config = new CompareEfSqlConfig();
            config.IgnoreTheseErrors(@"DIFFERENT: TptVer1->PrimaryKey 'PK_TptBases', constraint name. Expected = PK_TptBases, found = PK_TptVer1
DIFFERENT: TptVer1->Property 'Id', value generated. Expected = OnAdd, found = Never
DIFFERENT: TptVer1->Property 'MyVer1Int', nullability. Expected = NULL, found = NOT NULL
DIFFERENT: TptVer1->ForeignKey 'FK_TptVer1_TptBases_Id', delete behaviour. Expected = ClientCascade, found = NoAction
DIFFERENT: Entity 'TptVer1', constraint name. Expected = PK_TptBases, found = PK_TptVer1
DIFFERENT: TptVer2->PrimaryKey 'PK_TptBases', constraint name. Expected = PK_TptBases, found = PK_TptVer2
DIFFERENT: TptVer2->Property 'Id', value generated. Expected = OnAdd, found = Never
DIFFERENT: TptVer2->Property 'MyVer2Int', nullability. Expected = NULL, found = NOT NULL
DIFFERENT: TptVer2->ForeignKey 'FK_TptVer2_TptBases_Id', delete behaviour. Expected = ClientCascade, found = NoAction
DIFFERENT: Entity 'TptVer2', constraint name. Expected = PK_TptBases, found = PK_TptVer2");

            var comparer = new CompareEfSql(config);

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeFalse(comparer.GetAllErrors);
        }
    }
}