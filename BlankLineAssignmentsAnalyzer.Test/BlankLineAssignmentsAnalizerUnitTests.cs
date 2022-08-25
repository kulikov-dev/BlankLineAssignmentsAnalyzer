using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using VerifyCS = BlankLineAssignmentsAnalyzer.Test.CSharpCodeFixVerifier<
    BlankLineAssignmentsAnalyzer.BlankLineAssignmentsAnalyzer,
    BlankLineAssignmentsAnalyzer.BlankLineAssignmentsAnalyzerCodeFixProvider>;

namespace BlankLineAssignmentsAnalyzer.Test
{
    [TestClass]
    public class BlankLineAssignmentsAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TestClass
        {   
            private void Test()
            {
                var k = 1;
                {|#0:k = 1+2;|}
                Console.WriteLine(5);
            }
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TestClass
        {   
            private void Test()
            {
                var k = 1;
                k = 1+2;

                Console.WriteLine(5);
            }
        }
    }";

            var expected = VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(0);
                await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);

           // await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
