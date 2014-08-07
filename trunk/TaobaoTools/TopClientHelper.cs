using System;
using System.Collections.Generic;
using System.Text;
using Top.Api;
using Top.Api.Request;
using Top.Api.Domain;
using Top.Api.Response;
using System.Windows.Forms;

namespace TaobaoTools
{
    public class TopClientHelper
    {
        public static bool UpdatePostage(long tid, float postage)
        {
            ITopClient client = Global.DefulatClient();
            TradePostageUpdateRequest req = new TradePostageUpdateRequest();
            req.Tid = tid;
            req.PostFee = postage.ToString();
            TradePostageUpdateResponse response = client.Execute(req, Global.SessionKey);
            if (response.Trade == null || String.IsNullOrEmpty(response.Trade.PostFee))
                return false;

            float retFee = 0.0f;
            float.TryParse(response.Trade.PostFee, out retFee);
            return postage == retFee; 
        }

        public static bool LogisticsDummySend(long tid)
        {
            try
            {
                ITopClient client = Global.DefulatClient();
                LogisticsDummySendRequest req = new LogisticsDummySendRequest();
                req.Tid = tid;

                LogisticsDummySendResponse response = client.Execute(req, Global.SessionKey);
                if (response.IsError)
                    MessageBox.Show(response.SubErrMsg, "对不起，发货失败！");
                return response.Shipping != null ? response.Shipping.IsSuccess : false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "对不起，发货失败！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool LogisticsOfflineSend(long tid, String outSid, String companyCode)
        {
            try
            {
                ITopClient client = Global.DefulatClient();
                LogisticsOfflineSendRequest req = new LogisticsOfflineSendRequest();
                req.Tid = tid;
                req.OutSid = outSid;
                req.CompanyCode = companyCode;
                LogisticsOfflineSendResponse response = client.Execute(req, Global.SessionKey);

                if (response.IsError)
                    MessageBox.Show(response.SubErrMsg, "发货失败！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return response.Shipping != null ? response.Shipping.IsSuccess : false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "对不起，发货失败！");
                return false;
            }
        }

        public static Trade GetTradeInfo(long tid, string fields)
        {
            var req = new TradeGetRequest();
            req.Fields = fields;
            req.Tid = tid;
            var response = Global.DefulatClient().Execute(req, Global.SessionKey);
            return response.Trade;
        }

        public static List<Refund> GetRefund(DateTime begin, DateTime end)
        {
            var ret = new List<Refund>();
            try
            {
                for (long page = 1, num = 100; ; page++)
                {
                    ITopClient client = Global.DefulatClient();
                    var request = new RefundsReceiveGetRequest();
                    request.Fields = "tid,buyer_nick,status,created,refund_fee,order_status";
                    request.PageNo = page;
                    request.PageSize = num;
                    request.StartModified = begin;
                    request.EndModified = end;
                    var response = client.Execute(request, Global.SessionKey);
                    ret.AddRange(response.Refunds);

                    if ((long)response.Refunds.Count < num || ret.Count >= response.TotalResults)
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "对不起，RefundsReceiveGetRequest失败！");
            }
            return ret;
        }

        public static List<Shipping> GetLogisticsOrders(DateTime begin, DateTime end)
        {
            List<Shipping> ret = new List<Shipping>();
            try
            {
                for (long page = 1, num = 100; ; page++)
                {
                    ITopClient client = Global.DefulatClient();
                    LogisticsOrdersGetRequest req = new LogisticsOrdersGetRequest();
                    req.Fields = "tid,out_sid,receiver_name,buyer_nick";
                    req.PageNo = page;
                    req.PageSize = num;
                    req.StartCreated = begin;
                    req.EndCreated = end;
                    LogisticsOrdersGetResponse response = client.Execute(req, Global.SessionKey);
                    if (response == null || response.Shippings == null)
                        break;

                    ret.AddRange(response.Shippings);
                    if ((long)response.Shippings.Count < num || ret.Count >= response.TotalResults)
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "对不起，LogisticsOrdersGetRequest失败！");
            }
            return ret;
        }

        public static List<LogisticsCompany> GetLogisticsCompanies()
        {
            try
            {
                if (Global.LogisticsCompanys.Count == 0)
                {
                    ITopClient client = Global.DefulatClient();

                    LogisticsCompaniesGetRequest req = new LogisticsCompaniesGetRequest();
                    req.Fields = "id,code,name,reg_mail_no";
                    req.IsRecommended = true;
                    req.OrderMode = "offline";

                    LogisticsCompaniesGetResponse response = client.Execute(req, Global.SessionKey);
                    if (response.LogisticsCompanies != null)
                        Global.LogisticsCompanys.AddRange(response.LogisticsCompanies);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return Global.LogisticsCompanys;
        }

        public static List<Item> GetProductList(long page, long num, out long total)
        {
            List<Item> ret = new List<Item>();
            total = 0;
            try
            {
                ITopClient client = Global.DefulatClient();

                ItemsOnsaleGetRequest req = new ItemsOnsaleGetRequest();
                req.Fields = "num_iid,title,price";
                req.PageNo = page;
                req.PageSize = num;

                ItemsOnsaleGetResponse response = client.Execute(req, Global.SessionKey);
                total = response.TotalResults;

                ret.AddRange(response.Items);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return ret;
        }

        public static List<Trade> GetSimpleTradeList(String status, DateTime begin, DateTime end)
        {
            return GetSimpleTradeList(status, begin, end, null);
        }

        public static List<Trade> GetSimpleTradeList(String status, DateTime begin, DateTime end, String appedFileds)
        {
            List<Trade> ret = new List<Trade>();

            try
            {
                ITopClient client = Global.DefulatClient();
                for (long page = 1, num = 100; ; page++)
                {
                    TradesSoldGetRequest req = new TradesSoldGetRequest();
                    req.Fields = "buyer_nick,payment";
                    if (!String.IsNullOrEmpty(appedFileds))
                        req.Fields += "," + appedFileds;

                    req.Status = status;
                    req.StartCreated = begin;
                    req.EndCreated = end;
                    req.PageNo = page;
                    req.PageSize = num;

                    TradesSoldGetResponse response = client.Execute(req, Global.SessionKey);
                    if (response.Trades == null)
                        break;

                    ret.AddRange(response.Trades);
                    if ((long)response.Trades.Count < num || ret.Count >= response.TotalResults)
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return ret;
        }

        public static List<Trade> GetTradeList()
        {
            return GetTradeList("WAIT_SELLER_SEND_GOODS", null, null, null, true);
        }

        public static List<Trade> GetTradeList(string status, Nullable<DateTime> begin, Nullable<DateTime> end, String appedFileds, bool sellerMemo)
        {
            List<Trade> ret = new List<Trade>();
            try
            {
                ITopClient client = Global.DefulatClient();
                for (long page = 1, num = 100; ; page++)
                {
                    TradesSoldGetRequest req = new TradesSoldGetRequest();
                    req.Fields = "tid,created,pay_time,payment";
                    req.Fields += ",buyer_nick";
                    req.Fields += ",receiver_name,receiver_mobile,receiver_phone,receiver_state,receiver_city,receiver_district,receiver_address,receiver_zip";
                    req.Fields += ",seller_memo,seller_flag";
                    req.Fields += ",orders.title,orders.num,orders.num_iid,orders.refund_id,orders.refund_status,orders.status,orders.sku_properties_name";
                    if (!string.IsNullOrEmpty(appedFileds))
                        req.Fields += "," + appedFileds;
                    req.Status = status;
                    req.StartCreated = begin;
                    req.EndCreated = end;
                    req.PageNo = page;
                    req.PageSize = num;

                    TradesSoldGetResponse response = client.Execute(req, Global.SessionKey);
                    if (response.Trades == null)
                        break;

                    if (sellerMemo)
                    {
                        foreach (Trade trade in response.Trades)
                            GetSellerMemo(client, trade);
                    }

                    ret.AddRange(response.Trades);

                    if ((long)response.Trades.Count < num || ret.Count >= response.TotalResults)
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return ret;
        }

        static void GetSellerMemo(ITopClient client, Trade trade)
        {
            if (!String.IsNullOrEmpty(trade.SellerMemo))
                return;

            var tradeInfo = GetTradeInfo(trade.Tid, "buyer_message,seller_memo");
            if (tradeInfo != null)
            {
                if (!String.IsNullOrEmpty(tradeInfo.BuyerMessage))
                    trade.BuyerMessage = tradeInfo.BuyerMessage;

                if (!String.IsNullOrEmpty(tradeInfo.SellerMemo))
                    trade.SellerMemo = tradeInfo.SellerMemo;
            }
        }

        public static bool TradeOtherDeliver(Trade trade)
        {
            return (trade.SellerFlag & (long)Global.FlagBits.FB_OtherDeliver) != 0;
        }

        public static void TradeOtherDeliver(Trade trade, bool deliver)
        {
            if (deliver)
            {
                // other deliver is conflict with express print.
                long flag = trade.SellerFlag & ~(long)Global.FlagBits.FB_ExpressPrinted;
                ModifyTradeFlag(trade, flag | (long)Global.FlagBits.FB_OtherDeliver);
            }
            else
                ModifyTradeFlag(trade, trade.SellerFlag & ~(long)Global.FlagBits.FB_OtherDeliver);
        }

        public static bool TradePrinted(Trade trade)
        {
            return (trade.SellerFlag & (long)Global.FlagBits.FB_Printed) != 0;
        }

        public static void TradePrinted(Trade trade, bool ignore)
        {
            if (ignore)
                ModifyTradeFlag(trade, trade.SellerFlag | (long)Global.FlagBits.FB_Printed);
            else
                ModifyTradeFlag(trade, trade.SellerFlag & ~(long)Global.FlagBits.FB_Printed);
        }

        public static bool ExpressTradePrinted(Trade trade)
        {
            return (trade.SellerFlag & (long)Global.FlagBits.FB_ExpressPrinted) != 0;
        }

        public static void ExpressTradePrinted(Trade trade, bool ignore)
        {
            if (ignore)
                ModifyTradeFlag(trade, trade.SellerFlag | (long)Global.FlagBits.FB_ExpressPrinted);
            else
                ModifyTradeFlag(trade, trade.SellerFlag & ~(long)Global.FlagBits.FB_ExpressPrinted);
        }

        public static void ModifyTradeFlag(Trade trade, long flag)
        {
            try
            {
                ITopClient client = Global.DefulatClient();
                TradeMemoUpdateRequest req = new TradeMemoUpdateRequest();
                req.Tid = trade.Tid;
                req.Flag = flag;
                TradeMemoUpdateResponse response = client.Execute(req, Global.SessionKey);

                if (response.Trade != null && response.Trade.Tid == trade.Tid)
                    trade.SellerFlag = flag;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
