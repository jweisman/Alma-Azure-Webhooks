using System.Net;
using System.IO;
using Renci.SshNet;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;
using System;
using System.Text;
using Dropbox.Api;
using System.Threading.Tasks;
using Dropbox.Api.Files;

namespace AlmaWebhookAzure.Models
{
    public static class Utilities
    {
        public static string FtpDownload(FtpSettings settings, string filename)
        {
            using (var client = new SftpClient(settings.Host, settings.User, settings.Pass))
            {
                client.Connect();
                string file = Path.GetTempFileName();
                using (Stream stream = File.OpenWrite(file))
                {
                    client.DownloadFile(filename, stream);
                }
                return file;
            }
        }

        public static async Task DropboxUpload(string token, string filename, string content)
        {
            using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (var dbx = new DropboxClient(token))
            {
                var updated = await dbx.Files.UploadAsync(
                    filename,
                    WriteMode.Overwrite.Instance,
                    body: mem);
            }

        }

        public static string GetResource(string filename)
        {
            string assemblyName =
                typeof(Utilities).Assembly.GetName().Name;
            using (Stream stream = Assembly.GetExecutingAssembly().
                GetManifestResourceStream(assemblyName + "." + filename))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }

        }

        public static string XslTransform(string xml, string xsl)
        {
            string output;
            using (StringReader srt = new StringReader(xsl)) // xslInput is a string that contains xsl
            using (StringReader sri = new StringReader(xml)) // xmlInput is a string that contains xml
            {
                using (XmlReader xrt = XmlReader.Create(srt))
                using (XmlReader xri = XmlReader.Create(sri))
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(xrt);
                    using (UTF8StringWriter sw = new UTF8StringWriter())
                    using (XmlWriter xwo = XmlWriter.Create(sw, xslt.OutputSettings)) // use OutputSettings of xsl, so it can be output as HTML
                    {
                        xslt.Transform(xri, xwo);
                        output = sw.ToString();
                    }
                }
            }
            return output;
        }
    }

    // From: http://stackoverflow.com/questions/2858024/messing-with-encoding-and-xslcompiledtransform?rq=1
    public class UTF8StringWriter : StringWriter
    {
        public UTF8StringWriter() { }
        public UTF8StringWriter(IFormatProvider formatProvider) : base(formatProvider) { }
        public UTF8StringWriter(StringBuilder sb) : base(sb) { }
        public UTF8StringWriter(StringBuilder sb, IFormatProvider formatProvider) : base(sb, formatProvider) { }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}