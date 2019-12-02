using System.Collections.Generic;
using TSQL.Tokens;
using System;
using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Resolvers;
using TSQL;
using Xunit;

namespace DFEngine.Compilers.TSQL.UnitTests.ResolverTests
{
    
    public class InsertStatementResolverTest
    {
        [Fact]
        public void ShouldResolveInsertStatement()
        {
            //Arrange
            string rawTsql = "INSERT INTO BI_STAGE.dbo.EventLogTable (targetcolumn, MessageType, MessageTarget, " +
                             "StepName, Message, DetailedMessage, Duration, InsertDate) " +
                             "VALUES (LoadRunId, 'ERROR', 'some_sql_file.sql', 'ParentChild Dimension AccountScheduleTree [DWH]'," +
                             "'Error while creating Parentchild Accountschema for schema ' + @CurrentSchedule," +
                             "ERROR_MESSAGE(), null, GETDATE())";

            IDataManipulationResolver resolver = new InsertStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase", true);
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            DataManipulation statement = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Single(statement.Expressions);
            Assert.Equal("BI_STAGE.dbo.EventLogTable.targetcolumn", statement.Expressions[0].Name);
            Assert.Equal("unrelated.unrelated.unrelated.LoadRunId", statement.Expressions[0].ChildExpressions[0].Name);
        }

        [Fact]
        public void ShouldResolveRecursiveInsertStatement()
        {
            //Arrange
            string rawTsql = "INSERT INTO BI_STAGE.dbo.ERP_TABLE ([timestamp], [Name], [CustNo], [VendNo]) " +
                             "SELECT [timestamp], [Name], [CustNo], [VendNo] FROM ERP_SERVER.ERP2099PROD.dbo.[some Table]";

            IDataManipulationResolver resolver = new InsertStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase", true);
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            DataManipulation statement = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal("BI_STAGE.dbo.ERP_TABLE.timestamp", statement.Expressions[0].Name);
            Assert.Equal("ERP2099PROD.dbo.some Table.timestamp", statement.Expressions[0].ChildExpressions[0].Name);
        }

        //[Fact]
        //public void ShouldResolveInsertStatementWithWithExpression()
        //{
        //    //Arrange
        //    string rawTsql = "With tmp as (SELECT * FROM db1.dbo.someTable) INSERT INTO db2.dbo.targetTable (column_01, column_02) VALUES (tmp.col_01, tmp.col_02)";

        //    IDataManipulationResolver resolver = new InsertStatementResolver();
        //    int fileIndex = 0;
        //    CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
        //    ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

        //    //Act
        //    DataManipulation statement = resolver.Resolve(tokens, ref fileIndex, context);

        //    //Assert
        //}
    }
}
