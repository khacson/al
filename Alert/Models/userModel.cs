using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models;
using Alert.Models.Entity;
using System.Data.Entity;

namespace Alert.Models
{
    public class userModel
    {
        public static List<Entity.alert_user> getList()
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_user
                          where (us.isdelete == 0)
                          orderby us.fullname descending
                          select us;
                return sql.ToList();
            }
            catch
            {
                return null;
            }
        }
        public static Entity.alert_user checkedUsername(string username)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_user
                          where (us.username == username.ToLower().Trim())
                          select us;
                return sql.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public static Entity.alert_user Finds(int ids)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_user
                          where us.id == ids
                          select us;
                return sql.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public static int Save(Entity.alert_user obj)
        {
            try
            {
                var checkDuplicate = checkedUsername(obj.username);
                if (checkDuplicate == null)
                {
                    var db = DBcontext.Context;
                    db.alert_user.Add(obj);
                    db.SaveChanges();
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static int Update(Entity.alert_user obj, int id)
        {
            var db = DBcontext.Context;
            try
            {
                var upd = db.alert_user.Where(s => s.id == id).FirstOrDefault<alert_user>();
                upd.avatar = obj.avatar;
                upd.fullname = obj.fullname;
                upd.username = obj.username;
                upd.password = obj.password;
                upd.phone = obj.phone;
                upd.email = obj.email;
                upd.groupid = obj.groupid;
                upd.groupid = obj.groupid;
                upd.isactive = obj.isactive;
                upd.dateupdate = obj.dateupdate;
                upd.userupdate = obj.userupdate;
                upd.isdelete = obj.isdelete;
                upd.datecreate = obj.datecreate;
                upd.usercreate = obj.usercreate;

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
                db.alert_user.Where(o => o.id == id).ToList().ForEach(o=> db.alert_user.Remove(o));
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
    }
}