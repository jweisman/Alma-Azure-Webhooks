using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace AlmaWebhookAzure.Models
{
    public class WebhookHandler
    {
        private AppSettings _appSettings = new AppSettings();

        public WebhookHandler()
        {
            _appSettings.FtpSettings = new FtpSettings();
            _appSettings.FtpSettings.Host = ConfigurationManager.AppSettings["FtpHost"];
            _appSettings.FtpSettings.User = ConfigurationManager.AppSettings["FtpUser"];
            _appSettings.FtpSettings.Pass = ConfigurationManager.AppSettings["FtpPass"];

        }

        public async Task JobEnd(JToken jobInstance)
        {
            Trace.TraceInformation("Job received");
            try
            {
                JToken i = jobInstance.SelectToken("$.counter[?(@.type.value=='c.jobs.bibExport.link')]");
                if (i != null)
                {
                    // Download export file
                    string exportFile = System.IO.Path.Combine(
                        ConfigurationManager.AppSettings["FtpDir"],
                        i["value"].ToString() + "_1.xml");
                    Trace.TraceInformation("Downloading {0} from {1}", exportFile, _appSettings.FtpSettings.Host);
                    string file = Utilities.FtpDownload(_appSettings.FtpSettings, exportFile);

                    // Parse export file 
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);
                    XmlNodeList records = xmlDoc.SelectNodes("/collection/record");
                    Trace.TraceInformation("Retrieved {0} records", records.Count);

                    // Upload each record to Dropbox
                    string xsl = Utilities.GetResource("App_Data.target.xsl");
                    int n = 0;
                    var tasks = new List<Task>();
                    foreach (XmlNode record in records)
                    {
                        string output = Utilities.XslTransform(record.OuterXml, xsl);
                        tasks.Add(Utilities.DropboxUpload(
                            ConfigurationManager.AppSettings["DropboxToken"],
                            string.Format("/{0}.xml", n++),
                            output)
                        );
                    }
                    await Task.WhenAll(tasks);
                    Trace.TraceInformation("Uploaded records to Dropbox");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.StackTrace);
                throw e;
            }
        }
    }
}