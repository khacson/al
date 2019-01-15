using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Alert.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            string html = "";
            html += "<script type='text/javascript'>";
            html += "alert(12);";
            html += "<script>";
            JavaScript(html);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}