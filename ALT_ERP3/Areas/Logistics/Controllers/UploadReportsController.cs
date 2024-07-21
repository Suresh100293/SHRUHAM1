using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class UploadReportsController : BaseController
    {
        // GET: Logistics/UploadReports
        public ActionResult Index()
        {
            GetAllMenu(Session["ModuleName"].ToString());
            string path = Server.MapPath("~/Reports/");
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] files = dirInfo.GetFiles("*.rpt");
            List<string> lst = new List<string>(files.Length);
            foreach (var item in files)
            {
                lst.Add(item.Name);
            }
            ViewData["FileList"] = lst;
            return View(lst);
        }


        [HttpPost]
        public ActionResult AttachDocument(HttpPostedFileBase files, string DocPath)
        {
            string XYZ = "";
            string docstr = "";



            byte[] fileData = null;
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {

                    string attachmentPath = Server.MapPath("~") + @"\Reports";

                    var file = Request.Files[i];
                    var fileName = Path.GetFileName(file.FileName);

                    MemoryStream target = new MemoryStream();
                    using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                    {
                        fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                    }

                    string directoryPath = attachmentPath + @"\" + fileName;

                    System.IO.File.WriteAllBytes(directoryPath, fileData);
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Message = ex.InnerException.Message,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }



            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DownloadFile(string filename)
        {
            if (Path.GetExtension(filename) == ".rpt")
            {
                string fullPath = Path.Combine(Server.MapPath("~/Reports/"), filename);
                return File(fullPath, "application/octet-stream", filename);
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }
        }
    }
}