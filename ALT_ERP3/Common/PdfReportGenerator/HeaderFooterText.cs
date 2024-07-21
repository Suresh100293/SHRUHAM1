using System.Web;

namespace Common.ReportManagement
{
    public static class HeaderFooterText
    {
        public static string[] GetHeader()
        {
            string Headerline1 = "";

            string[] arrayHeader = { Headerline1 };
            return arrayHeader;
        }

        public static string[] GetFooter()
        {
            string FooterLine0 = "____________________________________________________________________________________________________________________";
            string FooterLine1 = "        ";

            //string FooterLine2 = "Regd. Office:Millennium Park, Office No.1, Plot No.17, Sector no.25, Nerul Navi Mumbai-400 706. Telefax:2770 6774/81";
            //string FooterLine3 = "Works:Millennium Park, Shop no.23 & 24, Plot no.17, Sector no. 25, Nerul, Navi Mumbai-400 706. ";
            //string FooterLine4 = "Pune:Telefax: 95-20-2443 3864.Email:global.elevators@yahoo.com";
            //string FooterLine5 = "        ";

            string[] arrayFooter = { FooterLine0, FooterLine1 };//, FooterLine2, FooterLine3, FooterLine4, FooterLine5 };

            return arrayFooter;
        }

        public static string GetImagePath()
        {
            string path = null;
            //string path = HttpContext.Current.Server.MapPath("~/PdfContent/report.png");
            // string path = HttpContext.Current.Server.MapPath("~/Images/bullet.png");
            return path;
            //return @"E:\workplace\MPlus_Online_231012\Common\MapleTree.Common\PdfReportGenerator\PageImage\report.jpg";
        }
    }
}