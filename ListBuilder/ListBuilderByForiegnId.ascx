<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ListBuilderByForiegnId.ascx.cs" Inherits="Plugins.com_DTS.Misc.ListBuilderByForiegnId" %>
<style>
    .nameCss {
        width: 40px;
    }
</style>
<div class="row">
    <div class="col-md-3">
        <div class="panel panel-block">
            <div class="panel-heading">
                <h4 class="panel-title">List Builder By F1 ID's</h4>
            </div>
            <div class="row">
                <div class="col-md-12 text-center">
                    <span>Paste Person Foriegn Id's:</span>
                </div>
            </div>
            <div class="row">

                <div class="col-md-10 col-md-offset-1">
                    <textarea style="resize: Auto;" runat="server" class="form-control" rows="10" id="comment"></textarea>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="comment" ErrorMessage="Please input value"></asp:RequiredFieldValidator>
                </div>
            </div>
            <div class="row">
                <div class="col-md-5"></div>
                <div class="col-md-7">
                    <div class="btnSetting" style="padding: 5px;">
                        <asp:Button OnClick="btnSubmit_Click" class="btn btn-primary" Text="Submit" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="col-md-9">
        <div class="panel panel-block">
            <div class="panel-heading">
                <h4 class="panel-title">List Results</h4>
            </div>
            <div class="row">
                <div class="col-md-12">
					
					    <Rock:Grid ID="gdPerson" OnRowDataBound="OnRowBound" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No People Found" PersonIdField="Id" DataKeyNames="Id" RowItemText="People" OnRowSelected="gList_RowSelected">
                                <Columns>
                                    <Rock:SelectField></Rock:SelectField>
									<Rock:RockBoundField DataField="Id" HeaderText="Id" HtmlEncode="false" />
									<Rock:RockBoundField DataField="GivingId" HeaderText="Giving Id" HtmlEncode="false" />
									<Rock:RockBoundField DataField="GivingLeaderId" HeaderText="Giving Leader Id" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="FirstName" HeaderText="First Name" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="LastName" HeaderText="Last Name" HtmlEncode="false" />
									<Rock:RockBoundField DataField="Email" HeaderText="Email" HtmlEncode="false" />
									<asp:TemplateField HeaderText="Mobile">
										<ItemTemplate>
											<asp:Label runat="server" ID="lblMobileNum"></asp:Label>
										</ItemTemplate>
									</asp:TemplateField>
                                </Columns>
                            </Rock:Grid>

                </div>
               
            </div>
        </div>
    </div>
</div>
