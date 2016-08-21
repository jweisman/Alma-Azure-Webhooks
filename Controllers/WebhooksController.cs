using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AlmaWebhookAzure.Models;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AlmaWebhookAzure.Controllers
{
    public class WebhooksController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Challenge(string challenge)
        {
            Trace.TraceInformation("Received webhook challenge: {0}", challenge);
            dynamic response = new ExpandoObject();
            response.challenge = challenge;
            return Ok(response);
        }

        [HttpPost]
        public async Task<IHttpActionResult> ProcessWebhook([FromBody]JObject body)
        {
            Trace.TraceInformation("Webhook received: {0}", body.ToString());
            string signature = Request.Headers.GetValues("X-Exl-Signature").First();
            if (!ValidateSignature(body.ToString(Newtonsoft.Json.Formatting.None),
                signature, ConfigurationManager.AppSettings["WebhookSecret"]))
            {
                return Unauthorized();
            }

            string action = body["action"].ToString();
            switch (action.ToLower())
            {
                case "job_end":
                    WebhookHandler handler = new WebhookHandler();
                    await handler.JobEnd(body["job_instance"]);
                    return Ok();
                default:
                    return BadRequest();
            }
        }

        private bool ValidateSignature(string body, string signature, string secret)
        {
            var hash = new System.Security.Cryptography.HMACSHA256(
                Encoding.ASCII.GetBytes(secret));
            var computedSignature = Convert.ToBase64String(
                hash.ComputeHash(Encoding.ASCII.GetBytes(body)));
            return computedSignature == signature;
        }
    }
}
