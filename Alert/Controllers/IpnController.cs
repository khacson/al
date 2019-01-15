using Alert.Models;
using Alert.Models.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Alert.Controllers
{
    public class IpnController : Controller
    {
        //
        // GET: /Ipn/
        public ActionResult Index()
        {
            return View();
        }
        public int PaypalCheck()
        {
            using (var reader = new StreamReader(Request.InputStream))
                content = reader.ReadToEnd();

            var db = DBcontext.Context;
            var obj = new alert_logfile();
            obj.logfile = content;
            db.alert_logfile.Add(obj);
            db.SaveChanges();
            return 1;
        }
        public string content { get; set; }
	}
}