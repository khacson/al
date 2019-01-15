using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alert.Models
{
    public class baseModel
    {
        [Serializable]
        public class userLogin
        {
            public int userID { set; get; }
            public string userName { set; get; }
            public string fullName { set; get; }
            public string email { set; get; }
            public string phone { set; get; }
            public string avatar { set; get; }
        }
        [Serializable]
        public class responseData
        {
            public int status { set; get; }
            public string msg { set; get; }
            public string statusSendMail { set; get; }
        }
        [Serializable]
        public class responsePaypalProfile
        {
            public string profileid { set; get; }
            public string correlationid { set; get; }
            public string build { set; get; }
            public string ack { set; get; }
            public string msg { set; get; }
            public string pnref { set; get; }
            public string authcode { set; get; }
            public string trxpnref { set; get; }
            public string status { set; get; }
        }
        [Serializable]
        public class responseGetList
        {
            public int status { set; get; }
            public int total { set; get; }
            public string msg { set; get; }
            public string content { set; get; }
        }
        [Serializable]
        public class menu
        {
            public int chk { set; get; }
            public string sub { set; get; }
        }
        [Serializable]
        public class getUniqueID {
            public string uniqueid { set; get; }
            public string timeNow { set; get; }
        }
        public class insertFirebase
        {
            public string country;
            public string deviceid;
            public string email;
            public int isactive;
            public int ischarge;
            public int isdelete;
            public int istrial;
            public string name_on_card;
            public string password;
            public string zipcode;
            public string profileid;

            public string pnref_check;
            public string authcode_check;
            public string pnref_profile;
            public string authcode_profile;
            public string trxpnref_profile;
            public string dateactive;
            public string pnref_cancel;
        }
        public class insertFirebaseCancel
        {
            public int isdelete;
            public string pnref_cancel;
            public DateTime datecancel;
        }
        public class insertFirebaseCancelIPN
        {
            public int isdelete;
            public int ischarge;
            public DateTime datecancel;
        }
        public class insertFirebaseActive
        {
            public int isactive;
            public string profileid;
            public string pnref_profile;
            public string authcode_profile;
            public string trxpnref_profile;
            public DateTime dateactive;
        }
    }
}