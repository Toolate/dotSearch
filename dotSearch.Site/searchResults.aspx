<%@ Page Title="" Language="C#" MasterPageFile="~/dotSearch.Master" AutoEventWireup="true"
    Inherits="searchResults" CodeBehind="searchResults.aspx.cs" %>

<%@ MasterType VirtualPath="~/dotSearch.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="Styles/results.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="SearchBar_cph">
    <div id="searchBar">
        <div id="menu">
            <a class="selected" href="searchResults.aspx">Web</a> | <a href="ImgResults.aspx"
                class="unselected">Images</a>
        </div>
        <a href="Default.aspx" class="unselected">
            <img id="logo2" src="Images/logo2.png" alt="dotSearch" />
        </a>
        <div id="searchBox">
            <asp:TextBox ID="SearchBox" Width="300px" runat="server" />
            <asp:Button ID="SearchButton" runat="server" Text=".Search!" OnClick="SearchButton_Click" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SearchResults_cph" runat="Server">
    <div id="searchOptions">
        <img src="Images/ajout_recherches.png" />
        <div id="boogle">
            <asp:CheckBox ID="Bing_CheckBox" runat="server" ViewStateMode="Enabled" OnCheckedChanged="Bing_CheckBox_CheckedChanged" />
            <img class="searchOption" src="Images/moteurs/bing.png" alt="chercher avec bing" />
            <br />
            <asp:CheckBox ID="Google_CheckBox" runat="server" ViewStateMode="Enabled" OnCheckedChanged="Google_CheckBox_CheckedChanged" />
            <img class="searchOption" src="Images/moteurs/google.png" alt="chercher avec google" />
        </div>
    </div>
    <div id="results">
        <div id="countNtimer">
            <asp:Label ID="resultsNbr" runat="server" />
        </div>
        <div id="cadre_transparent">
            <asp:ListView ID="resultsList" runat="server">
                <ItemTemplate>
                    <div id="resultat">
                        <p class="titre">
                            <asp:HyperLink ID="title" runat="server" Text='<%#Eval("Title") %>' NavigateUrl='<%#Eval("URL")%>'></asp:HyperLink><img
                                class="logo_moteur" src='Images/<%#Eval("Engine") %>.png' alt="dotSearch" />
                        </p>
                        <p class="url">
                            <asp:Label ID="url" Text='<%#Eval("URLcourte") %>' runat="server" />
                        </p>
                        <p class="content">
                            <asp:Label ID="description" Text='<%#Eval("Description") %>' runat="server" />
                        </p>
                    </div>
                </ItemTemplate>
            </asp:ListView>
        </div>
        <asp:DataPager ID="pager" PagedControlID="resultsList" runat="server" PageSize="25">
            <Fields>
                <asp:NumericPagerField />
            </Fields>
        </asp:DataPager>
    </div>
</asp:Content>
