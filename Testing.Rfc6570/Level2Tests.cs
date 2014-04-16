namespace Testing.Rfc6570
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Net;

    [TestClass]
    public class Level2Tests
    {
        private static readonly IDictionary<string, object> variables =
            new Dictionary<string, object>
            {
                { "var", "value" },
                { "hello", "Hello World!" },
                { "path", "/foo/bar" },
            };

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansion()
        {
            string template = "{+var}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("value", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["var"], match.Bindings["var"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionEscaping()
        {
            string template = "{+hello}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("Hello%20World!", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["hello"], match.Bindings["hello"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionReservedCharacters()
        {
            string template = "{+path}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/foo/bar/here", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["path"], match.Bindings["path"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionReservedCharactersInQuery()
        {
            string template = "here?ref={+path}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("here?ref=/foo/bar", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["path"], match.Bindings["path"].Value);
        }
    }
}
