using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class ImportExcelFileController : BaseController
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        public static object[,] objarray = null;
        public static string mWhat = "AccountsOpening";

        // GET: Accounts/ImportExcelFile
        public ActionResult Index(GridOption Model)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            mWhat = Model.OptionCode;
            UpdateAuditTrail(mbranchcode, "Excel", mWhat, "", DateTime.Now, 0, "", "", "A");
            return View();
        }

        public FileResult DownloadFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "DownloadFiles/";
            string mFile = mWhat == "AccountsOpening" ? "AccountOpeningTemplate.xls" : "ItemOpeningTemplate.xls";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + mFile);
            string fileName = mFile;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public JsonResult UploadFile()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    //object[,] objarray = null;
                    int noOfCol = 0;
                    int noOfRow = 0;
                    HttpFileCollectionBase file = Request.Files;
                    if ((file != null) && (file.Count > 0))
                    {
                        byte[] fileBytes = new byte[Request.ContentLength];
                        var data = Request.InputStream.Read(fileBytes, 0, Convert.ToInt32(Request.ContentLength));
                        using (var package = new ExcelPackage(Request.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            noOfCol = workSheet.Dimension.End.Column;
                            noOfRow = workSheet.Dimension.End.Row;
                            objarray = new object[noOfRow, noOfCol];
                            objarray = (object[,])workSheet.Cells.Value;
                        }
                    }
                    var jsonResult = Json(new
                    {
                        data = objarray,
                        row = noOfRow,
                        col = noOfCol
                    }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                    //return Json(new
                    //{
                    //    data = objarray,
                    //    row = noOfRow,
                    //    col = noOfCol
                    //}, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveData()
        {
            string mError = "";
            try
            {
                string mbranchcode = "";
                int mLocationCode = 100001;
                string mMainType = "";
                string mSubType = "";
                string mType = "";
                string mPrefix = "";
                string mSrl = "";
                int mSno = 1;
                DateTime mDocDate = DateTime.Today;
                string mCode = "";
                string mName = "";
                decimal mDebit = 0;
                decimal mCredit = 0;
                string mCheque = "";
                DateTime mChequeDate = DateTime.Today;
                DateTime mClearDate = DateTime.Today;
                string mNarration = "";
                // item variables
                string mItemCode = "";
                string mItemName = "";
                int mStore = 0;
                string mStoreName = "";
                double mQty = 0;
                string mUnit = "";
                double mFactor = 0;
                double mQty2 = 0;
                string mUnit2 = "";
                double mRate = 0;
                decimal mDisc = 0;
                decimal mDiscAmt = 0;
                decimal mAmt = 0;
                int mBINNumber = 0;
                string mStockSerial = "";
                string mStockBatch = "";
                string mErrorString = "";
                DateTime mFirst = ctxTFAT.TfatPerd.OrderByDescending(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
                List<string> mBatch = new List<string>();
                List<string> mSerial = new List<string>();

                // n=1 avoid header row
                for (int n = 1; n < objarray.GetLength(0); n++)
                {
                    mError = "";
                    if (mWhat == "AccountsOpening")
                    {
                        //  0          1        2     3    4    5   6      7    8     9     10      11      12         13       14
                        //Branch,LocationCode,Type,Prefix,Srl,Sno,DocDate,Code,Name,Debit,Credit,Cheque,ChequeDate,ClearDate,Narration
                        mbranchcode = objarray[n, 0] == null ? "" : objarray[n, 0].ToString();
                        mLocationCode = Convert.ToInt32(objarray[n, 1]);
                        mType = objarray[n, 2] == null ? "" : objarray[n, 2].ToString();
                        mPrefix = objarray[n, 3] == null ? "" : objarray[n, 3].ToString();
                        mSrl = objarray[n, 4] == null ? "" : objarray[n, 4].ToString();
                        mSno = objarray[n, 5] == null ? 1 : Convert.ToInt32(objarray[n, 5]);
                        mDocDate = objarray[n, 6] == null ? mFirst : Convert.ToDateTime(objarray[n, 6]);
                        mCode = objarray[n, 7] == null ? "" : objarray[n, 7].ToString();
                        mName = objarray[n, 8] == null ? "" : objarray[n, 8].ToString();
                        mDebit = Convert.ToDecimal(objarray[n, 9]);
                        mCredit = Convert.ToDecimal(objarray[n, 10]);
                        mCheque = objarray[n, 11] == null ? "" : objarray[n, 11].ToString();
                        mChequeDate = objarray[n, 12] == null ? mFirst : Convert.ToDateTime(objarray[n, 12]);
                        mClearDate = objarray[n, 13] == null ? mFirst : Convert.ToDateTime(objarray[n, 13]);
                        mNarration = objarray[n, 14] == null ? "" : objarray[n, 14].ToString();

                        if (mDebit == 0 && mCredit == 0)
                            mError = mError + "\nSr." + n + "-Both Debit & Credit are 0: " + mSrl;

                        if (mDebit != 0 && mCredit != 0)
                            mError = mError + "\nSr." + n + "-Only Debit or Credit may Exist: " + mSrl;

                        if (mDebit < 0 || mCredit < 0)
                            mError = mError + "\nSr." + n + "-Negative Debit or Credit Amount: " + mSrl;

                        if (mName != "")
                        {
                            mCode = ctxTFAT.Master.Where(z => z.Name == mName).Select(x => x.Code).FirstOrDefault();
                        }
                        if ((mCode == "" || mCode == null))
                            mError = mError + "\nSr." + n + "-Account Code or Name is Required: " + mSrl;
                        if (mCode != "")
                        {
                            if (ctxTFAT.Master.Where(z => z.Code == mCode).Select(x => x.Code).FirstOrDefault() == null)
                                mError = mError + "\nSr." + n + "-Invalid Account: " + mSrl;
                        }
                    }
                    else if (mWhat == "ItemsOpeningStock")
                    {
                        //   0        1         2     3    4   5     6       7    8           9        10     11    12      
                        //Branch,LocationCode,Type,Prefix,Srl,Sno,DocDate,Party,PartyName,ItemCode,ItemName,Store,StoreName,
                        //13  14    15    16   17    18   19     20   21     22          23           24       25
                        //Qty,Unit,Factor,Qty2,Unit2,Rate,Disc,DiscAmt,Amt,BINNumber,StockSerials,StockBatch,Narration
                        mbranchcode = objarray[n, 0] == null ? "" : objarray[n, 0].ToString().Trim();
                        mLocationCode = objarray[n, 1] == null ? 100001 : Convert.ToInt32(objarray[n, 1]);
                        mType = objarray[n, 2] == null ? "" : objarray[n, 2].ToString().Trim();
                        if (mType == "") mType = "OPSTK";
                        mPrefix = objarray[n, 3] == null ? "" : objarray[n, 3].ToString().Trim();
                        mSrl = objarray[n, 4] == null ? "" : objarray[n, 4].ToString().Trim();
                        mSno = objarray[n, 5] == null || objarray[n, 5] == "" ? 1 : Convert.ToInt32(objarray[n, 5]);
                        mDocDate = objarray[n, 6] == null ? mFirst : Convert.ToDateTime(objarray[n, 6].ToString().Trim());
                        mCode = objarray[n, 7] == null ? "" : objarray[n, 7].ToString().Trim();
                        mName = objarray[n, 8] == null ? "" : objarray[n, 8].ToString().Trim();
                        mItemCode = objarray[n, 9] == null ? "" : objarray[n, 9].ToString().Trim();
                        mItemName = objarray[n, 10] == null ? "" : objarray[n, 10].ToString().Trim();
                        mStore = objarray[n, 11] == null ? 100002 : Convert.ToInt32(objarray[n, 11]);
                        mStoreName = objarray[n, 12] == null ? "" : objarray[n, 12].ToString().Trim();
                        mQty = objarray[n, 13] == null || objarray[n, 13] == "" ? 0 : Convert.ToDouble(objarray[n, 13]);
                        mUnit = objarray[n, 14] == null ? "" : objarray[n, 14].ToString().Trim();
                        mFactor = objarray[n, 15] == null || objarray[n, 15] == "" ? 0 : Convert.ToDouble(objarray[n, 15]);
                        mQty2 = objarray[n, 16] == null || objarray[n, 16] == "" ? 0 : Convert.ToDouble(objarray[n, 16]);
                        mUnit2 = objarray[n, 17] == null ? "" : objarray[n, 17].ToString().Trim();
                        mRate = objarray[n, 18] == null || objarray[n, 18] == "" ? 0 : Convert.ToDouble(objarray[n, 18]);
                        mDisc = objarray[n, 19] == null || objarray[n, 19] == "" ? 0 : Convert.ToDecimal(objarray[n, 19]);
                        mDiscAmt = objarray[n, 20] == null || objarray[n, 20] == "" ? 0 : Convert.ToDecimal(objarray[n, 20]);
                        mAmt = objarray[n, 21] == null || objarray[n, 21] == "" ? 0 : Convert.ToDecimal(objarray[n, 21]);
                        mBINNumber = objarray[n, 22] == null || objarray[n, 22].ToString().Trim() == "" ? 0 : Convert.ToInt32(objarray[n, 22]);
                        mNarration = objarray[n, 23] == null ? "" : objarray[n, 23].ToString().Trim();
                        mStockSerial = objarray[n, 24] == null ? "" : objarray[n, 24].ToString().Trim();
                        mStockBatch = objarray[n, 25] == null ? "" : objarray[n, 25].ToString().Trim();
                        if (mStockBatch != "")
                        {
                            string mStr = (objarray[n, 25] == null ? "" : objarray[n, 25].ToString().Trim()) + "^";
                            mStr = mStr + (objarray[n, 26] == null ? "0" : objarray[n, 26].ToString().Trim()) + "^";
                            mStr = mStr + (objarray[n, 27] == null ? "" : objarray[n, 27].ToString().Trim()) + "^";
                            mStr = mStr + (objarray[n, 28] == null ? "" : objarray[n, 28].ToString().Trim()) + "^";
                            mStr = mStr + (objarray[n, 29] == null ? "0" : objarray[n, 29].ToString().Trim()) + "^";
                            mStockBatch = mStr;
                        }
                        //mStr = objarray[n, 29] == null ? "" : objarray[n, 29].ToString().Trim();
                        //mStockBatch = mStockBatch + mStr;
                        //mStockBatch = objarray[n, 25] == null ? "" : objarray[n, 25].ToString().Trim() + "^" +
                        //objarray[n, 26] == null ? "" : objarray[n, 26].ToString().Trim() + "^" +
                        //objarray[n, 27] == null ? "" : objarray[n, 27].ToString().Trim() + "^" +
                        //objarray[n, 28] == null ? "" : objarray[n, 28].ToString().Trim() + "^" + mStr;

                        if (mName != "")
                        {
                            mCode = ctxTFAT.Master.Where(z => z.Name == mName).Select(x => x.Code).FirstOrDefault();
                        }
                        if (mCode != "")
                        {
                            if (ctxTFAT.Master.Where(z => z.Code == mCode).Select(x => x.Code).FirstOrDefault() == null)
                                mError = mError + "\nSr." + n + "-Invalid Account Code: " + mSrl;
                        }

                        if (mQty == 0)
                            mError = mError + "\nSr." + n + "-Quantity is Required: " + mSrl;

                        if (mItemName != "")
                        {
                            mItemCode = ctxTFAT.ItemMaster.Where(z => z.Name == mItemName).Select(x => x.Code).FirstOrDefault();
                        }
                        if ((mItemCode == "" || mItemCode == null))
                            mError = mError + "\nSr." + n + "-Item Code or Name is Required: " + mSrl;
                        if (mItemCode != "")
                        {
                            if (ctxTFAT.ItemMaster.Where(z => z.Code == mItemCode).Select(x => x.Code).FirstOrDefault() == null)
                                mError = mError + "\nSr." + n + "-Invalid Item: " + mSrl;
                        }

                        if (mStockBatch == "" && (ctxTFAT.ItemDetail.Where(z => z.Code == mItemCode).Select(x => x.BatchForce).FirstOrDefault() == true))
                        {
                            mError = mError + "\nSr." + n + "-Stock Batch information is Required for the Item: " + mItemCode;
                        }

                        if (mStockSerial == "" && (ctxTFAT.ItemDetail.Where(z => z.Code == mItemCode).Select(x => x.SerialReq).FirstOrDefault() == true))
                        {
                            mError = mError + "\nSr." + n + "-Stock Serial information is Required for the Item: " + mItemCode;
                        }

                        if (mStoreName != "")
                        {
                            mStore = ctxTFAT.Stores.Where(z => z.Name == mStoreName).Select(x => x.Code).FirstOrDefault();
                        }
                        if ((mStore == 0))
                            mError = mError + "\nSr." + n + "-Store Code or Name is Required: " + mSrl;
                        if (mStore != 0)
                        {
                            if (ctxTFAT.Stores.Where(z => z.Code == mStore).Select(x => x.Code).FirstOrDefault() == 0)
                                mError = mError + "\nSr." + n + "-Invalid Store: " + mSrl;
                        }

                        mSerial = mStockSerial.Split(',').ToList();
                        if (mStockSerial != "")
                        {
                            if (mQty != mSerial.Count())
                                mError = mError + "\nSr." + n + "-Stock Serial Doesn't Match to Quantity: " + mSrl;
                        }

                        mBatch = mStockBatch.Split(',').ToList();
                        if (mStockBatch != "")
                        {
                            double mBQty = 0;
                            for (int x = 0; x < mBatch.Count; x++)
                            {
                                List<string> mBatch2 = new List<string>();
                                mBatch2 = mBatch[x].Split('^').ToList();
                                mBQty = mBQty + Convert.ToDouble(mBatch2[1]);
                            }

                            if (mQty != mBQty)
                                mError = mError + "\nSr." + n + "-Batch Quantity Total Doesn't Match to Quantity: " + mSrl;
                        }
                    }
                    //validate the data against database
                    if (ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).FirstOrDefault() == null)
                        mError = mError + "\nSr." + n + "-Invalid Branch Code: " + mbranchcode;

                    if (ctxTFAT.Warehouse.Where(z => z.Code == mLocationCode).Select(x => x.Code).FirstOrDefault() != mLocationCode)
                        mError = mError + "\nSr." + n + "-Invalid LocationCode Code: " + mLocationCode;

                    if (mType.Length < 5)
                        mType = mType.PadRight(5, '0');
                    mMainType = GetMainType(mType);
                    mSubType = GetSubType(mType);

                    if (ctxTFAT.DocTypes.Where(z => z.Code == mType).Select(x => x.Code).FirstOrDefault() == null)
                        mError = mError + "\nSr." + n + "-Invalid Transaction Code: " + mType;

                    if (mPrefix == "")
                        mError = mError + "\nSr." + n + "-Invalid Prefix:";

                    if (mSrl == "")
                        mError = mError + "\nSr." + n + "-Invalid Serial:";

                    if (mSno == 0)
                        mError = mError + "\nSr." + n + "-Invalid Sno:";

                    if (mDocDate >= mFirst)
                        mError = mError + "\nSr." + n + "-Date Must be from Previous Period: " + mDocDate;

                    if (mError == "")
                    {
                        string mParentKey = mType + mPrefix.Substring(0, 2) + mSrl;
                        if (mWhat == "AccountsOpening")
                        {
                            var mDel = ctxTFAT.Ledger.Where(z => z.ParentKey == mParentKey).ToList();
                            ctxTFAT.Ledger.RemoveRange(mDel);
                            // save data into Ledger table
                            Ledger mled = new Ledger();
                            mled.Branch = mbranchcode;
                            mled.LocationCode = mLocationCode;
                            mled.MainType = mMainType;
                            mled.SubType = mSubType;
                            mled.Type = mType;
                            mled.Prefix = mPrefix;
                            mled.Srl = mSrl;
                            mled.Sno = mSno;
                            mled.ParentKey = mParentKey;
                            mled.TableKey = mType + mPrefix.Substring(0, 2) + mSno.ToString("D3") + mSrl;
                            mled.DocDate = mDocDate;
                            mled.Code = mCode;
                            mled.Debit = mDebit;
                            mled.Credit = mCredit;
                            mled.Cheque = mCheque;
                            mled.ChequeDate = mChequeDate;
                            mled.ClearDate = mClearDate;
                            mled.Narr = mNarration;
                            mled.AltCode = mCode;
                            mled.BillDate = mDocDate;
                            mled.BillNumber = "";
                            mled.CompCode = mcompcode;
                            mled.ENTEREDBY = muserid;
                            mled.LASTUPDATEDATE = DateTime.Now;
                            mled.AUTHORISE = "A00";
                            mled.AUTHIDS = muserid;
                            ctxTFAT.Ledger.Add(mled);
                            //ctxTFAT.SaveChanges();

                            ctxTFAT.SaveChanges();
                        }
                        else
                        {
                            var mDelBat = ctxTFAT.StockBatch.Where(z => z.ParentKey == mParentKey).ToList();
                            ctxTFAT.StockBatch.RemoveRange(mDelBat);
                            var mDelS = ctxTFAT.StockSerial.Where(z => z.ParentKey == mParentKey).ToList();
                            ctxTFAT.StockSerial.RemoveRange(mDelS);
                            var mDelB = ctxTFAT.StockMore.Where(z => z.ParentKey == mParentKey).ToList();
                            ctxTFAT.StockMore.RemoveRange(mDelB);
                            var mDelX = ctxTFAT.StockTax.Where(z => z.ParentKey == mParentKey).ToList();
                            ctxTFAT.StockTax.RemoveRange(mDelX);
                            var mDel = ctxTFAT.Stock.Where(z => z.ParentKey == mParentKey).ToList();
                            ctxTFAT.Stock.RemoveRange(mDel);
                            // save data into Stock table
                            Stock mled = new Stock();
                            mled.Branch = mbranchcode;
                            mled.LocationCode = mLocationCode;
                            mled.CompCode = mcompcode;
                            mled.MainType = mMainType;
                            mled.SubType = mSubType;
                            mled.Type = mType;
                            mled.Prefix = mPrefix;
                            mled.Srl = mSrl;
                            mled.Sno = mSno;
                            mled.ParentKey = mParentKey;
                            mled.TableKey = mType + mPrefix.Substring(0, 2) + mSno.ToString("D3") + mSrl;
                            mled.DocDate = mDocDate;
                            mled.Code = mItemCode;
                            mled.Store = mStore;
                            mled.Qty = mQty;
                            mled.Unit = mUnit;
                            mled.Factor = mFactor;
                            mled.Qty2 = mQty2;
                            mled.Unit2 = mUnit2;
                            mled.Rate = mRate;
                            mled.Disc = mDisc;
                            mled.DiscAmt = mDiscAmt;
                            mled.Amt = mAmt;
                            mled.BINNumber = mBINNumber;
                            mled.Narr = mNarration;
                            mled.WasteFlag = "";
                            mled.ENTEREDBY = muserid;
                            mled.LASTUPDATEDATE = DateTime.Now;
                            mled.AUTHORISE = "A00";
                            mled.AUTHIDS = muserid;
                            ctxTFAT.Stock.Add(mled);
                            ctxTFAT.SaveChanges();
                            // save data into StockMore table
                            StockMore mstockmore = new StockMore();
                            mstockmore.Branch = mbranchcode;
                            mstockmore.LocationCode = mLocationCode;
                            mstockmore.CompCode = mcompcode;
                            mstockmore.MainType = mMainType;
                            mstockmore.SubType = mSubType;
                            mstockmore.Type = mType;
                            mstockmore.Prefix = mPrefix;
                            mstockmore.Srl = mSrl;
                            mstockmore.Sno = mSno;
                            mstockmore.ParentKey = mParentKey;
                            mstockmore.TableKey = mType + mPrefix.Substring(0, 2) + mSno.ToString("D3") + mSrl;
                            mstockmore.DocDate = mDocDate;
                            mstockmore.Qty = mQty;
                            mstockmore.Factor = mFactor;
                            mstockmore.Qty2 = mQty2;
                            mstockmore.Unit2 = mUnit2;
                            mstockmore.BillNumber = "";
                            mstockmore.WasteFlag = "";
                            mstockmore.ENTEREDBY = muserid;
                            mstockmore.LASTUPDATEDATE = DateTime.Now;
                            mstockmore.AUTHORISE = "A00";
                            mstockmore.AUTHIDS = muserid;
                            ctxTFAT.StockMore.Add(mstockmore);
                            //ctxTFAT.SaveChanges();

                            // save data into StockTax table
                            StockTax mstocktax = new StockTax();
                            mstocktax.Branch = mbranchcode;
                            mstocktax.SubType = mSubType;
                            mstocktax.ParentKey = mParentKey;
                            mstocktax.TableKey = mType + mPrefix.Substring(0, 2) + mSno.ToString("D3") + mSrl;
                            mstocktax.Taxable = 0;
                            mstocktax.TaxAmt = 0;
                            mstocktax.TaxCode = "GST0";
                            mstocktax.Cess = 0;
                            mstocktax.CVDCessAmt = 0;
                            mstocktax.CGSTAmt = 0;
                            mstocktax.CGSTAmt = 0;
                            mstocktax.CVDAmt = 0;
                            mstocktax.CVDExtra = 0;
                            mstocktax.DealerType = 0;
                            mstocktax.ENTEREDBY = muserid;
                            mstocktax.LASTUPDATEDATE = DateTime.Now;
                            mstocktax.AUTHORISE = "A00";
                            mstocktax.AUTHIDS = muserid;
                            ctxTFAT.StockTax.Add(mstocktax);

                            // save data into StockSerial table
                            for (int x = 0; x < mSerial.Count; x++)
                            {
                                if (mSerial[x].ToString() != "")
                                {
                                    StockSerial mserials = new StockSerial();
                                    mserials.Branch = mbranchcode;
                                    mserials.LocationCode = mLocationCode;
                                    mserials.CompCode = mcompcode;
                                    mserials.MainType = mMainType;
                                    mserials.SubType = mSubType;
                                    mserials.Type = mType;
                                    mserials.Prefix = mPrefix;
                                    mserials.Srl = mSrl;
                                    mserials.Sno = mSno;
                                    mserials.SrNo = x;
                                    mserials.ParentKey = mParentKey;
                                    mserials.TableKey = mType + mPrefix.Substring(0, 2) + mSno.ToString("D3") + mSrl;
                                    mserials.DocDate = mDocDate;
                                    mserials.Code = mItemCode;
                                    mserials.Store = mStore;
                                    mserials.Reference = mSerial[x].ToString();
                                    mserials.Qty = mQty;
                                    mserials.sQty = 1;
                                    mserials.Qty2 = mQty2;
                                    mserials.Rate = mRate;
                                    mserials.BillNumber = "";
                                    mserials.ENTEREDBY = muserid;
                                    mserials.LASTUPDATEDATE = DateTime.Now;
                                    mserials.AUTHORISE = "A00";
                                    mserials.AUTHIDS = muserid;
                                    ctxTFAT.StockSerial.Add(mserials);
                                    //ctxTFAT.SaveChanges();
                                }
                            }

                            // save data into StockBatch table
                            for (int x = 0; x < mBatch.Count; x++)
                            {
                                if (mBatch[x].ToString() != "")
                                {
                                    List<string> mBatch2 = new List<string>();
                                    mBatch2 = mBatch[x].Split('^').ToList();
                                    StockBatch mbatchs = new StockBatch();
                                    mbatchs.Branch = mbranchcode;
                                    mbatchs.LocationCode = mLocationCode;
                                    mbatchs.MainType = mMainType;
                                    mbatchs.SubType = mSubType;
                                    mbatchs.Type = mType;
                                    mbatchs.Prefix = mPrefix;
                                    mbatchs.Srl = mSrl;
                                    mbatchs.Sno = mSno;
                                    mbatchs.ParentKey = mParentKey;
                                    mbatchs.TableKey = mType + mPrefix.Substring(0, 2) + mSno.ToString("D3") + mSrl;
                                    mbatchs.DocDate = mDocDate;
                                    mbatchs.Code = mItemCode;
                                    mbatchs.Store = mStore;
                                    mbatchs.Batch = mBatch2[0].ToString();
                                    mbatchs.Qty = Convert.ToDouble(mBatch2[1]);
                                    mbatchs.MfgDate = Convert.ToDateTime(mBatch2[2]);
                                    mbatchs.ExpDate = Convert.ToDateTime(mBatch2[3]);
                                    mbatchs.MRP = Convert.ToDouble(mBatch2[4]);
                                    mbatchs.Factor = mFactor;
                                    mbatchs.Qty2 = mQty2;
                                    mbatchs.Rate = mRate;
                                    mbatchs.Narr = mNarration;
                                    mbatchs.ENTEREDBY = muserid;
                                    mbatchs.LASTUPDATEDATE = DateTime.Now;
                                    mbatchs.AUTHORISE = "A00";
                                    mbatchs.AUTHIDS = muserid;
                                    ctxTFAT.StockBatch.Add(mbatchs);
                                    //ctxTFAT.SaveChanges();
                                }
                            }
                            ctxTFAT.SaveChanges();
                        }
                    }
                    // mbranchcode is blank for the last record, ignore the whole row
                    if (mbranchcode != "") mErrorString = mErrorString + mError;
                }
                if (mErrorString.Trim() != "")
                {
                    return Json(new { Status = "Error", Message = "Partially Saved with following Errors\n" + mErrorString }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = "Success", Message = "Opening Saved Successfully.." }, JsonRequestBehavior.AllowGet);
        }
    }
}