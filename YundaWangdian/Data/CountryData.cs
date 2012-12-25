using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YundaWangdian.Data
{
    public class CountryData
    {
        public List<ProvinceData> Provinces { get; set; }

        public void Search(List<SiteData> siteDatas, string pattern)
        {
            foreach (ProvinceData data in Provinces)
                data.Search(siteDatas, pattern);
        }
    }
}
