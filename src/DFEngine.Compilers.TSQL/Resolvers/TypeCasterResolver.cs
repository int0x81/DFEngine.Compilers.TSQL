using System;
using System.Collections.Generic;
using DFEngine.Compilers.TSQL.Helpers;
using DFEngine.Compilers.TSQL.Models;
using TSQL.Tokens;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    class TypeCasterResolver : IExpressionResolver
    {
        public Expression Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            List<Expression> expressions = new List<Expression>();

            string functionName = tokens[fileIndex].Text.ToUpper();
            fileIndex++; //skiptype

            if (tokens[fileIndex].Text.Equals("("))
            {
                fileIndex++; //skip "("

                if (tokens[fileIndex].Text.Equals("max") || tokens[fileIndex].Text.Equals("min"))
                    fileIndex++;
                else
                {
                    do
                    {
                        var innerExpression = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

                        if (innerExpression.Type.Equals(ExpressionType.COLUMN))
                            expressions.Add(innerExpression);

                        if (tokens[fileIndex].Text.Equals(","))
                        {
                            fileIndex++; //skip ','
                            continue;
                        }
                        else
                            break;
                    }
                    while (true);
                }

                fileIndex++; //skip ")"
            }

            if(expressions.Count == 1)
                return expressions[0];
            else
            {
                return new Expression(ExpressionType.SCALAR_FUNCTION)
                {
                    Name = functionName,
                    ChildExpressions = expressions
                };
            }
        }
    }
}
