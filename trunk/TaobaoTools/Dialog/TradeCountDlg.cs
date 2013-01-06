using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Top.Api.Domain;
using TaobaoTools.Data;
using TaobaoTools.Properties;

namespace TaobaoTools.Dialog
{
    public partial class TradeCountDlg : Form
    {
        DateTime mBegin;
        DateTime mEnd;
        int mOrderNum = 0;
        long mItemNum = 0;
        float mTotalPrice = 0;
        float mTotalIncome = 0;
        List<Trade> mTradeList;
        Dictionary<long, long> mItemCountMap = new Dictionary<long, long>();

        public TradeCountDlg(DateTime begin, DateTime end, List<Trade> tradeList)
        {
            InitializeComponent();

            mBegin = begin;
            mEnd = end;
            mTradeList = tradeList;

            foreach (Trade trade in mTradeList)
                Count(trade);

            foreach (KeyValuePair<long, long> pair in mItemCountMap)
            {
                ItemData itemData = Global.ItemDataContainer.GetItem(pair.Key);
                if (itemData == null)
                    continue;

                mTotalPrice += itemData.InternalPrice * pair.Value;
            }
        }

        void Count(Trade trade)
        {
            float payment = 0;
            if (float.TryParse(trade.Payment, out payment))
                mTotalIncome += payment;

            foreach (Order order in trade.Orders)
            {
                if (!String.IsNullOrEmpty(order.Status) && order.Status == "TRADE_CLOSED_BY_TAOBAO")
                    continue;
                Count(order);
            }
        }

        void Count(Order order)
        {
            long number = 0;
            mItemCountMap.TryGetValue(order.NumIid, out number);
            mItemCountMap[order.NumIid] = number + order.Num;

            mItemNum += order.Num;
            mOrderNum++;
        }

        private void TradeCountDlg_Load(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            BuildHtml(sb);

            webBrowser1.DocumentText = sb.ToString();
        }

        void BuildHead(StringBuilder sb, string key, string value)
        {
            sb.AppendLine("<tr>");
            sb.AppendFormat("<td width='25%'>{0}:</td>\n", key);
            sb.AppendFormat("<td>{0}</td>\n", value);
            sb.AppendLine("<tr>");
        }

        void BuildLine(StringBuilder sb, string s0, string s1, string s2, string s3, string s4, string s5)
        {
            sb.AppendLine("<tr>");
            sb.AppendFormat("<td width='10%'>({0})</td>\n", s0);
            sb.AppendFormat("<td width='45%'>{0}</td>\n", s1);
            sb.AppendFormat("<td width='10%'>{0}</td>\n", s2);
            sb.AppendFormat("<td width='15%'>{0}</td>\n", s3);
            sb.AppendFormat("<td width='10%'><strong>{0}</strong></td>\n", s4);
            sb.AppendFormat("<td>{0}</td>\n", s5);
            sb.AppendLine("</tr>");
        }

        void BuildHtml(StringBuilder sb)
        {
            sb.AppendLine("<!DOCTYPE HTML PUBLIC '-//W3C//DTD HTML 4.0 Transitional//EN'>");
            sb.AppendLine("<html xmlns:v='http://www.taobao.com/'>");
            sb.AppendFormat("<style type='text/css'><--body{{FONT-SIZE:{0}pt}} td{{FONT-SIZE:{1}pt}}--></style>\n", Settings.Default.OrderFontSize, Settings.Default.OrderFontSize);
            sb.AppendLine("<head><style type='text/css'>v\\:* {behavior:url(#default#VML);}</style> </head><body>");

            sb.AppendLine("<table width='640px' align='center'>");
            BuildHead(sb, "起始时间", mBegin.ToString());
            BuildHead(sb, "结束时间", mEnd.ToString());
            BuildHead(sb, "订单总数", mOrderNum.ToString() + "单");
            BuildHead(sb, "宝贝总数", mItemNum.ToString() + "件");
            BuildHead(sb, "进货成本", mTotalPrice.ToString() + "元");
            BuildHead(sb, "总营业额", mTotalIncome.ToString() + "元");
            sb.AppendLine("</table>");

            sb.AppendLine("<table width='640px' align='center' border='1px' bordercolor='#000000' cellspacing='0px' style='border-collapse:collapse'>");
            BuildLine(sb, "编号", "名称", "进价", "总进价", "数量", "单位");

            int orderIndex = 1;
            List<KeyValuePair<long, long>> myList = new List<KeyValuePair<long, long>>(mItemCountMap);
            myList.Sort(
                delegate(KeyValuePair<long, long> firstPair,
                KeyValuePair<long, long> nextPair)
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                }
            );
            foreach (KeyValuePair<long, long> pair in myList)
            {
                ItemData itemData = Global.ItemDataContainer.GetItem(pair.Key);
                String itemName = itemData != null ? itemData.UserName : pair.Key.ToString();
                float internalPrice = itemData != null ? itemData.InternalPrice : 0.0f;

                BuildLine(sb, 
                    (orderIndex++).ToString(), 
                    itemName,
                    internalPrice.ToString(),
                    (internalPrice * pair.Value).ToString(),
                    pair.Value.ToString(),
                    itemData != null ? itemData.ItemType : "");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            sb.AppendLine("</body></html>");
        }
    }
}
