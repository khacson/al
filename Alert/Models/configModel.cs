using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models;
using Alert.Models.Entity;

namespace Alert.Models
{
    public class configModel
    {
        public static string getUniqueID()
        {
            Random random = new Random();
            string r = "";
            int i;
            for (i = 1; i < 7; i++)
            {
                r += random.Next(0, 9).ToString();
            }
            var timeNow = DateTime.Now;
            string strTime = timeNow.ToString("yyMMddHHmmss");
            return strTime+r;
        }
        public static int Save(Entity.alert_config obj)
        {
            try
            {
                var db = DBcontext.Context;
                db.alert_config.Add(obj);
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int Update(Entity.alert_config obj, int id)
        {
            var db = DBcontext.Context;
            try
            {
                var upd = db.alert_config.Where(s => s.id == id).FirstOrDefault<alert_config>();
                upd.config_name = obj.config_name;
                upd.config_value = obj.config_value;
                db.Entry(upd).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int deletes(int id)
        {
            var db = DBcontext.Context;
            try
            {
                db.alert_config.Where(o => o.id == id).ToList().ForEach(o => db.alert_config.Remove(o));
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static Entity.alert_config Finds(string config_key)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_config
                          where us.config_key == config_key
                          select us;
                return sql.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}