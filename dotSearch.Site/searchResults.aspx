<%@ Page Title="Resultats de votre recherche" Language="C#" MasterPageFile="~/Results.master"
    Inherits="searchResults" Codebehind="~/searchResults.aspx.cs" AutoEventWireup="True" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head" runat="server">
    <tite>Resultats de votre recherche</tite>
    <link href="Styles/Bubble.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form runat="server">
    <center>
        <div id="all">
            <div id="bloc2" style="background:url("Images/bg.jpg")">
                <div id="logo2">
                    <img src='Images/logo.png' width="80px" />
                </div>
                <div id="recherche">
                    <asp:TextBox runat="server" ID="searchArea" />
                    &nbsp;
                    <asp:Button Text="Rechercher" runat="server" ID="searchButton" OnClick="searchButton_Click" />
                </div>
            </div>
            <div id="result" style="color:White">
                <asp:Table runat="server" ID="resultsView" Width="90%">
                </asp:Table>
            </div>
        </div>
    </center>
    </form>
</body>
</html>
