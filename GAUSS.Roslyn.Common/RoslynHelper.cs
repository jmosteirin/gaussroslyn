using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GAUSS.Roslyn.Common
{
    public static class RoslynHelper
    {
        public static IfStatementSyntax GetProtectionForParameter(this ParameterSyntax paramParameterSyntax, SemanticModel paramSemanticModel)
        {
            var parameterSymbolInfo = paramSemanticModel.GetDeclaredSymbol(paramParameterSyntax);

            string typeName = String.Empty;
                        if (paramParameterSyntax.Type is IdentifierNameSyntax)
                            typeName = ((IdentifierNameSyntax)(paramParameterSyntax.Type)).Identifier.Text;
                        else if (paramParameterSyntax.Type is PredefinedTypeSyntax)
                            typeName = paramParameterSyntax.Type.ToString().ToLower();

            string parameterName = paramParameterSyntax.GetLastToken().Text;

            IfStatementSyntax returned = null;

            if(typeName.ToLower() == @"string")
            {
                returned =
                    SF.IfStatement(
                        SyntaxFactory.InvocationExpression(
                           SyntaxFactory.MemberAccessExpression(
                               SyntaxKind.SimpleMemberAccessExpression,
                               SyntaxFactory.IdentifierName(
                                   @"String"),
                               SyntaxFactory.IdentifierName(
                                   @"IsNullOrWhiteSpace"))
                           .WithOperatorToken(
                               SyntaxFactory.Token(
                                   SyntaxKind.DotToken))).
                            WithArgumentList(SF.ArgumentList(SF.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] { SF.Argument(SF.IdentifierName(parameterName)) })))
                        ,
                        SF.Block(
                            SF.ThrowStatement(SF.ObjectCreationExpression(SF.IdentifierName(@"ArgumentException")).
                            WithArgumentList(SF.ArgumentList(SF.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] { SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression).WithToken(SF.Literal(parameterName))) }))))
                        )
                    );
            }
            else if (((INamedTypeSymbol)(parameterSymbolInfo.Type)).IsReferenceType)
            {
                returned =
                    SF.IfStatement(
                        SF.BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            SF.IdentifierName(parameterName),
                            SF.LiteralExpression(SyntaxKind.NullLiteralExpression)
                        ),
                        SF.Block(
                            SF.ThrowStatement(SF.ObjectCreationExpression(SF.IdentifierName(@"ArgumentNullException")).
                            WithArgumentList(SF.ArgumentList(SF.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] { SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression).WithToken(SF.Literal(parameterName))) }))))
                        )
                    );
            }

            return returned;
        }
    }
}
