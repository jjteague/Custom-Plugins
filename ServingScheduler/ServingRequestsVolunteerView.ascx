<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServingRequestsVolunteerView.ascx.cs" Inherits="ServingRequestsVolunteerView" %>
<asp:UpdatePanel runat="server" ID="upGrid" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Literal ID="ltrlRequestVolunteer" runat="server"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>
