using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CyberTillAPI.Model
{
    public class BarcodeXmlModel
    {
        [XmlRoot(ElementName = "item")]
        public class Item
        {
            [XmlElement(ElementName = "stkItemId")]
            public string StkItemId { get; set; }
            [XmlElement(ElementName = "barcode")]
            public string Barcode { get; set; }
            [XmlElement(ElementName = "isPrimary")]
            public string IsPrimary { get; set; }
        }

        [XmlRoot(ElementName = "result")]
        public class Result
        {
            [XmlElement(ElementName = "item")]
            public List<Item> Item { get; set; }
        }
    }
}
