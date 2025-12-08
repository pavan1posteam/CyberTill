using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CyberTillAPI.Model
{
    public class ProductByCategoryXmlModel
    {
        [XmlRoot(ElementName = "result")]
        public class Result
        {
            [XmlElement(ElementName = "item")]
            public List<Item> Item { get; set; }
        }

        [XmlRoot(ElementName = "item")]
        public class Item
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
            [XmlElement(ElementName = "catId")]
            public string CatId { get; set; }
            [XmlElement(ElementName = "CatName")]
            public string CatName { get; set; }
        }
    }
}
