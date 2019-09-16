using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeworx.DataContract.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codeworx.DataContract
{
    public static class PropertyDeclarationSyntaxExtensions
    {
        private static readonly string[] _dataMemberNames = new[] { Constants.DataMemberName, Constants.DataMemberAttributeName };

        public static int GetMaxOrderNumber(this PropertyDeclarationSyntax property)
        {
            var orderNumber = 0;

            var type = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            var properties = type.Members.OfType<PropertyDeclarationSyntax>();

            foreach (var item in properties)
            {
                foreach (var attribute in item.AttributeLists.SelectMany(p => p.Attributes))
                {
                    if (_dataMemberNames.Contains(attribute.Name.GetIdentifierName()) && attribute.ArgumentList != null)
                    {
                        foreach (var argument in attribute.ArgumentList.Arguments)
                        {
                            if (argument.NameEquals.Name.Identifier.Text == Constants.OrderPropertyName)
                            {
                                var currentNumber = (int)((LiteralExpressionSyntax)argument.Expression).Token.Value;

                                if (currentNumber > orderNumber)
                                {
                                    orderNumber = currentNumber;
                                }
                            }
                        }
                    }
                }
            }

            return orderNumber;
        }

        public static string GetIdentifierName(this NameSyntax name)
        {
            if (name is SimpleNameSyntax simpleName)
            {
                return simpleName.Identifier.Text;
            }
            else if (name is QualifiedNameSyntax qualified)
            {
                return qualified.Right.Identifier.Text;
            }

            return name.ToString();
        }
    }
}
