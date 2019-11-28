using System.Collections.Generic;

namespace DFEngine.Compilers.TSQL.Models
{
    /// <summary>
    /// Represents a data manipulation that either directly manipulates
    /// data in a given object or loads data from one object into another
    /// </summary>
    public class DataManipulation
    {
        /// <summary>
        /// Contains the recursive expression set that represents the sql data flow. The difference
        /// to a data query is that the top level expression is ALWAYS a column that is beeing
        /// manipulated
        /// </summary>
        public List<Expression> Expressions { get; internal set; } = new List<Expression>();
    }
}
