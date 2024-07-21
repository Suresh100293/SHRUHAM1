using ALT_ERP3.Controllers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Models
{
    public class DatabaseBackup : BaseController
    {
        public string DBBackup()
        {
            string merror = "";
            try
            {
                string mdb = System.Web.HttpContext.Current.Session["CurrentDatabase"].ToString();
                string mpath = @"C:\Backup\";//System.Web.HttpContext.Current.Server.MapPath("~/Backup/");
                try { System.IO.Directory.CreateDirectory(mpath); } catch { }
                string mstr = @"BACKUP DATABASE " + mdb;
                mstr += @" TO DISK='" + mpath + "\\" + mdb + ".bak' WITH FORMAT,";
                mstr += @" NAME = 'Backup: " + mdb + ", Dated: " + DateTime.Now + ", User: " + muserid + "'";
                ExecuteStoredProc(mstr);
                
                // compress the file
                string mzippath = mpath + "\\" + mdb + ".zip";
                if (System.IO.File.Exists(mzippath))
                {
                    System.IO.File.Delete(mzippath);
                }
                mpath += "\\" + mdb + ".bak";
                ZipArchive zip = ZipFile.Open(mzippath, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(mpath, mdb + ".bak");
                zip.Dispose();
            }
            catch (Exception mex)
            {
                merror = mex.Message;
            }
            return merror;
        }


        public void ExecuteStoredProc(string mSQLQuery, string connstring = "", SqlConnection conn = null)
        {
            bool mnew = false;
            if (connstring == "") connstring = GetConnectionString();
            if (conn == null)
            {
                mnew = true;
                conn = new SqlConnection(connstring);
            }
            SqlCommand cmd = new SqlCommand(mSQLQuery, conn);
            try
            {
                if (mnew) conn.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            catch (Exception mex)
            {
            }
            finally
            {
                cmd.Dispose();
                if (mnew)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}