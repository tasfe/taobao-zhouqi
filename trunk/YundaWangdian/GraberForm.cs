using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YundaWangdian.Data;
using mshtml;
using System.Text.RegularExpressions;
using LitJson;
using System.Threading;

namespace YundaWangdian
{
    public partial class GraberForm : Form
    {
        CountryData mCountryData;
        const string BaseUrl = "http://www.yundaex.com/www/fuwuwangdian.html";
        const string ProvinceUrl = "http://www.yundaex.com/www/fuwuwangdian_list.php?id=";
        const string SitesUrl = "http://www.yundaex.com/www/fuwuwangdian_data.php?id=";
        const string SearchUrl = "http://www.yundaex.com/www/fuwuwangdian_search.php";
        int[] mParseStages = new int[] { 0, 0 };
        List<SiteData> mCheckSites = new List<SiteData>();

        public GraberForm(CountryData data)
        {
            mCountryData = data;

            InitializeComponent();
        }
        

        private void GraberForm_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigated += new WebBrowserNavigatedEventHandler(webBrowser1_Navigated);
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            webBrowser1.Url = new Uri(BaseUrl);
        }

        void ShowLogMessage(string msg)
        {
            listBox1.Items.Add(msg);
        }

        void InjectAlertBlocker()
        {
            HtmlElement head = webBrowser1.Document.GetElementsByTagName("head")[0];
            HtmlElement scriptEl = webBrowser1.Document.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
            string alertBlocker = "window.alert = function () { }";
            element.text = alertBlocker;
            head.AppendChild(scriptEl);
        }

        void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            InjectAlertBlocker();
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (mParseStages[0] == 0)
            {
                ParseProvinceID(webBrowser1.DocumentText);

                mParseStages[0] = 1;
                mParseStages[1] = 0;
                progressBar1.Maximum = mCountryData.Provinces.Count;
                if (mCountryData.Provinces.Count > 0)
                    webBrowser1.Url = new Uri(ProvinceUrl + mCountryData.Provinces[mParseStages[1]].ID);
            }
            else if (mParseStages[0] == 1)
            {
                if (ParseProvinceIndex(webBrowser1.DocumentText, mCountryData.Provinces[mParseStages[1]]))
                {
                    mParseStages[1]++;
                    progressBar1.Value = mParseStages[1];
                    if (mParseStages[1] < mCountryData.Provinces.Count)
                        webBrowser1.Url = new Uri(ProvinceUrl + mCountryData.Provinces[mParseStages[1]].ID);
                    else
                    {
                        foreach (ProvinceData provinceData in mCountryData.Provinces)
                        {
                            foreach (CityData cityData in provinceData.Citys)
                            {
                                foreach (SiteData siteData in cityData.Sites)
                                    mCheckSites.Add(siteData);
                            }
                        }
                        mParseStages[0] = 2;
                        mParseStages[1] = 0;
                        progressBar1.Maximum = mCheckSites.Count;
                        if (mCheckSites.Count > 0)
                            webBrowser1.Url = new Uri(SitesUrl + mCheckSites[mParseStages[1]].ID);
                    }
                }
            }
            else if (mParseStages[0] == 2)
            {
                try
                {
                    ParseSiteDetails(webBrowser1.DocumentText, mCheckSites[mParseStages[1]]);
                }
                catch (Exception ex)
                {
                    ShowLogMessage(ex.Message);
                }

                mParseStages[1]++;
                progressBar1.Value = mParseStages[1];
                if (mParseStages[1] < mCheckSites.Count)
                    webBrowser1.Url = new Uri(SitesUrl + mCheckSites[mParseStages[1]].ID);
                else
                    mParseStages[0] = 3;

                Thread.Sleep(100);
            }
        }

        void ParseProvinceID(string html)
        {
            mCountryData.Provinces = new List<ProvinceData>();

            MatchCollection values = Regex.Matches(html, @"id=\d{3,}\W>[\u4e00-\u9fa5]{2,}<");
            string[] splits = new string[] { "id=", "\">", "<" };
            foreach (Match match in values)
            {
                string[] idValue = match.Value.Split(splits, StringSplitOptions.RemoveEmptyEntries);
                if (idValue.Length != 2)
                    continue;

                //ShowLogMessage("ParseProvinceID: " + idValue[0] + "=" + idValue[1]);

                ProvinceData provinceData = new ProvinceData();
                provinceData.ID = idValue[0];
                provinceData.Name = idValue[1];
                mCountryData.Provinces.Add(provinceData);
            }
        }

        public class SiteJsonInfo
        {
            public string mc = null;
            public string bm = null;
            public string shi = null;
            public string sheng = null;
            public string city = null;
        }

        public class PagesInfo
        {
            public string currPage;
            public string perPageSize;
            public string totals;
        }

        public class TotalInfo
        {
            public List<SiteJsonInfo> datas;
            public PagesInfo page;
        }

        bool ParseProvinceIndex(string html, ProvinceData provinceData)
        {
            string key = "var yd_shi=";
            int start = html.IndexOf(key);
            if (start < 0)
                return false;

            int end = html.IndexOf("}}", start);
            if (end < 0)
                return false;

            string text = html.Substring(start + key.Length, end - start - key.Length + 2);
            if (string.IsNullOrEmpty(text))
                return true;

            if (provinceData.Citys == null)
                provinceData.Citys = new List<CityData>();

            TotalInfo totalInfo = JsonMapper.ToObject<TotalInfo>(text);
            foreach (SiteJsonInfo siteInfo in totalInfo.datas)
            {
                CityData cityData = provinceData.Citys.Find(delegate(CityData city) { return city.ID == siteInfo.shi; });
                if (cityData == null)
                {
                    int idx = siteInfo.city.LastIndexOf(',') + 1;

                    cityData = new CityData();
                    cityData.ID = siteInfo.shi;
                    cityData.Name = siteInfo.city.Substring(idx);
                    cityData.Sites = new List<SiteData>();
                    provinceData.Citys.Add(cityData);
                }

                //ShowLogMessage("ParseSiteJsonInfo: " + siteInfo.bm + "=" + siteInfo.mc);

                SiteData siteData = new SiteData();
                siteData.Name = siteInfo.mc;
                siteData.ID = siteInfo.bm;
                siteData.City = siteInfo.city;
                cityData.Sites.Add(siteData);
            }
        
            //http://www.yundaex.com/www/fuwuwangdian_search.php?cmd=search&sheng=086020&city=&keywords=&page=14
            int currPage = int.Parse(totalInfo.page.currPage);
            int perPageSize = int.Parse(totalInfo.page.perPageSize);
            int totals = int.Parse(totalInfo.page.totals);
            if (currPage * perPageSize < totals)
            {
                string url = string.Format("{0}?cmd=search&sheng={1}&city=&keywords=&page={2}", SearchUrl, provinceData.ID, currPage + 1);
                webBrowser1.Url = new Uri(url);
                return false;
            }

            return true;
        }

        public class SiteInfo
        {
            public string psfw;
            public string bz;
            public string dz;
            public string bpsfw;
            public string jj;
            public string city;
            public string mc;
            public string dh;
            public string sjgs;
            public string szd;
            public string bm;
            public string fzr;
            public string shi;
            public string sheng;
        }

        bool ParseSiteDetails(string html, SiteData siteData)
        {
            string key = "var yd_shi=";
            int start = html.IndexOf(key);
            if (start < 0)
                return false;

            int end = html.IndexOf("}", start);
            if (end < 0)
                return false;

            string text = html.Substring(start + key.Length, end - start - key.Length + 1);
            if (string.IsNullOrEmpty(text))
                return true;

            SiteInfo siteInfo = JsonMapper.ToObject<SiteInfo>(text);
            siteData.psfw = siteInfo.psfw;
            siteData.bz = siteInfo.bz;
            siteData.dz = siteInfo.dz;
            siteData.bpsfw = siteInfo.bpsfw;
            siteData.jj = siteInfo.jj;
            siteData.mc = siteInfo.mc;
            siteData.dh = siteInfo.dh;
            siteData.sjgs = siteInfo.sjgs;
            siteData.szd = siteInfo.szd;
            siteData.bm = siteInfo.bm;
            siteData.fzr = siteInfo.fzr;

            return true;
        }
    }
}
