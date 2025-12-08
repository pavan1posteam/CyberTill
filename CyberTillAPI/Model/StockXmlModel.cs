using System.Collections.Generic;
using System.Xml.Serialization;

namespace CyberTillAPI.Model
{
    public class StockXmlModel
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
            [XmlElement(ElementName = "stkItemId")]
            public string StkItemId { get; set; }

            [XmlElement(ElementName = "locationId")]
            public string LocationId { get; set; }

            [XmlElement(ElementName = "stock")]
            public string Stock { get; set; }

            [XmlElement(ElementName = "Reserved")]
            public string Reserved { get; set; }

            [XmlElement(ElementName = "pop")]
            public string Pop { get; set; }

            [XmlElement(ElementName = "ibt")]
            public string Ibt { get; set; }
        }
    }
}
