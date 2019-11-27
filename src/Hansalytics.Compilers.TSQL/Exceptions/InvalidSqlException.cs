using System;

namespace Hansalytics.Compilers.TSQL.Exceptions
{
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