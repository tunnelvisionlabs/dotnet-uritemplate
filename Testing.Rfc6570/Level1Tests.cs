// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Testing.Rfc6570
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TunnelVisionLabs.Net;

    [TestClass]
    public class Level1Tests
    {
        private static readonly IDictionary<string, object> Variables =
            new Dictionary<string, object>
            {
                { "var", "value" },
                { "hello", "Hello World!" },
            };

        private static readonly HashSet<string> RequiredVariables =
            new HashSet<string>
            {
                "var",
                "hello",
            };

        [TestMethod]
        [TestCategory(TestCategories.Level1)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestEmptyTemplate()
        {
            string template = string.Empty;
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(Variables);
            Assert.AreEqual(string.Empty, uri.OriginalString);

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.Bindings.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level1)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansion()
        {
            string template = "{var}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(Variables);
            Assert.AreEqual("value", uri.OriginalString);

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["var"], match.Bindings["var"].Value);

            match = uriTemplate.Match(uri, RequiredVariables);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["var"], match.Bindings["var"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level1)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansionEscaping()
        {
            string template = "{hello}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(Variables);
            Assert.AreEqual("Hello%20World%21", uri.OriginalString);

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["hello"], match.Bindings["hello"].Value);

            match = uriTemplate.Match(uri, RequiredVariables);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["hello"], match.Bindings["hello"].Value);
        }
    }
}
