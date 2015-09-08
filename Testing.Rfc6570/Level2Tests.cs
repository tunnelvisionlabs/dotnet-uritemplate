// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Testing.Rfc6570
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TunnelVisionLabs.Net;

    [TestClass]
    public class Level2Tests
    {
        private static readonly IDictionary<string, object> Variables =
            new Dictionary<string, object>
            {
                { "var", "value" },
                { "hello", "Hello World!" },
                { "path", "/foo/bar" },
            };

        private static readonly HashSet<string> RequiredVariables =
            new HashSet<string>
            {
                "var",
                "hello",
                "path",
            };

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansion()
        {
            string template = "{+var}";
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
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionEscaping()
        {
            string template = "{+hello}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(Variables);
            Assert.AreEqual("Hello%20World!", uri.OriginalString);

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["hello"], match.Bindings["hello"].Value);

            match = uriTemplate.Match(uri, RequiredVariables);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["hello"], match.Bindings["hello"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionReservedCharacters()
        {
            string template = "{+path}/here";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(Variables);
            Assert.AreEqual("/foo/bar/here", uri.OriginalString);

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["path"], match.Bindings["path"].Value);

            match = uriTemplate.Match(uri, RequiredVariables);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["path"], match.Bindings["path"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Level2)]
        [TestCategory(TestCategories.ReservedExpansion)]
        public void TestReservedExpansionReservedCharactersInQuery()
        {
            string template = "here?ref={+path}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(Variables);
            Assert.AreEqual("here?ref=/foo/bar", uri.OriginalString);

            UriTemplateMatch match = uriTemplate.Match(uri);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["path"], match.Bindings["path"].Value);

            match = uriTemplate.Match(uri, RequiredVariables);
            Assert.IsNotNull(match);
            Assert.AreEqual(Variables["path"], match.Bindings["path"].Value);
        }
    }
}
