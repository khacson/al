using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alert.Models;
using Newtonsoft.Json;
using Alert.Models.Entity;

namespace Alert.Controllers
{
    public class ConfigController : Controller
    {
        //
        // GET: /Config/
        public ActionResult Index()
        {
            var login = Session[baseContanst.user_Login];
            if (login == null)
            {
                return RedirectToAction("Index", "Authorize"); 
            }
            var logins = login as Alert.Models.baseModel.userLogin;
            ViewBag.username = logins.userName;
            return View();
        }
        public ActionResult getUniqueID()
        {
            var Now = DateTime.Now;
            string strTime = Now.ToString("yyyy-MM-dd HH:mm:ss");

            var response = new baseModel.getUniqueID();
            response.uniqueid = configModel.getUniqueID();
            response.timeNow = strTime;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Save(FormCollection form)
        {
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            byte isdelete = 0;
            var obj = new alert_config();
            string config_key = Request.Form["config_key"].ToString();
            string config_name = Request.Form["config_name"].ToString();
            string config_value = Request.Form["config_value"].ToString();

            obj.config_key = config_key;
            obj.config_name = config_name;
            obj.config_value = config_value;
            obj.isdelete = isdelete;
            var finds = configModel.Finds(config_key);
            int res = 0;
            if (finds == null)
            {
                obj.datecreate = DateTime.Now;
                obj.usercreate = login.userName;
                res = configModel.Save(obj);
            }
            else
            {
                obj.dateupdate = DateTime.Now;
                obj.userupdate = login.userName;
                res = configModel.Update(obj, finds.id);
            }

            var responseData = new baseModel.responseData();
            responseData.status = 1;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
	}
}