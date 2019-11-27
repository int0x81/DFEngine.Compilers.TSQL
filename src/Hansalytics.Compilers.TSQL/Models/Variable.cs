using System;
using System.Collections.Generic;
using System.Text;

namespace Hansalytics.Compilers.TSQL.Models
{
    /// <summary>
    /// Represents a TSQL variable
    /// </summary>
    class Variable
    {
        internal string Identifier { get; }
        internal string Value { get; set; }
    }
}
