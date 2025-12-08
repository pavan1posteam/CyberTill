using CyberTillAPI.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Xml;


namespace CyberTillAPI.BAL
{
    public class ProductByCategoriesBAL
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        Common common = new Common();
        public List<ProductByCategoryXmlModel.Item> ProductByCategories(string AuthId, string BaseUrl)
        {
            List<ProductByCategoryXmlModel.Item> ProductItemList = new List<ProductByCategoryXmlModel.Item>();

            HttpWebRequest http = common.CreateWebRequest(AuthId, BaseUrl);

             CategoriesBAL categoriesBAL = new CategoriesBAL();
            List<CategoriesXmlModel.Category> CatItemList = categoriesBAL.GetCategories(http);

            int Count = 0;

            foreach (CategoriesXmlModel.Category Category in CatItemList)
            {
                try
                {
                    Count = 0;
                    http = common.CreateWebRequest(AuthId, BaseUrl);
                    Count = GetProductbycategorylistCount(http, Category.Id);

                    int StartFrom = 1;
                    if (Count > 5000)
                    {
                        var LoopCount = (double)(Count / 5000);
                        LoopCount = Math.Round(LoopCount);                                               
                        for (int i = 0; i < LoopCount + 1; i++)
                        {
                            http = common.CreateWebRequest(AuthId, BaseUrl);
                            ProductByCategoryXmlModel.Result ProductsList = new ProductByCategoryXmlModel.Result();
                            ProductsList = GetProducts(http, Category.Id, StartFrom);
                            
                            StartFrom += 5000;
                            foreach (ProductByCategoryXmlModel.Item Product in ProductsList.Item)
                            {

                                Product.CatId = Category.Id;
                                Product.CatName = Category.Name;
                                ProductItemList.Add(Product);
                                //if (ProductItemList.Count == 10000)
                                //{
                                //    break;
                                //}
                            }
                        }
                    }
                    else
                    {
                        http = common.CreateWebRequest(AuthId, BaseUrl);
                        ProductByCategoryXmlModel.Result ProductsList = new ProductByCategoryXmlModel.Result();
                        ProductsList = GetProducts(http, Category.Id, 1);
                        if (ProductsList.Item != null)
                        {
                            foreach (ProductByCategoryXmlModel.Item Product in ProductsList.Item)
                            {
                                Product.CatId = Category.Id;
                                Product.CatName = Category.Name;
                                ProductItemList.Add(Product);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                }
            } 
            return ProductItemList;
        }
        private int GetProductbycategorylistCount(HttpWebRequest http, string CatId)
        {
            ProductXmlModel.Result Result = new ProductXmlModel.Result();
            int Count = 0;
            try
            {
                HttpWebRequest request = http;
                XmlDocument soapEnvelopeXml = new XmlDocument();
                soapEnvelopeXml.LoadXml(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        <Body>
                                            <product_by_category_list_count xmlns = ""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"">
                                                <cat_id>" + CatId + @"</cat_id>
                                                <availability >true</availability>
                                                <date_created ></date_created>
                                                <date_updated ></date_updated>
                                            </product_by_category_list_count>
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
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(soapResult);
                        XmlNode node = doc.DocumentElement.FirstChild;

                        int.TryParse(node.InnerText, out Count);

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Count;
        }
        private ProductByCategoryXmlModel.Result GetProducts(HttpWebRequest http, string CatId, int StartFrom)
        {
            ProductByCategoryXmlModel.Result Result = new ProductByCategoryXmlModel.Result();
            try
            {
                HttpWebRequest request = http;
                XmlDocument soapEnvelopeXml = new XmlDocument();
                soapEnvelopeXml.LoadXml(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        <Body>
                                            <product_by_category_list xmlns = ""http://cybertill.co.uk/wsdl/CybertillApi_v1_6/"" >
                                                <cat_id >" + CatId + @"</cat_id >
                                                <availability>true </availability>
                                                <date_created></date_created>
                                                <date_updated></date_updated>
                                                <start_from> " + StartFrom + @" </start_from>
                                                <limit>1000000</limit>
                                            </product_by_category_list>
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

                        var Obj = common.ObjectToXML(res, typeof(ProductByCategoryXmlModel.Result));
                        Result = (ProductByCategoryXmlModel.Result)Obj;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Result;
        }
    }
}
