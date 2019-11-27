using System;
using System.Collections.Generic;
using System.Text;

namespace Hansalytics.Compilers.TSQL.Helpers
{
    public static class StringHelper
    {
        public static string RemoveSquareBrackets(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                throw new ArgumentException("Trying to remove brackets from an empty string", "inputString");

            return inputString.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        public static string RemoveQuotationMarks(string inputString)
        {
            return inputString.Substring(1, inputString.Length - 2);
        }
    }
}