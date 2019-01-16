using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Alert.Models;
using Alert.Models.Entity;
using System.Net;
using System.Text;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace Alert.Controllers
{
    public class CompleteController : Controller
    {
        //
        // GET: /Complete/
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Account()
        {
            string idkey = this.Request.QueryString["key"].Trim();
            //string pnref = this.Request.QueryString["pnref"];
            //Update Memer Local DB
            var db = DBcontext.Context;
            var obj = db.alert_member.Where(s => s.idkey == idkey && s.isdelete == 0).FirstOrDefault<alert_member>();
            if (obj == null)
            {
                return View("Error");
            }
            else if (obj.isstatus == 1)
            {
                ViewBag.name = obj.name_on_card;
                return View("Activate");
            }
            string pnref = obj.pnref_check;
            //Tao profile
            var objRes = createProflie(obj.id, pnref, obj.name_on_card);
            if (objRes == "")
            {
                return View("Error");
            }
            ViewBag.name = obj.name_on_card;
            return View("Success");
        }
        public ActionResult AccountWeb()
        {
            string idkey = this.Request.QueryString["key"].Trim();
            //int id = Convert.ToInt32(idkey);
            var db = DBcontext.Context;
            var obj = db.alert_member.Where(s => s.email == idkey).FirstOrDefault<alert_member>();
            if (obj == null)
            {
                ViewBag.name = "";
                return View("Error");
            }
            else
            {
                //Tim thay
                var objs = db.alert_member.Where(s => s.id == obj.id).FirstOrDefault<alert_member>();
                objs.isstatus = 2;
                objs.dateactive = DateTime.Now; 
                db.Entry(objs).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                ViewBag.name = obj.lastname + " " + obj.firstname;
                return View("Activate");
            }
            ViewBag.name = obj.firstname;
            return View("Success");
        }
        public string createProflie(int id, string pnref_check, string fullname)
        {
            string apiEndPointAPITest = ConfigurationManager.AppSettings.Get("apiEndPointAPITest");
            string apiEndPointAPIRelease = ConfigurationManager.AppSettings.Get("apiEndPointAPIRelease");
            string apiEndPointAPIStatus = ConfigurationManager.AppSettings.Get("apiEndPointAPIStatus");
            string VENDOR = ConfigurationManager.AppSettings.Get("VENDOR");
            string USER = ConfigurationManager.AppSettings.Get("USER");
            string PWD = ConfigurationManager.AppSettings.Get("PWD");
            string PARTNER = ConfigurationManager.AppSettings.Get("PARTNER");

            
            
            var responseData = new baseModel.responseData();
            //string profileid = checkEmail.profileid.Trim();
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
            //getConfig
            var config = configModel.Finds("recurring_payments");
            string atm = "0";
            if (config != null)
            {
                atm = config.config_value;
            }
            DateTime date = DateTime.Now;
            date = date.AddDays(1);
            string requestParams = "VERBOSITY=HIGH";
            requestParams += "&TENDER=C";
            requestParams += "&TRXTYPE=R";
            requestParams += "&ACTION=A";
            requestParams += "&PROFILENAME=" + fullname.Trim();
            requestParams += "&ORIGID=" + pnref_check.Trim();
            requestParams += "&START=" + date.ToString("MMddyyyy") ;
            requestParams += "&PAYPERIOD=MONT"; //Theo thang
            requestParams += "&TERM=24" ; //
            requestParams += "&OPTIONALTRX=S"; //
            requestParams += "&OPTIONALTRXAMT=" + atm.Trim(); // //Phi ban dau
            requestParams += "&COMMENT1=Create Profile"; //
            requestParams += "&AMT=" + atm.Trim(); //

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
            string PROFILEID = "";
            string TRXPNREF = "";
            string AUTHCODE = "";
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
                if (key == "TRXPNREF")
                {
                    TRXPNREF = values;
                }
                if (key == "PROFILEID")
                {
                    PROFILEID = values;
                }
                if (key == "AUTHCODE")
                {
                    AUTHCODE = values;
                }
            }
            if (Convert.ToInt32(ACK) != 0)
            {
                return "";
            }
            else
            {
                //Update Memer Local DB
                var db = DBcontext.Context;
                var obj = db.alert_member.Where(s => s.id == id).FirstOrDefault<alert_member>();
                obj.isstatus = 1;
                obj.dateactive = DateTime.Now;
                obj.pnref_profile = RPREF;
                obj.authcode_profile = AUTHCODE;
                obj.trxpnref_profile = TRXPNREF;
                obj.profileid = PROFILEID;
                db.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //Update Firebase
                firebaseActive(id, RPREF);
                return RPREF;
            }
        }

        public Object firebaseActive(int id, string RPREF)
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
            var objInsert = new Alert.Models.baseModel.insertFirebaseActive
            {
                isactive = 1,
                profileid = objectData.profileid.Trim(),
                pnref_profile = objectData.pnref_profile.Trim(),
                authcode_profile = objectData.authcode_profile.Trim(),
                trxpnref_profile = objectData.trxpnref_profile.Trim(),
                dateactive = DateTime.Now

                /*country = objectData.country.Trim(),
                deviceid = "",
                email = objectData.email.Trim(),
                isactive = 1,
                ischarge = 1,
                isdelete = 0,
                istrial = 1,
                name_on_card = objectData.name_on_card.Trim(),
                password = objectData.password.Trim(),
                zipcode = objectData.zip_code.Trim(),
                profileid = objectData.profileid.Trim(),
                pnref_check = objectData.pnref_check.Trim(),
                authcode_check = objectData.authcode_check.Trim(),
                pnref_profile = objectData.pnref_profile.Trim(),
                authcode_profile = objectData.authcode_profile.Trim(),
                trxpnref_profile = objectData.trxpnref_profile.Trim(),
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

            var responseData = new baseModel.responseData();
            responseData.status = 1;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        public string content { get; set; }
    }
}