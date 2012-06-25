using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TaobaoTools.Data;
using Top.Api.Domain;

namespace TaobaoTools.Dialog
{
    public partial class AddTradeInfoDlg : Form
    {
        public class ManualTradeInfo
        {
            [DisplayName("姓名"), Category("收件人信息")]
            public string ReceiverName { get; set; }
            [DisplayName("手机"), Category("收件人信息")]
            public string ReceiverMobile { get; set; }
            [DisplayName("电话"), Category("收件人信息")]
            public string ReceiverPhone { get; set; }

            [DisplayName("省"), Category("收件人地址")]
            public string ReceiverState { get; set; }
            [DisplayName("市"), Category("收件人地址")]
            public string ReceiverCity { get; set; }
            [DisplayName("区"), Category("收件人地址")]
            public string ReceiverDistrict { get; set; }
            [DisplayName("地址"), Category("收件人地址")]
            public string ReceiverAddress { get; set; }
            [DisplayName("邮编"), Category("收件人地址")]
            public string ReceiverZip { get; set; }

            [DisplayName("订单编号"), Category("订单信息")]
            public long Tid { get; set; }
            [DisplayName("下单时间"), Category("订单信息")]
            public string Created { get; set; }
            [DisplayName("付款时间"), Category("订单信息")]
            public string PayTime { get; set; }
            [DisplayName("实付金额"), Category("订单信息")]
            public string Payment { get; set; }
            [DisplayName("买家留言"), Category("订单信息")]
            public string BuyerMessage { get; set; }
            [DisplayName("卖家备注"), Category("订单信息")]
            public string SellerMemo { get; set; }

            public class ManualOrderInfo
            {
                long mTid = 0;
                string mName = "";
                string mType = "";

                [DisplayName("商品编号"), Category("商品信息")]
                public long Tid { get { return mTid; } set { setTid(value); } }

                [DisplayName("商品数量"), Category("商品信息")]
                public int Num { get; set; }

                [DisplayName("商品名称"), Category("商品信息")]
                public string UserName { get { return mName; } }

                [DisplayName("商品单位"), Category("商品信息")]
                public string Type { get { return mType; } }

                void setTid(long value)
                {
                    mTid = value;
                    if (Num == 0)
                        Num = 1;
                    ItemData itemData = Global.ItemDataContainer.GetItem(value);
                    mName = (itemData != null) ? itemData.UserName : "未知名";
                    mType = (itemData != null) ? itemData.ItemType : "单位";
                }

                public override string ToString()
                {
                    return string.Format("{0}[{1}]{2}", mName, Num, mType);
                }
            };

            List<ManualOrderInfo> mTrades = new List<ManualOrderInfo>();
            [DisplayName("商品列表"), Category("订单信息")]
            public List<ManualOrderInfo> Trades { get { return mTrades; } }

            public ManualTradeInfo()
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Tid = (long)(UInt32)time.GetHashCode();
                Created = time;
                PayTime = time;
                Payment = "1.0";
                SellerMemo = "这里是手工添加的订单";
            }
        };

        public AddTradeInfoDlg()
        {
            InitializeComponent();

            propertyGrid1.SelectedObject = new ManualTradeInfo();
        }
    }
}
