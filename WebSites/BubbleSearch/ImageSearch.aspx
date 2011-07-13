<%@ Page Title="Bienvenue sur BubbleSearch" Language="C#" CodeFile="ImageSearch.aspx.cs"
    Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head" runat="server">
    <link href="Styles/Bubble.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <div id="all">
        <div id="bloc">
            <div id="logo2">
                <img src='Images/logo.png' width="80px" />
            </div>
            <div id="recherche">
                <input type="text" size="70" />
                &nbsp;
                <input id="bouton" type="submit" value="Rechercher">
            </div>
        </div>
        <div id="result">
            <div id="bloc_image">
                <div id="image">
                    <img src="Images/logo.png" width="140px">
                </div>
                <div id="titre_image">
                    http://www.google.com/Image_1.jpg
                </div>
            </div>
            <div id="bloc_image">
                <div id="image">
                    <img src="Images/logo.png" width="140px">
                </div>
                <div id="titre_image">
                    http://www.google.com/Image_1.jpg
                </div>
            </div>
            <div id="bloc_image">
                <div id="image">
                    <img src="Images/logo.png" width="140px">
                </div>
                <div id="titre_image">
                    http://www.google.com/Image_1.jpg
                </div>
            </div>
            <div id="bloc_image">
                <div id="image">
                    <img src="Images/logo.png" width="140px">
                </div>
                <div id="titre_image">
                    http://www.google.com/Image_1.jpg
                </div>
            </div>
        </div>
    </div>
    </center>
</body>
</html>
