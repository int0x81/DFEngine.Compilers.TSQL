using Hansalytics.Compilers.TSQL.Models;
using Hansalytics.Compilers.TSQL.Helpers;
using Hansalytics.Compilers.TSQL.Resolvers;
using System;
using TSQL;
using TSQL.Tokens;
using Xunit;

namespace Hansalytics.Compilers.TSQL.UnitTests.ResolverTests
{
    public class CaseStatementResolverTest
    {
        [Fact]
        public void ShouldResolveSimpleCaseStatement()
        {
            //Arrange
            string rawTsql = "CASE someTestcolumn" +
                "             WHEN 'string_01' THEN COL_13 " +
                "             WHEN 'string_02' THEN [dbo].[epic Table].COL_14 " +
                "             END";

            IExpressionResolver resolver = new CaseStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal(2, expression.ChildExpressions.Count);
            Assert.Equal("CASE", expression.Name);
            Assert.Equal("COL_13", expression.ChildExpressions[0].Name);
            Assert.Equal("dbo.epic Table.COL_14", expression.ChildExpressions[1].Name);

        }

        [Fact]
        public void ShouldResolveSearchCaseStatement()
        {
            //Arrange
            string rawTsql = "case when GL.DocumentId is null then 'string' else dbo.GL.DocumentID end";

            IExpressionResolver resolver = new CaseStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
            Assert.Equal("dbo.GL.DocumentID", expression.Name);
        }

        [Fact]
        public void ShouldResolveRecursiveCaseStatement()
        {
            //Arrange
            string rawTsql = " CASE                                                    " +
                             "   WHEN someValue > anotherValue THEN                    " +
                             "      CASE                                               " +
                             "         WHEN NullIf(val_01,0) IS NULL THEN NULL         " +
                             "         WHEN NullIf(val_02,0) IS NULL THEN Null         " +
                             "         WHEN(100 / val_02 * val_01) <= 100 THEN 'Green' " +
                             "         WHEN 100 + 25 <= 125 THEN 'Yellow'" +
                             "         ELSE 'Red'                                      " +
                             "      END                                                " +
                             "   ELSE                                                  " +
                             "      CASE                                               " +
                             "         WHEN NULLIF(val_03, 0) IS NULL THEN NULL        " +
                             "         WHEN NullIf(val_02,0) IS NULL THEN NULL         " +
                             "         WHEN(100 / val_02 * val_03) <= 100 THEN 'Green' " +
                             "         WHEN(100 / val_02 * val_03) <= 125 THEN 'Yellow'" +
                             "         ELSE 'Red'                                      " +
                             "      END                                                " +
                             " END"                                                      ;

            IExpressionResolver resolver = new CaseStatementResolver();
            int fileIndex = 0;
            CompilerContext context = new CompilerContext("xUnit", "stdserver", "stdDatabase");
            ReadOnlySpan<TSQLToken> tokens = TSQLTokenizer.ParseTokens(rawTsql).ToArray();

            //Act
            Expression expression = resolver.Resolve(tokens, ref fileIndex, context);

            //Assert
            Assert.Equal(tokens.Length, fileIndex);
        }
    }
}
