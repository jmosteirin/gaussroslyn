using GAUSS.Roslyn.Common.Schema;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GAUSS.Roslyn.Common
{
    public class ProtectMethodsRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel semanticModel = null;
        public ProtectMethodsRewriter(SemanticModel paramSemanticModel)
        {
            semanticModel = paramSemanticModel;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax paramNode)
        {
            MethodDeclarationSyntax returned = paramNode;

            var ifStatements = returned.Body.ChildNodes().TakeWhile(i => i is IfStatementSyntax).OfType<IfStatementSyntax>().ToArray();

            if (returned.ParameterList.ChildNodes().Count() != 0)
            {
                var parameters = returned.ParameterList.ChildNodes().OfType<ParameterSyntax>().ToList();
                foreach (var parameter in parameters.Where(p => (p.Type is IdentifierNameSyntax) || 
                    ((p.Type is PredefinedTypeSyntax) && ( (p.Type.ToString().ToLower() == @"string") || 
                                                           (p.Type.ToString().ToLower() == @"object") ))))
                {
                    string parameterName = parameter.GetLastToken().Text;
                    var alreadyProtected = ifStatements.Any(i => i.Condition.DescendantTokens().Any(t => t.Text == parameterName)
                                                                 && i.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Any(n => 
                                                                    (n.Type.GetFirstToken().Text == @"ArgumentException") || 
                                                                    (n.Type.GetFirstToken().Text == @"ArgumentNullException")));
                    if (!alreadyProtected)
                    {
                        IfStatementSyntax toBeAdded = parameter.GetProtectionForParameter(semanticModel);

                        if (toBeAdded != null)
                        {
                            if (returned.Body.ChildNodes().Any())
                                returned = returned.InsertNodesBefore(returned.Body.ChildNodes().First(), new SyntaxNode[] { toBeAdded });
                            else
                                returned = returned.AddBodyStatements(toBeAdded);
                        }
                    }
                }
            }



            return base.VisitMethodDeclaration(returned);
        }
    }
}
