using System;
using System.Collections.Generic;
using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using TSQL.Tokens;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    class CaseStatementResolver : IExpressionResolver
    {
        public Expression Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            List<Expression> expressions = new List<Expression>();

            // The CASE expression has two formats: 'simple' and 'searched'. For more information visit:
            // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/case-transact-sql?view=sql-server-2017
            bool isSimpleFormat = false;

            fileIndex++; //skip "case"

            if(!tokens[fileIndex].Text.ToLower().Equals("when"))
            {
                isSimpleFormat = true;
                StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
            }

            do
            {
                fileIndex++; //skip 'when'

                if (isSimpleFormat)
                    StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                else
                    SearchConditionResolver.Resolve(tokens, ref fileIndex, context);

                fileIndex++; //skip 'then'

                Expression thenExpression = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                if(thenExpression.Type.Equals(ExpressionType.COLUMN))
                    expressions.Add(thenExpression);
                else if (thenExpression.Type.Equals(ExpressionType.COMPLEX) || thenExpression.Type.Equals(ExpressionType.SCALAR_FUNCTION))
                    expressions.AddRange(thenExpression.ChildExpressions);

                if (tokens.Length <= fileIndex)
                    throw new InvalidSqlException("Trying to resolve case-statement without 'end'-keyword");

                if(tokens[fileIndex].Text.ToLower().Equals("when"))
                    continue;

                if (tokens[fileIndex].Text.ToLower().Equals("else"))
                {
                    fileIndex++; //skip 'else'
                    Expression elseExpression = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                    if(elseExpression.Type.Equals(ExpressionType.COLUMN))
                        expressions.Add(elseExpression);
                    else if(elseExpression.Type.Equals(ExpressionType.COMPLEX) || elseExpression.Type.Equals(ExpressionType.SCALAR_FUNCTION))
                        expressions.AddRange(elseExpression.ChildExpressions);
                }

                if (tokens[fileIndex].Text.ToLower().Equals("end"))
                {
                    fileIndex++; //skip 'end'
                    break;
                }

            } while (true);

            if (expressions.Count == 1)
                return expressions[0];
            else
            {
                return new Expression(ExpressionType.SCALAR_FUNCTION)
                {
                    Name = "CASE",
                    ChildExpressions = expressions
                };
            }
        }
    }
}
