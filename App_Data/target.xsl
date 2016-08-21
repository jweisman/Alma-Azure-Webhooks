<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <doc>
      <id>
        <xsl:value-of select="record/controlfield[@tag='001']"/>
      </id>
      <title>
        <xsl:value-of select="record/datafield[@tag='245']/subfield[@code='a']"/>
      </title>
    </doc>
  </xsl:template>
</xsl:stylesheet>