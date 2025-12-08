using CyberTillAPI.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Xml;

namespace CyberTillAPI.BAL
{
    public class StockBAL
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        Common common = new Common();
        public List<StockXmlModel.Item> GetStockList(string AuthId,string BaseUrl)
        {
            List<StockXmlModel.Item> StockListItems = new List<StockXmlModel.Item>();
            var request = common.CreateRestRequest(AuthId,BaseUrl);

            int Count = 0;
            int StartFrom = 1;
            try
            {
                Count = GetStockCount(request,BaseUrl);
                if (Count > 5000)
                {
                    
                    var LoopCount = (double)(Count / 5000);
                    LoopCount = Math.Round(LoopCount);

                    for (int i = 0; i < LoopCount + 1; i++)
                    {
                        request = common.CreateRestRequest(AuthId,BaseUrl);
                        StockXmlModel.Result StockList = new StockXmlModel.Result();
                        StockList = GetStock(request, StartFrom,BaseUrl);
                        List<StockXmlModel.Item> StockItems = StockList.Item;

                        foreach (StockXmlModel.Item item in StockItems)
                        {
                            StockListItems.Add(item);
                        }
                        StartFrom += 5000;
                    }
                }
                else
                {
                    request = common.CreateRestRequest(AuthId,BaseUrl);
                    StockXmlModel.Result StockList = new StockXmlModel.Result();
                    StockList = GetStock(request, 1, BaseUrl);
                    List<StockXmlModel.Item> StockItems = StockList.Item;
                    StockListItems = StockItems;
                }
            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
            }

            return StockListItems;
        }
        private int GetStockCount(RestRequest request,string BaseUrl)
        {
            //string baseUrl1 = @"https://ct73053.rs-pos.net/current/CybertillApi_v1_6.php";

            var client = new RestClient(BaseUrl);
            string body = @"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        <Body>
                                            <stock_list_count xmlns = ""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"">
                                              <date_created></date_created>
                                               <date_updated></date_updated>
                                            </stock_list_count>
                                        </Body>
                                    </Envelope>";

            request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            int Count = 0;
            try
            {
                string soapResult = response.Content;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(soapResult);
                XmlNode node = doc.DocumentElement.FirstChild;

                int.TryParse(node.InnerText, out Count);
            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            return Count;
        }

        private StockXmlModel.Result GetStock(RestRequest request, int StartFrom,string BaseUrl)
        {
            StockXmlModel.Result Result = new StockXmlModel.Result();
            try
            {
                //string baseUrl1 = @"https://ct73053.rs-pos.net/current/CybertillApi_v1_6.php";
                var client = new RestClient(BaseUrl);

                string body = @"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                            <Body>
                                                <stock_list xmlns=""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"">
                                                    <date_created></date_created>
                                                    <date_updated></date_updated>
                                                    <start_from>" + StartFrom + @"</start_from>
                                                    <limit>1000000</limit>
                                                </stock_list>
                                            </Body>
                                        </Envelope>";

                request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                string soapResult = response.Content;

                int start = soapResult.IndexOf("<result>");
                int end = soapResult.IndexOf("</result>");
                var res = soapResult.Substring(start, end - start + 9);
                var Obj = common.ObjectToXML(res, typeof(StockXmlModel.Result));
                Result = (StockXmlModel.Result)Obj;

            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            return Result;
        }
    }
}
