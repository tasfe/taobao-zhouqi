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
        int mTotalRequest = 0;
        int mFinishedRequest = 0;
        bool mCheckSiteDetail = true;

        public GraberForm(CountryData data)
        {
            mCountryData = data;

            InitializeComponent();
        }

        private void GraberForm_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mCheckSiteDetail = true;
            if (MessageBox.Show("点击确定继续", "确定对全部网点进行数据抓取么？", MessageBoxButtons.OKCancel) == DialogResult.OK)
                ProcessThreadWork(mCountryData);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string message = "";
            List<SiteData> siteDatas = new List<SiteData>();
            foreach (ProvinceData pData in mCountryData.Provinces)
            {
                foreach (CityData cData in pData.Citys)
                {
                    foreach (SiteData sData in cData.Sites)
                    {
                        if (sData.psfw != null && sData.bpsfw != null)
                            continue;

                        if (siteDatas.Count < 20)
                            message += sData.Name + "\n";
                        siteDatas.Add(sData);
                    }
                }
            }

            if (siteDatas.Count == 0)
            {
                MessageBox.Show("恭喜，所有的网点信息未发现失败的");
                return;
            }

            if (MessageBox.Show(message, string.Format("发现{0}个需要更新的网点，是否立即更新？", siteDatas.Count), MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                mCheckSiteDetail = true;
                foreach (SiteData sData in siteDatas)
                    ProcessThreadWork(sData);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            mCheckSiteDetail = false;
        }

        public string GetUrltoHtml(string Url)
        {
            try
            {
                System.Net.WebRequest wReq = System.Net.WebRequest.Create(Url);
                System.Net.WebResponse wResp = wReq.GetResponse();
                System.IO.Stream respStream = wResp.GetResponseStream();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (System.Exception ex)
            {
                ShowLogMessage(ex.Message);
            }
            return "";
        }

        void ThreadProc(Object stateInfo)
        {
            if (stateInfo is CountryData)
            {
                ParseProvinceID(GetUrltoHtml(BaseUrl), stateInfo as CountryData);
                foreach (ProvinceData provinceData in mCountryData.Provinces)
                    ProcessThreadWork(provinceData);
            }
            else if (stateInfo is ProvinceData)
            {
                ProvinceData provinceData = stateInfo as ProvinceData;
                string Url = ProvinceUrl + provinceData.ID;
                ParseProvinceIndex(GetUrltoHtml(Url), provinceData);
            }
            else if (stateInfo is SubPageRequest)
            {
                SubPageRequest request = stateInfo as SubPageRequest;
                ParseProvinceIndex(GetUrltoHtml(request.url), request.provinceData);
            }
            else if (stateInfo is SiteData)
            {
                SiteData siteData = stateInfo as SiteData;
                string Url = SitesUrl + siteData.ID;
                ParseSiteDetails(GetUrltoHtml(Url), siteData);
            }

            mFinishedRequest++;
            Invoke(new BeginCallBack(UpdateUIQueue), null);
        }

        delegate void BeginCallBack();
        void ProcessThreadWork(object stateInfo)
        {
            mTotalRequest++;

            Invoke(new BeginCallBack(UpdateUIQueue), null);

            ThreadPool.QueueUserWorkItem(ThreadProc, stateInfo);
        }

        void UpdateUIQueue()
        {
            progressBar1.Maximum = mTotalRequest;
            progressBar1.Value = mFinishedRequest;
            labelInfo.Text = string.Format("[{0}/{1}]", mFinishedRequest, mTotalRequest);
        }

        delegate void ShowLogMessageCallBack(string msg);
        void ShowLogMessage(string msg)
        {
            Invoke(new ShowLogMessageCallBack(ShowLogMessageThreadSafe), new object[] { msg });
        }
        void ShowLogMessageThreadSafe(string msg)
        {
            listBox1.Items.Add(msg);
        }

        void ParseProvinceID(string html, CountryData countryData)
        {
            countryData.Provinces = new List<ProvinceData>();

            MatchCollection values = Regex.Matches(html, @"id=\d{3,}\W>[\u4e00-\u9fa5]{2,}<");
            string[] splits = new string[] { "id=", "\">", "<" };
            foreach (Match match in values)
            {
                string[] idValue = match.Value.Split(splits, StringSplitOptions.RemoveEmptyEntries);
                if (idValue.Length != 2)
                    continue;

                ProvinceData provinceData = new ProvinceData();
                provinceData.ID = idValue[0];
                provinceData.Name = idValue[1];
                countryData.Provinces.Add(provinceData);
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

        public class SubPageRequest
        {
            public string url;
            public ProvinceData provinceData;
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

                SiteData siteData = new SiteData();
                siteData.Name = siteInfo.mc;
                siteData.ID = siteInfo.bm;
                siteData.City = siteInfo.city;
                cityData.Sites.Add(siteData);

                if (mCheckSiteDetail)
                    ProcessThreadWork(siteData);
            }
        
            //http://www.yundaex.com/www/fuwuwangdian_search.php?cmd=search&sheng=086020&city=&keywords=&page=14
            int currPage = int.Parse(totalInfo.page.currPage);
            int perPageSize = int.Parse(totalInfo.page.perPageSize);
            int totals = int.Parse(totalInfo.page.totals);
            if (currPage * perPageSize < totals)
            {
                string url = string.Format("{0}?cmd=search&sheng={1}&city=&keywords=&page={2}", SearchUrl, provinceData.ID, currPage + 1);
                SubPageRequest request = new SubPageRequest();
                request.url = url;
                request.provinceData = provinceData;
                ProcessThreadWork(request);
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

            int end = html.IndexOf("}</script>", start);
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
