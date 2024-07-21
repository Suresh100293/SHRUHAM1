using Common;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class HighestLowestAccountBalanceController : BaseController
    {
        //nEntities context = new nEntities();
        //tfatEntities ctxTFAT = new tfatEntities();
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        ////IReportGridOperation mIlst = new ListViewGridOperationreport();
        private decimal TOpening;
        private decimal PartyDr = 0;
        private decimal PartyCr = 0;
        private decimal mClosing = 0;
        // GET: Reports/HighestLowestAccountBalance
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
            if (mperiod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode && x.PerdCode == mperiod).Select(b => new
                {
                    b.StartDate,
                    b.LastDate
                }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");
            }
            return View(Model);
        }

        public ActionResult GetData(GridOption Model)
        {

            if (mperiod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode && x.PerdCode == mperiod).Select(b => new
                {
                    b.StartDate,
                    b.LastDate
                }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");
            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);

            var PDate = LDate.AddYears(-1);

            var result = (from m in ctxTFAT.Master
                          join l in ctxTFAT.Ledger
                          on new
                          {
                              m.Code
                          } equals new
                          {
                              l.Code
                          }
                          where l.MainType != "MV" && l.MainType != "PV" && m.Grp == Model.AccountName
                          && l.DocDate >= SDate && l.DocDate <= LDate
                          select new
                          {
                              m.Code,
                              m.Grp,
                              m.Name
                          }).Distinct().ToList();


            List<GridOption> newlist = new List<GridOption>();
            List<GridOption> newlist1 = new List<GridOption>();
            foreach (var item in result)
            {
                PartyDr = 0;
                PartyCr = 0;
                var result2 = (from l in ctxTFAT.Ledger
                               where l.MainType != "MV" && l.MainType != "PV" && l.Code == item.Code && l.Branch == mbranchcode
                               && l.DocDate >= SDate && l.DocDate <= LDate
                               group l by new
                               {
                                   l.DocDate
                               } into grps
                               select new
                               {
                                   DocDate = grps.Key.DocDate,
                                   Debit = grps.Sum(x => x.Debit),
                                   Credit = grps.Sum(x => x.Credit)
                               }).ToList();

                if (item.Code != null)
                {
                    var result1 = (from l in ctxTFAT.Ledger
                                   where l.MainType != "MV" && l.MainType != "PV" && l.Code == item.Code && l.Branch == mbranchcode && l.DocDate <= PDate
                                   select new
                                   {
                                       l.Credit,
                                       l.Debit
                                   }).ToList();

                    decimal OpeningBal = Convert.ToDecimal(result1.ToList().Select(X => X.Debit - X.Credit).Sum());
                    Model.Opening = OpeningBal;

                    for (int i = 0; i < result2.Count; i++)
                    {
                        if (i == 0)
                        {
                            TOpening = OpeningBal;
                            mClosing = (decimal)(TOpening + (result2[i].Debit - result2[i].Credit));
                            if (mClosing > PartyDr)
                            {
                                PartyDr = mClosing;
                            }
                            else if (mClosing < PartyCr)
                            {
                                PartyCr = mClosing;
                            }
                            Session.Add("bal", mClosing);
                            newlist1.Add(new GridOption
                            {
                                Highest = PartyDr,
                                lowest = PartyCr,
                            });
                        }
                        else
                        {
                            TOpening = Convert.ToDecimal(Session["bal"].ToString());
                            mClosing = (decimal)(Convert.ToDecimal(Session["bal"].ToString()) + (result2[i].Debit - result2[i].Credit));
                            if (mClosing > PartyDr)
                            {
                                PartyDr = mClosing;
                            }
                            else if (mClosing < PartyCr)
                            {
                                PartyCr = mClosing;
                            }
                            Session.Add("bal", mClosing);

                            newlist1.Add(new GridOption
                            {
                                Highest = PartyDr,
                                lowest = PartyCr,
                            });
                        }
                    }
                }
                newlist.Add(new GridOption
                {

                    Code = item.Name,
                    Opening = Model.Opening,
                    Highest = PartyDr,
                    lowest = PartyCr,
                    Closing = mClosing,

                });
            }
            Model.SumDebit = newlist.Sum(x => x.Highest);
            Model.SumCredit = newlist.Sum(x => x.lowest);
            Model.SumClosing = newlist.Sum(x => x.Closing);
            Model.list = newlist;
            return Json(new
            {
                Status = "Success",
                Html = this.RenderPartialView("HighLowAcBal", Model)
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetGroupList(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false && ((x.BaseGr == "B") || (x.BaseGr == "C") || (x.BaseGr == "O") || (x.BaseGr == "P") || (x.BaseGr == "V") || (x.BaseGr == "D") || (x.BaseGr == "U") || (x.BaseGr == "S"))).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false && x.Name.Contains(term) && ((x.BaseGr == "B") || (x.BaseGr == "C") || (x.BaseGr == "O") || (x.BaseGr == "P") || (x.BaseGr == "V") || (x.BaseGr == "D") || (x.BaseGr == "U") || (x.BaseGr == "S"))).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetExcel(GridOption Model)
        {
            if (mperiod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode && x.PerdCode == mperiod).Select(b => new
                {
                    b.StartDate,
                    b.LastDate
                }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");
            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);

            var PDate = LDate.AddYears(-1);

            var result = (from m in ctxTFAT.Master
                          join l in ctxTFAT.Ledger
                          on new
                          {
                              m.Code
                          } equals new
                          {
                              l.Code
                          }
                          where l.MainType != "MV" && l.MainType != "PV" && m.Grp == Model.AccountName
                          && l.DocDate >= SDate && l.DocDate <= LDate
                          select new
                          {
                              m.Code,
                              m.Grp,
                              m.Name
                          }).Distinct().ToList();


            List<GridOption> newlist = new List<GridOption>();
            List<GridOption> newlist1 = new List<GridOption>();
            foreach (var item in result)
            {
                PartyDr = 0;
                PartyCr = 0;
                var result2 = (from l in ctxTFAT.Ledger
                               where l.MainType != "MV" && l.MainType != "PV" && l.Code == item.Code && l.Branch == mbranchcode
                               && l.DocDate >= SDate && l.DocDate <= LDate
                               group l by new
                               {
                                   l.DocDate
                               } into grps
                               select new
                               {
                                   DocDate = grps.Key.DocDate,
                                   Debit = grps.Sum(x => x.Debit),
                                   Credit = grps.Sum(x => x.Credit)
                               }).ToList();

                if (item.Code != null)
                {
                    var result1 = (from l in ctxTFAT.Ledger
                                   where l.MainType != "MV" && l.MainType != "PV" && l.Code == item.Code && l.Branch == mbranchcode && l.DocDate <= PDate
                                   select new
                                   {
                                       l.Credit,
                                       l.Debit
                                   }).ToList();

                    decimal OpeningBal = Convert.ToDecimal(result1.ToList().Select(X => X.Debit - X.Credit).Sum());
                    Model.Opening = OpeningBal;

                    for (int i = 0; i < result2.Count; i++)
                    {
                        if (i == 0)
                        {
                            TOpening = OpeningBal;
                            mClosing = (decimal)(TOpening + (result2[i].Debit - result2[i].Credit));
                            if (mClosing > PartyDr)
                            {
                                PartyDr = mClosing;
                            }
                            else if (mClosing < PartyCr)
                            {
                                PartyCr = mClosing;
                            }
                            Session.Add("bal", mClosing);
                            newlist1.Add(new GridOption
                            {
                                Highest = PartyDr,
                                lowest = PartyCr,
                            });
                        }
                        else
                        {
                            TOpening = Convert.ToDecimal(Session["bal"].ToString());
                            mClosing =(decimal) (Convert.ToDecimal(Session["bal"].ToString()) + (result2[i].Debit - result2[i].Credit));
                            if (mClosing > PartyDr)
                            {
                                PartyDr = mClosing;
                            }
                            else if (mClosing < PartyCr)
                            {
                                PartyCr = mClosing;
                            }
                            Session.Add("bal", mClosing);

                            newlist1.Add(new GridOption
                            {
                                Highest = PartyDr,
                                lowest = PartyCr,
                            });
                        }
                    }
                }
                newlist.Add(new GridOption
                {
                    Code = item.Name,
                    Opening = Model.Opening,
                    Highest = PartyDr,
                    lowest = PartyCr,
                    Closing = mClosing,
                });
            }
            Model.SumDebit = newlist.Sum(x => x.Highest);
            Model.SumCredit = newlist.Sum(x => x.lowest);
            Model.SumClosing = newlist.Sum(x => x.Closing);
            Model.list = newlist;
            var products = newlist.OrderBy(x => x.Month).ToList();
            var grid = new GridView();
            grid.DataSource = from p in products
                              select new
                              {
                                  p.AccountDescription,
                                  p.Opening,
                                  p.Highest,
                                  p.lowest,
                                  p.Closing,
                              };
            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=HighLowAccBalance.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return null;
        }

        public ActionResult GetPDF(GridOption Model)
        {
            if (mperiod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode && x.PerdCode == mperiod).Select(b => new
                {
                    b.StartDate,
                    b.LastDate
                }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");
            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);

            var PDate = LDate.AddYears(-1);

            var result = (from m in ctxTFAT.Master
                          join l in ctxTFAT.Ledger
                          on new
                          {
                              m.Code
                          } equals new
                          {
                              l.Code
                          }
                          where l.MainType != "MV" && l.MainType != "PV" && m.Grp == Model.AccountName
                          && l.DocDate >= SDate && l.DocDate <= LDate
                          select new
                          {
                              m.Code,
                              m.Grp,
                              m.Name
                          }).Distinct().ToList();


            List<GridOption> newlist = new List<GridOption>();
            List<GridOption> newlist1 = new List<GridOption>();
            foreach (var item in result)
            {
                PartyDr = 0;
                PartyCr = 0;
                var result2 = (from l in ctxTFAT.Ledger
                               where l.MainType != "MV" && l.MainType != "PV" && l.Code == item.Code && l.Branch == mbranchcode
                               && l.DocDate >= SDate && l.DocDate <= LDate
                               group l by new
                               {
                                   l.DocDate
                               } into grps
                               select new
                               {
                                   DocDate = grps.Key.DocDate,
                                   Debit = grps.Sum(x => x.Debit),
                                   Credit = grps.Sum(x => x.Credit)
                               }).ToList();

                if (item.Code != null)
                {
                    var result1 = (from l in ctxTFAT.Ledger
                                   where l.MainType != "MV" && l.MainType != "PV" && l.Code == item.Code && l.Branch == mbranchcode && l.DocDate <= PDate
                                   select new
                                   {
                                       l.Credit,
                                       l.Debit
                                   }).ToList();

                    decimal OpeningBal = Convert.ToDecimal(result1.ToList().Select(X => X.Debit - X.Credit).Sum());
                    Model.Opening = OpeningBal;

                    for (int i = 0; i < result2.Count; i++)
                    {
                        if (i == 0)
                        {
                            TOpening = OpeningBal;
                            mClosing = (decimal)(TOpening + (result2[i].Debit - result2[i].Credit));
                            if (mClosing > PartyDr)
                            {
                                PartyDr = mClosing;
                            }
                            else if (mClosing < PartyCr)
                            {
                                PartyCr = mClosing;
                            }
                            Session.Add("bal", mClosing);
                            newlist1.Add(new GridOption
                            {
                                Highest = PartyDr,
                                lowest = PartyCr,
                            });
                        }
                        else
                        {
                            TOpening = Convert.ToDecimal(Session["bal"].ToString());
                            mClosing =(decimal) (Convert.ToDecimal(Session["bal"].ToString()) + (result2[i].Debit - result2[i].Credit));
                            if (mClosing > PartyDr)
                            {
                                PartyDr = mClosing;
                            }
                            else if (mClosing < PartyCr)
                            {
                                PartyCr = mClosing;
                            }
                            Session.Add("bal", mClosing);

                            newlist1.Add(new GridOption
                            {
                                Highest = PartyDr,
                                lowest = PartyCr,
                            });
                        }
                    }
                }
                newlist.Add(new GridOption
                {

                    Code = item.Name,
                    Opening = Model.Opening,
                    Highest = PartyDr,
                    lowest = PartyCr,
                    Closing = mClosing,

                });
            }
            Model.SumDebit = newlist.Sum(x => x.Highest);
            Model.SumCredit = newlist.Sum(x => x.lowest);
            Model.SumClosing = newlist.Sum(x => x.Closing);
            Model.list = newlist;
            var products = newlist.OrderBy(x => x.Month).ToList();

            var grid = new GridView();
            grid.DataSource = from p in products
                              select new
                              {
                                  p.Name,
                                  p.Opening,
                                  p.Highest,
                                  p.lowest,
                                  p.Closing,

                              };
            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.ContentType = "application / pdf";
            Response.AddHeader("content-disposition",
            "attachment;filename=HighLowAccBalance.pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            grid.RenderControl(hw);
            StringReader sr = new StringReader(sw.ToString());
            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();
            htmlparser.Parse(sr);
            pdfDoc.Close();
            Response.Write(pdfDoc);
            Response.End();
            return null;
        }
    }
}