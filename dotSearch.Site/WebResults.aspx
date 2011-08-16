<%@ Page Title="" Language="C#" MasterPageFile="~/Results.master" AutoEventWireup="true" Inherits="Default2" Codebehind="WebResults.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="Styles/results.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SearchResults_cph" runat="Server">
<p id="resultsCount">130 233 Results</p>
    <div id="searchOptions">
        <div>
            <asp:CheckBox ID="Bing_CheckBox" runat="server" />
            <img class="searchOption" src="Images/moteurs/bing.png" alt="chercher avec bing" /></div>
        <br />
        <div>
            <asp:CheckBox ID="Google_CheckBox" runat="server" />
            <img class="searchOption" src="Images/moteurs/google.png" alt="chercher avec google" /></div>
        <br />
        <div>
            <asp:CheckBox ID="Yahoo_CheckBox" runat="server" />
            <img class="searchOption" src="Images/moteurs/yahoo.png" alt="chercher avec Yahoo!" /></div>
    </div>
    <div id="results">
        <div class="result">
            <img class="dot" src="Images/dot.png" alt="dotSearch" />
            <p class="titre">
                L'ESGI, grande école informatique et de design numérique à Paris
            </p>
            <p class="content">
                Forme en 5 ans des ingénieurs en informatique des télécommunications, réseaux et
                développement. Présentation des formations, des enseignants et des Forme en 5 ans
                des ingénieurs en informatique des télécommunications, ...
            </p>
            <p class="url">
                http://www.esgi.fr/accueil.html
            </p>
        </div>
        <div class="result">
            <img class="dot" src="Images/dot.png" alt="dotSearch" />
            <p class="titre">
                L'ESGI, grande école informatique et de design numérique à Paris</p>
            <p class="content">
                Forme en 5 ans des ingénieurs en informatique des télécommunications, réseaux et
                développement. Présentation des formations, des enseignants et des Forme en 5 ans
                des ingénieurs en informatique des télécommunications, ...</p>
            <p class="url">
                http://www.esgi.fr/accueil.html</p>
        </div>
        <div class="result">
            <img class="dot" src="Images/dot.png" alt="dotSearch" />
            <p class="titre">
                L'ESGI, grande école informatique et de design numérique à Paris</p>
            <p class="content">
                Forme en 5 ans des ingénieurs en informatique des télécommunications, réseaux et
                développement. Présentation des formations, des enseignants et des Forme en 5 ans
                des ingénieurs en informatique des télécommunications, ...</p>
            <p class="url">
                http://www.esgi.fr/accueil.html</p>
        </div>
        <div class="result">
            <img class="dot" src="Images/dot.png" alt="dotSearch" />
            <p class="titre">
                L'ESGI, grande école informatique et de design numérique à Paris</p>
            <p class="content">
                Forme en 5 ans des ingénieurs en informatique des télécommunications, réseaux et
                développement. Présentation des formations, des enseignants et des Forme en 5 ans
                des ingénieurs en informatique des télécommunications, ...</p>
            <p class="url">
                http://www.esgi.fr/accueil.html</p>
        </div>
        <div class="result">
            <img class="dot" src="Images/dot.png" alt="dotSearch" />
            <p class="titre">
                L'ESGI, grande école informatique et de design numérique à Paris</p>
            <p class="content">
                Forme en 5 ans des ingénieurs en informatique des télécommunications, réseaux et
                développement. Présentation des formations, des enseignants et des Forme en 5 ans
                des ingénieurs en informatique des télécommunications, ...</p>
            <p class="url">
                http://www.esgi.fr/accueil.html</p>
        </div>

        <asp:DataPager runat="server"/>
    </div>
</asp:Content>
