namespace Testing.Rfc6570
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Net;

    [TestClass]
    public class Level1Tests
    {
        private static readonly IDictionary<string, object> variables =
            new Dictionary<string, object>
            {
                { "var", "value" },
                { "hello", "Hello World!" },
            };

        [TestMethod]
        [TestCategory(TestCategories.Level1)]
        public void TestSimpleExpansion()
        {
            string template = "{var}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("value", uri.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategories.Level1)]
        public void TestSimpleExpansionEscaping()
        {
            string template = "{hello}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("Hello%20World%21", uri.ToString());
        }
    }
}
