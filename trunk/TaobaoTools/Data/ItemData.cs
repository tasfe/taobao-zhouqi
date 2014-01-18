using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace TaobaoTools.Data
{
    [Serializable]
    public class ItemData
    {
        long mID = 0;
        [XmlAttribute("ID")]
        public long ID { get { return mID; } set { mID = value; } }

        float mInternalPrice = 0.0f;
        [XmlAttribute("InternalPrice"), DefaultValue(0.0f)]
        public float InternalPrice { get { return mInternalPrice; } set { mInternalPrice = value; } }

        [XmlAttribute("Weight"), DefaultValue(0.0f)]
        public int Weight { get; set; }

        string mUserName = "";
        [XmlAttribute("UserName"), DefaultValue("")]
        public String UserName { get { return mUserName; } set { mUserName = value; } }

        string mItemType = "";
        [XmlAttribute("ItemType"), DefaultValue("")]
        public String ItemType { get { return mItemType; } set { mItemType = value; } }
    }
}
