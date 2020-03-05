<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServingSchedulerDetail.ascx.cs" Inherits="ServingSchedulerDetail" %>
<script type="text/javascript">
    // Method will execute on opening the Add member/position popup.
    function UpdateStatusPopup(txt, id) {
        var model = $('#mdStatusPopup');
        // SHowing the model window.
        model.modal('show');
        document.getElementById('popupBody').style = '';
        var value = '0';

        if (txt == 'Declined')
            value = '2';
        else if (txt == 'Accepted')
            value = '1';
        else if (txt == 'Tentative')
            value = '3';
        $('#hdPopupStatusSelectedVal').val(value);
        $('#ddPopupStatus').val(value);
        $('#hdPopupSelectedOccuIdVal').val(id);
        model.show();
    }

    function OnUpdateStatus(action) {
        var model = $('#mdStatusPopup');
        model.modal('hide');
        var value = $('#ddPopupStatus').val();
        $('#hdPopupStatusSelectedVal').val(value);
        if (action == 'OK') {
            document.getElementById('btnUpdateStatus').click();
        }
    }

    function GridRowClick(sender) {
        var colIndex = $(sender).index();
        var table = $(sender).parent().parent().parent();
        var txtHeader = '';
        if (table[0].rows.length > 0) {
            txtHeader = table[0].rows[0].cells[colIndex].innerText;
        }
        if (txtHeader != 'Action' && txtHeader != 'Delete') {
            var row = $(sender).parent();
            var statusColIndex = -1;
            for (var i = 0; i < table[0].rows[0].cells.length; i++) {
                if (table[0].rows[0].cells[i].innerText == 'Status') {
                    statusColIndex = i;
                    break;
                }
            }
            if (statusColIndex > -1) {
                var rowid = $(row).attr('datakey');
                var status = row[0].cells[statusColIndex].innerText;
                UpdateStatusPopup(status, rowid);
            }
        }
        
    }

    function ConfirmSendReminder(e) {
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Do you want to send reminder?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });        
    }
</script>
<asp:UpdatePanel runat="server" ID="upGrid" UpdateMode="Conditional">
    <ContentTemplate>
        <div id="ctl00_main_ctl33_ctl03_ctl06_pnlGroupMembers">
            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h3 class="panel-title pull-left"><b>
                        <i class="fa fa-users"></i>&nbsp;&nbsp;Schedule Detail&nbsp;-&nbsp;<label id="lblServingDateTime" runat="server"></label></b>
                    </h3>
                    <div class="col-md-4 col-sm-6 col-xs-6 pull-right">
                        <font color="red">
                <div class="progress bg-success"  id="divProgressbarWb" runat="server" style="margin-bottom: 0px;">
                    <div id="progressbar" runat="server" class="progress-bar bg-success" role="progressbar" aria-valuemin="0" aria-valuemax="100" ></div>
                </div></font>
                    </div>
                </div>

                <%--<Rock:Grid ID="gdOccurenceDetailList" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No date selected above." DataKeyNames="Id" RowItemText="Serving Scheduler Detail" CssClass="js-grid-group-serving" AllowPaging="true" PageSize="2" EnableSortingAndPagingCallbacks="true">--%>
                <Rock:Grid ID="gdOccurenceDetailList" runat="server"  DisplayType="Full" AllowSorting="true" EmptyDataText="No date selected above" DataKeyNames="Id" RowItemText="Serving Scheduler Detail" CssClass="js-grid-group-serving" AllowPaging="true" PageSize="2" EnableSortingAndPagingCallbacks="true" RowClickEnabled="true">
                    <Columns>
                        <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" HtmlEncode="false" />
                        <Rock:RockBoundField DataField="Individual" HeaderText="Person" HtmlEncode="false" />
                        <Rock:RockBoundField DataField="sPosition" HeaderText="Position" HtmlEncode="false" />
                        <Rock:RockBoundField DataField="ServingDateTime" HeaderText="Serving Date" HtmlEncode="false" />
                       <Rock:RockTemplateField ShowHeader="true" HeaderText="Status">
                            <ItemTemplate>
                                <a href="#" title="Click to edit status" onclick="UpdateStatusPopup('<%#Convert.ToString(Eval("Status"))%>', '<%#Convert.ToString(Eval("Id"))%>')"><%#Convert.ToString(Eval("Status"))%></a>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:RockTemplateField ShowHeader="true" HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnResendReminder" runat="server" Visible='<%#Convert.ToBoolean(Eval("ShowBtn"))%>' OnClientClick="return ConfirmSendReminder(event);" class="btn btn-success btn-sm" CommandName="ResendReminder" CommandArgument='<%# Eval("Id") %>'>
                            <i class="fa fa-envelope"></i> Send Reminder
                                </asp:LinkButton>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:DeleteField HeaderText="Delete" OnClick="DeleteField_Click"></Rock:DeleteField>
                    </Columns>
                </Rock:Grid>
            </div>
        </div>

        <div id="mdStatusPopup" class="modal fade" role="dialog">
            <div class="modal-dialog ">

                <!-- Modal content-->
                <div class="modal-content rock-modal rock-modal-frame">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h3 class="modal-title" id="modelHeader">Serving Details</h3>
                    </div>
                    <div class="modal-body" id="popupBody">
                        <div class="form-group rock-drop-down-list " id="divGrpPerson">
                            <div class="control-wrapper">
                                <label class="control-label" for="ctl00_main_ctl33_ctl01_ctl06_mdEdit_txtPosition">Serving Status<a class="help" href="#" tabindex="-1"><i class="fa fa-question-circle"></i></a>
                                    <div class="alert alert-info help-message" style="display:none"><small>Status of the volunteer being scheduled (Open, Acceptd, or Declined)</small></div></label>
                                <asp:DropDownList ID="ddPopupStatus" CssClass="form-control" runat="server" ClientIDMode="Static">
                                    <asp:ListItem Text="Open" Value="0"></asp:ListItem>
                                    <asp:ListItem Text="Declined" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="Accepted" Value="1"></asp:ListItem>
                                </asp:DropDownList>

                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-link" data-dismiss="modal" onclick="OnUpdateStatus('Cancel')">Cancel</button>
                        <button type="button" class="btn btn-primary" onclick="OnUpdateStatus('OK')">Submit</button>
                    </div>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
<asp:Button ID="btnUpdateStatus" CausesValidation="false" ClientIDMode="Static" Style="display: none;" runat="server" OnClick="EditStatus" />
<input type="hidden" runat="server" id="hdPopupStatusSelectedVal" />
<input type="hidden" runat="server" id="hdPopupSelectedOccuIdVal" />
