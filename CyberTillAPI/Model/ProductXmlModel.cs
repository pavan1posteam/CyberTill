using System.Collections.Generic;
using System.Xml.Serialization;

namespace CyberTillAPI.Model
{
    public class ProductXmlModel
    {
        [XmlRoot(ElementName = "result")]
        public class Result
        {
            [XmlElement(ElementName = "item")]
            public List<Item> Item { get; set; }
        }
        [XmlRoot(ElementName = "product")]
        public class Product
        {
            [XmlElement(ElementName = "id")]
            public string Id { get; set; }
            [XmlElement(ElementName = "name")]
            public string Name { get; set; }
            [XmlElement(ElementName = "descrShort")]
            public string DescrShort { get; set; }
            [XmlElement(ElementName = "descrLong")]
            public string DescrLong { get; set; }
            [XmlElement(ElementName = "availableToSell")]
            public string AvailableToSell { get; set; }
            [XmlElement(ElementName = "deliverable")]
            public string Deliverable { get; set; }
            [XmlElement(ElementName = "itemWeight")]
            public string ItemWeight { get; set; }
            [XmlElement(ElementName = "typeLkp")]
            public string TypeLkp { get; set; }
            [XmlElement(ElementName = "stkBrandId")]
            public string StkBrandId { get; set; }
            [XmlElement(ElementName = "dataVatId")]
            public string DataVatId { get; set; }
        }

        [XmlRoot(ElementName = "productOption")]
        public class ProductOption
        {
            [XmlElement(ElementName = "id")]
            public string Id { get; set; }
            [XmlElement(ElementName = "name")]
            public string Name { get; set; }
            [XmlElement(ElementName = "ref")]
            public string Ref { get; set; }
            [XmlElement(ElementName = "styleString")]
            public string StyleString { get; set; }
            [XmlElement(ElementName = "availableToSell")]
            public string AvailableToSell { get; set; }
            [XmlElement(ElementName = "discontinued")]
            public string Discontinued { get; set; }
            [XmlElement(ElementName = "webVisible")]
            public string WebVisible { get; set; }
            [XmlElement(ElementName = "manNumber")]
            public string ManNumber { get; set; }
            [XmlElement(ElementName = "dataVatId")]
            public string DataVatId { get; set; }
            [XmlElement(ElementName = "product")]
            public Product Product { get; set; }
        }

        [XmlRoot(ElementName = "productOptionPrice")]
        public class ProductOptionPrice
        {
            [XmlElement(ElementName = "priceRrp")]
            public string PriceRrp { get; set; }
            [XmlElement(ElementName = "priceStore")]
            public string PriceStore { get; set; }
            [XmlElement(ElementName = "priceWeb")]
            public string PriceWeb { get; set; }
            [XmlElement(ElementName = "priceStaff")]
            public string PriceStaff { get; set; }
            [XmlElement(ElementName = "priceTrade")]
            public string PriceTrade { get; set; }
        }

        [XmlRoot(ElementName = "item")]
        public class Item
        {
            [XmlElement(ElementName = "productOption")]
            public ProductOption ProductOption { get; set; }
            [XmlElement(ElementName = "productOptionPrice")]
            public ProductOptionPrice ProductOptionPrice { get; set; }
        }
    }
}
