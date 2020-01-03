using System;

namespace DFEngine.Compilers.TSQL.Exceptions
{
    /// <summary>
    /// Custom exception that gets thrown whenever the compiler detects errors in the
    /// tsql syntax
    /// </summary>
    public class InvalidSqlException : Exception
    {
        public InvalidSqlException()
        {
        }

        public InvalidSqlException(string message) : base(message)
        {
        }
    }
}