using CyberTillAPI.BAL;
using CyberTillAPI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace CyberTillAPI
{
	class Program
	{
		private static string baseUrl = "";

		private static string storeUrl = "";

		private static string Id = "";

		private static void Main(string[] args)
		{
			string developerid = ConfigurationManager.AppSettings["DeveloperId"];
			string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
			string includedcategory = ConfigurationManager.AppSettings["included-category"];
			string categorynotincluded = ConfigurationManager.AppSettings["category-not-included"];
			string irrespectivestock = ConfigurationManager.AppSettings["irrespective-stock"];
			//string IrrespctioveOfStockAndTax = ConfigurationManager.AppSettings["IrrespctioveOfStockAndTax"];
			string upclength = ConfigurationManager.AppSettings["upc-length"];
			try
			{
				StoreSettings.POSSettings pOSSettings = new StoreSettings.POSSettings();
				pOSSettings.IntializeStoreSettings();
				foreach (StoreSettings.POSSetting posDetail in pOSSettings.PosDetails)
                {
                    /*if (posDetail.StoreSettings.StoreId == 11628)
                    {
                        Console.WriteLine("fetching_storeid = " + posDetail.StoreSettings.StoreId);
                    }
                    else { continue; }*/
                    int storeid = posDetail.StoreSettings.StoreId;
                    baseUrl = posDetail.StoreSettings.POSSettings.BaseUrl;
                    storeUrl = posDetail.StoreSettings.POSSettings.StoreUrl;
                    Id = posDetail.StoreSettings.POSSettings.Id;
                    string authId = GetAuthId();
                    Console.WriteLine("Generating Product File For Cybertill " + storeid);
                    Console.WriteLine("Generating Fullname File For Cybertill " + storeid);
                    string text6 = DateTime.Now.ToString("dddd");
                    ProductBAL productBAL = new ProductBAL();
                    List<ProductXmlModel.Item> ProductList = productBAL.GetProductList(authId, baseUrl);
                    ProductByCategoriesBAL productByCategoriesBAL = new ProductByCategoriesBAL();
                    List<ProductByCategoryXmlModel.Item> ProductByCatList = productByCategoriesBAL.ProductByCategories(authId, baseUrl);
                    StockBAL stockBAL = new StockBAL();
                    List<StockXmlModel.Item> ProductStockList = stockBAL.GetStockList(authId, baseUrl);
                    BarcodeBAL barcodeBAL = new BarcodeBAL();
                    List<BarcodeXmlModel.Item> barcodeList = barcodeBAL.GetBarcodeList(authId, baseUrl);
                    decimal tax = posDetail.StoreSettings.POSSettings.tax;
                    decimal MIxtax = posDetail.StoreSettings.POSSettings.Mixerstax;

                    //File.WriteAllText("ProductList11628.json", JsonConvert.SerializeObject(ProductList));
                    //File.WriteAllText("ProductByCatList11628.json", JsonConvert.SerializeObject(ProductByCatList));
                    //File.WriteAllText("BarcodeList11628.json", JsonConvert.SerializeObject(barcodeList));
                   // File.WriteAllText("ProductStockList11628.json", JsonConvert.SerializeObject(ProductStockList));

                    if (includedcategory.Contains(storeid.ToString()))
                    {
                        List<StoreProductModel> list = (from p in ProductList
                                                        join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                                                        join s in ProductStockList on p.ProductOption.Id equals s.StkItemId
                                                        join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                                                        where Convert.ToDecimal(s.Stock) > 0m && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && pc.CatName != "Tobacoo" && pc.CatName != "Cigarettes" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && p.ProductOption.Ref.Length > 5
                                                        select new StoreProductModel
                                                        {
                                                            StoreID = storeid,
                                                            upc = "#" + b.Barcode,
                                                            Qty = Convert.ToDecimal(s.Stock),
                                                            sku = "#" + p.ProductOption.Id,
                                                            pack = 1,
                                                            StoreProductName = p.ProductOption.Product.Name,
                                                            StoreDescription = p.ProductOption.Product.Name,
                                                            Price = Convert.ToDecimal(p.ProductOptionPrice.PriceStore),
                                                            sprice = 0m,
                                                            tax = ((pc.CatName == "Beer" || pc.CatName == "Micro-Beer" || pc.CatName == "Beer Keg") ? 0.105m : 0.125m),
                                                            Start = "",
                                                            End = "",
                                                            altupc1 = "",
                                                            altupc2 = "",
                                                            altupc3 = "",
                                                            altupc4 = "",
                                                            altupc5 = ""
                                                        }).ToList();
                        List<StoreFullNameModel> list2 = (from p in ProductList
                                                          join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                                                          join s in ProductStockList on p.ProductOption.Id equals s.StkItemId
                                                          join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                                                          where Convert.ToDecimal(s.Stock) > 0m && ProductList != null && ProductByCatList != null && ProductStockList != null && pc.CatName != "Cigarettes" && pc.CatName != "Tobacoo" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && p.ProductOption.Ref.Length > 5
                                                          select new StoreFullNameModel
                                                          {
                                                              upc = "#" + b.Barcode,
                                                              pname = p.ProductOption.Product.Name,
                                                              pdesc = p.ProductOption.Product.Name,
                                                              sku = "#" + p.ProductOption.Id,
                                                              Price = Convert.ToDecimal(p.ProductOptionPrice.PriceStore),
                                                              pcat = pc.CatName,
                                                              pcat1 = "",
                                                              pcat2 = "",
                                                              uom = p.ProductOption.StyleString.Replace("Size: $20", "").Replace("Size: ", "").Replace("$", ""),
                                                              country = "USA",
                                                              region = "Arkansas"
                                                          }).ToList();
                        GenerateCSVModel.GenerateCSVFile(list, "PRODUCT", storeid, BaseDirectory);
                        GenerateCSVModel.GenerateCSVFile(list2, "FULLNAME", storeid, BaseDirectory);
                        Console.WriteLine();
                        Console.WriteLine("Product FIle Generated For Cybertill " + storeid);
                        Console.WriteLine("Fullname FIle Generated For Cybertill " + storeid);
                        Console.WriteLine();
                    }
                    else if (categorynotincluded.Contains(storeid.ToString()))
                    {
                        List<StoreProductModel> source = (from p in ProductList
                                                          join s in ProductStockList on p.ProductOption.Id equals s.StkItemId
                                                          join b in barcodeList on s.StkItemId equals b.StkItemId
                                                          where Convert.ToDecimal(s.Stock) > 0m && Convert.ToDecimal(p.ProductOptionPrice.PriceWeb) > 0m && p.ProductOption.Ref.Length > 5 && s.LocationId == "1"
                                                          select new StoreProductModel
                                                          {
                                                              StoreID = storeid,
                                                              upc = "#" + b.Barcode,
                                                              Qty = Convert.ToDecimal(s.Stock),
                                                              sku = "#" + p.ProductOption.Id,
                                                              pack = 1,
                                                              StoreProductName = p.ProductOption.Product.Name,
                                                              StoreDescription = p.ProductOption.Product.Name,
                                                              Price = Convert.ToDecimal(p.ProductOptionPrice.PriceWeb),
                                                              sprice = 0m,
                                                              tax = tax,
                                                              Start = "",
                                                              End = "",
                                                              altupc1 = "",
                                                              altupc2 = "",
                                                              altupc3 = "",
                                                              altupc4 = "",
                                                              altupc5 = ""
                                                          }).ToList();
                        source = (from x in source.AsEnumerable()
                                  group x by x.upc into y
                                  select y.First()).ToList();
                        GenerateCSVModel.GenerateCSVFile(source, "PRODUCT", storeid, BaseDirectory);
                        Console.WriteLine();
                        Console.WriteLine("Product FIle Generated For Cybertill " + storeid);
                    }
                    else if (irrespectivestock.Contains(storeid.ToString()))
                    {
                        List<StoreProductModel> list3 = (from p in ProductList
                                                         join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                                                         join s in ProductStockList on p.ProductOption.Id equals s.StkItemId into ss
                                                         from s in ss.DefaultIfEmpty()
                                                         join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                                                         where ProductList != null && ProductByCatList != null && ProductStockList != null && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && pc.CatName != "Tobacoo" && pc.CatName != "Cigarettes" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && p.ProductOption.Ref.Length > 5
                                                         select new StoreProductModel
                                                         {
                                                             StoreID = storeid,
                                                             upc = "#" + b.Barcode,
                                                             Qty = ((s != null && s.Stock != null) ? Convert.ToDecimal(s.Stock) : 0m),
                                                             sku = "#" + p.ProductOption.Id,
                                                             pack = 1,
                                                             StoreProductName = p.ProductOption.Product.Name,
                                                             StoreDescription = p.ProductOption.Product.Name,
                                                             Price = ((Convert.ToDecimal(p.ProductOptionPrice.PriceRrp) != Convert.ToDecimal(p.ProductOptionPrice.PriceStore)) ? Convert.ToDecimal(p.ProductOptionPrice.PriceRrp) : Convert.ToDecimal(p.ProductOptionPrice.PriceStore)),
                                                             sprice = ((!(DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Wednesday") && !(DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Tuesday")) ? ((Convert.ToDecimal(p.ProductOptionPrice.PriceRrp) != Convert.ToDecimal(p.ProductOptionPrice.PriceStore)) ? Convert.ToDecimal(p.ProductOptionPrice.PriceStore) : 0m) : (((pc.CatName.Trim() == "Wine" || pc.CatName.Trim() == "Boxed & Canned Wine") && DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Wednesday") ? Math.Round(Convert.ToDecimal(p.ProductOptionPrice.PriceStore) - Convert.ToDecimal(p.ProductOptionPrice.PriceStore) * Convert.ToDecimal(0.15), 2) : ((pc.CatName.Trim() == "Tequila" && DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Tuesday") ? Math.Round(Convert.ToDecimal(p.ProductOptionPrice.PriceStore) - Convert.ToDecimal(p.ProductOptionPrice.PriceStore) * Convert.ToDecimal(0.1), 2) : 0m))),
                                                             WedSprice = ((!(DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Wednesday") && !(DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Tuesday")) ? 0m : (((pc.CatName.Trim() == "Wine" || pc.CatName.Trim() == "Boxed & Canned Wine") && DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Wednesday") ? Math.Round(Convert.ToDecimal(p.ProductOptionPrice.PriceStore) - Convert.ToDecimal(p.ProductOptionPrice.PriceStore) * Convert.ToDecimal(0.15), 2) : ((pc.CatName.Trim() == "Tequila" && DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Tuesday") ? Math.Round(Convert.ToDecimal(p.ProductOptionPrice.PriceStore) - Convert.ToDecimal(p.ProductOptionPrice.PriceStore) * Convert.ToDecimal(0.1), 2) : 0m))),
                                                             tax = ((pc.CatName == "Beer" || pc.CatName == "Micro-Beer" || pc.CatName == "Beer Keg") ? 0.105m : 0.125m),
                                                             Start = ((DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Wednesday" || DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Tuesday") ? "" : ((((Convert.ToDecimal(p.ProductOptionPrice.PriceRrp) != Convert.ToDecimal(p.ProductOptionPrice.PriceStore)) ? Convert.ToDecimal(p.ProductOptionPrice.PriceStore) : 0m) > 0m) ? DateTime.Now.ToString("MM/dd/yyyy") : "")),
                                                             End = ((DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Wednesday" || DateTime.UtcNow.AddHours(-5.0).ToString("dddd") == "Tuesday") ? "" : ((((Convert.ToDecimal(p.ProductOptionPrice.PriceRrp) != Convert.ToDecimal(p.ProductOptionPrice.PriceStore)) ? Convert.ToDecimal(p.ProductOptionPrice.PriceStore) : 0m) > 0m) ? "12/31/2999" : "")),
                                                             altupc1 = "",
                                                             altupc2 = "",
                                                             altupc3 = "",
                                                             altupc4 = "",
                                                             altupc5 = ""
                                                         }).ToList();
                        List<StoreFullNameModel> list4 = (from p in ProductList
                                                          join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                                                          join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                                                          where ProductList != null && ProductByCatList != null && ProductStockList != null && pc.CatName != "Cigarettes" && pc.CatName != "Tobacoo" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && p.ProductOption.Ref.Length > 5
                                                          select new StoreFullNameModel
                                                          {
                                                              upc = "#" + b.Barcode,
                                                              pname = p.ProductOption.Product.Name,
                                                              pdesc = p.ProductOption.Product.Name,
                                                              sku = "#" + p.ProductOption.Id,
                                                              Price = ((Convert.ToDecimal(p.ProductOptionPrice.PriceRrp) != Convert.ToDecimal(p.ProductOptionPrice.PriceStore)) ? Convert.ToDecimal(p.ProductOptionPrice.PriceRrp) : Convert.ToDecimal(p.ProductOptionPrice.PriceStore)),
                                                              pcat = pc.CatName,
                                                              pcat1 = "",
                                                              pcat2 = "",
                                                              uom = p.ProductOption.StyleString.Replace("Size: $20", "").Replace("Size: ", "").Replace("$", ""),
                                                              country = "USA",
                                                              region = "Arkansas"
                                                          }).ToList();
                        GenerateCSVModel.GenerateCSVFile(list3, "PRODUCT", storeid, BaseDirectory);
                        GenerateCSVModel.GenerateCSVFile(list4, "FULLNAME", storeid, BaseDirectory);
                        Console.WriteLine();
                        Console.WriteLine("Product FIle Generated For Cybertill " + storeid);
                        Console.WriteLine("Fullname FIle Generated For Cybertill " + storeid);
                        Console.WriteLine();
                    }
                    //else if (IrrespctioveOfStockAndTax.Contains(storeid.ToString()))
                    //{
                    //    List<StoreProductModel> list5 = (from p in ProductList
                    //                                     join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                    //                                     join s in ProductStockList on p.ProductOption.Id equals s.StkItemId into ss
                    //                                     from s in ss.DefaultIfEmpty()
                    //                                     join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                    //                                     where ProductList != null && ProductByCatList != null && ProductStockList != null && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && pc.CatName != "Tobacoo" && pc.CatName != "Cigarettes" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && p.ProductOption.Ref.Length > 5
                    //                                     select new StoreProductModel
                    //                                     {
                    //                                         StoreID = storeid,
                    //                                         upc = "#" + b.Barcode,
                    //                                         Qty = ((s != null && s.Stock != null) ? Convert.ToDecimal(s.Stock) : 0m),
                    //                                         sku = "#" + p.ProductOption.Id,
                    //                                         pack = 1,
                    //                                         StoreProductName = p.ProductOption.Product.Name,
                    //                                         StoreDescription = p.ProductOption.Product.Name,
                    //                                         Price = Convert.ToDecimal(p.ProductOptionPrice.PriceStore),
                    //                                         sprice = 0m,
                    //                                         tax = ((pc.CatName == "Soft Drinks" || pc.CatName == "Wine Accessories" || pc.CatName == "Snacks" || pc.CatName == "Grocery") ? MIxtax : tax),
                    //                                         Start = "",
                    //                                         End = "",
                    //                                         altupc1 = "",
                    //                                         altupc2 = "",
                    //                                         altupc3 = "",
                    //                                         altupc4 = "",
                    //                                         altupc5 = ""
                    //                                     }).ToList();
                    //    List<StoreFullNameModel> list6 = (from p in ProductList
                    //                                      join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                    //                                      join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                    //                                      where ProductList != null && ProductByCatList != null && ProductStockList != null && pc.CatName != "Cigarettes" && pc.CatName != "Tobacoo" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && p.ProductOption.Ref.Length > 5
                    //                                      select new StoreFullNameModel
                    //                                      {
                    //                                          upc = "#" + b.Barcode,
                    //                                          pname = p.ProductOption.Product.Name,
                    //                                          pdesc = p.ProductOption.Product.Name,
                    //                                          sku = "#" + p.ProductOption.Id,
                    //                                          Price = Convert.ToDecimal(p.ProductOptionPrice.PriceStore),
                    //                                          pcat = pc.CatName,
                    //                                          pcat1 = "",
                    //                                          pcat2 = "",
                    //                                          uom = p.ProductOption.StyleString.Replace("Size: $20", "").Replace("Size: ", "").Replace("$", ""),
                    //                                          country = "USA",
                    //                                          region = "Arkansas"
                    //                                      }).ToList();
                    //    GenerateCSVModel.GenerateCSVFile(list5, "PRODUCT", storeid, BaseDirectory);
                    //    GenerateCSVModel.GenerateCSVFile(list6, "FULLNAME", storeid, BaseDirectory);
                    //    Console.WriteLine();
                    //    Console.WriteLine("Product FIle Generated For Cybertill " + storeid);
                    //    Console.WriteLine("Fullname FIle Generated For Cybertill " + storeid);
                    //    Console.WriteLine();
                    //}
                    else if (upclength.Contains(storeid.ToString()))
                    {
                        List<StoreProductModel> list = (from p in ProductList
                                                        join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                                                        join s in ProductStockList on p.ProductOption.Id equals s.StkItemId
                                                        join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                                                        where Convert.ToDecimal(s.Stock) > 0m && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && pc.CatName != "Tobacoo" && pc.CatName != "Cigarettes" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && p.ProductOption.Ref.Length > 0
                                                        select new StoreProductModel
                                                        {
                                                            StoreID = storeid,
                                                            upc = "#" + b.Barcode,
                                                            Qty = Convert.ToDecimal(s.Stock),
                                                            sku = "#" + p.ProductOption.Id,
                                                            pack = 1,
                                                            StoreProductName = p.ProductOption.Product.Name,
                                                            StoreDescription = p.ProductOption.Product.Name,
                                                            Price = Convert.ToDecimal(p.ProductOptionPrice.PriceStore),
                                                            sprice = 0m,
                                                            tax = ((pc.CatName == "Beer" || pc.CatName == "Micro-Beer" || pc.CatName == "Beer Keg") ? 0.105m : 0.125m),
                                                            Start = "",
                                                            End = "",
                                                            altupc1 = "",
                                                            altupc2 = "",
                                                            altupc3 = "",
                                                            altupc4 = "",
                                                            altupc5 = ""
                                                        }).ToList();
                        List<StoreFullNameModel> list2 = (from p in ProductList
                                                          join pc in ProductByCatList on p.ProductOption.Product.Id equals pc.Id
                                                          join s in ProductStockList on p.ProductOption.Id equals s.StkItemId
                                                          join b in barcodeList on p.ProductOption.Id equals b.StkItemId
                                                          where Convert.ToDecimal(s.Stock) > 0m && ProductList != null && ProductByCatList != null && ProductStockList != null && pc.CatName != "Cigarettes" && pc.CatName != "Tobacoo" && pc.CatName != "Cigars/Humidor" && pc.CatName != "Cigars/Commercial" && pc.CatName != "Cigarette/Tobacco" && Convert.ToDecimal(p.ProductOptionPrice.PriceStore) > 0m && p.ProductOption.Ref.Length > 5
                                                          select new StoreFullNameModel
                                                          {
                                                              upc = "#" + b.Barcode,
                                                              pname = p.ProductOption.Product.Name,
                                                              pdesc = p.ProductOption.Product.Name,
                                                              sku = "#" + p.ProductOption.Id,
                                                              Price = Convert.ToDecimal(p.ProductOptionPrice.PriceStore),
                                                              pcat = pc.CatName,
                                                              pcat1 = "",
                                                              pcat2 = "",
                                                              uom = p.ProductOption.StyleString.Replace("Size: $20", "").Replace("Size: ", "").Replace("$", ""),
                                                              country = "USA",
                                                              region = "Arkansas"
                                                          }).ToList();
                        GenerateCSVModel.GenerateCSVFile(list, "PRODUCT", storeid, BaseDirectory);
                        GenerateCSVModel.GenerateCSVFile(list2, "FULLNAME", storeid, BaseDirectory);
                        Console.WriteLine();
                        Console.WriteLine("Product FIle Generated For Cybertill " + storeid);
                        Console.WriteLine("Fullname FIle Generated For Cybertill " + storeid);
                        Console.WriteLine();
                    }
                }
			}
			catch (Exception ex)
			{
				new Email.clsEmail().sendEmail(developerid, "", "", "Error in CybertillPOS@" + DateTime.UtcNow.ToString() + " GMT", ex.Message + "<br/>" + ex.StackTrace);
				Console.WriteLine(ex.Message);
				Console.Read();
			}
		}

		private static string GetAuthId()
		{
			string to = ConfigurationManager.AppSettings["DeveloperId"];
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
			httpWebRequest.Headers.Add("SOAP:Action");
			httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
			httpWebRequest.Accept = "text/xml";
			httpWebRequest.Method = "POST";
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    <authenticate_get xmlns=\"http://cybertill.co.uk/wsdl/CybertillApi_v1_6/\">\r\n                            <url>" + storeUrl + "</url>\r\n                            <id>" + Id + "</id>\r\n                    </authenticate_get>\r\n                  </soap:Body>\r\n                </soap:Envelope>");
			using (Stream outStream = httpWebRequest.GetRequestStream())
			{
				xmlDocument.Save(outStream);
			}
			string result = "";
			using (WebResponse webResponse = httpWebRequest.GetResponse()) 
			{
				using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
                {
					try
					{
						string xml = streamReader.ReadToEnd();
						XmlDocument xmlDocument2 = new XmlDocument();
						xmlDocument2.LoadXml(xml);
						XmlNode firstChild = xmlDocument2.DocumentElement.FirstChild;
						result = firstChild.InnerText;
					}
					catch (Exception ex)
					{
						new Email.clsEmail().sendEmail(to, "", "", "Error in CybertillPOS@" + DateTime.UtcNow.ToString() + " GMT", ex.Message + "<br/>" + ex.StackTrace);
						Console.WriteLine(ex.Message);
						Console.Read();
					}
				}
			}
			return result;
		}
	}
}
