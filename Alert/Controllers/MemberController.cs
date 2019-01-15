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
    public class MemberController : Controller
    {
        //
        // GET: /Member/
        public ActionResult Index()
        {
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
            byte isdelete = 0;

            var obj = new alert_member();
            string idkey = Request.Form["idkey"].ToString();
            obj.idkey = idkey;
            obj.country = Request.Form["country"].ToString();
            obj.card_number = Request.Form["credit_card_number"].ToString();
            obj.cvv = Request.Form["cvv"].ToString();
            obj.email = Request.Form["email"].ToString();
            obj.expdate = Request.Form["expdate"].ToString();
            obj.isstatus = Convert.ToByte(Request.Form["isactive"].ToString());
            obj.ischarge = Convert.ToByte(Request.Form["ischarge"].ToString());
            obj.isdelete = isdelete;
            obj.istrial = Convert.ToByte(Request.Form["istrial"].ToString());
            obj.date_create = Request.Form["datecreate"].ToString();
            obj.name_on_card = Request.Form["name_on_card"].ToString();
            obj.password = Request.Form["password"].ToString();
            obj.zip_code = Request.Form["zip_code"].ToString();
            var finds = Alert.Models.memberModel.Finds(idkey);
            int res = 0;
            if (finds == null)
            {
                res = memberModel.Save(obj);
            }
            else
            {
                res = memberModel.Update(obj, finds.id);
            }
            //Return 
            var responseData = new baseModel.responseData();
            responseData.status = 0;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        /*public ActionResult edits(FormCollection form)
        {
            var responseData = new baseModel.responseData();
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            var obj = new alert_member();

            obj.isdelete = Convert.ToByte(Request.Form["isdelete"].ToString()); 

            int id = Convert.ToInt32(Request.Form["id"].ToString());
            var finds = Alert.Models.memberModel.FindsID(id);
            if (finds != null)
            {
                var db = DBcontext.Context;
                var objs = db.alert_member.Where(s => s.id == id).FirstOrDefault<alert_member>();
                objs.isdelete = 1;
                objs.correlationid_cancel = "";
                objs.datecancel = DateTime.Now;
                db.Entry(objs).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                //Return 
                responseData.status = 1;
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                responseData.status = 0;
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
        }*/
        [HttpPost]
        public ActionResult deletes(FormCollection form)
        {
            int id = int.Parse(Request.Form["id"].ToString());
            int rep = memberModel.deletes(id);
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin;
            var responseData = new baseModel.responseData();
            responseData.status = rep;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
    }
}