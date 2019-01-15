using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alert.Models;
using Alert.Models.Entity;
using System.IO;
using Newtonsoft.Json;
using System.Text;
//using Alert.Models.Entity;
using System.Security.Cryptography;

//using System.Web.Script.Serialization;

namespace Alert.Controllers
{
    public class UserController : Controller
    {
        // GET: /User/
        public ActionResult Index()
        {
            var login = Session[baseContanst.user_Login];
            if (login == null)
            {
                return RedirectToAction("Index", "Authorize");
            }
            var groups = groupModel.getGroupAll();
            ViewBag.grous = groups; 
            return View();
        }
        [HttpPost]
        public ActionResult getList(FormCollection form)
        {
            var users = userModel.getList();
            ViewBag.users = users;
            var obj = new
            {
                total = users.Count(),
                content = helperModel.RenderHelper.PartialView(this, "List", users)
            };
            return Json(obj);
        }
        [HttpPost]
        public ActionResult save(FormCollection form)
        {
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin; 
            string search = Request.Form["search"].ToString();
            var data = JsonConvert.DeserializeObject<Alert.Models.userObject>(search);
            byte isdelete = 0;
            var obj = new alert_user();
            HttpPostedFileBase file = Request.Files["avatarfile"];
            if (file != null && file.ContentLength > 0)
            {
                string path = Path.Combine(Server.MapPath("~/Content/images"), Path.GetFileName(file.FileName));
                file.SaveAs(path);
                obj.avatar = file.FileName;
            }
            else
            {
                obj.avatar = "";
            }
            obj.fullname = data.fullName;
            obj.username = data.username;
            obj.password = GenerateMD5(data.password);
            obj.phone = data.phone;
            obj.email = data.email;
            obj.groupid = data.groupid;
            obj.groupid = data.groupid;
            obj.isactive = data.isactive;
            obj.isdelete = isdelete;
            obj.usercreate = login.userName;
            obj.datecreate = DateTime.Now; 
            int res = userModel.Save(obj);
            //Console.WriteLine(result.data);
            //Console.ReadKey();
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
            var data = JsonConvert.DeserializeObject<userObject>(search);
            //Finds
            var finds = userModel.Finds(id);
            var responseData = new baseModel.responseData();
            if (finds == null)
            {               
                responseData.status = 0;
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
            var login = Session[baseContanst.user_Login] as Alert.Models.baseModel.userLogin; 
            var obj = new alert_user();
            //Up load File
            HttpPostedFileBase file = Request.Files["avatarfile"];
            if (file != null && file.ContentLength > 0)  {
                string path = Path.Combine(Server.MapPath("~/Content/images"),  Path.GetFileName(file.FileName));  
                 file.SaveAs(path);  
                obj.avatar = file.FileName;
            }
            else{
                obj.avatar = finds.avatar;
            }
            //byte isdelete = 0;
           
            obj.fullname = data.fullName;
            obj.username = finds.username;
            if (data.password != null && data.password != "")
            {
                obj.password = GenerateMD5(data.password);
            }
            else
            {
                obj.password = finds.password;
            }
            obj.id = finds.id;
            obj.phone = data.phone;
            obj.email = data.email;
            obj.groupid = data.groupid;
            obj.groupid = data.groupid;
            obj.isactive = data.isactive;
            obj.dateupdate = DateTime.Now;
            obj.userupdate = login.userName;

            obj.isdelete = finds.isdelete;
            obj.datecreate = finds.datecreate;
            obj.usercreate = finds.usercreate;

            int res = userModel.Update(obj, finds.id);
            responseData.status = res;
            responseData.msg = "";
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
         public ActionResult deletes(FormCollection form)
         {
             int id = int.Parse(Request.Form["id"].ToString());
             int rep = userModel.deletes(id);
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
    }
}