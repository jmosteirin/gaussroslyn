using GAUSS.Roslyn.Common.Schema;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAUSS.Roslyn.Common
{
    public class GenericUpdater<T> : IUpdater
        where T : CSharpSyntaxRewriter
    {
        private T rewriter = null;

        public GenericUpdater(T paramRewriter)
        {
            rewriter = paramRewriter;
        }

        public string Update(string paramText)
        {
            var tree = CSharpSyntaxTree.ParseText(paramText);
            var root = tree.GetRoot();
            return rewriter.Visit(root).ToString();
        }
    }
}
