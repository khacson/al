using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alert.Models.NoneObject
{
    public class menuObject
    {
        public int id { set; get; }
        public string menu_name { set; get; }
        public string route_name { set; get; }
        public string classicon { set; get; }
        public string parameter { set; get; }
        public int parentid { set; get; }
        public int ordering { set; get; }
        public byte isdelete { set; get; }

    }
}