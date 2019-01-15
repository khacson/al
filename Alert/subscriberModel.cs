using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models;
using Alert.Models.Entity;
using System.Data.Entity;

namespace Alert.Models
{
    public class subscriberModel
    {
        public static int getTotal()
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from sc in db.alert_subscriber
                          where (sc.isdelete == 0)
                          orderby sc.datecreate descending
                          select sc;
                return sql.Count();
            }
            catch
            {
                return 0;
            }
        }
        public static List<Entity.alert_subscriber> getList(int pageNum, int pageSize)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from sc in db.alert_subscriber
                          where (sc.isdelete == 0)
                          orderby sc.datecreate descending
                          select sc;
                int rowsCount = sql.Count();
                if (pageSize <= 0) {
                    pageSize = 20;
                }
                if (rowsCount <= pageSize || pageNum <= 0) {
                    pageNum = 1;
                }
                int page = (pageNum - 1) * pageSize;
                return sql.Skip(page).Take(pageSize).ToList();
            }
            catch
            {
                return null;
            }
        }
        public static int Save(Entity.alert_subscriber obj)
        {
            try
            {
                var db = DBcontext.Context;
                db.alert_subscriber.Add(obj);
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int Update(Entity.alert_subscriber obj, int id)
        {
            var db = DBcontext.Context;
            try
            {
                var original = db.alert_subscriber.Find(id);
                if (original != null)
                {
                    db.Entry(original).CurrentValues.SetValues(obj);
                    db.SaveChanges();
                }
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
                db.alert_subscriber.Where(o => o.id == id).ToList().ForEach(o => db.alert_subscriber.Remove(o));
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static Entity.alert_subscriber Finds(long idkey)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_subscriber
                          where us.idkey == idkey
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