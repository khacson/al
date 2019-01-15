using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alert.Models;
using Alert.Models.Entity;
using Newtonsoft.Json;

namespace Alert.Controllers
{
    public class GroupController : Controller
    {
        //
        // GET: /Group/
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
            var groups = groupModel.getList();
            ViewBag.groups = groups;
            var obj = new
            {
                total = groups.Count(),
                content = helperModel.RenderHelper.PartialView(this, "List", groups)
            };
            return Json(obj);
        }
        [HttpPost]
        public ActionResult save(FormCollection form)
        {
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            string search = Request.Form["search"].ToString();
            var data = JsonConvert.DeserializeObject<groupObject>(search);
            byte isdelete = 0;
            var obj = new alert_group();
            obj.group_name = data.group_name;
            obj.grouptyid = data.grouptyid;
            obj.usercreate = login.userName;
            obj.datecreate = DateTime.Now;
            obj.isdelete = isdelete;
            int res = groupModel.Save(obj);
            //Console.WriteLine(result.data);
            //Console.ReadKey();
            var responseData = new baseModel.responseData();
            responseData.status = res;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult edits(FormCollection form)
        {
            string search = Request.Form["search"].ToString();
            int id = int.Parse(Request.Form["id"].ToString());
            var data = JsonConvert.DeserializeObject<groupObject>(search);
            //Finds
            var finds = groupModel.Finds(id);
            var responseData = new baseModel.responseData();
            if (finds == null)
            {
                responseData.status = 0;
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            var obj = new alert_group();

            obj.group_name = data.group_name;
            obj.grouptyid = data.grouptyid;
            obj.dateupdate = DateTime.Now;
            obj.userupdate = login.userName;
            obj.isdelete = finds.isdelete;
            obj.datecreate = finds.datecreate;
            obj.usercreate = finds.usercreate;
            obj.paramss = finds.paramss;
            
            int res = groupModel.Update(obj, finds.id);
            responseData.status = res;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult deletes(FormCollection form)
        {
            int id = int.Parse(Request.Form["id"].ToString());
            int rep = groupModel.deletes(id);
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            var responseData = new baseModel.responseData();
            responseData.status = rep;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Permission(int id)
        {
            var menus = groupModel.getListMenu();
            ViewBag.menus = menus;
            ViewBag.id = id;
            return View();
        } 
	}
}