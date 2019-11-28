using DFEngine.Compilers.TSQL.Models.DataEntities;

namespace DFEngine.Compilers.TSQL.Models
{
    /// <summary>
    /// Represents a SELECT statement
    /// </summary>
    public class SelectStatement
    {
        /// <summary>
        /// The actual query
        /// </summary>
        public Expression Expression { get; internal set; } = new Expression(ExpressionType.SCALAR_FUNCTION) { Name = "SELECT" };

        /// <summary>
        /// The target database object
        /// </summary>
        public DatabaseObject TargetObject { get; internal set; }
    }
}