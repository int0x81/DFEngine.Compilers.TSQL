using System;
using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using TSQL.Tokens;
using DFEngine.Compilers.TSQL.Helpers;
using System.Collections.Generic;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// A scalar function that returns a single value
    /// </summary>
    class TsqlFunctionResolver : IExpressionResolver
    {

        public Expression Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            List<Expression> expressions = new List<Expression>();
            string functionName = "";

            if (tokens[fileIndex + 1].Text.Equals("::")) //Special function types eg: "geography::STPointFromText(........)"
            {
                fileIndex += 4;
            }
            else
            {
                functionName = tokens[fileIndex].Text.ToUpper();
                fileIndex += 2; //skip function-keyword + '('
            }

            if (tokens[fileIndex].Text.Equals(")")) //parameterless function can be ignored
            {
                fileIndex++;
                return new Expression(ExpressionType.SCALAR_FUNCTION)
                {
                    Name = functionName
                };
            }
                
            do
            {
                var parameter = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

                if(parameter.Type.Equals(ExpressionType.COMPLEX) || parameter.Type.Equals(ExpressionType.SCALAR_FUNCTION))
                {
                    foreach (var subExp in parameter.ChildExpressions)
                        expressions.Add(subExp);
                }
                else if(parameter.Type.Equals(ExpressionType.COLUMN))
                    expressions.Add(parameter);

                if (fileIndex < tokens.Length && tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++; //skip ","
                    continue;
                }
                else
                    break;
            }
            while (true);

            fileIndex++; //skip ')'

            if (expressions.Count != 1)
            {
                return new Expression(ExpressionType.SCALAR_FUNCTION)
                {
                    Name = functionName,
                    ChildExpressions = expressions
                };
            }
            else
                return expressions[0];
        }
    }
}
