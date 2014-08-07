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

            [DisplayName("买家昵称")]
            public string BuyerNick { get; set; }

            [DisplayName("收获人姓名")]
            public string ReceiverName { get; set; }

            [DisplayName("查询退货"), DefaultValue(false)]
            public bool SearchRefund { get; set; }
        }

        Dictionary<string, string> StatusNameMap = new Dictionary<string, string>()
        {
            { "TRADE_NO_CREATE_PAY", "没有创建支付宝交易" },
            { "WAIT_BUYER_PAY", "等待买家付款" },
            { "WAIT_SELLER_SEND_GOODS", "等待卖家发货,即:买家已付款" },
            { "WAIT_BUYER_CONFIRM_GOODS", "等待买家确认收货,即:卖家已发货" },
            { "TRADE_BUYER_SIGNED", "买家已签收,货到付款专用" },
            { "TRADE_FINISHED", "交易成功" },
            { "TRADE_CLOSED", "交易关闭" },
            { "TRADE_CLOSED_BY_TAOBAO", "付款以前，卖家或买家主动关闭交易" }
        };

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

            if (searchData.SearchRefund)
            {
                var refunds = TopClientHelper.GetRefund(searchData.Begin, searchData.End);
                foreach (var refund in refunds)
                {
                    if (refund.Status == "CLOSED")
                        continue;

                    var trade = TopClientHelper.GetTradeInfo(refund.Tid, "status");
                    var status = "超时";
                    if (trade != null)
                        StatusNameMap.TryGetValue(trade.Status, out status);

                    var lvItem = listView1.Items.Add(refund.Created);
                    lvItem.SubItems.Add(refund.BuyerNick.ToString());
                    lvItem.SubItems.Add(refund.RefundFee);
                    lvItem.SubItems.Add(status);
                    lvItem.Tag = refund.Tid;
                }
            }
            else
            {
                var shippings = TopClientHelper.GetLogisticsOrders(searchData.Begin, searchData.End);
                if (shippings == null)
                    return;

                int founded = 0;
                foreach (Shipping shipping in shippings)
                {
                    if (string.IsNullOrEmpty(shipping.OutSid))
                        continue;

                    if ((!string.IsNullOrEmpty(searchData.OutSid) && shipping.OutSid.Contains(searchData.OutSid)) ||
                        (!string.IsNullOrEmpty(searchData.BuyerNick) && shipping.BuyerNick.Contains(searchData.BuyerNick)) ||
                        (!string.IsNullOrEmpty(searchData.ReceiverName) && shipping.ReceiverName.Contains(searchData.ReceiverName)))
                    {
                        var lvItem = listView1.Items.Add(shipping.OutSid);
                        lvItem.SubItems.Add(shipping.Tid.ToString());
                        lvItem.Tag = shipping.Tid;
                        founded++;
                    }
                }
                string text = string.Format("搜索了【{0}】条物流信息，找到了对应的【{1}】条！", shippings.Count, founded);
                MessageBox.Show(text);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            long tid = (long)listView1.SelectedItems[0].Tag;
            Process.Start(
                String.Format("http://trade.taobao.com/trade/detail/trade_item_detail.htm?bizOrderId={0}", tid));
        }
    }
}
