using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models.Entity;

namespace Alert.Models
{
    public class DBcontext
    {
        private static readonly string keyContext = "sonnk";
        public static DbEntities Context
        {
            get
            {
                //Gets MOAC model context
                var moacContext = HttpContext.Current.Items[keyContext];
                if (moacContext != null)
                {
                    return (DbEntities)moacContext;
                }
                //Sets MOAC model context
                HttpContext.Current.Items[keyContext] = new DbEntities();
                return (DbEntities)HttpContext.Current.Items[keyContext];
            }
        }
    }
}