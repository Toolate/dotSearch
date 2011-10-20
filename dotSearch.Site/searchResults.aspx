<%@ Page Title="" Language="C#" MasterPageFile="~/dotSearch.Master" AutoEventWireup="true"
    Inherits="searchResults" CodeBehind="searchResults.aspx.cs" %>

<%@ MasterType VirtualPath="~/dotSearch.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="Styles/results.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="SearchBar_cph">
    <div id="searchBar">
        <div id="menu">
            <a class="selected" href="">Web</a> | <a href="" class="unselected">Images</a> |
            <a href="" class="unselected">News</a>
        </div>
        <img id="logo2" src="Images/logo2.png" alt="dotSearch" />
        <div id="searchBox">
            <asp:TextBox ID="SearchBox" Width="300px" runat="server" />
            <asp:Button ID="SearchButton" runat="server" Text=".Search!" OnClick="SearchButton_Click" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SearchResults_cph" runat="Server">
    <div id="searchOptions">
        <div>
            <asp:CheckBox ID="Bing_CheckBox" runat="server" ViewStateMode="Enabled" 
                oncheckedchanged="Bing_CheckBox_CheckedChanged"/>
            <img class="searchOption" src="Images/moteurs/bing.png" alt="chercher avec bing" /></div>
        <br />
        <div>
            <asp:CheckBox ID="Google_CheckBox" runat="server" ViewStateMode="Enabled" 
                oncheckedchanged="Google_CheckBox_CheckedChanged" />
            <img class="searchOption" src="Images/moteurs/google.png" alt="chercher avec google" /></div>
        <br />
    </div>
    <div id="results">
        <asp:Label ID="resultsNbr" runat="server" />
        <br />
        <br />
        <asp:ListView ID="resultsList" runat="server">
            <ItemTemplate>
                <img class="dot" src="Images/dot.png" alt="dotSearch" />
                <p class="titre">
                    <asp:HyperLink ID="title" runat="server" Text='<%#Eval("Title") %>' NavigateUrl='<%#Eval("URL")%>'></asp:HyperLink>
                </p>
                <p class="content">
                    <asp:Label ID="description" Text='<%#Eval("Description") %>' runat="server" />
                </p>
                <p class="url">
                    <asp:Label ID="url" Text='<%#Eval("URL") %>' runat="server" />
                </p>
            </ItemTemplate>
        </asp:ListView>
        <asp:DataPager ID="pager" PagedControlID="resultsList" runat="server" PageSize="30" >
            <Fields>
                <asp:NumericPagerField />
            </Fields>
        </asp:DataPager>
    </div>
</asp:Content>
