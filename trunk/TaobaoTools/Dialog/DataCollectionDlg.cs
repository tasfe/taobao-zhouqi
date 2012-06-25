using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Top.Api.Domain;
using TaobaoTools.Data;

namespace TaobaoTools.Dialog
{
    public partial class DataCollectionDlg : UserControl
    {
        public DataCollectionDlg()
        {
            InitializeComponent();

            propertyGrid1.SelectedObject = Global.TradeGroupInfo;
            propertyGrid1.ExpandAllGridItems();

            dateTimePickerBegin.Value = DateTime.Today;
            dateTimePickerEnd.Value = DateTime.Now;

            dateTimePicker1.Value = DateTime.Now;
        }
        
        private void OnTodayChecked(object sender, EventArgs e)
        {
            dateTimePickerBegin.Value = DateTime.Today;
            dateTimePickerEnd.Value = DateTime.Now;
        }

        private void OnYestodayChecked(object sender, EventArgs e)
        {
            dateTimePickerBegin.Value = DateTime.Today - new TimeSpan(1, 0, 0, 0);
            dateTimePickerEnd.Value = DateTime.Today - new TimeSpan(0, 0, 0, 1);
        }

        private void OnBeforeYestodayChecked(object sender, EventArgs e)
        {
            dateTimePickerBegin.Value = DateTime.Today - new TimeSpan(2, 0, 0, 0);
            dateTimePickerEnd.Value = DateTime.Today - new TimeSpan(1, 0, 0, 1);
        }

        private void OnLastThreeDayChecked(object sender, EventArgs e)
        {
            dateTimePickerBegin.Value = DateTime.Today - new TimeSpan(3, 0, 0, 0);
            dateTimePickerEnd.Value = DateTime.Today - new TimeSpan(0, 0, 0, 1);
        }

        private void OnLastWeekChecked(object sender, EventArgs e)
        {
            dateTimePickerBegin.Value = DateTime.Today - new TimeSpan(7, 0, 0, 0);
            dateTimePickerEnd.Value = DateTime.Today - new TimeSpan(0, 0, 0, 1);
        }

        private void OnLastMonthChecked(object sender, EventArgs e)
        {
            dateTimePickerBegin.Value = DateTime.Today - new TimeSpan(30, 0, 0, 0);
            dateTimePickerEnd.Value = DateTime.Today - new TimeSpan(0, 0, 0, 1);
        }

        private void OnSearchClicked(object sender, EventArgs e)
        {
            Global.TradeGroupInfo.FillData(dateTimePickerBegin.Value, dateTimePickerEnd.Value);

            propertyGrid1.Refresh();
        }

        /// <summary>
        /// 获取时间段里面，符合条件的订单列表。
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="fullInfo"></param>
        /// <returns></returns>
        List<Trade> GetRawTradeList(DateTime begin, DateTime end, bool fullInfo)
        {
            List<Trade> searchTrades = null;
            if (fullInfo)
            {
                searchTrades = TopClientHelper.GetTradeList("WAIT_BUYER_CONFIRM_GOODS", begin, end, "consign_time");
                searchTrades.AddRange(TopClientHelper.GetTradeList("TRADE_FINISHED", begin, end, "consign_time"));
            }
            else
            {
                searchTrades = TopClientHelper.GetSimpleTradeList("WAIT_BUYER_CONFIRM_GOODS", begin, end, "consign_time");
                searchTrades.AddRange(TopClientHelper.GetSimpleTradeList("TRADE_FINISHED", begin, end, "consign_time"));
            }

            return searchTrades;
        }

        /// <summary>
        /// 获取指定时间，并向前推移多少天的订单列表
        /// </summary>
        /// <param name="select"></param>
        /// <param name="checkDayNum"></param>
        /// <param name="fullInfo"></param>
        /// <returns></returns>
        List<Trade> GetRawTradeList(DateTime select, int checkDayNum, bool fullInfo)
        {
            DateTime end = select + new TimeSpan(1, 0, 0, 0);
            DateTime begin = end - new TimeSpan(checkDayNum, 0, 0, 0);

            return GetRawTradeList(begin, end, fullInfo);
        }

        List<Trade> GetTradeList(List<Trade> searchTrades, DateTime select)
        {
            DateTime end = select + new TimeSpan(1, 0, 0, 0);

            List<Trade> todayTrades = new List<Trade>();
            foreach (Trade trade in searchTrades)
            {
                if (String.IsNullOrEmpty(trade.ConsignTime))
                    continue;

                DateTime consignTime = DateTime.Parse(trade.ConsignTime);
                if (consignTime > select && consignTime < end)
                    todayTrades.Add(trade);
            }

            return todayTrades;
        }

        List<Trade> GetSelectTradeList(DateTime select, int checkDays, bool fullInfo)
        {
            List<Trade> rawTrade = GetRawTradeList(select, checkDays, fullInfo);
            return GetTradeList(rawTrade, select);
        }

        void GetTradeInfoList(List<TradeInfo> noIndexTrades, 
            List<TradeInfo> indexTrades,
            DateTime select,
            int checkDays)
        {
            List<Trade> todayTrades = GetSelectTradeList(select, checkDays, true);
            todayTrades.Sort(new Global.TradeCreatedCompare());

            foreach (Trade trade in todayTrades)
            {
                if (TopClientHelper.TradeOtherDeliver(trade))
                    Global.AddTrade(noIndexTrades, trade);
                else
                {
                    TradeInfo info = Global.AddTrade(indexTrades, trade);
                    info.Index = indexTrades.IndexOf(info) + 1;
                }
            }
        }

        private void OnTodayDataClicked(object sender, EventArgs e)
        {
            List<Trade> todayTrades = GetSelectTradeList(dateTimePicker1.Value.Date, (int)numericUpDown1.Value, false);

            TradeListInfo todayInfo = new TradeListInfo();
            todayInfo.FillData(todayTrades);

            propertyGrid2.SelectedObject = todayInfo;

            button3.Enabled = true;
            button4.Enabled = true;
        }

        private void OnGenListClicked(object sender, EventArgs e)
        {
            List<TradeInfo> noIndexTrades = new List<TradeInfo>();
            List<TradeInfo> indexTrades = new List<TradeInfo>();
            GetTradeInfoList(noIndexTrades, indexTrades, dateTimePicker1.Value.Date, (int)numericUpDown1.Value);

            string dataTime = dateTimePicker1.Value.Date.ToShortDateString();
            PrintOrderDlg dlg = new PrintOrderDlg(noIndexTrades, indexTrades, dataTime);
            dlg.ShowDialog();
        }

        private void OnGenExpressClicked(object sender, EventArgs e)
        {
            List<TradeInfo> noIndexTrades = new List<TradeInfo>();
            List<TradeInfo> indexTrades = new List<TradeInfo>();
            GetTradeInfoList(noIndexTrades, indexTrades, dateTimePicker1.Value.Date, (int)numericUpDown1.Value);

            PrintExpressDlg dlg = new PrintExpressDlg(indexTrades);
            dlg.ShowDialog();
        }

        private void OnTradeCountClicked(object sender, EventArgs e)
        {
            DateTime begin = dateTimePickerBegin.Value.Date;
            DateTime end = dateTimePickerEnd.Value.Date + new TimeSpan(0, 23, 59, 59);
            DateTime searchBegin = begin - new TimeSpan(5, 0, 0, 0);
            DateTime searchEnd = end;

            List<Trade> searchTradeList = new List<Trade>();
            List<Trade> rawTradeList = GetRawTradeList(searchBegin, searchEnd, true);
            foreach (Trade trade in rawTradeList)
            {
                if (String.IsNullOrEmpty(trade.ConsignTime))
                    continue;

                DateTime consignTime = DateTime.Parse(trade.ConsignTime);
                if (consignTime > begin && consignTime < end)
                    searchTradeList.Add(trade);
            }

            TradeCountDlg dlg = new TradeCountDlg(begin, end, searchTradeList);
            dlg.ShowDialog();
        }
    }
}
