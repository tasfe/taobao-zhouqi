using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YundaWangdian.Data
{
    public class CityData
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        public List<SiteData> Sites { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
