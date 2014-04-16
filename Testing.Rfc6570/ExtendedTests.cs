namespace Testing.Rfc6570
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Net;
    using ICollection = System.Collections.ICollection;

    [TestClass]
    public class ExtendedTests
    {
        public static readonly Dictionary<string, object> variables1 =
            new Dictionary<string, object>
            {
                { "id"           , "person" },
                { "token"        , "12345" },
                { "fields"       , new[] { "id", "name", "picture" } },
                { "format"       , "json" },
                { "q"            , "URI Templates" },
                { "page"         , "5" },
                { "lang"         , "en" },
                { "geocode"      , new[] { "37.76","-122.427" } },
                { "first_name"   , "John" },
                { "last.name"    , "Doe" }, 
                { "Some%20Thing" , "foo" },
                { "number"       , 6 },
                { "long"         , 37.76 },
                { "lat"          , -122.427 },
                { "group_id"     , "12345" },
                { "query"        , "PREFIX dc: <http://purl.org/dc/elements/1.1/> SELECT ?book ?who WHERE { ?book dc:creator ?who }" },
                { "uri"          , "http://example.org/?uri=http%3A%2F%2Fexample.org%2F" },
                { "word"         , "drücken" },
                { "Stra%C3%9Fe"  , "Grüner Weg" },
                { "random"       , "šöäŸœñê€£¥‡ÑÒÓÔÕÖ×ØÙÚàáâãäåæçÿ" },
                { "assoc_special_chars", new Dictionary<string, string> { { "šöäŸœñê€£¥‡ÑÒÓÔÕ", "Ö×ØÙÚàáâãäåæçÿ" } } }
            };

        public static readonly Dictionary<string, object> variables2 =
            new Dictionary<string, object>
            {
                { "id" , new[] { "person","albums" } },
                { "token" , "12345" },
                { "fields" , new[] { "id", "name", "picture"} },
                { "format" , "atom" },
                { "q" , "URI Templates" },
                { "page" , "10" },
                { "start" , "5" },
                { "lang" , "en" },
                { "geocode" , new[] { "37.76","-122.427" } }
            };

        public static readonly Dictionary<string, object> variables3 =
            new Dictionary<string, object>
            {
                { "empty_list", new string[0] },
                { "empty_assoc", new Dictionary<string, string>() }
            };

        public static readonly Dictionary<string, object> variables4 =
            new Dictionary<string, object>
            {
                { "42", "The Answer to the Ultimate Question of Life, the Universe, and Everything" },
                { "1337", new[] { "leet", "as","it", "can","be" } },
                { "german", new Dictionary<string, string> {
                    { "11", "elf" },
                    { "12", "zwölf" } }
                }
            };

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        public void TestCompoundPathSegmentExpansion()
        {
            string template = "{/id*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/person", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["id"], match.Bindings["id"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestCompoundPathSegmentExpansionWithQueryString()
        {
            string template = "{/id*}{?fields,first_name,last.name,token}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            string[] allowed =
                {
                    "/person?fields=id,name,picture&first_name=John&last.name=Doe&token=12345",
                    "/person?fields=id,picture,name&first_name=John&last.name=Doe&token=12345",
                    "/person?fields=picture,name,id&first_name=John&last.name=Doe&token=12345",
                    "/person?fields=picture,id,name&first_name=John&last.name=Doe&token=12345",
                    "/person?fields=name,picture,id&first_name=John&last.name=Doe&token=12345",
                    "/person?fields=name,id,picture&first_name=John&last.name=Doe&token=12345"
                };

            CollectionAssert.Contains(allowed, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["id"], match.Bindings["id"].Value);
            CollectionAssert.AreEqual((ICollection)variables1["fields"], (ICollection)match.Bindings["fields"].Value);
            Assert.AreEqual(variables1["first_name"], match.Bindings["first_name"].Value);
            Assert.AreEqual(variables1["last.name"], match.Bindings["last.name"].Value);
            Assert.AreEqual(variables1["token"], match.Bindings["token"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.SimpleExpansion)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestSimpleExpansionWithQueryString()
        {
            string template = "/search.{format}{?q,geocode,lang,locale,page,result_type}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            string[] allowed =
                {
                    "/search.json?q=URI%20Templates&geocode=37.76,-122.427&lang=en&page=5",
                    "/search.json?q=URI%20Templates&geocode=-122.427,37.76&lang=en&page=5"
                };

            CollectionAssert.Contains(allowed, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["format"], match.Bindings["format"].Value);
            Assert.AreEqual(variables1["q"], match.Bindings["q"].Value);
            CollectionAssert.AreEqual((ICollection)variables1["geocode"], (ICollection)match.Bindings["geocode"].Value);
            Assert.AreEqual(variables1["lang"], match.Bindings["lang"].Value);
            Assert.IsFalse(match.Bindings.ContainsKey("locale"));
            Assert.AreEqual(variables1["page"], match.Bindings["page"].Value);
            Assert.IsFalse(match.Bindings.ContainsKey("result_type"));
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        public void TestPathSegmentExpansionWithEncodedCharacters()
        {
            string template = "/test{/Some%20Thing}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/test/foo", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["Some%20Thing"], match.Bindings["Some%20Thing"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestQueryExpansionWithIntegerVariable()
        {
            string template = "/set{?number}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/set?number=6", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["number"].ToString(), match.Bindings["number"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestQueryExpansionWithMultipleDoubleVariable()
        {
            string template = "/loc{?long,lat}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/loc?long=37.76&lat=-122.427", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["long"].ToString(), match.Bindings["long"].Value);
            Assert.AreEqual(variables1["lat"].ToString(), match.Bindings["lat"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestEscapeSequences1()
        {
            string template = "/base{/group_id,first_name}/pages{/page,lang}{?format,q}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/base/12345/John/pages/5/en?format=json&q=URI%20Templates", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["group_id"], match.Bindings["group_id"].Value);
            Assert.AreEqual(variables1["first_name"], match.Bindings["first_name"].Value);
            Assert.AreEqual(variables1["page"], match.Bindings["page"].Value);
            Assert.AreEqual(variables1["lang"], match.Bindings["lang"].Value);
            Assert.AreEqual(variables1["format"], match.Bindings["format"].Value);
            Assert.AreEqual(variables1["q"], match.Bindings["q"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestEscapeSequences2()
        {
            string template = "/sparql{?query}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/sparql?query=PREFIX%20dc%3A%20%3Chttp%3A%2F%2Fpurl.org%2Fdc%2Felements%2F1.1%2F%3E%20SELECT%20%3Fbook%20%3Fwho%20WHERE%20%7B%20%3Fbook%20dc%3Acreator%20%3Fwho%20%7D", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["query"], match.Bindings["query"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestEscapeSequences3()
        {
            string template = "/go{?uri}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/go?uri=http%3A%2F%2Fexample.org%2F%3Furi%3Dhttp%253A%252F%252Fexample.org%252F", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["uri"], match.Bindings["uri"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestEscapeSequences4()
        {
            string template = "/service{?word}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/service?word=dr%C3%BCcken", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["word"], match.Bindings["word"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestEscapeSequences5()
        {
            string template = "/lookup{?Stra%C3%9Fe}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("/lookup?Stra%C3%9Fe=Gr%C3%BCner%20Weg", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["Stra%C3%9Fe"], match.Bindings["Stra%C3%9Fe"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestEscapeSequences6()
        {
            string template = "{random}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("%C5%A1%C3%B6%C3%A4%C5%B8%C5%93%C3%B1%C3%AA%E2%82%AC%C2%A3%C2%A5%E2%80%A1%C3%91%C3%92%C3%93%C3%94%C3%95%C3%96%C3%97%C3%98%C3%99%C3%9A%C3%A0%C3%A1%C3%A2%C3%A3%C3%A4%C3%A5%C3%A6%C3%A7%C3%BF", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables1["random"], match.Bindings["random"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestEscapeSequences7()
        {
            string template = "{?assoc_special_chars*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables1);
            Assert.AreEqual("?%C5%A1%C3%B6%C3%A4%C5%B8%C5%93%C3%B1%C3%AA%E2%82%AC%C2%A3%C2%A5%E2%80%A1%C3%91%C3%92%C3%93%C3%94%C3%95=%C3%96%C3%97%C3%98%C3%99%C3%9A%C3%A0%C3%A1%C3%A2%C3%A3%C3%A4%C3%A5%C3%A6%C3%A7%C3%BF", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "fields", "geocode" }, new[] { "assoc_special_chars" });
            Assert.IsNotNull(match);
            CollectionAssert.AreEqual((ICollection)variables1["assoc_special_chars"], (ICollection)match.Bindings["assoc_special_chars"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        public void TestCompoundPathSegmentExpansionCollectionVariable()
        {
            string template = "{/id*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables2);
            string[] allowed =
                {
                    "/person/albums",
                    "/albums/person"
                };

            CollectionAssert.Contains(allowed, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "id", "fields", "geocode" }, new string[0]);
            Assert.IsNotNull(match);
            CollectionAssert.AreEqual((ICollection)variables2["id"], (ICollection)match.Bindings["id"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestCompoundPathSegmentExpansionWithQueryStringCollectionVariable()
        {
            string template = "{/id*}{?fields,token}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables2);
            string[] allowed =
                {
                    "/person/albums?fields=id,name,picture&token=12345",
                    "/person/albums?fields=id,picture,name&token=12345",
                    "/person/albums?fields=picture,name,id&token=12345",
                    "/person/albums?fields=picture,id,name&token=12345",
                    "/person/albums?fields=name,picture,id&token=12345",
                    "/person/albums?fields=name,id,picture&token=12345",
                    "/albums/person?fields=id,name,picture&token=12345",
                    "/albums/person?fields=id,picture,name&token=12345",
                    "/albums/person?fields=picture,name,id&token=12345",
                    "/albums/person?fields=picture,id,name&token=12345",
                    "/albums/person?fields=name,picture,id&token=12345",
                    "/albums/person?fields=name,id,picture&token=12345"
                };

            CollectionAssert.Contains(allowed, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "id", "fields", "geocode" }, new string[0]);
            Assert.IsNotNull(match);
            CollectionAssert.AreEqual((ICollection)variables2["id"], (ICollection)match.Bindings["id"].Value);
            CollectionAssert.AreEqual((ICollection)variables2["fields"], (ICollection)match.Bindings["fields"].Value);
            Assert.AreEqual(variables2["token"], match.Bindings["token"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        public void TestPathSegmentExpansionEmptyList()
        {
            string template = "{/empty_list}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables3);
            Assert.AreEqual(string.Empty, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "empty_list" }, new[] { "empty_assoc" });
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.Bindings.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.PathSegmentExpansion)]
        public void TestCompoundPathSegmentExpansionEmptyList()
        {
            string template = "{/empty_list*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables3);
            Assert.AreEqual(string.Empty, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "empty_list" }, new[] { "empty_assoc" });
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.Bindings.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestQueryExpansionEmptyList()
        {
            string template = "{?empty_list}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables3);
            Assert.AreEqual(string.Empty, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "empty_list" }, new[] { "empty_assoc" });
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.Bindings.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestCompoundQueryExpansionEmptyList()
        {
            string template = "{?empty_list*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables3);
            Assert.AreEqual(string.Empty, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "empty_list" }, new[] { "empty_assoc" });
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.Bindings.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestQueryExpansionEmptyMap()
        {
            string template = "{?empty_assoc}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables3);
            Assert.AreEqual(string.Empty, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "empty_list" }, new[] { "empty_assoc" });
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.Bindings.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestCompoundQueryExpansionEmptyMap()
        {
            string template = "{?empty_assoc*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables3);
            Assert.AreEqual(string.Empty, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "empty_list" }, new[] { "empty_assoc" });
            Assert.IsNotNull(match);
            Assert.AreEqual(0, match.Bindings.Count);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansionNumericKey()
        {
            string template = "{42}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables4);
            Assert.AreEqual("The%20Answer%20to%20the%20Ultimate%20Question%20of%20Life%2C%20the%20Universe%2C%20and%20Everything", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "1337" }, new[] { "german" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables4["42"], match.Bindings["42"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestQueryExpansionNumericKey()
        {
            string template = "{?42}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables4);
            Assert.AreEqual("?42=The%20Answer%20to%20the%20Ultimate%20Question%20of%20Life%2C%20the%20Universe%2C%20and%20Everything", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "1337" }, new[] { "german" });
            Assert.IsNotNull(match);
            Assert.AreEqual(variables4["42"], match.Bindings["42"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.SimpleExpansion)]
        public void TestSimpleExpansionNumericKeyCollectionVariable()
        {
            string template = "{1337}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables4);
            Assert.AreEqual("leet,as,it,can,be", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "1337" }, new[] { "german" });
            Assert.IsNotNull(match);
            CollectionAssert.AreEqual((ICollection)variables4["1337"], (ICollection)match.Bindings["1337"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestCompoundQueryExpansionNumericKeyCollectionVariable()
        {
            string template = "{?1337*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables4);
            Assert.AreEqual("?1337=leet&1337=as&1337=it&1337=can&1337=be", uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "1337" }, new[] { "german" });
            Assert.IsNotNull(match);
            CollectionAssert.AreEqual((ICollection)variables4["1337"], (ICollection)match.Bindings["1337"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategories.Extended)]
        [TestCategory(TestCategories.QueryExpansion)]
        public void TestCompoundQueryExpansionNumericKeyMapVariable()
        {
            string template = "{?german*}";
            UriTemplate uriTemplate = new UriTemplate(template);
            Uri uri = uriTemplate.BindByName(variables4);
            string[] allowed =
                {
                    "?11=elf&12=zw%C3%B6lf",
                    "?12=zw%C3%B6lf&11=elf"
                };

            CollectionAssert.Contains(allowed, uri.ToString());

            UriTemplateMatch match = uriTemplate.Match(uri, new[] { "1337" }, new[] { "german" });
            Assert.IsNotNull(match);
            CollectionAssert.AreEqual((ICollection)variables4["german"], (ICollection)match.Bindings["german"].Value);
        }
    }
}
