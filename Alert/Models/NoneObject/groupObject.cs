using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alert.Models
{
    public class groupObject
    {
        public int id { set; get; }
        public string group_name { set; get; }
        public int grouptyid { set; get; }
        public string paramss { set; get; }
        public DateTime datecreate { set; get; }
        public DateTime dateupdate { set; get; }
        public string usercreate { set; get; }
        public string userupdate { set; get; }
        public byte isdelete { set; get; }
    }
}