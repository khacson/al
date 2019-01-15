using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;


namespace Alert.Models
{
    public static class baseContanst
    {
        public static string user_Login = "user_Login";
        public static string urls = "https://gsi.greystonedatatech.com/checkreditcard";
        public class business{
            public static string business_account = "khacson1610-facilitator_api1.gmail.com";
            public static string business_password = "W4A5JDV92PHM4MKP";
            public static string business_signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31AHUQ9ctxasNS78cXHmz.rb9zKxf0";
            public static string business_endpoint = "https://api-3t.sandbox.paypal.com/nvp";
            public static string business_version = "115";
            public static string business_host = "https://www.sandbox.paypal.com";
            //https://pilot-payflowpro.paypal.com
            //https://pilot-payflowpro.paypal.com
        }
        public class persional
        {
            public static string persional_accout = "khacson1610-buyer@gmail.com";
            public static string persional_phone_number = "4089247029";
            public static string persional_account_number = "5050411574758";
            public static string persional_card_number = "4032030108193500";
            public static string persional_expiration_date = "04/2022";
        }
    }
}