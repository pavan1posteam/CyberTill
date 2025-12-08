using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using RestSharp;

namespace CyberTillAPI
{
    public class Common
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];


        public RestRequest CreateRestRequest(string AuthId,string BaseUrl)
        {
            try
            {
                //string baseUrl1 = @"https://ct73053.rs-pos.net/current/CybertillApi_v1_6.php";
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "text/xml");
                string credidentials = AuthId + ":" + "";
                string authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
                request.AddHeader("Authorization", "Basic " + authorization);
                return request;

            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
                return null;
            }            
        }

        public HttpWebRequest CreateWebRequest(string AuthId,string BaseUrl)
        {
            HttpWebRequest webRequest = null;
            try
            {
                webRequest = (HttpWebRequest)WebRequest.Create(BaseUrl);
                webRequest.Headers.Add(@"SOAP:Action");
                webRequest.ContentType = "text/xml;charset=\"utf-8\"";
                webRequest.Accept = "text/xml";
                webRequest.Method = "POST";
                string credidentials = AuthId + ":" + "";
                string authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
                webRequest.Headers["Authorization"] = "Basic " + authorization;

            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            return webRequest;
        }

        public Object ObjectToXML(string xml, Type objectType)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            Object obj = null;
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(objectType);
                xmlReader = new XmlTextReader(strReader);
                obj = serializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            finally
            {
                if (xmlReader != null)

                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }
    }
}
