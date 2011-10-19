<%@ Page Language="C#" AutoEventWireup="true" Inherits="Default" Codebehind="Default.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Search Engine Lab.</title>
    <link rel='stylesheet' href='Styles/style.css' />
</head>
<body>
    <form runat="server">
    <div id="main">
        <div id="menu">
            <a class="selected" href="">Web</a> | <a href="img" class="unselected">Images</a> | <a href="" class="unselected">News</a>
        </div>
        <img id="logo" src="Images/logo.png" alt="Poullistri Search Engine" />
        <asp:TextBox ID="searchBox" runat="server" />
        <asp:Button ID="searchButton" Text="Search!" runat="server" 
            onclick="searchButton_Click" />
        <%--<div id="bottom">
            <img class="logo" src="Images/esgi.png" alt="esgi" />
        </div>--%>
    </div>
    <p id="copyright">©2011 Poullin,Vestri - Esgi project</p>
    </form>
</body>
</html>
