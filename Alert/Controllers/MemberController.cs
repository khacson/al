using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alert.Models;
using Alert.Models.Entity;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Net.Mail;

namespace Alert.Controllers
{
    public class MemberController : Controller
    {
        //
        // GET: /Member/
        public ActionResult Index()
        {
            var country = memberModel.getCtuntry();
            ViewBag.countrys = country; 
            return View();
        }
        [HttpPost]
        public ActionResult getList(FormCollection form)
        {
            string search = Request.Form["search"].ToString();
            int pageNum = Convert.ToInt32(Request.Form["page"].ToString());
            int pageSize = 20;
            var datas = memberModel.getList(search, pageNum, pageSize);
            int total = memberModel.getTotal(search);
            int totalPage = 1;
            if (total > pageSize)
            {
                totalPage = Convert.ToInt32(total / pageSize);
                if (total % pageSize != 0)
                {
                    totalPage = totalPage + 1;
                }
            }
            ViewBag.datas = datas;
            var obj = new
            {
                total = total,
                totalPage = totalPage,
                content = helperModel.RenderHelper.PartialView(this, "List", datas)
            };
            return Json(obj);
        }
        [HttpPost]
        public ActionResult saveData(FormCollection form)
        {

            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            string search = Request.Form["search"].ToString();
            var dataForm = JsonConvert.DeserializeObject<Alert.Models.noneObject>(search);
            string paidfull = dataForm.paidfull;
            var responseData = new baseModel.responseData();
            var obj = new alert_member();
            string email = dataForm.email.Trim();
            //byte? isstatus = dataForm.isstatus;
            string idkey = email.Replace("@","_").Replace(".","-");
            var checkEmail = Alert.Models.memberModel.checkEmail(email);
            var configs = Alert.Models.memberModel.getConfig();
            if (checkEmail != null)
            {
                responseData.status = 0;
                responseData.password = "";
                responseData.msg = "Email already exists";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            var payment_method = dataForm.payment_method;
            if (payment_method == "0")//Credit card
            {
                int reponseCard = CheckCardNumber(dataForm.email.Trim(), dataForm.card_number.Trim(), dataForm.name_on_card.Trim(), dataForm.password, dataForm.expdate.Trim(), dataForm.cvv.Trim());
                responseData.status = reponseCard;
                responseData.password = GenerateMD5(dataForm.password);
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            //obj.idkey = idkey;
            obj.firstname = dataForm.firstname.Trim();
            obj.lastname = dataForm.lastname.Trim();
            obj.phonenumber = dataForm.phonenumber.Trim();
            obj.money_order = dataForm.money_order.Trim();
            obj.name_on_card = dataForm.name_on_card.Trim(); 

            obj.email = email;
            var pass = dataForm.password; 
            obj.password = GenerateMD5(pass);
            string money = "0";
            if (configs == null)
            {
                obj.money = "0";
            }
            else
            {
                obj.money = configs.config_value.Trim();
                money = configs.config_value.Trim();
            }
            string expireday = dataForm.expiredays.Trim();
            if (expireday != "")
            {
                var arrDate = expireday.Split('/');
                string expiredays =  arrDate[2] + '/' + arrDate[1] + '/' + arrDate[0];
                obj.expiredays = Convert.ToDateTime(expiredays);
            }
            if (Convert.ToByte(paidfull) == 1)
            {
                obj.isstatus = 1;// dataForm.isstatus; payment_method acount_type
            }
            else
            {
                obj.isstatus = 0;// dataForm.isstatus;
            }
            string customer_number = RandomString(8); 
            //long result = localDate.Year * 10000000000 + localDate.Month * 100000000 + localDate.Day * 1000000 + localDate.Hour * 10000 + localDate.Minute * 100 + localDate.Second;
            obj.isdelete = 0;// dataForm.isdelete;
            obj.payment_method = dataForm.payment_method;
            obj.acount_type = 1;
            obj.paidfull = Convert.ToByte(paidfull);
            obj.datecreate = DateTime.Now;
            obj.customer_number = customer_number;
            int res = 0;
            res = memberModel.Save(obj);
            if (res == 1)
            {
                //Save Data Firebases
                //Inser firebase
                string apiKey = ConfigurationManager.AppSettings.Get("apiKey");
                string authDomain = ConfigurationManager.AppSettings.Get("authDomain");
                string databaseURL = ConfigurationManager.AppSettings.Get("databaseURL");
                string projectId = ConfigurationManager.AppSettings.Get("projectId");
                string storageBucket = ConfigurationManager.AppSettings.Get("storageBucket");
                string messagingSenderId = ConfigurationManager.AppSettings.Get("messagingSenderId");

                string path = "/pilot_member/" + idkey;
                //var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
                var objInsert = new Alert.Models.baseModel.insertFirebase
                {
                    country = dataForm.country,
                    deviceid = "",
                    email = dataForm.email,
                    isactive = Convert.ToByte(paidfull),
                    ischarge = 1,
                    isdelete = 0,
                    istrial = 1,
                    acount_type = 1,
                    firstname = dataForm.firstname.Trim(),
                    lastname = dataForm.lastname.Trim(),
                    money_order = dataForm.money_order.Trim(),
                    customer_number = customer_number,
                    password = GenerateMD5(pass),
                    money = money,
                    expired = expireday
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

                sendMail(dataForm.firstname.Trim(), idkey, email);
            }
            responseData.status = 1;
            responseData.password = GenerateMD5(pass);
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult edits(FormCollection form)
        {
            var responseData = new baseModel.responseData();
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            string search = Request.Form["search"].ToString();
            var dataForm = JsonConvert.DeserializeObject<Alert.Models.noneObject>(search);
            string paidfull = dataForm.paidfull;
            var obj = new alert_member();
            string idkey = Request.Form["id"].ToString();
            string email = dataForm.email.Trim();
            var id = Convert.ToInt32(idkey);
            var checkEmail = Alert.Models.memberModel.checkEmailEdit(email, id);
            var configs = Alert.Models.memberModel.getConfig();
            if (checkEmail != null)
            {
                responseData.status = 0;
                responseData.password = "";
                responseData.msg = "Email already exists";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            var finds = Alert.Models.memberModel.FindID(id);
            if (finds != null)
            {
                var db = DBcontext.Context;
                var objs = db.alert_member.Where(s => s.id == id).FirstOrDefault<alert_member>();
                //obj.idkey = idkey;
                objs.firstname = dataForm.firstname.Trim();
                objs.lastname = dataForm.lastname.Trim();
                objs.email = email;
                var password = "";
                if (dataForm.password == "")
                {
                    password = objs.password.Trim();
                }
                else
                {
                    password = GenerateMD5(dataForm.password);
                }
                objs.password = password;
                objs.phonenumber = dataForm.phonenumber.Trim();
                objs.money_order = dataForm.money_order.Trim();
                objs.isstatus = Convert.ToByte(paidfull);
                objs.isdelete = dataForm.isdelete;
                //objs.datecreate = DateTime.Now;
                string expireday = dataForm.expiredays.Trim();
                string money = "0";
                if (objs.acount_type == 1)
                {
                    
                    if (expireday != "")
                    {
                        var arrDate = expireday.Split('/');
                        string expiredays = arrDate[2] + '/' + arrDate[1] + '/' + arrDate[0];
                        objs.expiredays = Convert.ToDateTime(expiredays);
                    }
                    objs.paidfull = Convert.ToByte(paidfull);
                    //objs.payment_method = dataForm.payment_method;
                    if (configs == null)
                    {
                        objs.money = "0";
                    }
                    else
                    {
                        objs.money = configs.config_value.Trim();
                        money = configs.config_value.Trim();
                    }
                }
                db.Entry(objs).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //Save Data Firebases
                //Inser firebase
                string apiKey = ConfigurationManager.AppSettings.Get("apiKey");
                string authDomain = ConfigurationManager.AppSettings.Get("authDomain");
                string databaseURL = ConfigurationManager.AppSettings.Get("databaseURL");
                string projectId = ConfigurationManager.AppSettings.Get("projectId");
                string storageBucket = ConfigurationManager.AppSettings.Get("storageBucket");
                string messagingSenderId = ConfigurationManager.AppSettings.Get("messagingSenderId");

                string keyUpdate = email.Replace("@", "_").Replace(".", "-");
                string path = "/pilot_member/" + keyUpdate;
                //var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
                var objInsert = new Alert.Models.baseModel.insertFirebase
                {
                    email = dataForm.email,
                    isactive = Convert.ToByte(paidfull),
                    ischarge = 1,
                    isdelete = 0,
                    istrial = 1,
                    firstname = dataForm.firstname.Trim(),
                    lastname = dataForm.lastname.Trim(),
                    money_order = dataForm.money_order.Trim(),
                    password = password,
                    pnref_check = objs.pnref_check,
                    money = money,
                    expired = expireday
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
                //End firebase
                //Return 
                responseData.status = 1;
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                responseData.status = 0;
                responseData.msg = "";
                responseData.password = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public ActionResult deletes(FormCollection form)
        {
            var db = DBcontext.Context;
            string id = Request.Form["id"].ToString();
            int idDelete = Convert.ToInt32(id);
            var objs = db.alert_member.Where(s => s.id == idDelete).FirstOrDefault<alert_member>();
            //Update firebae
            //Inser firebase
            string apiKey = ConfigurationManager.AppSettings.Get("apiKey");
            string authDomain = ConfigurationManager.AppSettings.Get("authDomain");
            string databaseURL = ConfigurationManager.AppSettings.Get("databaseURL");
            string projectId = ConfigurationManager.AppSettings.Get("projectId");
            string storageBucket = ConfigurationManager.AppSettings.Get("storageBucket");
            string messagingSenderId = ConfigurationManager.AppSettings.Get("messagingSenderId");

            string email = objs.email.Trim();
            string keyUpdate = email.Replace("@", "_").Replace(".", "-");
            string path = "/pilot_member/" + keyUpdate;
            //var objectData = JsonConvert.DeserializeObject<Alert.Models.NoneObject.inputStreamMember>(content);
            var objInsert = new Alert.Models.baseModel.insertFirebase
            {
                isdelete = 1
            };
            string objString = new JavaScriptSerializer().Serialize(objInsert);
            //Curl
            string url = databaseURL + path + ".json?";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            byte[] bytes = Encoding.ASCII.GetBytes(objString);

            request.Method = "DELETE";
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
            //End update firebase

            int rep = memberModel.deletes(objs.id);
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            var responseData = new baseModel.responseData();
            responseData.status = rep;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        public string GenerateMD5(string yourString)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(yourString)).Select(s => s.ToString("x2")));
        }
        public string sendMail(string fullname, string idkey, string tomail)
        {
            try
            {
                string accountmail = ConfigurationManager.AppSettings.Get("accountmail");
                string passmail = ConfigurationManager.AppSettings.Get("passmail");
                int portmail = Convert.ToInt32(ConfigurationManager.AppSettings.Get("portmail"));
                string servermail = ConfigurationManager.AppSettings.Get("servermail");
                string protocolmail = ConfigurationManager.AppSettings.Get("protocolmail");


                string from = accountmail;
                string link = "http://alertahub.com/Complete/AccountWeb?key=" + tomail + "&pnref=" + fullname; 
                //string link = "http://localhost:26827/Complete/AccountWeb?key=" + tomail + "&pnref=" + fullname; 
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
        /// Tạo tài khoản bằng credit card
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public int CheckCardNumber(string email, string credit_card_number, string name_on_card, string password ,string expdate, string cvv)
        {
            string apiEndPointAPITest = ConfigurationManager.AppSettings.Get("apiEndPointAPITest");
            string apiEndPointAPIRelease = ConfigurationManager.AppSettings.Get("apiEndPointAPIRelease");
            string apiEndPointAPIStatus = ConfigurationManager.AppSettings.Get("apiEndPointAPIStatus");
            string VENDOR = ConfigurationManager.AppSettings.Get("VENDOR");
            string USER = ConfigurationManager.AppSettings.Get("USER");
            string PWD = ConfigurationManager.AppSettings.Get("PWD");
            string PARTNER = ConfigurationManager.AppSettings.Get("PARTNER");

            var responseData = new baseModel.responseData();
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
            requestParams += "&ACCT=" + credit_card_number.Trim();
            requestParams += "&EXPDATE=" + expdate.Trim();
            requestParams += "&CVV2=" + cvv.Trim();
            requestParams += "&AMT=0";
            requestParams += "&CURRENCYCODE=US";
            requestParams += "&PONUM=PFDCCTEST";
            requestParams += "&COMMENT1=COMMENT1";
            requestParams += "&COMMENT2=COMMENT2";
            requestParams += "&FIRSTNAME=" + name_on_card.Trim();
            requestParams += "&LASTNAME=";
            requestParams += "&STATE=" + "";
            requestParams += "&COUNTRYCODE=US";
            requestParams += "&ZIP=" + "";
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
                /*responseData.status = 0;
                responseData.msg = dataRes.msg;
                return Json(responseData, JsonRequestBehavior.AllowGet);*/
                return 2;
            }
            //Ghi vao database local
            int save = saveDataLocal(email, name_on_card, password, pnref, authcode);
            if (save != 1)
            {
                /*responseData.status = 0;
                responseData.msg = "Save Failed";
                return Json(responseData, JsonRequestBehavior.AllowGet);*/
                return 0;
            }
            string idkey = (email.Replace("@", "_")).Replace(".", "-");
            //Ghi Firebase
            firebase(email, name_on_card, idkey, password, pnref, authcode);
            //Gui mail thong bao xac nhan
            string sendMail = sentMail2(name_on_card, idkey, email, pnref);
            responseData.status = 1;
            responseData.msg = dataRes.msg;
            responseData.statusSendMail = sendMail;
            return 1; /*Json(responseData, JsonRequestBehavior.AllowGet);*/
        }
        public string sentMail2(string fullname, string idkey, string tomail, string pnref)
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
        private string firebase(string email, string name_on_card, string idkey, string password, string pnref, string authcode)
        {
            //Inser firebase
            string apiKey = ConfigurationManager.AppSettings.Get("apiKey");
            string authDomain = ConfigurationManager.AppSettings.Get("authDomain");
            string databaseURL = ConfigurationManager.AppSettings.Get("databaseURL");
            string projectId = ConfigurationManager.AppSettings.Get("projectId");
            string storageBucket = ConfigurationManager.AppSettings.Get("storageBucket");
            string messagingSenderId = ConfigurationManager.AppSettings.Get("messagingSenderId");

            string path = "/pilot_member/" + idkey;
            var objInsert = new Alert.Models.baseModel.insertFirebase
            {
                country = "",
                deviceid = "",
                email = email.Trim(),
                isactive = 0,
                ischarge = 1,
                isdelete = 0,
                istrial = 1,
                name_on_card = name_on_card.Trim(),
                password = password.Trim(),
                zipcode = "",
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
        public int saveDataLocal(string email, string name_on_card, string password, string pnref, string authcode)
        {
            string idkey = (email.Replace("@", "_")).Replace(".", "-");
            var obj = new alert_member();
            obj.idkey = idkey;
            obj.country = "";
            obj.email = email.Trim();
            obj.isstatus = 0;
            obj.ischarge = 1;
            obj.isdelete = 0;
            obj.payment_method = "0";
            obj.istrial = 1;
            obj.datecreate = DateTime.Now;
            obj.date_create = "";
            obj.name_on_card = name_on_card;
            obj.password = password;
            obj.zip_code = "";
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
    }
   
}