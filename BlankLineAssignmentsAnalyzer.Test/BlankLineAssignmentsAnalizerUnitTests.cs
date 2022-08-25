using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = BlankLineAssignmentsAnalyzer.Test.CSharpCodeFixVerifier<
    BlankLineAssignmentsAnalyzer.BlankLineAssignmentsAnalyzer,
    BlankLineAssignmentsAnalyzer.BlankLineAssignmentsAnalyzerCodeFixProvider>;

namespace BlankLineAssignmentsAnalyzer.Test
{
    [TestClass]
    public class BlankLineAssignmentsAnalyzerUnitTest
    {
        private const string TestCodeAfterBlock = @"
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

                for (var i = 0; i < 10; ++i)
                {
                    {|#1:var temp = i;|}
                    temp++;
                }

                try
                {
                    var k1 = 1;
                    {|#2:k1 = 1+2;|}
                    Console.WriteLine(5);
                }
                finally
                {
                    var k2 = 1;
                    {|#3:k2 = 1+2;|}
                    Console.WriteLine(5);
                }
            }
        }
    }";

        private const string TestCodeBeforeBlock = @"
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
                {|#0:Console.WriteLine(5);|}
                var k = 1;

                for (var i = 0; i < 10; ++i)
                {
                    {|#1:++i;|}
                    var temp = i;
                }

                try
                {
                    {|#2:Console.WriteLine(5);|}
                    var k1 = 1;
                }
                finally
                {
                    {|#3:Console.WriteLine(5);|}
                    var k2 = 1;
                }
            }
        }
    }";

        [TestMethod]
        public async Task TestAnalizerBlankAfterBlock()
        {
            var expectedList = new[]
            {
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(0),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(1),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(2),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(3),
            };

            await VerifyCS.VerifyAnalyzerAsync(TestCodeAfterBlock, expectedList);
        }

        [TestMethod]
        public async Task TestCodeFixBlankAfterBlock()
        {
            const string fixtest = @"
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

                for (var i = 0; i < 10; ++i)
                {
                    var temp = i;

                    temp++;
                }

                try
                {
                    var k1 = 1;
                    k1 = 1+2;

                    Console.WriteLine(5);
                }
                finally
                {
                    var k2 = 1;
                    k2 = 1+2;

                    Console.WriteLine(5);
                }
            }
        }
    }";

            var expectedList = new[]
            {
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(0),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(1),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(2),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleAfter).WithLocation(3),
            };

            await VerifyCS.VerifyCodeFixAsync(TestCodeAfterBlock, expectedList, fixtest);
        }

        [TestMethod]
        public async Task TestAnalizerBlankBeforeBlock()
        {
            var expectedList = new[]
            {
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(0),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(1),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(2),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(3),
            };

            await VerifyCS.VerifyAnalyzerAsync(TestCodeBeforeBlock, expectedList);
        }

        [TestMethod]
        public async Task TestCodeFixBlankBeforeBlock()
        {
            const string fixtest = @"
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
                Console.WriteLine(5);

                var k = 1;

                for (var i = 0; i < 10; ++i)
                {
                    ++i;

                    var temp = i;
                }

                try
                {
                    Console.WriteLine(5);

                    var k1 = 1;
                }
                finally
                {
                    Console.WriteLine(5);

                    var k2 = 1;
                }
            }
        }
    }";

            var expectedList = new[]
            {
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(0),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(1),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(2),
                VerifyCS.Diagnostic(BlankLineAssignmentsAnalyzer.AssignmentsRuleBefore).WithLocation(3),
            };

            await VerifyCS.VerifyCodeFixAsync(TestCodeBeforeBlock, expectedList, fixtest);
        }
    }
}
