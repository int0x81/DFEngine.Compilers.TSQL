using Hansalytics.Compilers.TSQL.Models;
using Hansalytics.Compilers.TSQL.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;
using TSQL;
using TSQL.Tokens;
using Xunit;

namespace Hansalytics.Compilers.TSQL.UnitTests.ResolverTests
{
    public class WithStatementResolverTest
    {
        //[Fact]
        //public void ShouldResolveSingleWithStatementWithColumns()
        //{
        //    //Arrange
        //    string rawTsql = "";

        //    var resolver = new WithStatementResolver();
        //    int fileIndex = 0;
        //    CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
        //    ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

        //    //Act
        //    resolver.Resolve(tokens, ref fileIndex, context);

        //    //Assert
        //    Assert.Equal(tokens.Length, fileIndex);
        //    Assert.Single(context.CurrentDatabaseObjectContext);
        //}

        [Fact]
        public void ShouldResolveWithStatementWithoutColumns()
        {
            //Arrange
            string rawTsql = "with tmpTarget as (select * from dbo.someTable where tValidUntil >= getDate())";

            var resolver = new WithStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Single(context.CurrentDatabaseObjectContext);
        }

        //[Fact]
        //public void ShouldResolveWithStatementWithMultipleCTEs()
        //{
        //    //Arrange
        //    string rawTsql = "";

        //    var resolver = new WithStatementResolver();
        //    int fileIndex = 0;
        //    CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
        //    ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

        //    //Act
        //    resolver.Resolve(tokens, ref fileIndex, context);

        //    //Assert
        //    Assert.Equal(tokens.Length, fileIndex);
        //    Assert.Single(context.CurrentDatabaseObjectContext);
        //}
    }
}
