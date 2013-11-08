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
        public void TestReservedExpansion()
        {
            string template = "{+var}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("value", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        public void TestReservedExpansionEscaping()
        {
            string template = "{+hello}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("Hello%20World!", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        public void TestReservedExpansionReservedCharacters()
        {
            string template = "{+path}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("/foo/bar/here", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        public void TestReservedExpansionReservedCharactersInQuery()
        {
            string template = "here?ref={+path}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("here?ref=/foo/bar", uri.ToString());
        }
    }
}
