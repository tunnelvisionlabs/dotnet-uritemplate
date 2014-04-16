namespace Testing.Rfc6570
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Net;

    [TestClass]
    public class Level3Tests
    {
        private static readonly IDictionary<string, object> variables =
            new Dictionary<string, object>
            {
                { "var", "value" },
                { "hello", "Hello World!" },
                { "empty", "" },
                { "path", "/foo/bar" },
                { "x", "1024" },
                { "y", "768" },
            };

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansionMultipleVariablesInQuery()
        {
            string template = "map?{x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("map?1024,768", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
            Assert.AreEqual(variables["y"], match.Bindings["y"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansionMultipleVariables()
        {
            string template = "{x,hello,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("1024,Hello%20World%21,768", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
            Assert.AreEqual(variables["hello"], match.Bindings["hello"].Value);
            Assert.AreEqual(variables["y"], match.Bindings["y"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionMultipleVariables()
        {
            string template = "{+x,hello,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("1024,Hello%20World!,768", uri.ToString());

            try
            {
                // cannot call match on a uri with multiple reserved variables
                uriTemplate.Match(uri);
                Assert.Fail("Expected a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionMultipleVariablesWithSlash()
        {
            string template = "{+path,x}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/foo/bar,1024/here", uri.ToString());

            try
            {
                // cannot call match on a uri with multiple reserved variables
                uriTemplate.Match(uri);
                Assert.Fail("Expected a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.FragmentExpansion)]
        public void TestFragmentExpansionMultipleVariables()
        {
            string template = "{#x,hello,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("#1024,Hello%20World!,768", uri.ToString());

            try
            {
                // cannot call match on a uri with multiple fragment variables
                uriTemplate.Match(uri);
                Assert.Fail("Expected a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.FragmentExpansion)]
        public void TestFragmentExpansionMultipleVariablesAndLiteral()
        {
            string template = "{#path,x}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("#/foo/bar,1024/here", uri.ToString());

            try
            {
                // cannot call match on a uri with multiple fragment variables
                uriTemplate.Match(uri);
                Assert.Fail("Expected a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.LabelExpansion)]
        public void TestLabelExpansionMultipleVariables()
        {
            string template = "X{.x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("X.1024.768", uri.ToString());

            try
            {
                // cannot call match on a uri with multiple label variables
                uriTemplate.Match(uri);
                Assert.Fail("Expected a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        public void TestPathSegmentExpansion()
        {
            string template = "{/var}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/value", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["var"], match.Bindings["var"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        public void TestPathSegmentExpansionMultipleVariables()
        {
            string template = "{/var,x}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/value/1024/here", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["var"], match.Bindings["var"].Value);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.PathParameterExpansion)]
        public void TestPathParameterExpansionMultipleVariables()
        {
            string template = "{;x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual(";x=1024;y=768", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
            Assert.AreEqual(variables["y"], match.Bindings["y"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.PathParameterExpansion)]
        public void TestPathParameterExpansionEmptyValue()
        {
            string template = "{;x,y,empty}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual(";x=1024;y=768;empty", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
            Assert.AreEqual(variables["y"], match.Bindings["y"].Value);
            Assert.AreEqual(variables["empty"], match.Bindings["empty"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestQueryExpansionMultipleVariables()
        {
            string template = "{?x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("?x=1024&y=768", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
            Assert.AreEqual(variables["y"], match.Bindings["y"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestQueryExpansionEmptyValue()
        {
            string template = "{?x,y,empty}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("?x=1024&y=768&empty=", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
            Assert.AreEqual(variables["y"], match.Bindings["y"].Value);
            Assert.AreEqual(variables["empty"], match.Bindings["empty"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.QueryContinuationExpansion)]
        public void TestQueryContinuationExpansion()
        {
            string template = "?fixed=yes{&x}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("?fixed=yes&x=1024", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        [TestCategory(TestCategories.QueryContinuationExpansion)]
        public void TestQueryContinuationExpansionMultipleVariables()
        {
            string template = "{&x,y,empty}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("&x=1024&y=768&empty=", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["x"], match.Bindings["x"].Value);
            Assert.AreEqual(variables["y"], match.Bindings["y"].Value);
            Assert.AreEqual(variables["empty"], match.Bindings["empty"].Value);
        }
    }
}
