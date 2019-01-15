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
    public class memberModel
    {
        public static string getSearch(string search)
        {
            var searchs = JsonConvert.DeserializeObject<Alert.Models.NoneObject.memberObject>(search);
            string sql = "";
            if (searchs.name_on_card != "")
            {
                sql += " and sc.name_on_card like '%" + searchs.name_on_card + "%' ";
            }
            if (searchs.isdelete  != null)
            {
                sql += " and sc.isdelete in (" + searchs.isdelete + ") ";
            }
            if (searchs.isstatus != null)
            {
                sql += " and sc.isstatus in (" + searchs.isstatus + ") ";
            }
            return sql;
        }
        public static int getTotal(string search)
        {
            try
            {
                string and = getSearch(search);
                var db = DBcontext.Context;
                string query = " select * from alert_member sc  where 1 = 1 ";
                query += and;
                return db.alert_member.SqlQuery(query).Count();
            }
            catch
            {
                return 0;
            }
        }
        public static List<Entity.alert_member> getList(string search, int pageNum, int pageSize)
        {
            try
            {
                string and = getSearch(search);
                var db = DBcontext.Context;
                string query = " select * from alert_member sc  where 1 = 1 ";
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
                return db.alert_member.SqlQuery(query).ToList();
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
        public static int Save(Entity.alert_member obj)
        {
            try
            {
                var db = DBcontext.Context;
                db.alert_member.Add(obj);
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static int Update(Entity.alert_member obj, int id)
        {
            var db = DBcontext.Context;
            try
            {
                var original = db.alert_member.Find(id);
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
                string sql = " select * from alert_member sc  where sc.id in ("+id+")";
                var datas = db.alert_member.SqlQuery(sql).ToList();
                foreach (var item in datas)
                {
                    int iddelete = item.id;
                    db.alert_member.Where(o => o.id == iddelete).ToList().ForEach(o => db.alert_member.Remove(o));
                    db.SaveChanges();
                }
               
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static Entity.alert_member Finds(string idkey)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_member
                          where us.idkey == idkey
                          select us;
                return sql.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public static Entity.alert_member checkEmail(string email)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from us in db.alert_member
                          where us.email == email
                          where us.isdelete == 0
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