using System;
using System.Collections.Generic;
using System.Text;

namespace DFEngine.Compilers.TSQL
{
    /// <summary>
    /// Options for the compiler that state, how the expression chains should be created
    /// </summary>
    public class CompilerOptions
    {
        /// <summary>
        /// States if columns considered on compilation.
        /// </summary>
        public bool ConsiderQueries { get; set; }
    }
}
