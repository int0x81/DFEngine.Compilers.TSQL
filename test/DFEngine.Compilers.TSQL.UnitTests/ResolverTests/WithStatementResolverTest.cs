using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Resolvers;
using System;
using TSQL;
using TSQL.Tokens;
using Xunit;

namespace DFEngine.Compilers.TSQL.UnitTests.ResolverTests
{
    public class WithStatementResolverTest
    {
        [Fact]
        public void ShouldResolveWithStatementWithoutColumns()
        {
            //Arrange
            string rawTsql = "with tmpTarget as (select * from dbo.someTable where tValidUntil >= getDate())";

            var resolver = new WithStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase", true);
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
