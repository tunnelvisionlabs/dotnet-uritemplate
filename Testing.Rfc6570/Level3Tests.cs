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
        public void TestSimpleExpansionMultpleVariablesInQuery()
        {
            string template = "map?{x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("map?1024,768", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestSimpleExpansionMultpleVariables()
        {
            string template = "{x,hello,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("1024,Hello%20World%21,768", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestReservedExpansionMultpleVariables()
        {
            string template = "{+x,hello,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("1024,Hello%20World!,768", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestReservedExpansionMultpleVariablesWithSlash()
        {
            string template = "{+path,x}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/foo/bar,1024/here", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestFragmentExpansionMultpleVariables()
        {
            string template = "{#x,hello,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("#1024,Hello%20World!,768", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestFragmentExpansionMultpleVariablesAndLiteral()
        {
            string template = "{#path,x}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("#/foo/bar,1024/here", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestLabelExpansionMultpleVariables()
        {
            string template = "X{.x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("X.1024.768", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestPathSegmentExpansion()
        {
            string template = "{/var}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/value", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestPathSegmentExpansionMultipleVariables()
        {
            string template = "{/var,x}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/value/1024/here", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestPathParameterExpansionMultipleVariables()
        {
            string template = "{;x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual(";x=1024;y=768", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestPathParameterExpansionEmptyValue()
        {
            string template = "{;x,y,empty}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual(";x=1024;y=768;empty", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestQueryExpansionMultipleVariables()
        {
            string template = "{?x,y}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("?x=1024&y=768", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestQueryExpansionEmptyValue()
        {
            string template = "{?x,y,empty}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("?x=1024&y=768&empty=", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestQueryContinuationExpansion()
        {
            string template = "?fixed=yes{&x}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("?fixed=yes&x=1024", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level3)]
        public void TestQueryContinuationExpansionMultipleVariables()
        {
            string template = "{&x,y,empty}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("&x=1024&y=768&empty=", uri.ToString());
        }
    }
}
