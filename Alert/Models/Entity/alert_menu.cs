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
    
    public partial class alert_menu
    {
        public int id { get; set; }
        public string menu_name { get; set; }
        public string route_name { get; set; }
        public string classicon { get; set; }
        public Nullable<int> parentid { get; set; }
        public string parameter { get; set; }
        public Nullable<int> ordering { get; set; }
        public Nullable<byte> isdelete { get; set; }
    }
}
