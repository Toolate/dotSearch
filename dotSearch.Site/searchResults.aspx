<%@ Page Title="Resultats de votre recherche" Language="C#" MasterPageFile="~/Results.master"
    Inherits="searchResults" Codebehind="~/searchResults.aspx.cs" AutoEventWireup="True" %>


<asp:Content ContentPlaceHolderID="SearchResults_cph" ID="SearchResults_cph" runat="server">
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
            <div id="testData">
                <asp:DataList runat="server">
                    <ItemTemplate>
                        <asp:Label Text="testResult" runat="server" />
                    </ItemTemplate>
                </asp:DataList>
            </div>
        </div>
    </center>
    </asp:Content>
