using CyberTillAPI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;

namespace CyberTillAPI.BAL
{
    public class ProductBAL
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        Common common = new Common();
        public List<ProductXmlModel.Item> GetProductList(string AuthId,string BaseUrl)
        {
            List<ProductXmlModel.Item> PList = new List<ProductXmlModel.Item>();
            HttpWebRequest http = common.CreateWebRequest(AuthId,BaseUrl);
            int Count = 0;
            int StartFrom = 1;
            try
            {
                Count = GetProductsCount(http);
                if (Count > 5000)
                {
                    var LoopCount = (double)(Count / 5000);
                    LoopCount = Math.Round(LoopCount);

                    for (int i = 0; i < LoopCount + 1; i++)
                    {
                        http = common.CreateWebRequest(AuthId,BaseUrl);
                        ProductXmlModel.Result ProductsList = new ProductXmlModel.Result();
                        ProductsList = GetProducts(http, StartFrom);
                        List<ProductXmlModel.Item> ProductItems = ProductsList.Item;
                        if (ProductItems != null)
                        {
                            foreach (ProductXmlModel.Item item in ProductItems)
                            {
                                PList.Add(item);
                            }
                            StartFrom += 5000;                        
                            //if(PList.Count<(i+1)*5000)
                            //{
                            //    break;
                            //}
                            
                        }
                      
                    }
                }
                else
                {
                    http = common.CreateWebRequest(AuthId,BaseUrl);
                    ProductXmlModel.Result ProductsList = new ProductXmlModel.Result();
                    ProductsList = GetProducts(http, 1);
                    PList = ProductsList.Item;
                }
            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                //Console.Read();
            }
            string abc = JsonConvert.SerializeObject(PList).ToString();
            return PList;


        }
        private int GetProductsCount(HttpWebRequest http)
        {
            HttpWebRequest request = http;
            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        <Body>
                                            <product_list_count xmlns = ""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"">
                                                <availability >true</availability>
                                                <date_updated ></date_updated>
                                                <date_created ></date_created>
                                            </product_list_count>
                                        </Body>
                                    </Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
            int Count = 0;
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(soapResult);
                    XmlNode node = doc.DocumentElement.FirstChild;

                    int.TryParse(node.InnerText, out Count);
                    return Count;
                }
            }
        }
        private ProductXmlModel.Result GetProducts(HttpWebRequest http, int StartFrom)
        {
            ProductXmlModel.Result Result = new ProductXmlModel.Result();
            try
            {
                HttpWebRequest request = http;
                XmlDocument soapEnvelopeXml = new XmlDocument();
                soapEnvelopeXml.LoadXml(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                            <Body>
                                                <item_list xmlns=""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"">
                                                    <available>true</available>
                                                    <date_updated></date_updated>
                                                    <cntryId>235</cntryId>
                                                    <includeProduct>true</includeProduct>
                                                    <discontinued>false</discontinued>
                                                    <web_visible>true</web_visible>
                                                    <date_created></date_created>
                                                    <start_from>" + StartFrom + @"</start_from>
                                                    <limit>100000</limit>
                                                </item_list>
                                            </Body>
                                      </Envelope>");

                using (Stream stream = request.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        string soapResult = rd.ReadToEnd();

                        int start = soapResult.IndexOf("<result>");
                        int end = soapResult.IndexOf("</result>");
                        var res = soapResult.Substring(start, end - start + 9);

                        var Obj = common.ObjectToXML(res, typeof(ProductXmlModel.Result));
                        Result = (ProductXmlModel.Result)Obj;
                    }
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    //Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    {
                        string ResponseJson1 = new StreamReader(data).ReadToEnd();
                        //dynamic JsonResponse = JsonConvert.DeserializeObject(ResponseJson1);
                        (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ResponseJson1 + "<br/>" +"Product start from "+StartFrom);
                        //Console.WriteLine(ResponseJson1);
                    }
                }
                throw ex;
            }
            return Result;
        }
    }
}
