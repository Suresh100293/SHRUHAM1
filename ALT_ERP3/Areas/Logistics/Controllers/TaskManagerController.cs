using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class TaskManagerController : BaseController
    {
        // GET: Logistics/TaskManager
        private static string msubcodeof = "";

        public ActionResult GetTypes(string term)
        {
            List<TfatPass> tfatPasses = new List<TfatPass>();

            if (muserid == "Super")
            {
                tfatPasses.AddRange(ctxTFAT.TfatPass.ToList());
                if (!(String.IsNullOrEmpty(term)))
                {
                    tfatPasses = tfatPasses.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
                }
            }
            else
            {
                TfatPass tfatPass = ctxTFAT.TfatPass.Where(x => x.Code == muserid).FirstOrDefault();
                tfatPasses.Add(tfatPass);
                if (!String.IsNullOrEmpty(tfatPass.UserList))
                {
                    var ListChildUser = tfatPass.UserList.Split(',').ToList();
                    var List = ctxTFAT.TfatPass.Where(x => ListChildUser.Contains(x.Code)).ToList();
                    tfatPasses.AddRange(List);
                }
                if (!(String.IsNullOrEmpty(term)))
                {
                    tfatPasses = tfatPasses.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
                }
            }



            var Modified = tfatPasses.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak") && x.DefaultReport == true).Select(x => new { x.Code, x.DrillQuery }).FirstOrDefault();
            if (result != null)
            {
                ViewBag.id = result.Code.Trim();
                ViewBag.ViewDataId = result.Code.Trim();
                ViewBag.DrillQuery = result.DrillQuery == null ? "" : result.DrillQuery.Trim();
            }
            else
            {
                var result2 = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == Model.ViewDataId && !x.Code.EndsWith(".bak")).Select(x => new { x.Code, x.DrillQuery }).FirstOrDefault();
                if (result2 != null)
                {
                    ViewBag.id = result2.Code.Trim();
                    ViewBag.ViewDataId = result2.Code.Trim();
                    ViewBag.DrillQuery = result2.DrillQuery == null ? "" : result2.DrillQuery.Trim();
                }
            }
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            Model.Grp = muserid;


            if (muserid.ToLower().Trim() == "super")
            {
                Model.ScheduleFlow = true;
            }
            Model.mVar1 = muserid;
            Model.mVar2 = ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.Name).FirstOrDefault();

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            //ITransactionGridOperation mIlst = new TransactionGridOperation();
            return GetGridDataColumns(Model.id==null? "AssignTasks" : Model.id, "L", "EDVX", "");
        }

        public ActionResult GetGridData(GridOption Model)
        {
            Model.Code = muserid;
            if (!String.IsNullOrEmpty(Model.mVar3) && Model.mVar3!= "undefined")
            {
                Model.Code = Model.mVar3;
            }
            return GetGridReport(Model, "M", "Code^" + Model.Code + (mpara != "" ? "~" + mpara : ""), false, 0);
        }

        public ActionResult GetSubGridStructureTask(GridOption Model)
        {
            int xvar = Convert.ToInt32(Model.Document);
            var mvar = ctxTFAT.Task.Where(z => z.Code == xvar).Select(x => x).FirstOrDefault();
            string mvar1 = Model.Document + "|";
            if (mvar != null)
            {
                mvar1 += mvar.Descr + "|";
                mvar1 += mvar.AssignedBy + "|";
                mvar1 += RecToString("Select AssignedTo from Task Where Code=" + Model.Document, ",") + "|";
                mvar1 += mvar.Status + "|";
                mvar1 += mvar.Priority + "|";
                mvar1 += mvar.StartDate + "|";
                mvar1 += mvar.EndDate + "|";
                if (mvar.Status != "In-Progress")
                {
                    mvar1 += "NA" + "|";
                    mvar1 += "NA" + "|";
                }
                else
                {
                    mvar1 += mvar.aStartDate.ToString() + "|";
                    mvar1 += mvar.aEndDate.ToString() + "|";
                }
                mvar1 += mvar.Narr ?? "" + "|";
                mvar1 += mvar.PercentComplete ==null? "0": mvar.PercentComplete.ToString() + "|";
                
            }
            Model.mVar1 = mvar1;
            return GetGridDataColumns(Model.id, "X", "", Model.mVar1, Model.mVar2, Model.mVar3, Model.mVar4, Model.mVar5);
            //return GetSubGridStructure(Model);
        }
    }
}