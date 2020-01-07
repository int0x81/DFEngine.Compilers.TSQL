using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFEngine.Compilers.TSQL.Helpers
{
    /// <summary>
    /// Contains multiple helper methods that can change expression names
    /// cut expression chains by alias resolution and other techniques
    /// </summary>
    static class Beautifier
    {
        /// <summary>
        /// Takes in an expression and searches for columns that may have incomplete notations.
        /// the method then tries to assign these values by the given list of databse objects.
        /// It also replaces aliases by its actual datasources
        /// </summary>
        internal static Expression BeautifyColumns(Expression query, CompilerContext context)
        {
            if (query.Type.Equals(ExpressionType.COLUMN))
            {
                query = BeautifyColumn(query, context);
            }

            var newSubExpr = new List<Expression>();

            foreach (var subExpr in query.ChildExpressions)
            {
                newSubExpr.Add(BeautifyColumns(subExpr, context));
            }

            query.ChildExpressions = newSubExpr;
            return query;
        }

        /// <summary>
        /// Adds the informations from a database object to a column notation
        /// and then returns the enhanced notation
        /// </summary>
        internal static string EnhanceNotation(DatabaseObject dbo, string originalNotation)
        {
            Helper.SplitColumnNotationIntoSingleParts(originalNotation, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName, true);

            string enhancedNotation = $"{dbo.Name}.{columnName}";

            if (dbo.Schema != null)
            {
                enhancedNotation = $"{dbo.Schema}.{enhancedNotation}";

                if (dbo.Database != null)
                    enhancedNotation = $"{dbo.Database}.{enhancedNotation}";
            }

            return enhancedNotation;
        }

        /// <summary>
        /// Builds the full notation of a column by respecting the current context and also resolves aliases
        /// </summary>
        private static Expression BeautifyColumn(Expression column, CompilerContext context)
        {
            Helper.SplitColumnNotationIntoSingleParts(column.Name, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName, true);

            if (columnName.Equals(InternalConstants.WHOLE_OBJECT_SYNONYMOUS, StringComparison.InvariantCultureIgnoreCase))
                return column;

            if (databaseObjectName == null)
            {
                if (context.CurrentDatabaseObjectContext.Count == 1 && context.CurrentDatabaseObjectContext.Peek().Type.Equals(DatabaseObjectType.REAL))
                {
                    databaseObjectName = context.CurrentDatabaseObjectContext.Peek().Name;
                    databaseSchema = context.CurrentDatabaseObjectContext.Peek().Schema;
                    databaseName = context.CurrentDatabaseObjectContext.Peek().Database;
                }
                else
                {
                    databaseObjectName = InternalConstants.UNRELATED_OBJECT_NAME;
                    databaseSchema = InternalConstants.UNRELATED_SCHEMA_NAME;
                    databaseName = InternalConstants.UNRELATED_DATABASE_NAME;
                }
            }
            else
            {
                foreach (var dbo in context.CurrentDatabaseObjectContext)
                {
                    //Check if object name is one of the current aliases
                    if (!string.IsNullOrEmpty(dbo.Alias) && dbo.Alias.Equals(databaseObjectName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (dbo.Name == null)
                        {
                            Expression mappedExpression = MapExpressionFromRowSet(dbo, columnName);

                            if (mappedExpression == null)
                                throw new InvalidSqlException("Column does not exist");

                            if (mappedExpression.HasUnrelatedDatabaseObject || !mappedExpression.Type.Equals(ExpressionType.COLUMN))
                                AddWholeObjectSynonymousToChildExpressions(mappedExpression, dbo);

                            return mappedExpression;
                        }
                        else
                        {
                            databaseObjectName = dbo.Name;

                            if (dbo.Database != null)
                                databaseName = dbo.Database;

                            if (dbo.Schema != null)
                                databaseSchema = dbo.Schema;
                        }

                        break;
                    }
                    if (dbo.Name != null && dbo.Name.Equals(databaseObjectName))
                    {
                        if (databaseSchema == null || databaseSchema.Equals(dbo.Schema, StringComparison.InvariantCultureIgnoreCase))
                            databaseSchema = dbo.Schema;

                        if (databaseName == null || databaseName.Equals(dbo.Database, StringComparison.InvariantCultureIgnoreCase))
                            databaseName = dbo.Database;

                        break;
                    }
                }

                if (databaseSchema == null)
                    databaseSchema = InternalConstants.UNRELATED_SCHEMA_NAME;

                if (databaseName == null)
                    databaseName = context.CurrentDbContext;
            }
            column.Name = $"{databaseName}.{databaseSchema}.{databaseObjectName}.{columnName}";
            return column;
        }

        /// <summary>
        /// Loops through a given rowset and returns the expression that matches the name
        /// </summary>
        private static Expression MapExpressionFromRowSet(DatabaseObject rowSet, string columnName)
        {
            foreach (var item in rowSet.Expressions)
            {
                if (item.Type.Equals(ExpressionType.COLUMN))
                {
                    Helper.SplitColumnNotationIntoSingleParts(item.Name, out _, out _, out _, out string itemColumnName);
                    if (columnName.Equals(itemColumnName, StringComparison.InvariantCultureIgnoreCase))
                        return item;
                }
                if (item.Type.Equals(ExpressionType.ALIAS))
                {
                    if (columnName.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase))
                        return item;
                }
            }

            //At this point we did not find a direct match in the rowset.
            //Now we want to check if the rowset is build out of a single database object.
            //If so, we just expected the column to be in this dbo, else we return a
            //unrelated column.
            if (TryGetSingleDataSource(rowSet.Expressions, out DatabaseObject singleDbo))
                return new Expression(ExpressionType.COLUMN) { Name = EnhanceNotation(singleDbo, columnName) };
            else
            {
                Helper.SplitColumnNotationIntoSingleParts(columnName, out _, out _, out _, out string itemColumnName, true);
                return new Expression(ExpressionType.COLUMN) { Name = $"unrelated.unrelated.unrelated.{itemColumnName}" };
            }
        }

        /// <summary>
        /// Adds all whole object synonymous objects of a given dbo as child expressions to a given expression
        /// </summary>
        /// <param name="parentExpression">The parent expression to what the synonymous shall be added to</param>
        /// <param name="dbo">The rowset we are quering for whole object synonymous</param>
        private static void AddWholeObjectSynonymousToChildExpressions(Expression parentExpression, DatabaseObject dbo)
        {
            foreach (var expression in dbo.Expressions)
            {
                if (expression.IsWholeObjectSynonymous)
                    parentExpression.ChildExpressions.Add(expression);
            }
        }


        private static bool TryGetSingleDataSource(List<Expression> expressions, out DatabaseObject singleDataSource)
        {
            singleDataSource = null;

            foreach (var expression in expressions)
            {
                if (!expression.Type.Equals(ExpressionType.COLUMN))
                    continue;

                Helper.SplitColumnNotationIntoSingleParts(expression.Name, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName, true);

                if (columnName.Equals(InternalConstants.WHOLE_OBJECT_SYNONYMOUS, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (singleDataSource == null)
                {
                    singleDataSource = new DatabaseObject(DatabaseObjectType.REAL)
                    {
                        Database = databaseName,
                        Schema = databaseSchema,
                        Name = databaseObjectName
                    };
                }
                else
                {
                    if (!databaseObjectName.Equals(singleDataSource.Name))
                    {
                        singleDataSource = null;
                        return false;
                    }
                }
            }

            return singleDataSource != null;
        }
    }
}