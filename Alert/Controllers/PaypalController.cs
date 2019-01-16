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
        public string content { get; set; }
        /// <summary>
        /// Check card Number
        /// </summary>
        /// <returns></returns>
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
            string email = objectData.email.Trim();
           

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
            //getConfig
            var config = configModel.Finds("recurring_payments");
            string atm = "0";
            if (config != null)
            {
                atm = config.config_value.Trim();
            }
            //Pram
            string requestParams = "VERBOSITY=HIGH";
            requestParams += "&TENDER=C";
            requestParams += "&TRXTYPE=A";
            requestParams += "&ACCT=" + objectData.credit_card_number.Trim();
            requestParams += "&EXPDATE=" + objectData.expdate.Trim();
            requestParams += "&CVV2=" + objectData.cvv.Trim();
            requestParams += "&AMT=0";
            requestParams += "&CURRENCYCODE=US";
            requestParams += "&PONUM=PFDCCTEST";
            requestParams += "&COMMENT1=COMMENT1";
            requestParams += "&COMMENT2=COMMENT2";
            requestParams += "&FIRSTNAME=" + objectData.name_on_card.Trim();
            requestParams += "&LASTNAME=";
            requestParams += "&STATE=" + objectData.country.Trim();
            requestParams += "&COUNTRYCODE=US";
            requestParams += "&ZIP=" + objectData.zipcode.Trim();
            requestParams += "&PARTNER=" + PARTNER.Trim();
            requestParams += "&PWD=" + PWD.Trim();
            requestParams += "&VENDOR=" + VENDOR.Trim();
            requestParams += "&USER=" + USER.Trim();
            //CURL
            var objRes = getPNREFInfo(url, requestParams);
            var dataRes = objRes as baseModel.responsePaypalProfile;
            int ACK = Convert.ToInt32(dataRes.ack);
            string pnref = dataRes.pnref;
            string authcode = dataRes.authcode;
            if (ACK != 0)
            { //Loi card
                responseData.status = 0;
                responseData.msg = dataRes.msg;
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            //Ghi vao database local
            int save = saveDataLocal(content, pnref, authcode);
            if (save != 1)
            {
                responseData.status = 0;
                responseData.msg = "Save Failed";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            string idkey = (email.Replace("@", "_")).Replace(".", "-");
            //Ghi Firebase
            firebase(content, idkey, pnref, authcode);
            //Gui mail thong bao xac nhan
            string sendMail = sentMail(objectData.name_on_card, idkey, email, pnref);
            responseData.status = 1;
            responseData.msg = dataRes.msg;
            responseData.statusSendMail = sendMail; 
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Send Mail
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="idkey"></param>
        /// <param name="tomail"></param>
        /// <param name="pnref"></param>
        /// <returns></returns>
        public string sentMail(string fullname, string idkey, string tomail, string pnref)
        {
            try
            {
                string accountmail = ConfigurationManager.AppSettings.Get("accountmail");
                string passmail = ConfigurationManager.AppSettings.Get("passmail");
                int portmail = Convert.ToInt32(ConfigurationManager.AppSettings.Get("portmail"));
                string servermail = ConfigurationManager.AppSettings.Get("servermail");
                string protocolmail = ConfigurationManager.AppSettings.Get("protocolmail");


                string from = accountmail;
                string link = "http://alertahub.com/Complete/Account?key=" + idkey + "&pnref=" + pnref;
                string html = "";
                html += "<h2>Welcome to Alert</h2>\n\n";
                html += "<p>Hello " + fullname + ",</p>\n";
                html += "<p>Congratulations! Your Alert account has been successfully created. To complete your account setup, simply confirm your email.</p>\n";
                html += "<a href='" + link + "'>Yes, this is my email</a>\n\n";
                html += "<p>Please do not reply to this email.</p>\n\n";
                html += "<p>Regards,</p>\n";
                html += "<p>Alert team</p>\n";

                using (MailMessage mail = new MailMessage(from, tomail))
                {
                    /*mail.Body = "You Are Successfully Register...!!!\n\nDear " + tomail + "," + "\n" + fullname + "," + "\n" + pnref + "," +
                                 "\n\tThank you  registering for Onlile Shopping As A Member." +
                                 "\nIf you have any query please contact 9096562086." +
                                 "\nThank you and Have a nic Day....!" +
                                 "\n\n[Mr.RAHUL SINGH]";*/

                    mail.Body = html;
                    mail.IsBodyHtml = true;
                    mail.Subject = "Complete your Alert account setup";

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
        /// <summary>
        /// Ghi firebase
        /// </summary>
        /// <param name="content"></param>
        /// <param name="idkey"></param>
        /// <param name="pnref"></param>
        /// <param name="authcode"></param>
        /// <returns></returns>
        private string firebase(string content, string idkey, string pnref, string authcode)
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
                pnref_check = pnref,
                authcode_check = authcode
            };
            string objString = new JavaScriptSerializer().Serialize(objInsert);
            //Curl
            string url = databaseURL + path + ".json?";
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
            return idkey;
        }
        /// <summary>
        /// Save data local
        /// </summary>
        /// <param name="content"></param>
        /// <param name="pnref"></param>
        /// <param name="authcode"></param>
        /// <returns></returns>
        public int saveDataLocal(string content, string pnref, string authcode)
        {
            var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
            string email = objectData.email.Trim();
            string idkey = (email.Replace("@", "_")).Replace(".", "-");
            var obj = new alert_member();
            obj.idkey = idkey;
            obj.country = objectData.country;
            obj.email = objectData.email;
            obj.isstatus = 0;
            obj.ischarge = 1;
            obj.isdelete = 0;
            obj.istrial = 1;
			obj.payment_method = "0";
            obj.datecreate = DateTime.Now;
            obj.date_create = objectData.datecreate;
            obj.name_on_card = objectData.name_on_card;
            obj.password = objectData.password;
            obj.zip_code = objectData.zipcode;
            obj.pnref_check = pnref;
            obj.authcode_check = authcode;
            int res = memberModel.Save(obj);
            return res;
        }
        public Object getPNREFInfo(string url, string requestParams)
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
            string PNREF = "";
            string AUTHCODE = "";
            string RESPMSG = "";
            foreach (string item in stringArray)
            {
                var arrItem = item.Split('=');
                string key = arrItem[0];
                string values = arrItem[1];
                if (key == "RESULT")
                {
                    ACK = values;
                }
                if (key == "PNREF")
                {
                    PNREF = values;
                }
                if (key == "AUTHCODE")
                {
                    AUTHCODE = values;
                }
                if (key == "RESPMSG")
                {
                    RESPMSG = values;
                }
            }
            //Ngung profile
            //suspendProfile(profileid);
            var responseData = new baseModel.responsePaypalProfile();
            responseData.pnref = PNREF;
            responseData.authcode = AUTHCODE;
            responseData.ack = ACK;
            responseData.msg = RESPMSG;
            return responseData;
            //return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CancelRecurring()
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
            var responseData = new baseModel.responseData();
            var checkEmail = memberModel.checkEmail(email);
            if (checkEmail == null)
            {
                responseData.status = 0;
                responseData.msg = "Not found email";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            string profileid = checkEmail.profileid.Trim();
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

            string requestParams = "VERBOSITY=HIGH";
            requestParams += "&TENDER=C";
            requestParams += "&TRXTYPE=R";
            requestParams += "&ACTION=C";
            requestParams += "&ORIGPROFILEID=" + profileid;
            
            requestParams += "&PARTNER=" + PARTNER.Trim();
            requestParams += "&PWD=" + PWD.Trim();
            requestParams += "&VENDOR=" + VENDOR.Trim();
            requestParams += "&USER=" + USER.Trim();

            
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
            string msg = "";
            string RPREF = "";
            foreach (string item in stringArray)
            {
                var arrItem = item.Split('=');
                string key = arrItem[0];
                string values = arrItem[1];
                if (key == "RESULT")
                {
                    ACK = values;
                }
                if (key == "RESPMSG")
                {
                    msg = values;
                }
                if (key == "RPREF")
                {
                    RPREF = values;
                }
            }
            if (Convert.ToInt32(ACK) != 0)
            {
                responseData.status = 0;
                responseData.msg = msg;
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //Update Memer Local DB
                var db = DBcontext.Context;
                var obj = db.alert_member.Where(s => s.id == checkEmail.id).FirstOrDefault<alert_member>();
                obj.isdelete = 1;
                obj.datecancel = DateTime.Now;
                obj.pnref_cancel = RPREF;
                db.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //Update Firebase
                firebaseCancel(checkEmail.id, 1, 1, RPREF);
                responseData.status = 1;
                responseData.msg = msg;
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
        }
        public string firebaseCancel(int id, int isdelete, int isactive, string RPREF)
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
            var objInsert = new Alert.Models.baseModel.insertFirebaseCancel
            {
                isdelete = 1,
                pnref_cancel = RPREF.Trim(),
                datecancel = DateTime.Now
                /*country = objectData.country,
                //deviceid = "",
                email = objectData.email,
                isactive = isactive,
                ischarge = 1,
                isdelete = isdelete,
                istrial = 1,
                name_on_card = objectData.name_on_card,
                password = objectData.password,
                zipcode = objectData.zip_code,
                profileid = objectData.profileid,
                pnref_check = objectData.pnref_check,
                authcode_check = objectData.authcode_check,
                pnref_profile = objectData.pnref_profile,
                authcode_profile = objectData.authcode_profile,
                trxpnref_profile = objectData.trxpnref_profile,
                dateactive = Convert.ToString(objectData.dateactive),
                pnref_cancel = RPREF*/
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
    }
}