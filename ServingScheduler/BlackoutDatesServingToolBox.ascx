<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlackoutDatesServingToolBox.ascx.cs" Inherits="Plugins.com_DTS.ServingScheduler.BlackoutDatesServingToolBox" %>

<style>
    .nameCss {
        width: 40px;
    }
</style>


<asp:UpdatePanel runat="server" ID="upGrid" UpdateMode="Conditional">
    <ContentTemplate>
        <input type="hidden" runat="server" id="hdPagePersonAliasId" value="-1" />
        <div class="row">
            <div class="col-md-12">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h4 class="panel-title">Blackout Dates</h4>
                    </div>
                    <div class="row">
                        <div class="col-md-12">                          
                            <Rock:Grid ID="gdBlackoutDates" runat="server" OnRowSelected="OnEditRowClick" DisplayType="Full" AllowSorting="true" EmptyDataText="No Blackout Found" DataKeyNames="Id" RowItemText="Blackout Dates" CssClass="js-grid-group-blackoutdates">
                                <Columns>
                                    <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Person" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="StartDateTime" HeaderText="Start Date" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="EndDateTime" HeaderText="End Date" HtmlEncode="false"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="Notes" HeaderText="Note" HtmlEncode="false"></Rock:RockBoundField>
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
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Blackout Detail" SaveButtonText="Submit" ValidationGroup="vgPlaceElsewhere" OnOkScript="return OnClickOK();">
                <Content>
                    <input type="hidden" runat="server" id="hdBlackoutDateId" />
                    <div class="row">
                        <div class="col-md-4">
                            <asp:ValidationSummary ID="vsPlaceDescPos" runat="server" ValidationGroup="vgPlaceElsewhere" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                            <label id="lblValidator" style="color: red;"></label>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:CompareValidator ID="cmpVal1" ControlToCompare="dtStartDate" Display="Static"
                                ControlToValidate="dtEndDate" Type="Date" Operator="GreaterThanEqual" ForeColor="Red"
                                ErrorMessage="  *End Date must be greater then equal to Start Date." runat="server" ValidationGroup="vgPlaceElsewhere"></asp:CompareValidator>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:DatePicker ID="dtStartDate" runat="server" ClientIDMode="Static" AllowFutureDateSelection="true" AllowPastDateSelection="false" DisplayRequiredIndicator="true" Label="Start Date"></Rock:DatePicker>
                        </div>
                        <div class="col-md-2" style="padding-top: 10px">
                            <Rock:DatePicker ID="dtEndDate" runat="server" ClientIDMode="Static" AllowFutureDateSelection="true" AllowPastDateSelection="false" DisplayRequiredIndicator="true" Label="End Date"></Rock:DatePicker>
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockTextBox ID="txtDesc" runat="server" Label="Description" TextMode="MultiLine" Rows="3" CssClass="input-xlarge" Help="Blackout description" />
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
