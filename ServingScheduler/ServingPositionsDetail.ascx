<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServingPositionsDetail.ascx.cs" Inherits="Plugins.com_DTS.Misc.ServingPositionsDetail" %>

<style>
    .nameCss {
        width: 40px;
    }
</style>


<asp:UpdatePanel runat="server" ID="upGrid" UpdateMode="Conditional">
    <ContentTemplate>

        <div class="row">
            <div class="col-md-12">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h4 class="panel-title">Serving Positions</h4>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue"  OnApplyFilterClick="rFilter_ApplyFilterClick" OnClearFilterClick="rFilter_ClearFilterClick" >
                                <Rock:RockTextBox ID="txtFilterNamePos" runat="server" Label="Position Name" />
                                <Rock:RockDropDownList ID="ddFilterActive" runat="server" Label="Active">
                                    <asp:ListItem Text="Active/Inactive" Value="Active/Inactive" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Active" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="InActive" Value="0"></asp:ListItem>
                                </Rock:RockDropDownList>
                         <%--       <Rock:RockCheckBox ID="chkFilterActive" runat="server" Text="Active" />--%>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gdServing" runat="server" OnRowSelected="OnEditRowClick" DisplayType="Full" AllowSorting="true" EmptyDataText="No Positions Found" DataKeyNames="Id" RowItemText="Serving Position" CssClass="js-grid-group-serving">
                                <Columns>
                                    <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="Position" HeaderText="Position" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" HtmlEncode="false" />
                                    <Rock:BoolField DataField="Active" HeaderText="Active" HtmlEncode="false"></Rock:BoolField>
                                    <Rock:DeleteField HeaderText="Delete" OnClick="DeleteField_Click"></Rock:DeleteField>
                                </Columns>
                            </Rock:Grid>

                        </div>

                    </div>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel runat="server" ID="upnlEdit" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false" ClientIDMode="Static">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Serving Positions Detail" SaveButtonText="Submit" ValidationGroup="vsPlaceDescPos">
                <Content>
                    <input type="hidden" runat="server" id="hdServingId" />
                    <div class="row">
                        <div class="col-md-4">
                            <asp:ValidationSummary ID="vsPlaceDescPos" runat="server" ValidationGroup="vgPlaceElsewhere" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="txtPosition" runat="server" Label="Position Name" DisplayRequiredIndicator="true" Required="true" RequiredErrorMessage="Position is required." CssClass="input-large" Help="Serving positions" ValidationGroup="vsPlaceDescPos" />
                            <asp:RequiredFieldValidator ID="reqPos" ControlToValidate="txtPosition" runat="server" ForeColor="Red" Text="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="col-md-2" style="Padding-Top:10px">
                            <Rock:RockCheckBox ID="chkActive" runat="server" Text="Active" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockTextBox ID="txtDesc" runat="server" Label="Description" DisplayRequiredIndicator="true" Required="true" RequiredErrorMessage="Description is required." TextMode="MultiLine" Rows="3" CssClass="input-xlarge" Help="Serving positions description" ValidationGroup="vsPlaceDescPos" />
                            <asp:RequiredFieldValidator ID="reqDesc" ControlToValidate="txtDesc" runat="server" ForeColor="Red" Text="*"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
