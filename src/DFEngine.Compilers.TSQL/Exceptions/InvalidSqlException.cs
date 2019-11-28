using System;

namespace DFEngine.Compilers.TSQL.Exceptions
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