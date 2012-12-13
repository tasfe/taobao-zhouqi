using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YundaWangdian.Data
{
    public class ProvinceData
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        public List<CityData> Citys { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
