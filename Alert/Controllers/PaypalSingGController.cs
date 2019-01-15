using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using System.IO;
using System.Text;
using System.Configuration;

using Alert.Models;
using Alert.Models.Entity;
using Newtonsoft.Json;

using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Web.Helpers;
using System.Net.Mail;


namespace Alert.Controllers
{
    public class PaypalController : Controller
    {
        //
        // GET: /Paypal/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CheckCardNumber()
        {
            string apiEndPointAPITest = ConfigurationManager.AppSettings.Get("apiEndPointAPITest");
            string apiEndPointAPIRelease = ConfigurationManager.AppSettings.Get("apiEndPointAPIRelease");
            string apiEndPointAPIStatus = ConfigurationManager.AppSettings.Get("apiEndPointAPIStatus");
            string VENDOR = ConfigurationManager.AppSettings.Get("VENDOR");
            string USER = ConfigurationManager.AppSettings.Get("USER");
            string PWD = ConfigurationManager.AppSettings.Get("PWD");
            string PARTNER = ConfigurationManager.AppSettings.Get("PARTNER");

            using (var reader = new StreamReader(Request.InputStream))
                content = reader.ReadToEnd();

            var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
            string email = objectData.email;
            string acct = objectData.credit_card_number;
            string expdate = objectData.expdate;
            string cvv = objectData.cvv;
            if (expdate.Length == 4)
            {
                string sub1 = expdate.Substring(0, 2);
                string sub2 = expdate.Substring(2, 2);
                expdate = sub1 + "20" + sub2;
            }
            var responseData = new baseModel.responseData();
            //Check Email
            var checkEmail = memberModel.checkEmail(email);
            if (checkEmail != null)
            {
                responseData.status = -1;
                responseData.msg = "Email Exist";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            //GET apiEndPoint
            int statusPoint = Convert.ToInt16(apiEndPointAPIStatus);
            string url = "";
            if (statusPoint == 1)
            {
                url = apiEndPointAPIRelease;
            }
            else
            {
                url = apiEndPointAPITest;
            }
            DateTime date = DateTime.Now;
            //Pram


            responseData.status = 1;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        public string content { get; set; }
        /// <summary>
        /// kiem tra tai khoan payflow
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCardNumberSingle()
        {
            using (var reader = new StreamReader(Request.InputStream)) 
                     content = reader.ReadToEnd();

            var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
            string email = objectData.email;
            string acct = objectData.credit_card_number;
            string expdate = objectData.expdate;
            string cvv = objectData.cvv;
            if (expdate.Length == 4)
            {
                string sub1 = expdate.Substring(0, 2);
                string sub2 = expdate.Substring(2, 2);
                expdate = sub1 + "20" + sub2;
            }
            var responseData = new baseModel.responseData();
            //Check Email
            var checkEmail = memberModel.checkEmail(email);
            if (checkEmail != null)
            {
                responseData.status = -1;
                responseData.msg = "Email Exist";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            string endpointTest = ConfigurationManager.AppSettings.Get("endpointTest");
            string endpointRelease = ConfigurationManager.AppSettings.Get("endpointRelease");
            string endpointStatus = ConfigurationManager.AppSettings.Get("endpointStatus");
            string username = ConfigurationManager.AppSettings.Get("username");
            string password = ConfigurationManager.AppSettings.Get("password");
            string signature = ConfigurationManager.AppSettings.Get("signature");


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
            DateTime date = DateTime.Now;
            //getConfig
            var config = configModel.Finds("recurring_payments");
            string atm = "0";
            if (config != null)
            {
                atm = config.config_value;
            }


            string requestParams = "METHOD=CreateRecurringPaymentsProfile";
            requestParams += "&VERSION=115";
            requestParams += "&PWD=" + password.Trim();
            requestParams += "&USER=" + username.Trim();
            requestParams += "&SIGNATURE=" + signature.Trim();
            requestParams += "&PROFILESTARTDATE=" + date.ToString("yyyy-MM-dd") + "T00:00:00Z";
            requestParams += "&DESC=Recurring+Payment";
            requestParams += "&BILLINGPERIOD=Month";
            requestParams += "&BILLINGFREQUENCY=24";
            requestParams += "&AMT=" + atm.Trim();
            requestParams += "&MAXFAILEDPAYMENTS=3";
            requestParams += "&ACCT=" + objectData.credit_card_number.Trim();
            requestParams += "&CREDITCARDTYPE=VISA";
            requestParams += "&CVV2=" + cvv.Trim();
            requestParams += "&FIRSTNAME=" + objectData.name_on_card.Trim();
            requestParams += "&LASTNAME=";
            requestParams += "&STREET=";
            requestParams += "&CITY=";
            requestParams += "&STATE=";
            requestParams += "&COUNTRYCODE=US";
            requestParams += "&CURRENCYCODE=USD";
            //Trail
            requestParams += "&TRIALBILLINGPERIOD=Year";
            requestParams += "&TRIALBILLINGFREQUENCY=1";
            requestParams += "&TRIALAMT=0";
            requestParams += "&TRIALTOTALBILLINGCYCLES=1";

            requestParams += "&EXPDATE=" + expdate.Trim(); 

            var objRes = createProfile(url, requestParams);
            var dataRes = objRes as baseModel.responsePaypalProfile;
            if (dataRes.ack == "Failure")
            {
                responseData.status = 0;
                responseData.msg = dataRes.msg;
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //Insert database firebase
                string idkey = (email.Replace("@", "_")).Replace(".", "-");
                var obj = new alert_member();
                obj.idkey = idkey;
                obj.country = objectData.country;
                obj.email = objectData.email;
                obj.isstatus = 0;
                obj.ischarge = 1;
                obj.isdelete = 0;
                obj.istrial = 1;
                obj.datecreate = DateTime.Now;
                obj.date_create = objectData.datecreate;
                obj.name_on_card = objectData.name_on_card;
                obj.password = objectData.password;
                obj.zip_code = objectData.zipcode;
                obj.profileid = dataRes.profileid;
                obj.correlationid = dataRes.correlationid;
                obj.build = dataRes.build;
                obj.transactionid = "";
                string transactionid = "";
                int res = memberModel.Save(obj);
                if (res == 1)//Ghi xuong firebase
                {
                    firebase(content, idkey, dataRes.profileid, dataRes.correlationid, dataRes.build, transactionid);
                    //Send mail
                    string sendMail = sentMail(objectData.name_on_card, objectData.email, dataRes.profileid);
                    responseData.status = 1;
                    responseData.msg = dataRes.ack;
                    responseData.statusSendMail = sendMail;
                    return Json(responseData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    responseData.status = 0;
                    responseData.msg = dataRes.ack;
                    return Json(responseData, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public Object createProfile(string url, string requestParams)
        {
            string objString = requestParams.Trim();
            //Curl
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            byte[] bytes = Encoding.ASCII.GetBytes(objString);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
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

            string ACK = "";
            string profileid = "";
            string correlationid = "";
            string build = "";
            string msg = "";
            foreach (string item in stringArray)
            {
                var arrItem = item.Split('=');
                string key = arrItem[0];
                string values = arrItem[1];
                if (key == "ACK")
                {
                    ACK = values;
                }
                if (key == "PROFILEID")
                {
                    profileid = values;
                }
                if (key == "CORRELATIONID")
                {
                    correlationid = values;
                }
                if (key == "BUILD")
                {
                    build = values;
                }
                if (key == "L_LONGMESSAGE0")
                {
                    msg = values;
                }
            }
            //Ngung profile
            suspendProfile(profileid);
            var responseData = new baseModel.responsePaypalProfile();
            responseData.profileid = profileid;
            responseData.correlationid = correlationid;
            responseData.build = build;
            responseData.ack = ACK;
            responseData.msg = msg;
            return responseData;
            //return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CancelRecurring()
        {
            using (var reader = new StreamReader(Request.InputStream))
                content = reader.ReadToEnd();

            var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
            string email = objectData.email;
            var responseData = new baseModel.responseData();
            var checkEmail = memberModel.checkEmail(email);
            if (checkEmail == null)
            {
                 responseData.status = 0;
                responseData.msg = "Not found email";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            string profileid = checkEmail.profileid.Trim();

            string endpointTest = ConfigurationManager.AppSettings.Get("endpointTest");
            string endpointRelease = ConfigurationManager.AppSettings.Get("endpointRelease");
            string endpointStatus = ConfigurationManager.AppSettings.Get("endpointStatus");
            string username = ConfigurationManager.AppSettings.Get("username");
            string password = ConfigurationManager.AppSettings.Get("password");
            string signature = ConfigurationManager.AppSettings.Get("signature");

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

            string requestParams = "METHOD=ManageRecurringPaymentsProfileStatus";
            requestParams += "&VERSION=115";
            requestParams += "&PWD=" + password.Trim();
            requestParams += "&USER=" + username.Trim();
            requestParams += "&SIGNATURE=" + signature.Trim();
            requestParams += "&ACTION=Cancel";
            requestParams += "&PROFILEID=" + profileid;
            requestParams += "&NOTE=unsubscribe";

            string objString = requestParams.Trim();
            //Curl
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            byte[] bytes = Encoding.ASCII.GetBytes(objString);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
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

            string ACK = "";
            string correlationid = "";
            string msg = "";
            foreach (string item in stringArray)
            {
                var arrItem = item.Split('=');
                string key = arrItem[0];
                string values = arrItem[1];
                if (key == "ACK")
                {
                    ACK = values;
                }
                if (key == "CORRELATIONID")
                {
                    correlationid = values;
                }
                if (key == "L_SHORTMESSAGE0")
                {
                    msg = values;
                }
            }
            if (ACK == "Failure")
            {
                responseData.status = 0;
                responseData.msg = msg;
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //Update Memer
                var db = DBcontext.Context;
                var obj = db.alert_member.Where(s => s.id == checkEmail.id).FirstOrDefault<alert_member>();
                obj.isdelete = 1;
                obj.correlationid_cancel = correlationid;
                obj.datecancel =  DateTime.Now;
                db.Entry(obj).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                //Cancel Firebase
                firebaseCancel(checkEmail.id,1,1);
                responseData.status = 1;
                responseData.msg = msg;
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }           
        }
        protected string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
        public string suspendProfile(string profileid){
            string endpointTest = ConfigurationManager.AppSettings.Get("endpointTest");
            string endpointRelease = ConfigurationManager.AppSettings.Get("endpointRelease");
            string endpointStatus = ConfigurationManager.AppSettings.Get("endpointStatus");
            string username = ConfigurationManager.AppSettings.Get("username");
            string password = ConfigurationManager.AppSettings.Get("password");
            string signature = ConfigurationManager.AppSettings.Get("signature");

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

            string requestParams = "METHOD=ManageRecurringPaymentsProfileStatus";
            requestParams += "&VERSION=115";
            requestParams += "&PWD=" + password.Trim();
            requestParams += "&USER=" + username.Trim();
            requestParams += "&SIGNATURE=" + signature.Trim();
            requestParams += "&PROFILEID=" + profileid;
            requestParams += "&NOTE=Suspend";
            requestParams += "&ACTION=Suspend";

            string objString = requestParams.Trim();
            //Curl
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            byte[] bytes = Encoding.ASCII.GetBytes(objString);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
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
            return profileid;
        }
        [HttpGet]
        public ActionResult reactivateProfile(string profileid)
        {
            string endpointTest = ConfigurationManager.AppSettings.Get("endpointTest");
            string endpointRelease = ConfigurationManager.AppSettings.Get("endpointRelease");
            string endpointStatus = ConfigurationManager.AppSettings.Get("endpointStatus");
            string username = ConfigurationManager.AppSettings.Get("username");
            string password = ConfigurationManager.AppSettings.Get("password");
            string signature = ConfigurationManager.AppSettings.Get("signature");

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

            string requestParams = "METHOD=ManageRecurringPaymentsProfileStatus";
            requestParams += "&VERSION=115";
            requestParams += "&PWD=" + password.Trim();
            requestParams += "&USER=" + username.Trim();
            requestParams += "&SIGNATURE=" + signature.Trim();
            requestParams += "&PROFILEID=" + profileid;
            requestParams += "&NOTE=Reactivate";
            requestParams += "&ACTION=Reactivate";

            string objString = requestParams.Trim();
            //Curl
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            byte[] bytes = Encoding.ASCII.GetBytes(objString);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
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
            string ACK = "";
            var stringArray = resDecode.Split('&');
            foreach (string item in stringArray)
            {
                var arrItem = item.Split('=');
                string key = arrItem[0];
                string values = arrItem[1];
                if (key == "ACK")
                {
                    ACK = values;
                }
            }
            if (ACK == "Success")
            {
                //Update Memer
                try
                {
                    var db = DBcontext.Context;
                    var obj = db.alert_member.Where(s => s.profileid == profileid).FirstOrDefault<alert_member>();
                    obj.isstatus = 1;
                    db.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //Set Firebase ki hoat tai khoan
                    firebaseCancel(obj.id,0,1);
                }
                catch
                {
                    return RedirectToAction("activeFailed", "Home");
                }
                
                return RedirectToAction("activeSuccess", "Home");
            }
            else
            {
                return RedirectToAction("activeFailed", "Home");
            }
           
        }
        public string firebaseCancel(int id, int isdelete, int isactive)
        {
            var db = DBcontext.Context;
            var objectData = db.alert_member.Where(s => s.id == id).FirstOrDefault<alert_member>();
            string idkey = objectData.idkey;
            //Inser firebase
            string apiKey = ConfigurationManager.AppSettings.Get("apiKey");
            string authDomain = ConfigurationManager.AppSettings.Get("authDomain");
            string databaseURL = ConfigurationManager.AppSettings.Get("databaseURL");
            string projectId = ConfigurationManager.AppSettings.Get("projectId");
            string storageBucket = ConfigurationManager.AppSettings.Get("storageBucket");
            string messagingSenderId = ConfigurationManager.AppSettings.Get("messagingSenderId");

            string path = "/pilot_member/" + idkey.Trim();
            var objInsert = new Alert.Models.baseModel.insertFirebase
            {
                country = objectData.country,
                deviceid = "",
                email = objectData.email,
                isactive = isactive,
                ischarge = 1,
                isdelete = isactive,
                istrial = 1,
                name_on_card = objectData.name_on_card,
                password = objectData.password,
                zipcode = objectData.zip_code,
                profileid = objectData.profileid,
                correlationid = objectData.correlationid,
                build = objectData.build,
                transactionid = objectData.transactionid
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
            return "";
        }
        public string firebase(string content, string idkey, string profileid, string correlationid, string build, string transactionid)
        {
            //Inser firebase
            string apiKey = ConfigurationManager.AppSettings.Get("apiKey");
            string authDomain = ConfigurationManager.AppSettings.Get("authDomain");
            string databaseURL = ConfigurationManager.AppSettings.Get("databaseURL");
            string projectId = ConfigurationManager.AppSettings.Get("projectId");
            string storageBucket = ConfigurationManager.AppSettings.Get("storageBucket");
            string messagingSenderId = ConfigurationManager.AppSettings.Get("messagingSenderId");

            string path = "/pilot_member/" + idkey;
            var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
            var objInsert = new Alert.Models.baseModel.insertFirebase
            {
                country = objectData.country,
                deviceid = "",
                email = objectData.email,
                isactive = 0,
                ischarge = 1,
                isdelete = 0,
                istrial = 1,
                name_on_card = objectData.name_on_card,
                password = objectData.password,
                zipcode = objectData.zipcode,
                profileid = profileid,
                correlationid = correlationid,
                build = build,
                transactionid = transactionid
            };
            string objString = new JavaScriptSerializer().Serialize(objInsert);
            //Curl
            string url = databaseURL+ path+".json?";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            byte[] bytes = Encoding.ASCII.GetBytes(objString);

            request.Method = "PUT";
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

            return "";
        }
        public string sentMail(string fullname, string tomail, string profileid)
        {
            try
            {
                string accountmail = ConfigurationManager.AppSettings.Get("accountmail");
                string passmail = ConfigurationManager.AppSettings.Get("passmail");
                int portmail = Convert.ToInt32(ConfigurationManager.AppSettings.Get("portmail"));
                string servermail = ConfigurationManager.AppSettings.Get("servermail");
                string protocolmail = ConfigurationManager.AppSettings.Get("protocolmail");

               
                string from = accountmail;
                using (MailMessage mail = new MailMessage(from, tomail))
                {
                    mail.Body = "You Are Successfully Register...!!!\n\nDear " + tomail + "," + "\n" + fullname + "," + "\n" + profileid + "," +
                                 "\n\tThank you  registering for Onlile Shopping As A Member." +
                                 "\nIf you have any query please contact 9096562086." +
                                 "\nThank you and Have a nic Day....!" +
                                 "\n\n[Mr.RAHUL SINGH]";

                    mail.IsBodyHtml = false;
                    mail.Subject = "";

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = servermail;
                    smtp.EnableSsl = true;
                    NetworkCredential networkCredential = new NetworkCredential(from, passmail);
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = networkCredential;
                    smtp.Port = portmail;
                    smtp.Send(mail);
                }
                return "Send mail success";
            }
            catch (Exception)
            {
                return "Send mail error";
            }
           
        } 
    }
}