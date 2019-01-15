using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models;
using Alert.Models.Entity;
using System.Data.Entity;

namespace Alert.Models
{
    public class ApplinkModel
    {
        public static List<Entity.alert_applink> getList()
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from gr in db.alert_applink
                          where (gr.isdelete == 0)
                          orderby gr.app_name ascending
                          select gr;
                return sql.ToList();
            }
            catch
            {
                return null;
            }
        }
        public static int Save(Entity.alert_applink obj)
        {
            try
            {
                var db = DBcontext.Context;
                db.alert_applink.Add(obj);
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int Update(Entity.alert_applink obj, int id)
        {
            var db = DBcontext.Context;
            try
            {
                var upd = db.alert_applink.Where(s => s.id == id).FirstOrDefault<alert_applink>();
                upd.app_name = obj.app_name;
                upd.app_type = obj.app_type;
                upd.app_link = obj.app_link;
                upd.userupdate = obj.userupdate;
                upd.dateupdate = obj.dateupdate;
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
                db.alert_applink.Where(o => o.id == id).ToList().ForEach(o => db.alert_applink.Remove(o));
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static Entity.alert_applink Finds(int ids)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from gr in db.alert_applink
                          where gr.id == ids
                          select gr;
                return sql.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}