<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="dotSearch.Site.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Bienvenue sur dotSearch</title>
    <link href="Styles/Bubble.css" rel="stylesheet" type="text/css" />
    <style type="text/css">
        .style1
        {
            text-align: left;
        }
        #bloc_recherche
        {
            height: 135px;
            width: 676px;
        }
    </style>
    <script language="javascript" type="text/javascript">
// <![CDATA[

        function logo2_onclick() {

        }

// ]]>
    </script>
</head>
<body>
    <form id="Form1" runat="server">
    <center>
        <div id="all">
            <div id="bloc">
                <div id="bloc_recherche">
                    <img id="logo2" src='Images/logo.png' alt="" width="100px" onclick="return logo2_onclick()" />
                    <div id="type_recherche" class="style1">
                        | &nbsp; <a href="Default.aspx">Web</a> &nbsp; | &nbsp; <a href="ImageSearch.aspx">Images</a>
                        &nbsp; |
                    </div>
                    <div id="recherche">
                        &nbsp;<asp:TextBox runat="server" ID="searchArea" Width="445px" />
                        &nbsp;<asp:Button Text="Rechercher" runat="server" ID="searchButton" OnClick="searchButton_Click" />
                    </div>
                </div>
            </div>
        </div>
    </center>
    </form>
</body>
</html>

