using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alert.Models
{
    public class userObject
    {
        public int id { set; get; }
        //public int? isactive { set; get; }
        public int? groupid { set; get; }
        public string username { set; get; }
        public string fullName { set; get; }
        public string email { set; get; }
        public string phone { set; get; }
        public string avatar { set; get; }
        public DateTime datecreate { set; get; }
        public string groupName { set; get; }
        public string password { set; get; }
        public byte isactive { get; set; }
        public byte isdelete { get; set; }
    }
}