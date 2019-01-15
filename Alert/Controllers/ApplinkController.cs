using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Alert.Models;
using Alert.Models.Entity;
using Newtonsoft.Json;
using Alert.Models.NoneObject;


namespace Alert.Controllers
{
    public class ApplinkController : Controller
    {
        //
        // GET: /Applink/
        public ActionResult Index()
        {
            var login = Session[baseContanst.user_Login];
            if (login == null)
            {
                return RedirectToAction("Index", "Authorize");
            }
            return View();
        }
        [HttpPost]
        public ActionResult getList(FormCollection form)
        {
            var applink = ApplinkModel.getList();
            ViewBag.datas = applink;
            var obj = new
            {
                total = applink.Count(),
                content = helperModel.RenderHelper.PartialView(this, "List", applink)
            };
            return Json(obj);
        }
        [HttpPost]
        public ActionResult save(FormCollection form)
        {
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            string search = Request.Form["search"].ToString();
            var data = JsonConvert.DeserializeObject<applinkObject>(search);
            byte isdelete = 0;
            var obj = new alert_applink();
            obj.app_link = data.app_link;
            obj.app_name = data.app_name;
            obj.app_type = data.app_type;
            obj.usercreate = login.userName;
            obj.datecreate = DateTime.Now;
            obj.isdelete = isdelete;
            int res = ApplinkModel.Save(obj);
            var responseData = new baseModel.responseData();
            responseData.status = res;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
         [HttpPost]
        public ActionResult edits(FormCollection form)
        {
            string search = Request.Form["search"].ToString();
            int id = int.Parse(Request.Form["id"].ToString());
            var data = JsonConvert.DeserializeObject<applinkObject>(search);
            //Finds
            var finds = ApplinkModel.Finds(id);
            var responseData = new baseModel.responseData();
            if (finds == null)
            {
                responseData.status = 0;
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            var obj = new alert_applink();

            obj.app_link = data.app_link;
            obj.app_name = data.app_name;
            obj.app_type = data.app_type;
            obj.userupdate = login.userName;
            obj.dateupdate = DateTime.Now;

            int res = ApplinkModel.Update(obj, finds.id);
            responseData.status = res;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult deletes(FormCollection form)
        {
            int id = int.Parse(Request.Form["id"].ToString());
            int rep = ApplinkModel.deletes(id);
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            var responseData = new baseModel.responseData();
            responseData.status = rep;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
	}
}