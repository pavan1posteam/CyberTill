using CyberTillAPI.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace CyberTillAPI.BAL
{
    public class CategoriesBAL
    {
        Common common = new Common();
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        public List<CategoriesXmlModel.Category> GetCategories(HttpWebRequest http)
        {
            List<CategoriesXmlModel.Category> CatItemList = new List<CategoriesXmlModel.Category>();
            CategoriesXmlModel.Result Result = new CategoriesXmlModel.Result();
            try
            {
                HttpWebRequest request = http;
                XmlDocument soapEnvelopeXml = new XmlDocument();
                soapEnvelopeXml.LoadXml(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                            <Body>
                                                <category_list xmlns=""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/""/>
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

                        var Obj = common.ObjectToXML(res, typeof(CategoriesXmlModel.Result));
                        Result = (CategoriesXmlModel.Result)Obj;
                        CatItemList.Add(Result.Category);
                        GetCategoryObj(Result.Child_categories.Item, ref CatItemList);
                        void GetCategoryObj(List<CategoriesXmlModel.Item> item, ref List<CategoriesXmlModel.Category> categories)
                        {
                            try
                            {
                                foreach (var c in item)
                                {
                                    if (!categories.Any(a => a.Id == c.Category.Id))
                                    {
                                        categories.Add(c.Category);
                                    }
                                }
                                foreach (var c in item.Where(w => w != null && w.Child_categories != null))
                                {
                                    GetCategoryObj(c.Child_categories.Item, ref categories);
                                }
                            }
                            catch (Exception ex)
                            {
                                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                                Console.WriteLine(ex.Message);
                                Console.Read();
                            }
                        }

                    }
                }
                //CatItemList = Result.Child_categories.Item.Select(a => a.Category).Where(a => a.Name.ToUpper() == "WINE" || a.Name.ToUpper() == "BEER" || a.Name.ToUpper() == "MICRO-BEER"
                //    || a.Name.ToUpper() == "LIQUOR" || a.Name.ToUpper() == "LIQUOR 100ML" || a.Name.ToUpper() == "NON-ALCOHOLIC MIXERS").ToList();
               // CatItemList = Result.Child_categories.Item.Select(a => a.Category).ToList();
            }
            catch(Exception ex)
            {
                (new CyberTillAPI.Email.clsEmail()).sendEmail(DeveloperId, "", "", "Error in CybertillPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.Read();
            }
              return CatItemList;
        }
    }
}
