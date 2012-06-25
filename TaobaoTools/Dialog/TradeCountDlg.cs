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
        }

        void Count(Trade trade)
        {
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

        void BuildHtml(StringBuilder sb)
        {
            sb.AppendLine("<!DOCTYPE HTML PUBLIC '-//W3C//DTD HTML 4.0 Transitional//EN'>");
            sb.AppendLine("<html xmlns:v='http://www.taobao.com/'>");
            sb.AppendFormat("<style type='text/css'><--body{{FONT-SIZE:{0}pt}} td{{FONT-SIZE:{1}pt}}--></style>\n", Settings.Default.OrderFontSize, Settings.Default.OrderFontSize);
            sb.AppendLine("<head><style type='text/css'>v\\:* {behavior:url(#default#VML);}</style> </head><body>");

            sb.AppendLine("<table width='550px' align='center'>");
            BuildHead(sb, "起始时间", mBegin.ToString());
            BuildHead(sb, "结束时间", mEnd.ToString());
            BuildHead(sb, "订单总数", mOrderNum.ToString());
            BuildHead(sb, "宝贝总数", mItemNum.ToString());
            sb.AppendLine("</table>");

            sb.AppendLine("<table width='550px' align='center' border='1px' bordercolor='#000000' cellspacing='0px' style='border-collapse:collapse'>");
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

                sb.AppendLine("<tr>");
                sb.AppendFormat("<td width='16%'>({0})</td>\n", orderIndex++);
                sb.AppendFormat("<td width='65%'>{0}</td>\n", itemName);
                sb.AppendFormat("<td width='10%'><strong>{0}</strong></td>\n", pair.Value);
                sb.AppendFormat("<td>{0}</td>\n", itemData != null ? itemData.ItemType : "");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            sb.AppendLine("</body></html>");
        }
    }
}
