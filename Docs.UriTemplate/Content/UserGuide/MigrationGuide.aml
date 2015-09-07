<?xml version="1.0" encoding="utf-8"?>
<topic id="8e5cf0b3-d565-44f8-a763-6586cf5e56db" revisionNumber="1">
  <developerConceptualDocument
    xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5"
    xmlns:xlink="http://www.w3.org/1999/xlink">

    <!--
    <summary>
      <para>Optional summary abstract</para>
    </summary>
    -->

    <introduction>
      <para>
        The .NET Framework includes its own implementation of URI templates through its own
        <codeEntityReference>T:System.UriTemplate</codeEntityReference> class. The reference
        implementation of this class does not use the same syntax as RFC 6570, and also does
        not provide many of the advanced features described in the RFC. The URI Template
        Library is intended to reduce the effort required to migrate existing code that uses
        the .NET Framework, but syntax and functionality differences may require code changes.
        This page includes a partial list of migration issues developers may face when
        switching to this library.
      </para>
      <autoOutline/>
    </introduction>

    <section address="SyntaxChanges">
      <title>Template Syntax</title>
      <content>
        <para>
          Several aspects of URI Templates are affected by syntax differences between the
          .NET Framework implementation and RFC 6570. The needed changes are grouped into
          path segments syntax and query parameters. In addition to the syntax changes,
          there are differences in the behavior of the template matching operation. This
          will require additional consideration for code making use of the
          <codeEntityReference>Overload:TunnelVisionLabs.Net.UriTemplate.Match</codeEntityReference>
          operation.
        </para>
      </content>
      <sections>
        <section>
          <title>Path Variables</title>
          <content>
            <para>
              In most cases, expansion variables which appear in the path portion of a .NET
              Framework URI template are directly compatible with RFC 6570. The exception
              to this rule is templates which use an asterisk (<codeInline>*</codeInline>)
              to indicate the "rest of the path". RFC 6570 provides a special syntax called
              a <legacyItalic>reserved expansion</legacyItalic> which provides a similar
              feature. The following specific example appears in the documentation for the
              .NET Framework <codeEntityReference>T:System.UriTemplate</codeEntityReference>
              class.
            </para>
            <!-- A bug somewhere prevents /* from appearing in the output without the zero-width space -->
            <code language="none">
              <![CDATA[
              weather/*
              ]]>
            </code>
            <para>
              In this library, the equivalent to this template would be the following, where
              <codeInline>var</codeInline> can be any variable name.
            </para>
            <code language="none">
              <![CDATA[
              weather/{+var}
              ]]>
            </code>
            <para>
              If code requires access to individual path segments, such as existing code
              making use of the
              <codeEntityReference>P:System.UriTemplateMatch.WildcardPathSegments</codeEntityReference>
              property, an alternative syntax called a <legacyItalic>path segment expansion</legacyItalic>
              may be used in combination with a <legacyItalic>composite value modifier </legacyItalic>
              instead of the reserved expansion. The following example shows the use of the
              path segment expansion to match the rest of the path.
            </para>
            <code language="none">
              <![CDATA[
              weather{/segments*}
              ]]>
            </code>
            <alert class="important">
              <para>
                While the reserved expansion is capable of matching both the path component
                of the URI and the query string, a path segment expansion will only match
                the path component. To include the ability to match query parameters when
                using the path segment expansion, see the
                <link xlink:href="#AdditionalParameters">Additional Query Parameters</link>
                section below.
              </para>
            </alert>
          </content>
        </section>
        <section>
          <title>Query Parameters</title>
          <content>
            <para>
              In the .NET Framework, the syntax for declaring query parameters for the template
              expansion operation differs from RFC 6570. In addition, during the matching
              operation the .NET Framework has implicit support for arbitrary query parameters
              which are not declared in the template itself. Each of these issues needs to be
              addressed when migrating to this library.
            </para>
          </content>
          <sections>
            <section address="AdditionalParameters">
              <title>Additional Query Parameters</title>
              <content>
                <para>
                  Support for additional query parameters during template matching is
                  accomplished by adding including a query expansion with a composite value
                  modifier. For templates which do not already contain a query expansion,
                  this is as simple as adding <codeInline>{?params*}</codeInline> to the
                  end of the template. For templates which do include a query expansion,
                  the new variable must be added at the end of the list. A template ending
                  with <codeInline>{?foo,bar}</codeInline> would thus become
                  <codeInline>{?foo,bar,params*}</codeInline>.
                </para>
              </content>
            </section>
          </sections>
        </section>
        <section>
          <title>Summary</title>
          <content>
            <para>
              The following table summarizes the changes required to make .NET URI Templates
              compatible with RFC 6570, using the examples provided in the documentation for
              the .NET Framework <codeEntityReference>T:System.UriTemplate</codeEntityReference>
              class.
            </para>
            <table>
              <tableHeader>
                <row>
                  <entry>
                    <para>.NET Framework</para>
                  </entry>
                  <entry>
                    <para>RFC 6570 (Binding)</para>
                  </entry>
                  <entry>
                    <para>RFC 6570 (Matching)</para>
                  </entry>
                </row>
              </tableHeader>
              <row>
                <entry>
                  <para>"weather/WA/Seattle"</para>
                </entry>
                <entry>
                  <para>"weather/WA/Seattle"</para>
                </entry>
                <entry>
                  <para>"weather/WA/Seattle{?params*}"</para>
                </entry>
              </row>
              <row>
                <entry>
                  <para>"weather/{state}/{city}"</para>
                </entry>
                <entry>
                  <para>"weather/{state}/{city}"</para>
                </entry>
                <entry>
                  <para>"weather/{state}/{city}{?params*}"</para>
                </entry>
              </row>
              <row>
                <entry>
                  <para>"weather/*"</para>
                </entry>
                <entry>
                  <para>"weather/{+rest}"</para>
                </entry>
                <entry>
                  <para>"weather{/rest*}{?params*}", if "weather" matches</para>
                  <para>-or-</para>
                  <para>"weather/{rest0}{/rest1*}{?params*}", if "weather/" matches but "weather" does not</para>
                </entry>
              </row>
              <row>
                <entry>
                  <para>"weather/{state}/{city}?forecast=today"</para>
                </entry>
                <entry>
                  <para>"weather/{state}/{city}?forecast=today"</para>
                </entry>
                <entry>
                  <para>"weather/{state}/{city}{?forecast,params*}"</para>
                </entry>
              </row>
              <row>
                <entry>
                  <para>"weather/{state}/{city}?forecast={day}"</para>
                </entry>
                <entry>
                  <para>"weather/{state}/{city}{?forecast}"</para>
                </entry>
                <entry>
                  <para>"weather/{state}/{city}{?forecast,params*}"</para>
                </entry>
              </row>
            </table>
          </content>
        </section>
      </sections>
    </section>

    <section address="TemplateBinding">
      <title>Template Binding</title>
      <content>
        <para>
          This library contains additional differences regarding the way parameters are
          inserted into templates to produce a bound URI.
        </para>
      </content>

      <sections>
        <section address="BindByPosition">
          <title>Bind By Position</title>
          <content>
            <para>
              This library does not contain an equivalent to the
              <codeEntityReference>M:System.UriTemplate.BindByPosition(System.Uri,System.String[])</codeEntityReference>
              method. Any calls which use this method must be converted to use
              <codeEntityReference>Overload:TunnelVisionLabs.Net.UriTemplate.BindByName</codeEntityReference>
              when migrating existing code to this library.
            </para>
          </content>
        </section>

        <section address="RelativeResolution">
          <title>Relative Resolution</title>
          <content>
            <para>
              For the various
              <codeEntityReference>Overload:TunnelVisionLabs.Net.UriTemplate.BindByName</codeEntityReference>
              methods which take a base address as a parameter, this library uses the
              <codeEntityReference>M:System.Uri.#ctor(System.Uri,System.Uri)</codeEntityReference>
              constructor to implement the resolution of relative addresses against a base
              address. While the .NET Framework's URI Template implementation behaves in a
              manner "similar" to
              <codeEntityReference>Overload:System.IO.Path.Combine</codeEntityReference>, the
              behavior of this library is consistent with <externalLink>
                <linkText>RFC 3986 §5.2</linkText>
                <linkAlternateText>RFC 3986 §5.2 (Relative Resolution - URI Generic Syntax)</linkAlternateText>
                <linkUri>http://tools.ietf.org/html/rfc3986#section-5.2</linkUri>
              </externalLink>. As a result, this library differs from the .NET Framework's
              implementation in two key instances.
            </para>
            <alert class="note">
              <para>
                In the following description, the
                <codeEntityReference>P:System.Uri.Query</codeEntityReference> property of the
                base address is assumed to be empty.
              </para>
            </alert>
            <list class="bullet">
              <listItem>
                <para>
                  When the URI Template starts with a leading <codeInline>/</codeInline>
                  character. In this case, the
                  <codeEntityReference>P:System.Uri.AbsolutePath</codeEntityReference> property
                  of the base address is removed from the base address during the resolution
                  process.
                </para>
              </listItem>
              <listItem>
                <para>
                  When the <codeEntityReference>P:System.Uri.AbsolutePath</codeEntityReference>
                  property of the base address is not empty, and does not end with a
                  <codeInline>/</codeInline> character. In this case, the last path segment is
                  removed from the base address during the resolution process. In other words,
                  everything after the last <codeInline>/</codeInline> character is ignored.
                </para>
              </listItem>
            </list>
          </content>
        </section>
      </sections>
    </section>

    <relatedTopics>
    </relatedTopics>
  </developerConceptualDocument>
</topic>
