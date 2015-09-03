namespace Testing.Rfc6570
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Net;

    [TestClass]
    public class NegativeTests
    {
        public readonly Dictionary<string, object> variables =
            new Dictionary<string, object>
            {
                { "id"                , "thing" },
                { "var"               , "value" },
                { "hello"             , "Hello World!" },
                { "with space"        , "fail" },
                { " leading_space"    , "Hi!" },
                { "trailing_space "   , "Bye!" },
                { "empty"             , string.Empty },
                { "path"              , "/foo/bar" },
                { "x"                 , "1024" },
                { "y"                 , "768" },
                { "list"              , new[] { "red", "green", "blue" } },
                { "keys"              , new Dictionary<string, object> { { "semi", ";" }, { "dot", "." }, { "comma", "," } } },
                { "example"           , "red" },
                { "searchTerms"       , "uri templates" },
                { "~thing"            , "some-user" },
                { "default-graph-uri" , new[] { "http://www.example/book/", "http://www.example/papers/" } },
                { "query"             , "PREFIX dc: <http://purl.org/dc/elements/1.1/> SELECT ?book ?who WHERE { ?book dc:creator ?who }" }
            };

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestUnclosedTemplate()
        {
            string template = "{/id*";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestUnopenedTemplate()
        {
            string template = "/id*}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestTwoOperatorsTemplate()
        {
            string template = "{/?id}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestNonIntegralPrefixTemplate()
        {
            string template = "{var:prefix}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestCompositePrefixTemplate()
        {
            string template = "{hello:2*}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestDuplicateOperatorTemplate()
        {
            string template = "{??hello}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestSpaceInExpressionTemplate()
        {
            string template = "{with space}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestSpaceAtStartOfExpressionTemplate()
        {
            string template = "{ leading_space}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestSpaceAtEndOfExpressionTemplate()
        {
            string template = "{trailing_space }";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestEqualsOperatorTemplate()
        {
            string template = "{=path}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestDollarOperatorTemplate()
        {
            string template = "{$var}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestPipeOperatorTemplate()
        {
            string template = "{|var*}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestReverseOperatorTemplate()
        {
            string template = "{*keys?}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidQueryTemplate()
        {
            string template = "{?empty=default,var}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidAlternativesTemplate()
        {
            string template = "{var}{-prefix|/-/|var}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidPrefixTemplate()
        {
            string template = "?q={searchTerms}&amp;c={example:color?}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestAlternativesQueryTemplate()
        {
            string template = "x{?empty|foo=none}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestCompoundFragmentTemplate()
        {
            string template = "/h{#hello+}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidFragmentTemplate()
        {
            string template = "/h#{hello+}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestPrefixAssociativeMapTemplate()
        {
            string template = "{keys:1}";
            UriTemplate uriTemplate = new UriTemplate(template);
            uriTemplate.BindByName(variables);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestPlusOperatorAssociativeMapTemplate()
        {
            string template = "{+keys:1}";
            UriTemplate uriTemplate = new UriTemplate(template);
            uriTemplate.BindByName(variables);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestCompountPathParameterPrefixAssociativeMapTemplate()
        {
            string template = "{;keys:1*}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidPipeOperatorTemplate()
        {
            string template = "?{-join|&|var,list}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidTildeOperatorTemplate()
        {
            string template = "/people/{~thing}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestHypenatedVariableNameTemplate()
        {
            string template = "/{default-graph-uri}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestHypenatedVariableName2Template()
        {
            string template = "/sparql{?query,default-graph-uri}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestMismatchedBracesTemplate()
        {
            string template = "/sparql{?query){&default-graph-uri*}";
            new UriTemplate(template);
        }

        [TestMethod]
        [TestCategory(TestCategories.InvalidTemplates)]
        [ExpectedException(typeof(FormatException))]
        public void TestSpaceAfterCommaTemplate()
        {
            string template = "/resolution{?x, y}";
            new UriTemplate(template);
        }
    }
}
