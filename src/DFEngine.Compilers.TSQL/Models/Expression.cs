using DFEngine.Compilers.TSQL.Helpers;
using System.Collections.Generic;

namespace DFEngine.Compilers.TSQL.Models
{
    /// <summary>
    /// Expressions can be a single constant, variable, column, or scalar function, but also wrappers around other expression
    /// and complex expressions (a concatenation between multiple expressions connected with operators).
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/language-elements/expressions-transact-sql?view=sql-server-ver15"/>
    public class Expression
    {
        /// <summary>
        /// The type of this expression
        /// </summary>
        public ExpressionType Type { get; internal set; }

        /// <summary>
        /// The value of the expression if its not a scalar_function
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The child sources which this data source is made of
        /// </summary>
        public List<Expression> ChildExpressions { get; internal set; } = new List<Expression>();
        public bool IsWholeObjectSynonymous 
        { 
            get 
            {
                Helper.SplitColumnNotationIntoSingleParts(Name, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName, true);
                return !string.IsNullOrEmpty(columnName) && columnName.Equals(InternalConstants.WHOLE_OBJECT_SYNONYMOUS);
            } 
        }

        public bool HasUnrelatedDatabaseObject
        {
            get
            {
                Helper.SplitColumnNotationIntoSingleParts(Name, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName, true);
                return !string.IsNullOrEmpty(databaseObjectName) && databaseObjectName.Equals(InternalConstants.UNRELATED_OBJECT_NAME);
            }
        }

        internal Expression(ExpressionType type)
        {
           Type = type;
        }

        public override string ToString() => "[" + Type.ToString() + "]: " + Name;
    }
}