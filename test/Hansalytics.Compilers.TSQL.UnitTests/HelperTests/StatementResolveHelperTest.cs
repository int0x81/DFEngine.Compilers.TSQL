using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using System;
using System.Collections.Generic;
using TSQL;
using TSQL.Tokens;
using Xunit;

namespace DFEngine.Compilers.TSQL.UnitTests.HelperTests
{
    
    public class StatementResolveHelperTest
    {
        [Fact]
        public void ShouldResolveWhereStatement_02()
        {
            //Arrange
            string whereStatement = "WHERE stamm6.deleted = 0 AND object14.deleted = 0 and feld10 is not null AND (ISNUMERIC(LEFT([feld34], 4)) = 1 " +
                                    "OR([feld34] LIKE '[0-9][0-9][0-9][0-9][A-Z][ ]'))";
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(whereStatement).ToArray();

            //Act
            StatementResolveHelper.ResolveWhereStatement(tokens, ref fileIndex, context);

            //Assert
            Assert.True(tokens.Length == fileIndex);
        }

        [Fact]
        public void ShouldResolveWhereStatement_03()
        {
            //Arrange
            string whereStatement = "WHERE someTable.someColumn in ('string_01', 'string_02', 'string_03')";
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(whereStatement).ToArray();

            //Act
            StatementResolveHelper.ResolveWhereStatement(tokens, ref fileIndex, context);

            //Assert
            Assert.True(tokens.Length == fileIndex);
        }

        [Fact]
        public void ShouldResolveComplexWhereStatement_01()
        {
            //Arrange
            string whereStatement = "where (([Budget Name] LIKE 'B%' AND YEAR([Date])>=2010) " +
                                    "OR     ([Budget Name] LIKE 'P%' AND [Budget Name] >= 'P2011' AND YEAR([Date])>=2011)) " +
                                    "AND     YEAR([Date])<=2010";
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(whereStatement).ToArray();

            //Act
            StatementResolveHelper.ResolveWhereStatement(tokens, ref fileIndex, context);

            //Assert
            Assert.True(tokens.Length == fileIndex);
        }

        [Fact]
        public void ShouldResolveFromStatement_01()
        {
            //Arrange
            string fromStatement = "FROM ( SELECT HIST.someCol_01, HIST.someCol_02 FROM server_01.db_01.dbo.historyTable HIST) AS R " +
                "INNER JOIN DWH.dbo.someDimension SOME_ALIAS_01 ON SOME_ALIAS_01.someCol_03 = some_FK COLLATE Latin1_General_CI_AS " +
                "AND SOME_DATE BETWEEN SOME_ALIAS_01.ValidFrom AND SOME_ALIAS_01.ValidUntil";
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(fromStatement).ToArray();

            //Act
            StatementResolveHelper.ResolveFromStatement(tokens, ref fileIndex, context);

            //Assert
            Assert.True(tokens.Length == fileIndex);
        }

        [Fact]
        public void ShouldResolveFromStatementwithApplyKeyword_02()
        {
            //Arrange
            string fromStatement = "FROM t1                     " +
                                   "CROSS APPLY                 " +
                                   "(                           " +
                                   "SELECT TOP 3 *              " +
                                   "FROM    t2                  " +
                                   "WHERE   t2.t1_id = t1.id    " +
                                   "ORDER BY                    " +
                                   "       t2.rank DESC         " +
                                   ") t2o                       ";

            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(fromStatement).ToArray();

            //Act
            StatementResolveHelper.ResolveFromStatement(tokens, ref fileIndex, context);

            //Assert
            Assert.True(tokens.Length == fileIndex);
        }

        [Fact]
        public void ShouldResolveFromStatementWithPivotClause()
        {
            //Arrange
            string joinStatement = "FROM (SELECT DaysToManufacture, StandardCost " +
                                   "      FROM Production.dbo.Product) AS SourceTable " +
                                   "PIVOT( AVG(StandardCost)" +
                                   "FOR DaysToManufacture IN([0], [1], [2], [3], [4])" +
                                   "     ) AS PivotTable";
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(joinStatement).ToArray();

            //Act
            StatementResolveHelper.ResolveFromStatement(tokens, ref fileIndex, context);

            //Assert
            Assert.True(tokens.Length == fileIndex);
        }

        [Fact]
        public void ShouldResolveFromStatementWithFunctionAsDataObject()
        {
            //Arrange
            string joinStatement = "FROM fn_helpcollations()";
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(joinStatement).ToArray();

            //Act
            StatementResolveHelper.ResolveFromStatement(tokens, ref fileIndex, context);

            //Assert
            Assert.True(tokens.Length == fileIndex);
        }

        [Fact]
        public void ShouldSplitObjectNotation_01()
        {
            //Arrange
            string notation = "server.database.schema.object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Equal("server", serverName);
            Assert.Equal("database", databaseName);
            Assert.Equal("schema", databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        [Fact]
        public void ShouldSplitObjectNotation_02()
        {
            //Arrange
            string notation = "server.database..object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Equal("server", serverName);
            Assert.Equal("database", databaseName);
            Assert.Null(databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        [Fact]
        public void ShouldSplitObjectNotation_03()
        {
            //Arrange
            string notation = "server..schema.object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Equal("server", serverName);
            Assert.Null(databaseName);
            Assert.Equal("schema", databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        [Fact]
        public void ShouldSplitObjectNotation_04()
        {
            //Arrange
            string notation = "server...object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Equal("server", serverName);
            Assert.Null(databaseName);
            Assert.Null(databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        [Fact]
        public void ShouldSplitObjectNotation_05()
        {
            //Arrange
            string notation = "database.schema.object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Null(serverName);
            Assert.Equal("database", databaseName);
            Assert.Equal("schema", databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        [Fact]
        public void ShouldSplitObjectNotation_06()
        {
            //Arrange
            string notation = "database..object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Null(serverName);
            Assert.Equal("database", databaseName);
            Assert.Null(databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        [Fact]
        public void ShouldSplitObjectNotation_07()
        {
            //Arrange
            string notation = "schema.object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Null(serverName);
            Assert.Null(databaseName);
            Assert.Equal("schema", databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        [Fact]
        public void ShouldSplitObjectNotation_08()
        {
            //Arrange
            string notation = "object";

            //Act
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            //Assert
            Assert.Null(serverName);
            Assert.Null(databaseName);
            Assert.Null(databaseSchema);
            Assert.Equal("object", databaseObjectName);
        }

        /// <summary>
        /// Should resolve a '*' character as column expression
        /// </summary>
        [Fact]
        public void ShouldResolveAllColumnsSynonymous()
        {
            //Arrange
            string expression = "*";
            CompilerContext context = new CompilerContext("xUnit", "irrelevant", "irrelevant");
            int fileIndex = 0;
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(expression).ToArray();

            //Act
            var result = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal("*", result.Name);
            Assert.Equal(ExpressionType.COLUMN, result.Type);
        }
    }
}
