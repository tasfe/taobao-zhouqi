using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YundaWangdian.Data
{
    public class SiteData
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("City")]
        public string City { get; set; }
    }
}
