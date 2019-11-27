using Hansalytics.Compilers.TSQL.Models;
using Hansalytics.Compilers.TSQL.Helpers;
using Hansalytics.Compilers.TSQL.Resolvers;
using System;
using TSQL;
using TSQL.Tokens;
using Xunit;

namespace Hansalytics.Compilers.TSQL.UnitTests.StatementResolverTests
{
    public class TsqlFunctionTest
    {
        [Fact]
        public void ShouldResolveMaxFunction()
        {
            //Arrange
            string rawTsql = "MAX(CAST(timestamp as bigint))";

            IExpressionResolver resolver = new TsqlFunctionResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveHashBytesFunction()
        {
            //Arrange
            string rawTsql = "hashbytes ('sha1337', isNull(cast(somecolumn_01 as varchar), '') " +
                "+ isNull(cast(cast(somecolumn_02 as date) as varchar), '')" +
                "+ isNull(cast(somecolumn_04 collate Latin1_General_CI_AS as NVARCHAR(64)), ''))";

            IExpressionResolver resolver = new TsqlFunctionResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveSelectWithCostumFunction()
        {
            //Arrange
            string rawTsql = "SELECT " +
                "DWH.dbo.COSTUMFUNCTION(DWH.dbo.ANOTHERCOSTUMFUNCTION(OBF.schemaa.table1.[some Column_01], 'YEN'))" +
                "FROM someTable";

            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }
    }
}
