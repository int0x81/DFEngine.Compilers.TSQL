using System;
using System.Collections.Generic;
using System.Text;

namespace Hansalytics.Compilers.TSQL.Helpers
{
    /// <summary>
    /// Contains constants as defined by Microsoft SQL server
    /// </summary>
    static class SQLServerConstants
    {
        /// <summary>
        /// The default schema of the database.
        /// </summary>
        internal static string DEFAULT_SCHEMA { get => BUILD_IN_SCHEMAS[0]; }
        /// <summary>
        /// The build-in schemas every database has and can not be deleted
        /// </summary>
        /// <see href="https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/ownership-and-user-schema-separation-in-sql-server"/>
        internal readonly static IReadOnlyList<string> BUILD_IN_SCHEMAS = new List<string>()
        {
            "dbo",
            "guest",
            "sys",
            "INFORMATION_SCHEMA"
        };
    }
}