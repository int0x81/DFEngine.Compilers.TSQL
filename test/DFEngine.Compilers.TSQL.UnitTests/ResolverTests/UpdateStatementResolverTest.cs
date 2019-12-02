using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Resolvers;
using System;
using TSQL;
using TSQL.Tokens;
using Xunit;

namespace DFEngine.Compilers.TSQL.UnitTests.StatementResolverTests
{
    public class UpdateStatementResolverTest
    {
        [Fact]
        public void ShouldResolveUpdateStatementWithRecursiveExpressions()
        {
            //Arrange
            string rawTsql = "update alias1 " +
                             "set alias1.tIsLastRecord = case when alias2.tValidUntil is Null THEN 0 ELSE alias2.tValidUntil END " +
                             "from Stage_DB.dbo.someTable as alias1 " +
                             "join (SELECT * from someOtherTable) alias2 on alias1.col13 = alias2.col37";

            IDataManipulationResolver resolver = new UpdateStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase", true);
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            DataManipulation statement = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal(ExpressionType.COLUMN, statement.Expressions[0].Type);
            Assert.Equal("Stage_DB.dbo.someTable.tIsLastRecord", statement.Expressions[0].Name);
            Assert.Single(statement.Expressions[0].ChildExpressions);
            Assert.Equal("unrelated.unrelated.unrelated.tValidUntil", statement.Expressions[0].ChildExpressions[0].Name);
        }

        [Fact]
        public void ShouldResolveUpdateStatementWithAliasTarget()
        {
            //Arrange
            string rawTsql = "update GL " +
                             "set somevalue = nil.col_01, someOtherValue = gl.col_02 " +
                             "from [SOME_table] nil " +
                             "inner join [dbo].[TABLE_02] GL";

            IDataManipulationResolver resolver = new UpdateStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase", true);
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            DataManipulation statement = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal(ExpressionType.COLUMN, statement.Expressions[0].Type);
            Assert.Equal("stdDatabase.dbo.TABLE_02.somevalue", statement.Expressions[0].Name);
            Assert.Equal("stdDatabase.unrelated.SOME_table.col_01", statement.Expressions[0].ChildExpressions[0].Name);
            Assert.Equal(ExpressionType.COLUMN, statement.Expressions[1].Type);
            Assert.Equal("stdDatabase.dbo.TABLE_02.someOtherValue", statement.Expressions[1].Name);
            Assert.Equal("stdDatabase.dbo.TABLE_02.col_02", statement.Expressions[1].ChildExpressions[0].Name);
        }

        [Fact]
        public void ShouldResolveUpdateStatement()
        {
            //Arrange
            string rawTsql = "update someFactTable " +
                             "set somecolumn = nil.col_01, someOtherColumn = [dbo].gl.col_02 " +
                             "from DWH_DB.[dbo].[someFactTable]";

            IDataManipulationResolver resolver = new UpdateStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase", true);
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            DataManipulation statement = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal(2, statement.Expressions.Count);
            Assert.Equal(ExpressionType.COLUMN, statement.Expressions[0].Type);
            Assert.Equal("DWH_DB.dbo.someFactTable.somecolumn", statement.Expressions[0].Name);
            Assert.Equal("stdDatabase.unrelated.nil.col_01", statement.Expressions[0].ChildExpressions[0].Name);
            Assert.Equal(ExpressionType.COLUMN, statement.Expressions[1].Type);
            Assert.Equal("DWH_DB.dbo.someFactTable.someOtherColumn", statement.Expressions[1].Name);
            Assert.Equal("stdDatabase.dbo.gl.col_02", statement.Expressions[1].ChildExpressions[0].Name);
        }
    }
}