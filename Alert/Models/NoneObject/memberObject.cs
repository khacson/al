using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alert.Models.NoneObject
{
    public class memberObject
    {
        public int id { set; get; }
        //public int? isactive { set; get; }
        public string name_on_card { set; get; }
        public string card_number { set; get; }
        public string cvv { set; get; }
        public string expdate { set; get; }
        public string password { set; get; }
        public string country { set; get; }
        public string zip_code { set; get; }
        public byte? isstatus { set; get; }
        public byte istrial { get; set; }
        public byte ischarge { get; set; }
        public string accounts { get; set; }
        public string email { get; set; }
        public string datecreate { get; set; }
        public string idkey { set; get; }
        public byte? isdelete { get; set; }
        public string date_create { set; get; }
    }
}