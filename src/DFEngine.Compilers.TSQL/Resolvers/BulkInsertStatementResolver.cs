using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using TSQL.Tokens;
using System;
using DFEngine.Compilers.TSQL.Models;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Resolves a BULK INSERT statement
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/statements/bulk-insert-transact-sql?view=sql-server-ver15"/>
    class BulkInsertStatementResolver
    {
        /// <summary>
        /// ATM this resolver just skips the statement without adding anything to the context
        /// </summary>
        public void Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex += 2; //skip "bulk insert"
            StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);

            if (!tokens[fileIndex].Text.ToLower().Equals("from"))
                throw new InvalidSqlException("Missing 'from'-keyword in bulk insert statement");

            fileIndex++; //skip "from"

            string filePath = tokens[fileIndex].Text;

            fileIndex++; //skip file

            if (tokens[fileIndex].Text.ToLower().Equals("with"))
            {
                fileIndex += 2; //skip "with ("

                int openBracketCounter = 1;

                while (openBracketCounter > 0)
                {
                    if (tokens[fileIndex].Text.Equals(")"))
                    {
                        openBracketCounter--;
                    }
                    if (tokens[fileIndex].Text.Equals("("))
                    {
                        openBracketCounter++;
                    }
                    fileIndex++;
                }

            }
        }
    }
}