using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alert.Models.NoneObject
{
    public class subscriberObject
    {
        public int id { set; get; }
        //public int? isactive { set; get; }
        public string member_type { set; get; }
        public string member { set; get; }
        public string os_type { set; get; }
        public string longitude { set; get; }
        public string latitude { set; get; }
        public string datecreate { set; get; }
        public int ids { set; get; }
        public Int64 idkey { set; get; }
        public byte isdelete { get; set; }
    }
}