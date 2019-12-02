using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DFEngine.Compilers.TSQL.UnitTests
{
    public class CompilerTest
    {
        [Fact]
        public void ShouldCompileSQL()
        {
            //Arrange
            string rawTsql = "SELECT justAColumn FROM testTable " +
                "UPDATE targetTable t SET t.col01 = s.col1337 FROM testTable s";
            Compiler compiler = new Compiler();

            var compilerOptions = new CompilerOptions() { ConsiderQueries = true };

            //Act
            var result = compiler.Compile(rawTsql, "stdServer", "stdDb", "xUnit", compilerOptions);

            //Assert
            //Assert.Equal()
        }
    }
}
