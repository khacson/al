//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Alert.Models.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class alert_config
    {
        public int id { get; set; }
        public string config_name { get; set; }
        public string config_key { get; set; }
        public string config_value { get; set; }
        public Nullable<System.DateTime> datecreate { get; set; }
        public string usercreate { get; set; }
        public Nullable<System.DateTime> dateupdate { get; set; }
        public string userupdate { get; set; }
        public Nullable<byte> isdelete { get; set; }
    }
}