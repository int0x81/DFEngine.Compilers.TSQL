using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Resolvers;
using System;
using TSQL;
using TSQL.Tokens;
using Xunit;

namespace DFEngine.Compilers.TSQL.UnitTests.HelperTests
{
    
    public class SearchConditionResolverTest
    {
        [Fact]
        public void ShouldResolveExpression_01()
        {
            //Arrange
            string expression = "value IN (SELECT columnname FROM tablename2 WHERE condition = 1337)";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_02()
        {
            //Arrange
            string expression = "principal_id BETWEEN 16385 AND 16390";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_03()
        {
            //Arrange
            string expression = "ep.Rate > 27";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_04()
        {
            //Arrange
            string expression = "e.JobTitle IN ('Design Engineer', 'Tool Designer', 'Marketing Assistant')";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_05()
        {
            //Arrange
            string expression = "(5 * 4 + 5 * (2.4 - 6)) = 6";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_06()
        {
            //Arrange
            string expression = "p.BusinessEntityID NOT IN (SELECT BusinessEntityID FROM Sales.SalesPerson WHERE SalesQuota > 250000)";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_07()
        {
            //Arrange
            string expression = "(obfuscatedTable.abc = (obfuscatedTable.def * obfuscatedTable.ghi))";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_08()
        {
            //Arrange
            string expression = "(obfuscatedTable.abc * obfuscatedTable.def) = (obfuscatedTable.ghi)";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_09()
        {
            //Arrange
            string expression = "(not ([some field] like 'TOP%' OR [some field] like 'LP %')) or ([some field] is Null)";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_10()
        {
            //Arrange
            string expression = "(obfuscatedTable.abc * obfuscatedTable.def) BETWEEN (obfuscatedTable.ghi) AND 5";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpression_11()
        {
            //Arrange
            string expression = "[line_type] NOT IN ('Q') and NullIf(disputed_flag,'N') IS NULL";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }

        [Fact]
        public void ShouldResolveExpressionWithExistsKeyword()
        {
            //Arrange
            string expression = "NOT EXISTS (SELECT B.Cat_ID FROM Category_B B WHERE B.Cat_ID = A.Cat_ID)";
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();
            int fileIndex = 0;

            //Act
            SearchConditionResolver.Resolve(tokens, ref fileIndex, new CompilerContext("xUnit", "irrelevant", "irrelevant"));

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }
    }
}
