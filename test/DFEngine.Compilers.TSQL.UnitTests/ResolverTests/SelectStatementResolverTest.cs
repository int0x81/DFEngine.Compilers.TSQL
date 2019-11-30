using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Helpers;
using DFEngine.Compilers.TSQL.Resolvers;
using System;
using TSQL.Tokens;
using Xunit;
using TSQL;

namespace DFEngine.Compilers.TSQL.UnitTests.StatementResolverTests
{
    public class SelectStatementResolverTest
    {
        [Fact]
        public void ShouldResolveSelectStatement()
        {
            //Arrange
            string rawTsql = "SELECT * FROM someTable";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Single(expression.ChildExpressions);
            Assert.Equal("stdDatabase.unrelated.someTable.*", expression.ChildExpressions[0].Name);
            Assert.Equal("SELECT", expression.Name);
        }

        [Fact]
        public void ShouldResolveSelectStatementWithAlias()
        {
            //Arrange
            string rawTsql = "SELECT someColumn_01 as someAlias FROM someTable";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Single(expression.ChildExpressions);
            Assert.Equal("stdDatabase.unrelated.someTable.someColumn_01", expression.ChildExpressions[0].Name);
            Assert.Equal("SELECT", expression.Name);
        }

        [Fact]
        public void ShouldResolveSelectStatementWithComplexExpression()
        {
            //Arrange
            string rawTsql = "SELECT someColumn_01, someColumn_02 * -2 AS someAlias FROM someTable";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal(2, expression.ChildExpressions.Count);
            Assert.Equal("stdDatabase.unrelated.someTable.someColumn_01", expression.ChildExpressions[0].Name);
            Assert.Equal(ExpressionType.COLUMN, expression.ChildExpressions[1].Type);
            Assert.Equal("stdDatabase.unrelated.someTable.someColumn_02", expression.ChildExpressions[1].Name);
            Assert.Equal("SELECT", expression.Name);
        }

        [Fact]
        public void ShouldResolveSelectStatementWithMultipleTables()
        {
            //Arrange
            string rawTsql = "SELECT someColumn FROM someTable, anotherTable";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Single(expression.ChildExpressions);
            Assert.Equal("unrelated.unrelated.unrelated.someColumn", expression.ChildExpressions[0].Name);
            Assert.Equal("SELECT", expression.Name);
        }

        [Fact]
        public void ShouldResolveSelectStatementWithTopKeyword()
        {
            //Arrange
            string rawTsql = "SELECT top 1 Keycol FROM DWH.dbo.someDim a";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Single(expression.ChildExpressions);
            Assert.Equal("DWH.dbo.someDim.Keycol", expression.ChildExpressions[0].Name);
            Assert.Equal("SELECT", expression.Name);
        }

        [Fact]
        public void ShouldResolveSelectStatementWithAliasResolution()
        {
            //Arrange
            string rawTsql = "SELECT a.[Key] FROM DWH.dbo.someDim a";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Single(expression.ChildExpressions);
            Assert.Equal("DWH.dbo.someDim.Key", expression.ChildExpressions[0].Name);
            Assert.Equal("SELECT", expression.Name);
        }

        [Fact]
        public void ShouldResolveRecursiveSelectStatement()
        {
            //Arrange
            string rawTsql = " SELECT CASE WHEN IsNull(ParamString27, '') = '' THEN 'string_01' ELSE ParamString27 END " +
                "              FROM someserver.[DB Name].[dbo].[tablix] fl " +
                "              WHERE ProjectNo = ( SELECT column_01 " +
                "                                  FROM DWH.dbo.dwhtable_01 dP" +
                "                                  WHERE dP.dwhColumn_01 = fL.outerColumn" +
                "                                )";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal("SELECT", expression.Name);
            //TODO
        }

        [Fact]
        public void ShouldResolveSelectStatementWithOverClause()
        {
            //Arrange
            string rawTsql = "select row_number() over (partition by [someColumn] order by [timestamp] desc) as rn " +
                             "from  someServer.someDb.dbo.[someTable]";
            IExpressionResolver resolver = new SelectStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal("SELECT", expression.Name);
        }
    }
}
