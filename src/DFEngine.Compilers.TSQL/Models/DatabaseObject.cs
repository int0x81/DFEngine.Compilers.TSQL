using DFEngine.Compilers.TSQL.Constants;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using System.Collections.Generic;

namespace DFEngine.Compilers.TSQL.Models
{
    /// <summary>
    /// Represents a database object (e.g. tables, views, etc)
    /// Temporary selections (sub-selects) may also be handled as database objects
    /// </summary>
    public class DatabaseObject
    {

        /// <summary>
        /// The name of the database object
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The alias under which this object can also be reached in its current context
        /// </summary>
        public string Alias { get; internal set; }

        /// <summary>
        /// The type of the database object
        /// </summary>
        public DatabaseObjectType Type { get; internal set; }

        /// <summary>
        /// A database object can be a tempopary object that is
        /// assembled by one or multiple expressions
        /// </summary>
        public List<Expression> Expressions { get; internal set; } = new List<Expression>();

        /// <summary>
        /// The name of the server on which this object exists
        /// </summary>
        public string Server { get; internal set; }

        /// <summary>
        /// The name of the database in which this object exists
        /// </summary>
        public string Database { get; internal set; }

        /// <summary>
        /// The name of the schema this object belongs to
        /// </summary>
        public string Schema { get; internal set; }

        internal DatabaseObject(string notation, CompilerContext context, DatabaseObjectType type, bool addStdObjects = true)
        {
            StatementResolveHelper.SplitObjectNotationIntoSingleParts(notation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName);

            Server = serverName;
            Database = databaseName;

            if (serverName == null && addStdObjects)
                Server = context.CurrentServerContext; 

            if (databaseName == null && addStdObjects)
                Database = context.CurrentDbContext;

            if (databaseSchema == null && addStdObjects)
                Schema = InternalConstants.UNRELATED_SCHEMA_NAME;
            else
                Schema = databaseSchema;

            if (databaseObjectName == null)
                throw new InvalidSqlException("Can't construct a database object without identifier");
            else
                Name = databaseObjectName;

            Expressions = null;
        }

        internal DatabaseObject(DatabaseObjectType type)
        {
            Type = type;
        }

        /// <summary>
        /// Two database objects are considered equal if all parts of the dbos match.
        /// When comparing two objects with incomplete information the comparison can
        /// lead two true although they are actual not equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is DatabaseObject dboToCompare)
            {
                if (!Server.Equals(dboToCompare.Server))
                    return false;

                if (!Database.Equals(dboToCompare.Database))
                        return false;

                if (!Schema.Equals(dboToCompare.Schema))
                    return false;

                if (!Name.Equals(dboToCompare.Name))
                    return false;

                return true;
            }
            else
                return false;
        }
    }
}
