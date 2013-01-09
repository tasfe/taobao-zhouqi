using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Top.Api.Domain;

namespace TaobaoTools.Data
{
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class TradeListInfo
    {
        int mCount = 0;
        [DefaultValue(0), DisplayName("交易笔数")]
        public int Count { get { return mCount; } }

        float mPayment = 0.0f;
        [DefaultValue(0.0f), DisplayName("交易金额")]
        public float Payment { get { return mPayment; } }

        int mBuyerNum = 0;
        [DefaultValue(0), DisplayName("交易人数")]
        public int BuyerNum { get { return mBuyerNum; } }

        int mConsignNum = 0;
        [DefaultValue(0), DisplayName("发货单数")]
        public int ConsignNum { get { return mConsignNum; } }

        public override string ToString() { return String.Empty; }

        public void Reset()
        {
            mCount = 0;
            mPayment = 0.0f;
            mBuyerNum = 0;
            mConsignNum = 0;
        }

        public void Include(TradeListInfo info)
        {
            mCount += info.Count;
            mPayment += info.Payment;
            mBuyerNum += info.BuyerNum;
            mConsignNum += info.ConsignNum;
        }

        public void FillData(List<Trade> trades)
        {
            Reset();

            List<String> buyerList = new List<string>();
            List<String> consignList = new List<string>();
            foreach (Trade trade in trades)
            {
                float payment = 0.0f;
                if (float.TryParse(trade.Payment, out payment))
                    mPayment += payment;

                string consignStr = trade.BuyerNick;
                if (!string.IsNullOrEmpty(trade.ConsignTime))
                {
                    DateTime consignTime = DateTime.ParseExact(trade.ConsignTime, "yyyy-MM-dd HH:mm:ss", null);
                    consignStr += consignTime.ToLongDateString();
                }

                if (!consignList.Contains(consignStr))
                    consignList.Add(consignStr);

                if (!buyerList.Contains(trade.BuyerNick))
                    buyerList.Add(trade.BuyerNick);
            }

            mCount += trades.Count;
            mBuyerNum += buyerList.Count;
            mConsignNum += consignList.Count;
        }
    }
}
