using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Codeworx.DataContract.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CW9001MissingOrderNumberAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CW9001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.CW9001AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.CW9001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.CW9001AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Categories.AttributeSyntax, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;

            if (propertySymbol.ContainingType.GetAttributes().Any(p => IsDataContract(p)))
            {
                if (TryGetDataMember(propertySymbol, out var dataMember))
                {
                    var node = dataMember.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax;

                    if (!(node.ArgumentList?.Arguments.Any(p => p.NameEquals.Name.Identifier.ValueText == Constants.OrderPropertyName) ?? false))
                    {
                        var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), propertySymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool TryGetDataMember(IPropertySymbol propertySymbol, out AttributeData dataMember)
        {
            dataMember = propertySymbol.GetAttributes().FirstOrDefault(p => IsDataMember(p));

            return dataMember != null;
        }

        private static bool IsDataMember(AttributeData attribute)
        {
            return attribute.AttributeClass.Name == Constants.DataMemberAttributeName;
        }

        private static bool IsDataContract(AttributeData attribute)
        {
            return attribute.AttributeClass.Name == Constants.DataContractAttributeName;
        }
    }
}
