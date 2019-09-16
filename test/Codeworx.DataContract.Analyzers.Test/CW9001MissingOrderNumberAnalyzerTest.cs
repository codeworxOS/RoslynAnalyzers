using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Codeworx.DataContract.Analyzers;

namespace Tests
{
    [TestClass]
    public class CW9001MissingOrderNumberAnalyzerTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestEmptyCodeFile()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMissingDataMemberOrder()
        {
            var test = @"using System.Runtime.Serialization;

namespace Test
{
    [DataContract]
    public class SampleDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public string FirstName { get; set; }

        [DataMember(Order = 3)]
        public string LastName { get; set; }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = CW9001MissingOrderNumberAnalyzer.DiagnosticId,
                Message = "Property 'Id' should have an order number on the DataMember attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 10)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Runtime.Serialization;

namespace Test
{
    [DataContract]
    public class SampleDto
    {
        [DataMember(Order = 4)]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public string FirstName { get; set; }

        [DataMember(Order = 3)]
        public string LastName { get; set; }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }


        [TestMethod]
        public void TestMissingDataMemberOrderWithFullNameAttributes()
        {
            var test = @"using System.Runtime.Serialization;

namespace Test
{
    [DataContractAttribute]
    public class SampleDto
    {
        [DataMemberAttribute]
        public int Id { get; set; }

        [DataMemberAttribute(Order = 1)]
        public string FirstName { get; set; }

        [DataMemberAttribute(Order = 3)]
        public string LastName { get; set; }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = CW9001MissingOrderNumberAnalyzer.DiagnosticId,
                Message = "Property 'Id' should have an order number on the DataMember attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 10)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"using System.Runtime.Serialization;

namespace Test
{
    [DataContractAttribute]
    public class SampleDto
    {
        [DataMemberAttribute(Order = 4)]
        public int Id { get; set; }

        [DataMemberAttribute(Order = 1)]
        public string FirstName { get; set; }

        [DataMemberAttribute(Order = 3)]
        public string LastName { get; set; }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestMissingDataMemberOrderWithNamespaceNameAttributes()
        {
            var test = @"namespace Test
{
    [System.Runtime.Serialization.DataContractAttribute]
    public class SampleDto
    {
        [System.Runtime.Serialization.DataMemberAttribute]
        public int Id { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute(Order = 1)]
        public string FirstName { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute(Order = 3)]
        public string LastName { get; set; }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = CW9001MissingOrderNumberAnalyzer.DiagnosticId,
                Message = "Property 'Id' should have an order number on the DataMember attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 10)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"namespace Test
{
    [System.Runtime.Serialization.DataContractAttribute]
    public class SampleDto
    {
        [System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
        public int Id { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute(Order = 1)]
        public string FirstName { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute(Order = 3)]
        public string LastName { get; set; }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }


        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CW9001MissingOrderNumberCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CW9001MissingOrderNumberAnalyzer();
        }
    }
}
