<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServingSchedulerListLava.ascx.cs" Inherits="ServingSchedulerListLava" %>


<asp:UpdatePanel runat="server" ID="upGrid" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Literal ID="ltrlServingSchedulerList" runat="server"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>
