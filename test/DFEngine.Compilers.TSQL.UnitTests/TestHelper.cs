using DFEngine.Compilers.TSQL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFEngine.Compilers.TSQL.UnitTests
{
    /// <summary>
    /// Contains various methods needed for Unit Testing
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Gets the expected expression by part name out of a list of expressions. If the expected expression
        /// is not found, an exception is thrown. The found expression is returned
        /// </summary>
        /// <param name="expressions">The list of expressions</param>
        /// <param name="expectedExpressionName">The name of the expected expression. Does not have to be case sensitive</param>
        /// <returns>The found expression</returns>
        public static Expression GetExpectedExpression(this List<Expression> expressions, string expectedExpressionName)
        {
            foreach (var exp in expressions)
            {
                if (exp.Name.Equals(expectedExpressionName, StringComparison.InvariantCultureIgnoreCase))
                    return exp;
            }

            throw new Xunit.Sdk.ContainsException(new Expression(ExpressionType.COLUMN) { Name = expectedExpressionName }, expressions);
        }
    }
}
