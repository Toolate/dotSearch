<%@ Page Title="" Language="C#" MasterPageFile="~/dotSearch.Master" AutoEventWireup="true"
    Inherits="ImgResults" CodeBehind="ImgResults.aspx.cs" %>

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
    <div id="results">
        <asp:Label ID="resultsNbr" runat="server" />
        <br />
        <br />
        <asp:ListView ID="resultsList" runat="server">
            <ItemTemplate>
                <asp:Image ID="image" runat="server" ImageUrl='<%#Eval("URL") %>' />
                <p class="url">
                    <asp:Label ID="url" Text='<%#Eval("Title") %>' runat="server" />
                </p>
                <p class="titre">
                    <asp:HyperLink ID="title" runat="server" Text='<%#Eval("URLcourte") %>' NavigateUrl='<%#Eval("URL") %>'></asp:HyperLink>
                </p>
            </ItemTemplate>
        </asp:ListView>
    </div>
</asp:Content>
