using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;

namespace GAUSS.Roslyn.Common.Test
{
    [TestClass]
    public class ProtectMethodsRewriterTest
    {
        public ProtectMethodsRewriterTest()
        {
        }

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void Test()
        {
            Workspace cw = new AdhocWorkspace();
            var tree = CSharpSyntaxTree.ParseText(@"public class foo
                                                    {
                                                        public int Method1(string param1, int param2, foo param3)
                                                        {
                                                            if (String.IsNullOrWhiteSpace(param1))
                                                                throw new ArgumentException(@""param1"");

                                                            if (param3 == null)
                                                            throw new ArgumentNullException(@""param3"");

                                                            return 1;
                                                        }
                                                        public void Method2(string param1, int param2, object param3)
                                                        {
                                                        }
                                                        public string Method3(string param1, int param2, object param3)
                                                        {
                                                            if (String.IsNullOrWhiteSpace(param1))
                                                                throw new ArgumentException(@""param1"");

                                                            return String.Empty;
                                                        }
                                                        public void Method4()
                                                        {
                                                        }
                                                    }");
            var root = tree.GetRoot();

            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);

            var newRoot = (new ProtectMethodsRewriter(model)).Visit(root);
            OptionSet options = cw.Options;
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, false);
            SyntaxNode formattedNode = Formatter.Format(newRoot, cw, options);
            var returned = formattedNode.ToString();
        }
    }
}
