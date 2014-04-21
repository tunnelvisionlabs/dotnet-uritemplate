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
        private static readonly HashSet<string> requiredVariables =
            new HashSet<string>
            {
                "var",
                "hello",
            };

        [TestMethod]
        [TestCategory(TestCategories.Level1)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansion()
        {
            string template = "{var}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("value", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["var"], match.Bindings["var"].Value);

            match = uriTemplate.Match(uri, requiredVariables);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["var"], match.Bindings["var"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level1)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansionEscaping()
        {
            string template = "{hello}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables);
            Assert.AreEqual("Hello%20World%21", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["hello"], match.Bindings["hello"].Value);

            match = uriTemplate.Match(uri, requiredVariables);
            Assert.IsNotNull(match);
            Assert.AreEqual(variables["hello"], match.Bindings["hello"].Value);
        }
    }
}
