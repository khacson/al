using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using Alert.Models;
using Alert.Models.NoneObject;

namespace Alert.Controllers
{
    public class protocolController : Controller
    {
        //
        // GET: /protocol/
        public ActionResult Index()
        {
            var response = new serviceObject();
            response.status = 1;
            response.msg = "";
            response.acount = "acount";
            response.cvv = "cvv";
            response.balance = "100000000";
            return Json(response, JsonRequestBehavior.AllowGet);
        }
	}
}