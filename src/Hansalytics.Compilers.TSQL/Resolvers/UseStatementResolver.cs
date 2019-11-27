using Hansalytics.Compilers.TSQL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TSQL.Tokens;

namespace Hansalytics.Compilers.TSQL.Resolvers
{
    class UseStatementResolver
    {
        /// <summary>
        /// Resolves a USE statement, that changes the current database context
        /// </summary>
        /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/language-elements/use-transact-sql?view=sql-server-ver15"/>
        internal void ResolveUseStatement(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex++;

            context.CurrentDbContext = tokens[fileIndex].Text.ToLower();
        }
    }
}
