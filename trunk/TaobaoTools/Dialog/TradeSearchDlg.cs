using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Top.Api.Domain;
using System.Diagnostics;

namespace TaobaoTools.Dialog
{
    public partial class TradeSearchDlg : UserControl
    {
        public class SearchData
        {
            DateTime mBegin = DateTime.Now - new TimeSpan(3, 0, 0, 0);
            [DisplayName("开始时间")]
            public DateTime Begin { get { return mBegin; } set { mBegin = value; } }

            DateTime mEnd = DateTime.Now;
            [DisplayName("结束时间")]
            public DateTime End { get { return mEnd; } set { mEnd = value; } }

            [DisplayName("运单号码")]
            public string OutSid { get; set; }
        }

        public TradeSearchDlg()
        {
            InitializeComponent();
        }

        private void TradeSearchDlg_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = new SearchData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            SearchData searchData = propertyGrid1.SelectedObject as SearchData;
            List<Shipping> shippings = TopClientHelper.GetLogisticsOrders(searchData.Begin, searchData.End);
            if (shippings == null)
                return;

            int founded = 0;
            foreach (Shipping shipping in shippings)
            {
                if (string.IsNullOrEmpty(shipping.OutSid))
                    continue;

                if (shipping.OutSid.Contains(searchData.OutSid))
                {
                    ListViewItem lvItem = listView1.Items.Add(shipping.OutSid);
                    lvItem.SubItems.Add(shipping.Tid.ToString());
                    founded++;
                }
            }

            string text = string.Format("搜索了【{0}】条物流信息，找到了对应的【{1}】条！", shippings.Count, founded);
            MessageBox.Show(text);
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            long tid = long.Parse(listView1.SelectedItems[0].SubItems[1].Text);
            Process.Start(
                String.Format("http://trade.taobao.com/trade/detail/trade_item_detail.htm?bizOrderId={0}", tid));
        }
    }
}
