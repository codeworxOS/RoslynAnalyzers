using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace DataMember
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(DataMemberCodeRefactoringProvider)), Shared]
    internal class DataMemberCodeRefactoringProvider : CodeRefactoringProvider
    {
        private const string DataMember = "DataMember";
        private const string IgnoreDataMember = "IgnoreDataMember";
        private const string DataContract = "DataContract";
        private const string Order = "Order";
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a DataContract Attribute.
            var attributeSyntax = node ;
         
            if (attributeSyntax != null && attributeSyntax.GetText().ToString().Contains(DataContract))
            {
                var properties = root.ChildNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault().ChildNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault().ChildNodes().OfType<PropertyDeclarationSyntax>();//.FirstOrDefault().ChildNodes().FirstOrDef
                var publicProperties = properties.Where(p => p.Modifiers.Any(m => m.Text.Contains("public"))  && GetIgnoreDataMemberAttribute(p) == null).ToArray();

                var propertiesWithDataMemberAttribute = publicProperties.Where(pp => GetDataMemberAttribute(pp) != null).ToArray();


                var propertiesWithDataMemberAttributeWithoutOrder = propertiesWithDataMemberAttribute.Where(p => GetOrder(GetDataMemberAttribute(p)) == null).ToArray();

                var propertiesWithDataMemberAttributeWithOrder = propertiesWithDataMemberAttribute.Except(propertiesWithDataMemberAttributeWithoutOrder).ToArray();
               
                //Not All public properties have a "DataMember" Attribute with Order
                if (publicProperties.Count() != propertiesWithDataMemberAttributeWithOrder.Count())
                {                  

                    var action = CodeAction.Create("Add DataMember Attribute with Order", c => AddDateMemberAttributesAndFixOrder(context.Document, publicProperties.Where(p => p.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration)).ToArray(), propertiesWithDataMemberAttributeWithoutOrder, propertiesWithDataMemberAttributeWithOrder, c));
                    context.RegisterRefactoring(action);
                }
            }
            
        }

        private static AttributeSyntax GetDataMemberAttribute(PropertyDeclarationSyntax property)
        {
            return property.ChildNodes().OfType<AttributeListSyntax>().SelectMany(al=>al.Attributes).FirstOrDefault(a => a.Name.ToString() == DataMember);
        }

        private static AttributeSyntax GetIgnoreDataMemberAttribute(PropertyDeclarationSyntax property)
        {
            return property.ChildNodes().OfType<AttributeListSyntax>().SelectMany(al => al.Attributes).FirstOrDefault(a => a.Name.ToString() == IgnoreDataMember);
        }

        private static int? GetOrder(AttributeSyntax dataMemberAttribute)
        {
            if (dataMemberAttribute == null)
                return null;

           
            var ordernumber = dataMemberAttribute.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals.Name.Identifier.ToString().StartsWith(Order))?.
                Expression.ToString();

            if (ordernumber != null)
                return int.Parse(ordernumber);
            return null;
        }

        private async Task<Document> AddDateMemberAttributesAndFixOrder(Document document, PropertyDeclarationSyntax[] publicProperties, PropertyDeclarationSyntax[] propertiesWithDataMemberAttributeWithoutOrder, PropertyDeclarationSyntax[] propertiesWithDataMemberAttributeWithOrder, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            int max = 0;

            if (propertiesWithDataMemberAttributeWithOrder.Any())
            {
                max = propertiesWithDataMemberAttributeWithOrder.Max(p => GetOrder(GetDataMemberAttribute(p)) ?? 0);
            }

            var updateNodes = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach (var property in propertiesWithDataMemberAttributeWithoutOrder)
            {

                var attribute = GetDataMemberAttribute(property); //DataMember
                AttributeArgumentListSyntax arguments = CreateOrderArgument(++max);
                var args = attribute.ArgumentList?.Arguments.ToArray();
                AttributeArgumentListSyntax allArgs = arguments;
                if (args != null)
                    allArgs = arguments.AddArguments(args);
                var newAttribute = attribute.WithArgumentList(allArgs); //DataMember(Order = #)
                updateNodes.Add(attribute, newAttribute);
            }
            foreach (var property in publicProperties.Except(propertiesWithDataMemberAttributeWithoutOrder.Union(propertiesWithDataMemberAttributeWithOrder)))
            {

                var name = SyntaxFactory.ParseName(DataMember);
                AttributeArgumentListSyntax arguments = CreateOrderArgument(++max);
                var attribute = SyntaxFactory.Attribute(name, arguments); //DataMember(Order = #)

                var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
                attributeList = attributeList.Add(attribute);
                var list = SyntaxFactory.AttributeList(attributeList);   //[DataMember(Order = #)]
               var modifier = property.Modifiers.FirstOrDefault();

                if (modifier != null)
                {
                    var commentsT = modifier.LeadingTrivia;

                    list = list.WithLeadingTrivia(commentsT);
                }
               

                var newModifiers = SyntaxFactory.TokenList(property.Modifiers.Skip(1).Concat(new[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword) })); ;
                var newProperty = property.AddAttributeLists( list ).WithModifiers(newModifiers);

                updateNodes.Add(property, newProperty);
               
            }
            root = root.ReplaceNodes(updateNodes.Keys, (n1, n2) =>
            {
                return updateNodes[n1];
            });
            return document.WithSyntaxRoot(root);

        }

        private static AttributeArgumentListSyntax CreateOrderArgument( int max)
        {
            return SyntaxFactory.ParseAttributeArgumentList($"({Order} = {max})");
        }
    }
}