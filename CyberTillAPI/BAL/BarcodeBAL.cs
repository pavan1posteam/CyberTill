using CyberTillAPI.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CyberTillAPI.BAL
{
    public class BarcodeBAL
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        Common common = new Common();
        public List<BarcodeXmlModel.Item> GetBarcodeList(string AuthId, string BaseUrl)
        {
            List<BarcodeXmlModel.Item> BarcodeListItems = new List<BarcodeXmlModel.Item>();
            var request = common.CreateRestRequest(AuthId, BaseUrl);

            int Count = 0;
            int StartFrom = 1;
            try
            {
                Count = GetBarcodeCount(request, BaseUrl);
                if (Count > 5000)
                {
                    var LoopCount = (double)(Count / 5000);
                    LoopCount = Math.Round(LoopCount);

                    for (int i = 0; i < LoopCount + 1; i++)
                    {
                        request = common.CreateRestRequest(AuthId, BaseUrl);
                        BarcodeXmlModel.Result BarcodeList = new BarcodeXmlModel.Result();
                        BarcodeList = GetBarcode(request, StartFrom, BaseUrl);
                        List<BarcodeXmlModel.Item> BarcodeItems = BarcodeList.Item;

                        foreach (BarcodeXmlModel.Item item in BarcodeItems)
                        {
                            BarcodeListItems.Add(item);
                        }
                        StartFrom += 5000;
                    }
                }
                else
                {
                    request = common.CreateRestRequest(AuthId, BaseUrl);
                    BarcodeXmlModel.Result BarcodeList = new BarcodeXmlModel.Result();
                    BarcodeList = GetBarcode(request, 1, BaseUrl);
                    List<BarcodeXmlModel.Item> BarcodeItems = BarcodeList.Item;
                    BarcodeListItems = BarcodeItems;
                }
            }
            catch (Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            string abc = JsonConvert.SerializeObject(BarcodeListItems).ToString();
            return BarcodeListItems;
        }
        private int GetBarcodeCount(RestRequest request, string BaseUrl)
        {
            //string baseUrl1 = @"https://ct73053.rs-pos.net/current/CybertillApi_v1_6.php";
            var client = new RestClient(BaseUrl);
            string body = @"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        <Body>
                                            <item_barcode_list_count xmlns = ""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"">
                                             <date_updated></date_updated>
                                             <date_created></date_created>
                                            </item_barcode_list_count>
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

        private BarcodeXmlModel.Result GetBarcode(RestRequest request, int StartFrom, string BaseUrl)
        {
            BarcodeXmlModel.Result Result = new BarcodeXmlModel.Result();
            try
            {
                //string baseUrl1 = @"https://ct73053.rs-pos.net/current/CybertillApi_v1_6.php";
                var client = new RestClient(BaseUrl);

                string body = @"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                            <Body>
                                                <item_barcode_list xmlns=""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"">
                                                    <date_created></date_created>
                                                    <date_updated></date_updated>
                                                    <start_from>" + StartFrom + @"</start_from>
                                                    <limit>1000000</limit>
                                                </item_barcode_list>
                                            </Body>
                                        </Envelope>";

                request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                string soapResult = response.Content;

                int start = soapResult.IndexOf("<result>");
                int end = soapResult.IndexOf("</result>");
                var res = soapResult.Substring(start, end - start + 9);
                var Obj = common.ObjectToXML(res, typeof(BarcodeXmlModel.Result));
                Result = (BarcodeXmlModel.Result)Obj;

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
