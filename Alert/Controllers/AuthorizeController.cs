using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alert.Models;
using Alert.Models.Entity;
using System.Security.Cryptography;
using System.Text;

namespace Alert.Controllers
{
    public class AuthorizeController : Controller
    {
        //
        // GET: /authorize/
        public ActionResult Index()
        {

            var login = Session[baseContanst.user_Login];
            if (login == null)
            {
                var applink = ApplinkModel.getList();
                ViewBag.datas = applink;
                return View();
            }
            else
            {
                //return Content("<script>window.location = 'home';</script>");
                return RedirectToAction("Index", "Home");
            }
           
        }
        [HttpPost]
        public ActionResult checkLogin(FormCollection form)
        {
            string email = Request.Form["email"].ToString();
            string password = Request.Form["password"].ToString();
            string hashed = GenerateMD5(password);
            var finds = authorize.checkedLogin(email, hashed);
            if (finds == null)
            {
                var responseData = new baseModel.responseData();
                responseData.status = 0;
                responseData.msg = "";
                return Json(responseData, JsonRequestBehavior.AllowGet);
                //return new JsonResult { Data = responseData };
            }
            else
            {
                var userLogin = new baseModel.userLogin ();
                userLogin.userID = finds.id;
                userLogin.userName = finds.username;
                userLogin.fullName = finds.fullname;
                userLogin.avatar = finds.avatar;
                Session.Add(baseContanst.user_Login,userLogin);

                var responseData = new baseModel.responseData();
                responseData.status = 1;
                responseData.msg = finds.username; 
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }
        }
        public string GenerateMD5(string yourString)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(yourString)).Select(s => s.ToString("x2")));
        }
        public ActionResult logout()
        {
            Session.RemoveAll();
            Session.Clear();
            //var login = Session[baseContanst.user_Login];
            return RedirectToAction("Index", "Authorize");
        }
	}
}