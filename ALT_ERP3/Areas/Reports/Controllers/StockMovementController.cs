/* -----------------------------------------
   Copyright 2019, Suchan Software Pvt. Ltd.
   ----------------------------------------- */
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class StockMovementController : ReportController
    {
        // GET: Reports/ReportCentre
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            string murl = Request.Url.ToString();
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", murl, "A");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = GetEffectiveDate().ToShortDateString();
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmaintype = Model.MainType;
            var reportheader = ctxTFAT.ReportHeader.Where(z => z.SubCodeOf == msubcodeof && z.DefaultReport == true).Select(x => new { x.Code, x.InputPara }).FirstOrDefault();
            if (reportheader == null)
            {
                reportheader = ctxTFAT.ReportHeader.Where(z => z.SubCodeOf == msubcodeof).Select(x => new { x.Code, x.InputPara }).FirstOrDefault();
            }
            if (reportheader != null)
            {
                Model.ViewCode = reportheader.Code == null ? "" : reportheader.Code;
                Model.IsFormatSelected = (Model.ViewCode == null || Model.ViewCode == "") ? false : true;
                List<string> inputlist = new List<string>();
                List<string> inputlist2 = new List<string>();
                inputlist = (reportheader.InputPara == "" || reportheader.InputPara == null) ? inputlist : reportheader.InputPara.Trim().Split('~').ToList();
                foreach (var ai in inputlist)
                {
                    if (ai != "" && ai != null)
                    {
                        var a = ai.Split('^');
                        string a1 = a[0];
                        string a2 = GetQueryText(a[1]);
                        inputlist2.Add(a1 + "^" + a2);
                    }
                }
                Model.AddOnParaList = inputlist2;
            }
            return View("ReportStandard/Index", Model);
        }


        #region executereport
        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            Model.MainType = mmaintype;
            ExecuteStockMovement(Model.Date);
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        private void ExecuteStockMovement(string mDate)
        {
            //DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPTFAT_StockMovementStatus", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            var date = mDate.Replace("-", "/").Split(':');
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = Convert.ToDateTime(date[1]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mItems", SqlDbType.VarChar).Value = pitems;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = pwarehouse;
            cmd.Parameters.Add("@mStores", SqlDbType.VarChar).Value = pstores;
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = pcategory;
            cmd.Parameters.Add("@mItemGroups", SqlDbType.VarChar).Value = pitemgroups;
            cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = paccounts;
            cmd.Parameters.Add("@mpara1", SqlDbType.Int).Value = ppara01 == "" ? 0 : Convert.ToInt32(ppara01);
            cmd.Parameters.Add("@mpara2", SqlDbType.Int).Value = ppara02 == "" ? 0 : Convert.ToInt32(ppara02); ;
            cmd.Parameters.Add("@mSuppress", SqlDbType.Int).Value = ppara03 == "Yes" || ppara03 == "" ? -1 : 0;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            tfat_conx.Close();
            cmd.Dispose();
            tfat_conx.Dispose();
            ExecuteStoredProc("Update ztmp_StockMovement set ReOrdQty=CAST(isnull(dbo.fn_GetReOrderLevel(Code,'" + Convert.ToDateTime(date[1]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "','" + mbranchcode + "','" + pstores + "'),0) as Decimal(18,4))");
        }
        #endregion executereport

        //public ActionResult SaveData(GridOption Model)
        //{
        //    if (Model.mWhat == "FSN")
        //    {

        //        if (Model.list == null)
        //            return Json(new { Success = "Success", Message = "Nothing to Save.." }, JsonRequestBehavior.AllowGet);

        //        foreach (var mitem in Model.list)
        //        {
        //            using (var trx = ctxTFAT.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    if (Model.mWhat == "FSN")
        //                    {
        //                        ItemDetail mobj = ctxTFAT.ItemDetail.Where(z => z.Code == mitem.Code && z.Branch == mbranchcode).Select(x => x).FirstOrDefault();
        //                        if (mobj != null)
        //                        {
        //                            mobj.FSN = mitem.FSN.Substring(0, 1);
        //                            ctxTFAT.Entry(mobj).State = EntityState.Modified;
        //                            ctxTFAT.SaveChanges();
        //                        }
        //                    }
        //                    trx.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    trx.Rollback();
        //                    return Json(new { Success = "Error", Message = "Error! while Generating Documents\n" + ex.Message }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //        return Json(new { Success = "Success", Message = "Document Saved Successfully" }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        DateTime mdate = DateTime.Now;
        //        string mauthorise = "A00";
        //        string mtype = "";
        //        string mserial = "";
        //        double mqty;
        //        string mcode = "";
        //        string mbomsrl = "";
        //        string mparentkey = "";
        //        string mtypename = "";
        //        string mindnumber = "";
        //        string mwonumber = "";
        //        string mponumber = "";
        //        string mdocs = "";
        //        foreach (var mitem in Model.list)
        //        {
        //            using (var trx = ctxTFAT.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    mqty = (double)mitem.Balance;
        //                    mcode = (mitem.Code ?? "").Trim();
        //                    int n = mcode.IndexOf("^");
        //                    if (n > 0)
        //                    {
        //                        mbomsrl = mcode.Substring(n + 1);
        //                        mcode = mcode.Substring(0, n);
        //                    }
        //                    bool mindent = mitem.xAdd;
        //                    bool mplan = mitem.xEdit;
        //                    //bool mwo = mitem.xView;
        //                    //bool mreserve = mitem.xDelete;
        //                    var mitemdetail = ctxTFAT.ItemDetail.Where(z => z.Code == mcode && z.Branch == mbranchcode).Select(x => new { x.MRPIPType, x.MRPISType, x.MRPWOType, x.UnitP, x.UnitP2, x.UnitM, x.UnitM2 }).FirstOrDefault();
        //                    if (mcode != "" && mqty > 0)
        //                    {
        //                        if (mindent == true)
        //                        {
        //                            mtype = mitemdetail.MRPIPType == null || mitemdetail.MRPIPType == "" ? ctxTFAT.DocTypes.Where(z => z.Code.StartsWith("PIP") && z.AppBranch == mbranchcode).Select(x => x.Code).FirstOrDefault() : mitemdetail.MRPIPType;
        //                            mauthorise = GetAuthorise(mtype, 0, mbranchcode);
        //                            var mdoc = ctxTFAT.DocTypes.Where(z => z.Code == mtype).Select(x => new { x.FromStore, x.ToStore, x.Name }).FirstOrDefault();
        //                            int mIndFrom = (int)(mdoc.FromStore == 0 ? 100001 : mdoc.FromStore);
        //                            int mIndTo = (int)(mdoc.ToStore == 0 ? 100001 : mdoc.ToStore);
        //                            mserial = GetLastSerial("Indent", mbranchcode, mtype, mperiod, "IP", mdate);
        //                            mtypename = mdoc.Name;
        //                            mparentkey = mtype + mperiod.Substring(0, 2) + mserial;
        //                            Indent mobj = new Indent();
        //                            mobj.TableKey = mparentkey;
        //                            mobj.Branch = mbranchcode;
        //                            mobj.MainType = "PR";
        //                            mobj.SubType = "IP";
        //                            mobj.Type = mtype;
        //                            mobj.Prefix = mperiod;
        //                            mobj.Srl = mserial;
        //                            mobj.DocDate = mdate;
        //                            mobj.IndDate = mdate;
        //                            mobj.BillNumber = mserial;
        //                            mobj.IndTo = mIndTo;
        //                            mobj.Store = mIndFrom;
        //                            mobj.Narr = "Auto-Generated from Stock Status & Plan";
        //                            mobj.OrdNumberPLN = string.Empty;
        //                            mobj.PlanNumber = string.Empty;
        //                            mobj.WONumber = "";
        //                            mobj.EmpIDBy = "";
        //                            mobj.EmpIDTo = "";
        //                            mobj.LocationCode = mlocationcode;
        //                            mobj.AUTHIDS = muserid;
        //                            mobj.AUTHORISE = mauthorise;
        //                            mobj.ENTEREDBY = muserid;
        //                            mobj.LASTUPDATEDATE = mdate;
        //                            mobj.CompCode = mcompcode;
        //                            ctxTFAT.Indent.Add(mobj);

        //                            IndentStk mobjstk = new IndentStk();
        //                            mobjstk.Branch = mbranchcode;
        //                            mobjstk.ParentKey = mparentkey;
        //                            mobjstk.TableKey = mtype + mperiod.Substring(0, 2) + "001" + mserial;
        //                            mobjstk.MainType = "PR";
        //                            mobjstk.SubType = "IP";
        //                            mobjstk.Type = mtype;
        //                            mobjstk.Prefix = mperiod;
        //                            mobjstk.Srl = mserial;
        //                            mobjstk.DocDate = mdate;
        //                            mobjstk.Sno = 1;
        //                            mobjstk.Code = mcode;
        //                            mobjstk.Dely = 0;
        //                            mobjstk.DelyDate = mdate;
        //                            mobjstk.EstReturnDate = mdate;
        //                            mobjstk.Factor = 1;
        //                            mobjstk.BillDate = mdate;
        //                            mobjstk.Store = mIndFrom;
        //                            mobjstk.IndTo = mIndTo;
        //                            mobjstk.IsReturnable = false;
        //                            mobjstk.Narr = "Auto-Generated from Stock Status & Plan";
        //                            mobjstk.NotInStock = false;
        //                            mobjstk.OrdKey = null;
        //                            mobjstk.PSPKey = null;
        //                            mobjstk.ReqKey = null;
        //                            mobjstk.ProcessCode = 100001;
        //                            mobjstk.QtnKey = null;
        //                            mobjstk.Unit = mitemdetail.UnitP ?? "Pcs";
        //                            mobjstk.Unit2 = mitemdetail.UnitP2 ?? mobjstk.Unit;
        //                            mobjstk.Qty = mqty;
        //                            mobjstk.Qty2 = CalculateQty2(mcode, mobjstk.Unit, mobjstk.Unit2, mqty);
        //                            mobjstk.Rate = 1;
        //                            mobjstk.Amt = (decimal)mqty;
        //                            mobjstk.RateOn = 0;
        //                            mobjstk.ReservedQty = 0;
        //                            mobjstk.Stage = 0;
        //                            mobjstk.WOKey = null;
        //                            mobjstk.HSNCode = "";
        //                            mobjstk.IndKey = null;
        //                            mobjstk.LocationCode = mlocationcode;
        //                            mobjstk.ENTEREDBY = muserid;
        //                            mobjstk.AUTHIDS = muserid;
        //                            mobjstk.AUTHORISE = mauthorise;
        //                            mobjstk.LASTUPDATEDATE = mdate;
        //                            mobjstk.CompCode = mcompcode;
        //                            mobjstk.PCCode = 100002;
        //                            ctxTFAT.IndentStk.Add(mobjstk);
        //                        }
        //                        else if (mplan == true)  // for production plan
        //                        {
        //                            mtype = ctxTFAT.DocTypes.Where(z => z.Code.StartsWith("PLN") && z.AppBranch == mbranchcode).Select(x => x.Code).FirstOrDefault();
        //                            mauthorise = GetAuthorise(mtype, 0, mbranchcode);
        //                            mserial = GetLastSerial("Production", mbranchcode, mtype, mperiod, "PP", mdate);
        //                            var mdoc = ctxTFAT.DocTypes.Where(z => z.Code == mtype).Select(x => new { x.Store, x.Name }).FirstOrDefault();
        //                            int mstore = mdoc.Store == 0 ? 100001 : (int)mdoc.Store;
        //                            mtypename = mdoc.Name;
        //                            mparentkey = mtype + mperiod.Substring(0, 2) + mserial;

        //                            Production mobj = new Production();
        //                            mobj.DocDate = mdate;
        //                            mobj.Srl = mserial ?? "";
        //                            mobj.LocationCode = mlocationcode;
        //                            mobj.BillNumber = mserial ?? "";
        //                            mobj.BillDate = mdate;
        //                            mobj.Party = "" ?? "";
        //                            mobj.Narr = "Auto-Generated from Stock Status & Plan";
        //                            // iX9: default values for the fields not used @Form
        //                            mobj.Branch = mbranchcode;
        //                            mobj.CompCode = mcompcode;
        //                            mobj.MainType = "IV";
        //                            mobj.Prefix = mperiod;
        //                            mobj.Stage = 0;
        //                            mobj.SubType = "PP";
        //                            mobj.TableKey = mparentkey;
        //                            mobj.Type = mtype;
        //                            // iX9: Save default values to Std fields
        //                            mobj.AUTHIDS = muserid;
        //                            mobj.ENTEREDBY = muserid;
        //                            mobj.LASTUPDATEDATE = DateTime.Now;
        //                            mobj.AUTHORISE = mauthorise;
        //                            ctxTFAT.Production.Add(mobj);

        //                            ProductionStk mgriddata = new ProductionStk();
        //                            mgriddata.ParentKey = mparentkey;
        //                            mgriddata.Code = mcode;
        //                            mgriddata.Store = mstore;
        //                            mgriddata.Qty = mqty;
        //                            mgriddata.DelyDate = mdate;
        //                            mgriddata.Narr = "Auto-Generated from Stock Status & Plan";
        //                            mgriddata.Sno = 1;
        //                            mgriddata.CompCode = mcompcode;
        //                            mgriddata.Branch = mbranchcode;
        //                            mgriddata.LocationCode = mlocationcode;
        //                            mgriddata.MainType = "IV";
        //                            mgriddata.SubType = "PP";
        //                            mgriddata.Type = mtype;
        //                            mgriddata.Prefix = mperiod;
        //                            mgriddata.Srl = mserial;
        //                            mgriddata.TableKey = mtype + mperiod.Substring(0, 2) + "001" + mserial;
        //                            mgriddata.DocDate = mdate;
        //                            mgriddata.Factor = 0;
        //                            mgriddata.Unit = "";
        //                            mgriddata.Unit2 = "";
        //                            mgriddata.ENTEREDBY = muserid;
        //                            mgriddata.LASTUPDATEDATE = DateTime.Now;
        //                            mgriddata.AUTHORISE = mauthorise;
        //                            mgriddata.AUTHIDS = muserid;
        //                            ctxTFAT.ProductionStk.Add(mgriddata);

        //                        }
        //                        if (mindent == true || mplan == true)
        //                        {
        //                            ctxTFAT.SaveChanges();
        //                            mdocs += mtype + "-" + mserial + ", ";
        //                            UpdateAuditTrail(mbranchcode, "Add", mtypename, mparentkey, mdate, 0, "", "Auto Generated voucher by MRP, " + muserid);
        //                            SendTrnsMsg("Add", 0, mbranchcode + mparentkey, mdate, "");
        //                            if (!mauthorise.StartsWith("A"))
        //                            {
        //                                string mAuthUser = SaveAuthorise(mparentkey, 0, mdate.ToString(), 1, 1, DateTime.Now, "", mbranchcode, muserid, -1);
        //                                SendAuthoriseMessage(mAuthUser, mparentkey, mdate.ToString());
        //                            }
        //                        }
        //                    }
        //                    trx.Commit();
        //                    trx.Dispose();
        //                }
        //                catch (DbEntityValidationException ex1)
        //                {
        //                    trx.Rollback();
        //                    return Json(new
        //                    {
        //                        Status = "Error",
        //                        ex1.Message
        //                    }, JsonRequestBehavior.AllowGet);
        //                }
        //                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
        //                {
        //                    trx.Rollback();
        //                    return Json(new { Status = "Error", ex.Message }, JsonRequestBehavior.AllowGet);
        //                }
        //                catch (Exception e)
        //                {
        //                    trx.Rollback();
        //                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
        //                    return Json(new { Status = "Error", e.Message }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //        return Json(new { Status = "Success", Message = "Vouchers Are Generated Successfully.\n" + CutRightString(mdocs, 2, ", ") }, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}