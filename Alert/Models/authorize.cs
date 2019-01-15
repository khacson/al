using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models.Entity;

namespace Alert.Models
{
    public class authorize
    {
        public static Alert.Models.Entity.alert_user checkedLogin(string username, string password){
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_user
                          where (us.username == username & us.password == password.ToLower().Trim())
                          select us;
                return sql.FirstOrDefault();
            }
            catch{
                return null;
            }
        }
    }
}