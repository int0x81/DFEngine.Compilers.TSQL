using System;
using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using TSQL.Tokens;
using DFEngine.Compilers.TSQL.Models.DataEntities;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Resolves a merge statement
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql?view=sql-server-ver15"/>
    class MergeStatementResolver : IDataManipulationResolver
    {
        DataManipulation statement;

        public DataManipulation Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            statement = new DataManipulation();

            fileIndex++; //skip "merge"

            //skip top expression
            SkipTopExpression(tokens, ref fileIndex, context);

            if (tokens[fileIndex].Text.ToLower().Equals("into"))
                fileIndex++;

            var targetTable = StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);

            if (!tokens[fileIndex].Text.ToLower().Equals("using"))
                throw new InvalidSqlException("Trying to resolve a merge-statement without using keyword");
            
            var source = ResolveUsingStatement(tokens, ref fileIndex, context);

            if (!tokens[fileIndex].Text.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidSqlException("Expected 'ON' keyword when resolving a 'MERGE'-statement");

            fileIndex++; //skip 'on'

            SearchConditionResolver.Resolve(tokens, ref fileIndex, context);

            while (!tokens[fileIndex].Text.ToLower().Equals(";"))
            {
                fileIndex++;
                if (fileIndex == tokens.Length)
                    throw new InvalidSqlException("Trying to resolve a merge-statement without proper ';' determination");
            }

            fileIndex++; //skip ';'

            return statement;
        }

        private DatabaseObject ResolveUsingStatement(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex++; //skip 'using'
            return StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);
        }

        private void SkipTopExpression(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (!tokens[fileIndex].Text.ToLower().Equals("top"))
                return;

            fileIndex++; //skip top

            StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

            if (tokens[fileIndex].Text.ToLower().Equals("percent"))
                fileIndex++;
        }
    }
}