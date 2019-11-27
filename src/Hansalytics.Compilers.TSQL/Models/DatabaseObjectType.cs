using System;
using System.Collections.Generic;
using System.Text;

namespace Hansalytics.Compilers.TSQL.Models
{
    public enum DatabaseObjectType
    {
        REAL, // Table or Views
        CTE, // A Common Table Expression
        VARIABLE, //A Table Variable
        SET, //A temporary data set
        SELECTION //A temporary selection
    }
}
