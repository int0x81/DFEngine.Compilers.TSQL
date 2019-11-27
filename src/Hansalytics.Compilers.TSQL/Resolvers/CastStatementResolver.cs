using Hansalytics.Compilers.TSQL.Models;
using Hansalytics.Compilers.TSQL.Exceptions;
using Hansalytics.Compilers.TSQL.Helpers;
using System;
using TSQL.Tokens;
using System.Collections.Generic;

namespace Hansalytics.Compilers.TSQL.Resolvers
{
    class CastStatementResolver : IExpressionResolver
    {
        public Expression Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            List<Expression> expressions = new List<Expression>();

            fileIndex += 2; //Skip 'cast ('

            Expression innerExpression = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

            if (innerExpression.Type.Equals(ExpressionType.COLUMN))
                expressions.Add(innerExpression);
            else if (innerExpression.Type.Equals(ExpressionType.COMPLEX) || innerExpression.Type.Equals(ExpressionType.SCALAR_FUNCTION))
                expressions.AddRange(innerExpression.ChildExpressions);

            fileIndex ++; //skip 'as'

            TypeCasterResolver caster = new TypeCasterResolver();
            caster.Resolve(tokens, ref fileIndex, context);

            if (!tokens[fileIndex].Text.Equals(")"))
                throw new InvalidSqlException("Cast statement does not end with a closing bracket");
            
            fileIndex++; //skip ')' 
            
            if(expressions.Count != 1)
            {
                return new Expression(ExpressionType.SCALAR_FUNCTION)
                {
                    Name = "CAST",
                    ChildExpressions = expressions
                };
            }
            else
                return expressions[0];
        }
    }
}