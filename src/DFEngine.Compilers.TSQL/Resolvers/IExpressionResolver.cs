using DFEngine.Compilers.TSQL.Models;
using System;
using TSQL.Tokens;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Can resolve an expression recursivly by a given set of tsql tokens
    /// </summary>
    interface IExpressionResolver
    {
        /// <summary>
        /// Resolves an expression recursivly by a given set of tsql tokens
        /// </summary>
        Expression Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context);
    }
}
