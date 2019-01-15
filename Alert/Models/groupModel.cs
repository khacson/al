using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alert.Models;
using Alert.Models.Entity;
using System.Data.Entity;

namespace Alert.Models
{
    public class groupModel
    {
        public static List<Entity.alert_group> getGroupAll()
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from gr in db.alert_group
                          where (gr.isdelete == 0)
                          orderby gr.group_name ascending
                          select gr;
                return sql.ToList();
            }
            catch
            {
                return null;
            }
        }
        public static List<Entity.alert_menu> getListMenu()
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from mn in db.alert_menu
                          where (mn.isdelete == 0)
                          orderby mn.ordering ascending
                          select mn;
                return sql.ToList();
            }
            catch
            {
                return null;
            }
        }
        public static List<Entity.alert_group> getList()
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from gr in db.alert_group
                          where (gr.isdelete == 0)
                          orderby gr.group_name ascending
                          select gr;
                return sql.ToList();
            }
            catch
            {
                return null;
            }
        }
        public static Entity.alert_group checkedGroupname(string groupname)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from gr in db.alert_group
                          where (gr.group_name == groupname.ToLower().Trim())
                          select gr;
                return sql.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public static Entity.alert_group Finds(int ids)
        {
            try
            {
                var db = DBcontext.Context;
                var sql = from gr in db.alert_group
                          where gr.id == ids
                          select gr;
                return sql.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public static int Save(Entity.alert_group obj)
        {
            try
            {
                var checkDuplicate = checkedGroupname(obj.group_name);
                if (checkDuplicate == null)
                {
                    var db = DBcontext.Context;
                    db.alert_group.Add(obj);
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
        public static int Update(Entity.alert_group obj, int id)
        {
            var db = DBcontext.Context;
            try
            {
                var upd = db.alert_group.Where(s => s.id == id).FirstOrDefault<alert_group>();
                upd.group_name = obj.group_name;
                upd.grouptyid = obj.grouptyid;
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
                db.alert_group.Where(o => o.id == id).ToList().ForEach(o => db.alert_group.Remove(o));
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public string getMenuRight(Array current = null)
        {
            return "";
            /*var db = DBcontext.Context;
            try
            {
                var sql = from mn in db.alert_menu
                          where (mn.parentid == 0  & mn.isdelete == 0)
                          select mn;
                var query = sql.ToList();
                string menu = "";
                int i = 0;
                int n = query.Count();
                int rchk_c = 0;
                foreach(var item in query){
                     int vid = item.id;
                     string vname = item.menu_name;
                     var asub = getChildren(vid, current);
                     
                     if (asub != null)
                     {
                         var sub = asub.sub;
                          var rchk = asub.chk;

                         menu+= "{id:\"node- "+vid+ "\",";
                         menu += "value:\"parent-" + vid + "\",";
                         menu += "text:\"" + vname + "\",";
                         menu += "showcheck:true";
                         menu += "complete:true";
                         menu += "isexpand:false";
                         menu += "checkstate:\"" + rchk + "\",";
                         menu += "hasChildren:true";
                         menu += "checkstate:\"" + sub + "\",";
                         menu += "}";
                         if (rchk == 1 || rchk == 2)
                         {
                             rchk_c++;
                         }
                     }
                     else
                     {
                         var parameter = item.parameter;
                         string[] separators = { ","};
                         string[] paramss = parameter.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                         string str_param = "";
                         int j = 0;
                         int m = parameter.Count();
                         //int l = (isset($current[$vid])) ? count($current[$vid]) : 0;
                        
                         int pos = Array.IndexOf(current, vid);
                         int l = 0;
                         if (pos > -1)
                         {
                             l = 1;
                         }
                         foreach (var items in paramss) {
                             int chk = Array.IndexOf(current, vid);
                         }
                     }
                     i++;
                }
            }
            catch
            {
                return null;
            }
            return null;
            // ViewBag.HTMLData = HttpUtility.HtmlEncode(htmlString);
           */
        }
        public baseModel.menu getChildren(int id, string current)
        {
            var menus = new baseModel.menu();
            menus.chk = 1;
            menus.sub = "";
            return menus;
        }
    }
}