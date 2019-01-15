using Alert.Models;
using Alert.Models.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Alert.Controllers
{
    public class PaypalIPNController : Controller
    {
        //
        // GET: /PaypalIPN/
        public int Index()
        {
            using (var reader = new StreamReader(Request.InputStream))
                content = reader.ReadToEnd();

            //Luu log
            var db = DBcontext.Context;
            var obj = new alert_logfile();
            obj.logfile = content;
            db.alert_logfile.Add(obj);
            db.SaveChanges();
            //Huy profile
            string recurring_payment_expired = "";
            string recurring_payment_failed = "";
            string recurring_payment_profile_cancel = "";
            string recurring_payment_id = "";
            var stringArray = content.Split('&');
            int check = 0;
            foreach (string item in stringArray)
            {
                var arrItem = item.Split('=');
                string key = arrItem[0];
                string values = arrItem[1];
                if (key == "recurring_payment_expired") //thanh toan dinh ky het han
                {
                    recurring_payment_expired = values;
                    check+=1;
                }
                if (key == "recurring_payment_failed") //thanh toan lỗi
                {
                    recurring_payment_failed = values;
                     check+=1;
                }
                if (key == "recurring_payment_profile_cancel") //thanh toan dinh ky het han
                {
                    recurring_payment_profile_cancel = values;
                     check+=1;
                }
                if (key == "recurring_payment_id") //thanh toan dinh ky het han
                {
                    recurring_payment_id = values;
                }
            }
            if (check > 0)
            {
                string cancel = firebaseCancel(recurring_payment_id);
                if (cancel != "")
                {
                    sendToPayPal(content);
                }
            }
            return 1;
        }
        public string sendToPayPal(string content)
        {
            string endpointTest = ConfigurationManager.AppSettings.Get("verifyurltest");
            string endpointRelease = ConfigurationManager.AppSettings.Get("verifyurl");
            string endpointStatus = ConfigurationManager.AppSettings.Get("endpointStatus");
            int statusPoint = Convert.ToInt16(endpointStatus);
            string url = "";
            if (statusPoint == 1)
            {
                url = endpointRelease;
            }
            else
            {
                url = endpointTest;
            }
            //Curl
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            byte[] bytes = Encoding.ASCII.GetBytes(content);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContinueTimeout = 30;
            request.Headers.Add("Connection","Close"); 

            request.ContentLength = bytes.Length;
            //request.SendChunked = true;
            request.AllowAutoRedirect = true;
            request.AllowWriteStreamBuffering = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            request.ServicePoint.CloseConnectionGroup(request.ConnectionGroupName);
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            string resDecode = HttpUtility.UrlDecode(responseString);
            return "";
        }
        public string content { get; set; }
        public string firebaseCancel(string profileid)
        {
            var db = DBcontext.Context;
            var objectData = db.alert_member.Where(s => s.profileid == profileid).FirstOrDefault<alert_member>();
            try
            {
                string idkey = objectData.idkey;
                //Inser firebase
                string apiKey = ConfigurationManager.AppSettings.Get("apiKey");
                string authDomain = ConfigurationManager.AppSettings.Get("authDomain");
                string databaseURL = ConfigurationManager.AppSettings.Get("databaseURL");
                string projectId = ConfigurationManager.AppSettings.Get("projectId");
                string storageBucket = ConfigurationManager.AppSettings.Get("storageBucket");
                string messagingSenderId = ConfigurationManager.AppSettings.Get("messagingSenderId");

                string path = "/pilot_member/" + idkey.Trim();
                var objInsert = new Alert.Models.baseModel.insertFirebaseCancelIPN
                {
                    isdelete = 1,
                    ischarge = 0,
                    datecancel = DateTime.Now
                };
                string objString = new JavaScriptSerializer().Serialize(objInsert);
                //Curl
                string url = databaseURL + path + ".json?";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
                byte[] bytes = Encoding.ASCII.GetBytes(objString);

                request.Method = "PATCH";
                request.ContentType = "application/json";
                request.ContentLength = objString.Length;
                request.ContinueTimeout = 30;

                request.ContentLength = bytes.Length;
                //request.SendChunked = true;
                request.AllowAutoRedirect = true;
                request.AllowWriteStreamBuffering = true;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                request.ServicePoint.CloseConnectionGroup(request.ConnectionGroupName);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                string resDecode = HttpUtility.UrlDecode(responseString);

                var stringArray = resDecode.Split('&');
                var results = new Dictionary<string, object>();
                return "1";
            }
            catch
            {
                return "";
            }
        }
    }
}