using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models;
using Alert.Models.Entity;
using System.Data.Entity;
using Newtonsoft.Json;

namespace Alert.Models
{
    public class subscriberModel
    {
        public static string getSearch(string search)
        {
            var searchs = JsonConvert.DeserializeObject<Alert.Models.NoneObject.subscriberObject>(search);
            string sql = "";
            if (searchs.member_type != "" && searchs.member_type != null)
            {
                sql += " and sc.member_type in ("+searchs.member_type+")";
            }
            if (searchs.member != "" && searchs.member != null)
            {
                sql += " and sc.member like '%" + searchs.member + "%' ";
            }
            if (searchs.os_type != "" && searchs.os_type != null)
            {
                sql += " and sc.os_type in (" + searchs.os_type + ")";
            }
            if (searchs.longitude != "" && searchs.longitude != null)
            {
                sql += " and sc.longitude like '%" + searchs.longitude + "%' ";
            }
            if (searchs.latitude != "" && searchs.latitude != null)
            {
                sql += " and sc.latitude like '%" + searchs.latitude + "%' ";
            }
            if (searchs.datecreate != "" && searchs.datecreate != null)
            {
                sql += "  and sc.datecreate >= '" + searchs.datecreate + " 00:00:00' and sc.datecreate <= '" + searchs.datecreate + " 23:59:59'";
            }
            return sql;
        }
        public static int getTotal(string search)
        {
            try
            {
                string and = getSearch(search);
                var db = DBcontext.Context;
                string query = " select * from alert_subscriber sc  where sc.isdelete = 0 ";
                query += and;
                return  db.alert_subscriber.SqlQuery(query).Count();
            }
            catch
            {
                return 0;
            }
        }
        public static List<Entity.alert_subscriber> getList(string search, int pageNum, int pageSize)
        {
            try
            {
                string and = getSearch(search);
                var db = DBcontext.Context;
                string query = " select * from alert_subscriber sc  where sc.isdelete = 0 ";
                query += and;
                query += " order by sc.datecreate desc";

                int rowsCount = getTotal(search);
                if (pageSize <= 0)
                {
                    pageSize = 20;
                }
                if (rowsCount <= pageSize || pageNum <= 0)
                {
                    pageNum = 1;
                }
                int page = (pageNum - 1) * pageSize;
                query += " offset " + page + " rows FETCH NEXT " + pageSize + " rows only";
                return db.alert_subscriber.SqlQuery(query).ToList();
                //return db.alert_subscriber.SqlQuery(query).Skip(page).Take(pageSize).ToList();
                /*
                var sql = (from sc in db.alert_subscriber
                           where (sc.isdelete == 0 && sc.member.Contains(searchs.member))
                          orderby sc.datecreate descending
                          select sc);
                int rowsCount = sql.Count();
                if (rowsCount <= pageSize || pageNum <= 0) {
                    pageNum = 1;
                }
                int page = (pageNum - 1) * pageSize;
                return sql.Skip(page).Take(pageSize).ToList();*/
            }
            catch
            {
                return null;
            }
            /*
             select *
            from alert_subscriber a
            where a.isdelete = 0 
            order by a.id asc
            offset 20 rows FETCH NEXT 20 rows only
             */
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