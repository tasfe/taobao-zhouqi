﻿using System;
using System.Collections.Generic;
using System.Text;
using Top.Api.Domain;
using System.ComponentModel;

namespace TaobaoTools.Data
{
    class ItemDataInfo
    {
        public ItemDataInfo(Item item)
        {
            mItemID = item.NumIid;
            mItemName = item.Title;
            mItemPrice = item.Price;

            ItemData itemData = Global.ItemDataContainer.GetItem(ItemID);
            if (itemData != null)
            {
                mUserName = itemData.UserName;
                mItemType = itemData.ItemType;
                mInternalPrice = itemData.InternalPrice;
                Weight = itemData.Weight;
            }
        }

        long mItemID = 0;
        [Category("固定属性"), DisplayName("宝贝编号")]
        public long ItemID { get { return mItemID; } }

        string mItemName = "";
        [Category("固定属性"), DisplayName("宝贝名称")]
        public string ItemName { get { return mItemName; } }

        string mItemPrice = "";
        [Category("固定属性"), DisplayName("宝贝价格")]
        public string ItemPrice { get { return mItemPrice; } }

        float mInternalPrice = 0.0f;
        [Category("附加属性"), DisplayName("宝贝进价"), Description("宝贝的进价")]
        public float InternalPrice { get { return mInternalPrice; } set { mInternalPrice = value; } }

        [Category("附加属性"), DisplayName("净重"), Description("宝贝的净重量，支持小数点，单位为克")]
        public int Weight { get; set; }

        string mUserName = "";
        [Category("附加属性"), DisplayName("宝贝代号"), Description("宝贝的内部名称，如[散鱼]")]
        public string UserName { get { return mUserName; } set { mUserName = value; } }

        string mItemType = "";
        [Category("附加属性"), DisplayName("宝贝类型"), Description("宝贝的类型，如[袋/斤/小包/箱/盒]")]
        public string ItemType { get { return mItemType; } set { mItemType = value; } }
    }
}
