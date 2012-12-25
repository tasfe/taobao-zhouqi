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

        public void Search(List<SiteData> siteDatas, string pattern)
        {
            foreach (CityData data in Citys)
                data.Search(siteDatas, pattern);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
