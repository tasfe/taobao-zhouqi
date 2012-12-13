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

        [XmlAttribute("psfw")]
        public string psfw { get; set; }

        [XmlAttribute("bz")]
        public string bz { get; set; }

        [XmlAttribute("dz")]
        public string dz { get; set; }

        [XmlAttribute("bpsfw")]
        public string bpsfw { get; set; }

        [XmlAttribute("jj")]
        public string jj { get; set; }

        [XmlAttribute("mc")]
        public string mc { get; set; }

        [XmlAttribute("dh")]
        public string dh { get; set; }

        [XmlAttribute("sjgs")]
        public string sjgs { get; set; }

        [XmlAttribute("szd")]
        public string szd { get; set; }

        [XmlAttribute("bm")]
        public string bm { get; set; }

        [XmlAttribute("fzr")]
        public string fzr { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
