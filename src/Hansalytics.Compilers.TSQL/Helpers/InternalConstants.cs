namespace Hansalytics.Compilers.TSQL.Helpers
{
    /// <summary>
    /// Contains self defined constants
    /// </summary>
    public static class InternalConstants
    {
        /// <summary>
        /// The name of the server that contains all columns that are detected
        /// by the compiler but can not be associated with a specific database object.
        /// E.G. A Select statement has multiple joins but the columns are not specified
        /// </summary>
        internal const string UNRELATED_SERVER_NAME = "unrelated";

        /// <summary>
        /// The name of the database that contains all columns that are detected
        /// by the compiler but can not be associated with a specific database object.
        /// E.G. A Select statement has multiple joins but the columns are not specified
        /// </summary>
        internal const string UNRELATED_DATABASE_NAME = "unrelated";

        /// <summary>
        /// The name of the database schema that contains all columns that are detected
        /// by the compiler but can not be associated with a specific database object.
        /// E.G. A Select statement has multiple joins but the columns are not specified
        /// </summary>
        internal const string UNRELATED_SCHEMA_NAME = "unrelated";

        /// <summary>
        /// The name of the database object that contains all columns that are detected
        /// by the compiler but can not be associated with a specific database object.
        /// E.G. A Select statement has multiple joins but the columns are not specified
        /// </summary>
        internal const string UNRELATED_OBJECT_NAME = "unrelated";

        /// <summary>
        /// This special column name can be used as a reference, if the actual column name
        /// can not be identified.
        /// </summary>
        public const string UNRELATED_COLUMN_NAME = "unrelated";
    }
}
