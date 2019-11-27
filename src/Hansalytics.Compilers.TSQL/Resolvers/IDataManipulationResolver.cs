using Hansalytics.Compilers.TSQL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TSQL.Tokens;

namespace Hansalytics.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Can resolve a data manipulation statement recursivly
    /// </summary>
    interface IDataManipulationResolver
    {
        /// <summary>
        /// Resolves a data manipulation statement recursivly
        /// </summary>
        DataManipulation Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context);
    }
}
