using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using YundaWangdian.Data;
using System.Xml.Serialization;
using System.IO;

namespace YundaWangdian
{
    public partial class MainForm : Form
    {
        CountryData mCountryData = new CountryData();
        string mDataFile = Application.StartupPath + "\\db.xml";
        string mTemplateFile = Application.StartupPath + "\\template.html";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadData(mDataFile);
            FillData();
        }

        public void LoadData(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            StreamReader sr = new StreamReader(fileName);
            XmlSerializer ser = new XmlSerializer(typeof(CountryData));
            mCountryData = (CountryData)ser.Deserialize(sr);
            sr.Close();
        }

        public void SaveData(string fileName)
        {
            StreamWriter sw = new StreamWriter(fileName);
            XmlSerializer ser = new XmlSerializer(typeof(CountryData));
            ser.Serialize(sw, mCountryData);
            sw.Close();
        }

        private void OnParseWebClicked(object sender, EventArgs e)
        {
            GraberForm graberForm = new GraberForm(mCountryData);
            graberForm.ShowDialog();
            SaveData(mDataFile);
            FillData();
        }

        void FillData()
        {
            comboBox1.Items.Clear();

            if (mCountryData.Provinces == null)
                return;

            foreach (ProvinceData provinceData in mCountryData.Provinces)
                comboBox1.Items.Add(provinceData.Name);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            foreach (ProvinceData provinceData in mCountryData.Provinces)
            {
                if (comboBox1.Text == provinceData.Name)
                {
                    comboBox2.Tag = provinceData;
                    foreach (CityData cityData in provinceData.Citys)
                        comboBox2.Items.Add(cityData);
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            if (comboBox2.SelectedItem == null)
                return;

            CityData cityData = comboBox2.SelectedItem as CityData;
            foreach (SiteData siteData in cityData.Sites)
            {
                ListViewItem lvItem = new ListViewItem((listView1.Items.Count + 1).ToString());
                lvItem.Tag = siteData;

                lvItem.SubItems.Add(siteData.ID);
                lvItem.SubItems.Add(siteData.Name);
                lvItem.SubItems.Add(siteData.City);
                listView1.Items.Add(lvItem);
            }
        }

        string BuildHtml(SiteData siteData)
        {
            StringBuilder sb = new StringBuilder();
            StreamReader sr = new StreamReader(mTemplateFile);
            sb.Append(sr.ReadToEnd());
            sr.Close();

            sb.Replace("company_bm", siteData.bm);
            sb.Replace("company_mc", siteData.mc);
            sb.Replace("company_city", siteData.City);
            sb.Replace("company_dh", siteData.dh);
            sb.Replace("company_fzr", siteData.fzr);
            sb.Replace("company_address", siteData.dz);
            sb.Replace("company_psfw", siteData.psfw);
            sb.Replace("company_bpsfw", siteData.bpsfw);

            return sb.ToString();
        }

        private void OnSiteInfoClicked(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;

            SiteData siteData = listView1.SelectedItems[0].Tag as SiteData;
            if (siteData == null)
                return;

            webBrowser1.DocumentText = BuildHtml(siteData);
        }

        private void OnSearchClicked(object sender, EventArgs e)
        {
            string pattern = textBox1.Text;
            if (string.IsNullOrEmpty(pattern))
            {
                comboBox2_SelectedIndexChanged(null, null);
                return;
            }

            List<ListViewItem> filterItem = new List<ListViewItem>();
            foreach (ListViewItem lvItem in listView1.Items)
            {
                SiteData siteData = lvItem.Tag as SiteData;
                if (siteData == null || siteData.psfw == null || siteData.bpsfw == null)
                    continue;

                if (siteData.Name.Contains(pattern) || 
                    siteData.psfw.Contains(pattern) || 
                    siteData.bpsfw.Contains(pattern))
                    filterItem.Add(lvItem);
            }

            listView1.Items.Clear();
            foreach (ListViewItem lvItem in filterItem)
                listView1.Items.Add(lvItem);
        }
    }
}
