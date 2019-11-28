using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using DFEngine.Compilers.TSQL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TSQL.Tokens;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Resolves a with expression and adds the common table expressions (CTE)
    /// to the current context
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/queries/with-common-table-expression-transact-sql?view=sql-server-ver15"/>
    class WithStatementResolver
    {
        internal void Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex++; //skip 'with'

            do
            {
                var alias = tokens[fileIndex].Text.ToLower();

                if (string.IsNullOrEmpty(alias))
                    throw new InvalidSqlException("Common table expression must have an alias");

                fileIndex++; //skip alias

                List<Expression> columns = new List<Expression>();

                if(!tokens[fileIndex].Text.Equals("as", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (tokens[fileIndex].Text.Equals("("))
                        fileIndex++; //skip '('

                    do
                    {
                        var column = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

                        if (!column.Type.Equals(ExpressionType.COLUMN))
                            throw new InvalidSqlException("Common Table Expressions only may contain conrete columns");

                        columns.Add(column);

                        if (tokens[fileIndex].Text.Equals(","))
                        {
                            fileIndex++; //skip ','
                            continue;
                        }
                        else
                            break;
                    }
                    while (true);

                    if (tokens[fileIndex].Text.Equals(")"))
                        fileIndex++; //skip ')'
                }

                fileIndex++; //skip 'as'

                var cte = StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);
                cte.Alias = alias;
                cte.Type = DatabaseObjectType.CTE;
                cte.Columns = columns;

                context.CurrentDatabaseObjectContext.Push(cte);

                if (tokens.Length > fileIndex && tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++; //skip ','
                    continue;
                }
                else
                    break;

            } while (true);
        }
    }
}
