using GAUSS.Roslyn.Common.Schema;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GAUSS.Roslyn.Common
{
    public class EntityRewriter : CSharpSyntaxRewriter
    {
        public ApplicationElement applicationElement { get; set; }
        public EntityRewriter(ApplicationElement paramApplicationElement)
        {
            applicationElement = paramApplicationElement;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax paramNode)
        {
            ClassDeclarationSyntax returned = paramNode;
            var entityElement = GetEntityElement(returned.Identifier.Text);

            if (entityElement != null)
            {
                var properties = entityElement.Properties.Select(p => SyntaxFactory.PropertyDeclaration(
                                                SyntaxFactory.ParseTypeName(p.Type), 
                                                SyntaxFactory.Identifier(p.Name))
                                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                                .WithAccessorList(
                                                SyntaxFactory.AccessorList(
                                                    SyntaxFactory.List<AccessorDeclarationSyntax>(
                                                        new AccessorDeclarationSyntax[]{
                                                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                                        }))));
                if (returned.ChildNodes().Any())
                {
                    //First we add new properties and after (and only after) we remove the properties that are no more needed
                    foreach (var p in properties)
                    {
                        if(!returned.ChildNodes().Any(n => n.IsKind(SyntaxKind.PropertyDeclaration) && (((PropertyDeclarationSyntax)n).Identifier.Text == p.Identifier.Text)))
                        { 
                            var first = returned.ChildNodes().First();
                            returned = returned.InsertNodesBefore(first, new PropertyDeclarationSyntax[] { p });
                        }
                    }
                    foreach (var propertyName in returned.ChildNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).Select(n1 => ((PropertyDeclarationSyntax)n1).Identifier.Text).ToList())
                    {
                        if (!properties.Any(n => n.IsKind(SyntaxKind.PropertyDeclaration) && (n.Identifier.Text == propertyName)))
                        {
                            var nodeToBeRemoved = returned.ChildNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == propertyName);
                            if(nodeToBeRemoved != null)
                                returned = returned.RemoveNode(nodeToBeRemoved, SyntaxRemoveOptions.KeepNoTrivia);
                        }
                    }
                }
                else
                {
                    returned = returned.AddMembers(properties.Select(p => (MemberDeclarationSyntax)p).ToArray());
                }
            }

            return base.VisitClassDeclaration(returned);
        }

        private EntityElement GetEntityElement(string paramName)
        {
            return applicationElement.Entities.FirstOrDefault(e => e.Name == paramName);
        }
    }
}
