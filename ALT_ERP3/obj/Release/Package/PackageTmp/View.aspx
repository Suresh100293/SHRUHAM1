<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="View.aspx.cs" Inherits="ReportViewer2013.View" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Report Viewer</title>
</head>

<body>
    <form id="form1" runat="server">
        <asp:scriptmanager runat="server"></asp:scriptmanager>
        <div>
            <asp:Label ID="lblError" runat="server" ForeColor="Red">Error: none</asp:Label><br />
            <asp:Button ID="btnDownload" Text="Download" runat="server" onclick="btnDownload_Click" />
            <rsweb:ReportViewer ID="rvReportViewer" runat="server" Width="723px"></rsweb:ReportViewer>
        </div>
    </form>
</body>

</html>
