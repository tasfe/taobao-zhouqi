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
                ListViewItem lvItem = new ListViewItem(siteData.ID);
                lvItem.SubItems.Add(siteData.Name);
                lvItem.SubItems.Add(siteData.City);
                listView1.Items.Add(lvItem);
            }
        }
    }
}
