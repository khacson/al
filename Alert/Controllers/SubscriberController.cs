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
    public class SubscriberController : Controller
    {
        //
        // GET: /Subscriber/
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult viewMap(FormCollection form)
        {
            string latitude = Request.Form["latitude"].ToString();
            string longitude = Request.Form["longitude"].ToString();
            ViewBag.latitude = latitude;
            ViewBag.longitude = longitude;
            var obj = new
            {
                longitude = longitude,
                latitude = latitude,
                content = helperModel.RenderHelper.PartialView(this, "viewMap", longitude)
            };
            return Json(obj);
        }
        [HttpPost]
        public ActionResult getList(FormCollection form)
        {
            string search = Request.Form["search"].ToString();
            int pageNum = Convert.ToInt32(Request.Form["page"].ToString()); 
            int pageSize = 20;
            var datas = subscriberModel.getList(search, pageNum, pageSize);
            int total = subscriberModel.getTotal(search);
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
            var obj = new alert_subscriber();
            byte isdelete = 0;
            var idkey = Convert.ToInt64(Request.Form["key"].ToString());
            obj.idkey = idkey; 
            obj.ids = Convert.ToInt32(Request.Form["id"].ToString());
            obj.member_type = Convert.ToByte(Request.Form["membertype"].ToString());
            obj.member = Request.Form["memberid"].ToString();
            obj.os_type = Request.Form["ostype"].ToString();
            obj.longitude = Request.Form["longitude"].ToString();
            obj.latitude = Request.Form["latitude"].ToString();
            obj.isdelete = isdelete;
            obj.datecreate = Convert.ToDateTime(Request.Form["datecreate"].ToString());
            var finds = subscriberModel.Finds(idkey);
            int res = 0;
            if (finds == null)
            {
                res = subscriberModel.Save(obj);
            }
            else
            {
                res = subscriberModel.Update(obj, finds.id);
            }
            var responseData = new baseModel.responseData();
            responseData.status = res;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
       [HttpPost]
       public ActionResult deletes(FormCollection form)
       {
           int id = int.Parse(Request.Form["id"].ToString());
           int rep = subscriberModel.deletes(id);
           var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
           var responseData = new baseModel.responseData();
           responseData.status = rep;
           responseData.msg = "";
           return Json(responseData, JsonRequestBehavior.AllowGet);
       }
	}
}

