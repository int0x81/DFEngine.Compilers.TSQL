using System;
using DFEngine.Compilers.TSQL.Helpers;
using TSQL.Tokens;
using DFEngine.Compilers.TSQL.Models;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Resolves an OPEN QUERY statement 
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/openquery-transact-sql?view=sql-server-ver15"/>
    class OpenQueryStatementResolver
    {
        public DatabaseObject Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex += 2; //skip "openquery("

            string serverLink = StringHelper.RemoveSquareBrackets(tokens[fileIndex].Text.ToLower());

            fileIndex += 2; //skip '<serverlink>,'

            string query = StringHelper.RemoveQuotationMarks(tokens[fileIndex].Text);

            var compilerOptions = new CompilerOptions() { ConsiderQueries = context.ConsiderQueries };

            var result = new Compiler().Compile(query, serverLink, serverLink + "_stdDB", context.Causer, compilerOptions);

            var openQuery = new DatabaseObject(DatabaseObjectType.SELECTION)
            {
                Name = "OPENQUERY" 
            };

            openQuery.Expressions.AddRange(result.DataQueries);

            return openQuery;
        }
    }
}
