<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MoveCheckinAttendance.ascx.cs" Inherits="MoveParticipantsLocation" %>
<%--<script runat="server">

    protected void ScheduleDropdown_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

</script>--%>

<script type="text/javascript">
    var isMovingKids = true;
    // Method will execute on opening the Add member/position popup.
    function MoveInGroup(isKids) {
        isMovingKids = isKids;
        // divGrpPerson, divGrpPositions       
        var model = $('#mdMoveAttendee');
        // SHowing the model window.        
        document.getElementById('popupBody').style = '';
        var html = '';
        var selected = [];
        var i = 0;
        debugger;
        //document.getElementById('ddGroupVolunteerSelect').style.display = isKids ? 'none' : '';
        //document.getElementById('ddGroupSelect').style.display = isKids ? '' : 'none';
        var arrChk = document.getElementById('participantsList').getElementsByTagName('input');
        for (var index = 0; index < arrChk.length; index++) {
            var t = arrChk[index];
            if (t.checked == false) continue;
            if (isKids && t.className.indexOf('kids') <= 0) {
                continue;
            } else if (isKids == false && t.className.indexOf('volunteer') <= 0) {
                continue;
            }
            var lbl = $('#' + t.id).next('label');
            var name = t.title;
            var id = t.id.replace('chkPerticipant', '');
            if (i == 0) {
                html += "<div class='row'>";
            }
            html += "<div class='col-sm-4'>";
            html += "<input type='checkbox' id='chkPerticipantItem" + id + "' checked='checked'>" + name + "</input>";
            html += "</div>";
            i = i + 1;
            if (i == 2) {
                i = 0;
                html += "</div>";
            }
        }
        if (html == '') {
            alert('Select atleast one attendee to move to a different room.');
            return false;
        }
        else {
            document.getElementById('chkBoxList').innerHTML = html;
            model.modal('show');
        }
        return false;
    }
    function MoveIntoGroup() {
        var idList = '';
        var arrChk = document.getElementById('chkBoxList').getElementsByTagName('input');
        for (var index = 0; index < arrChk.length; index++) {
            var t = arrChk[index];
            if (t.checked == false) continue;
            var id = t.id.replace('chkPerticipantItem', '');
            idList += id + ',';
        }
        if (idList == '') {
            alert('Select atleast one attendee to move to a different room.');
            return false;
        }
        var hd = $('#hdSelectedPerson');
        hd.val(idList);
        var value = '';
        //ddGroupVolunteerSelect
        value = $('#ddGroupSelect').val();
        $('#hdSelectedMoveToGroupVal').val(value);
        if (isMovingKids) {
            document.getElementById('bootStrapBtn').click();
        }
        else {            
            document.getElementById('bootStrapVolunteerBtn').click();
        }        
        return true;
    }
    function CloseGroup() {
        var innerTxt = document.getElementById('lblGroupName').innerText;
        Rock.dialogs.confirm('Are you sure you want to delete ' + innerTxt + ' Group?', function (result) {
            if (result) {
                document.getElementById('bootStrapBtnClose').click();
            }
            return false;
        });
        return false;
    }

    function RemoveAttendee() {
        var ids = GetSelectedAttendeeId();
        if (ids == '') {
            alert('Select atleast one Attendee to remove.');
            return false;
        }        
        $('#hdRemoveAttendee').val(ids);
        Rock.dialogs.confirm('Are you sure you want to remove selected Attendee(s)?', function (result) {
            if (result) {
                document.getElementById('btnRemoveAttendee').click();
                return true;
            }
            return false;
        });
        return false;
    }

     function DeleteAttendee() {
        var ids = GetSelectedAttendeeId();
        if (ids == '') {
            alert('Select atleast one Attendee to delete.');
            return false;
        }        
        $('#hdRemoveAttendee').val(ids);
        Rock.dialogs.confirm('Are you sure you want to delete selected Attendee(s)?', function (result) {
            if (result) {
                document.getElementById('btnDeleteAttendee').click();
                return true;
            }
            return false;
        });
        return false;
    }

    function GetSelectedAttendeeId() {
        var ids = '';
        var arrChk = document.getElementById('participantsList').getElementsByTagName('input');
        for (var index = 0; index < arrChk.length; index++) {
            var t = arrChk[index];
            if (t.checked == false) continue;
            
            var id = t.id.replace('chkPerticipant', '');
            ids += id + ';';
        }
        return ids;
    }

    function ActiveLocation(isActive) {
        if (isActive) {
            document.getElementById('btnOnLoc').click();
        } else
            document.getElementById('btnOffLoc').click();
    }
</script>
<style type="text/css">
    .btn-default.btn-on.active {
        background-color: #5BB75B;
        color: white;
    }

    .btn-default.btn-off.active {
        background-color: #DA4F49;
        color: white;
    }
    .grid-paging{
        display: none;
    }
    .grid-actions{
        display: none;
    }
    .Caphidden {
        display: none!important;
    }
</style>
   
<asp:UpdatePanel runat="server" ID="upGridMoveParticipants" UpdateMode="Always">
    <ContentTemplate>
        <div class="col-sm-12 text-left" style="padding-bottom:5px;">
           <span class="text-left col-sm-3">
               <Rock:RockDropDownList ID="ScheduleDropdown" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ScheduleDropdown_SelectedIndexChanged" />
           </span>
        </div>
        <div class="col-sm-4">
            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h3 class="panel-title pull-left"><b>
                        <i class="fa fa-building"></i>&nbsp;&nbsp;Room List&nbsp;&nbsp;</b>
                    </h3>                
                    <div class="col-sm-12" style="padding-right:0px !important;">
                        <span class="text-right col-sm-12" ><b>Volunteers:</b> <label id="lblVolunteersCount" runat="server">XXX</label></span>
                        <span class="text-right col-sm-12"><b>Children:</b> <label id="lblChildCount" runat="server">YYY</label></span>
                        <span class="text-right col-sm-12"><b>Total:</b> <label id="lblTotalCount" runat="server">ZZZZ</label></span>
                    </div>

                </div>
                <input type="hidden" runat="server" id="hdSelectedGroupId" />
                <Rock:Grid ID="gdGroupList" OnRowSelected="OnEditRowClick" runat="server" DisplayType="Full" DataKeyNames="Id" AllowSorting="true" EmptyDataText="No data" RowItemText="Group Detail" CssClass="js-grid-group-serving" AllowPaging="true" PageSize="2" RowClickEnabled="true">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" HtmlEncode="false"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Capacity" HeaderText="Capacity"  HtmlEncode="false" ItemStyle-CssClass="Caphidden" HeaderStyle-CssClass="Caphidden"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="VolunteerCount" HeaderText="Vols"  HtmlEncode="false"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="ChildCount" HeaderText="Kids" HtmlEncode="false"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="IsActive" HeaderText="Rm Status" HtmlEncode="false"></Rock:RockBoundField>
                    </Columns>
                </Rock:Grid>
            </div><small>Move Check-in Attendance V2.0</small>
        </div>
        <div class="col-sm-8" id="divRightPanel" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h3 class="panel-title pull-left"><b>
                        <i class="fa fa-users"></i>&nbsp;&nbsp;<label id="lblGroupName" runat="server"></label>&nbsp;&nbsp;</b>
                    </h3>                            
                    <span class="btn-group pull-right" id="status" data-toggle="buttons">
                        <label class="btn btn-default btn-on btn-xs active" id="lblOnLocation" runat="server" onclick="ActiveLocation(true)">
                            <input type="radio" value="1" name="multifeatured_module" checked="checked">Open</label>
                        <label class="btn btn-default btn-off btn-xs " id="lblOffLocation" runat="server" onclick="ActiveLocation(false)">
                            <input type="radio" value="0" name="multifeatured_module">Close</label>                                                               
                    </span>
                        <asp:Button ID="btnOnLoc" style="display: none;" runat="server" ClientIDMode="Static" OnClick="BtnOnLoc_Click" CausesValidation="false"></asp:button>
                        <asp:Button ID="btnOffLoc" style="display: none;" runat="server" ClientIDMode="Static" OnClick="BtnOffLoc_Click" CausesValidation="false"></asp:Button>                                
                </div>
                <input type="hidden" runat="server" id="hdGroupIdVal" />
                <div class="panel-body">
                    <div class="form-group schedule-picker ">
                        <div class="control-wrapper">
                            <Rock:NumberUpDown ID="numCapacity" OnNumberUpdated="OnUpdateCapacity" runat="server" Required="true" Minimum="0" onchange="OnCapacityChange(this);" RequiredErrorMessage="Enter capacity" Label="Capacity" />
                        </div>                        
                    </div>
                    <div id="participantsList" runat="server"></div>
                    <div class="col-sm-12" style="padding-top:15px">
                            <button type="button" class="btn btn-default" id="btnMoveKids" runat="server" title="Move" onclick="return MoveInGroup(true);" value="Move">
                                Move Kids</button>
                            <button type="button" class="btn btn-default" id="btnMoveVolunteer" runat="server" title="Move" onclick="return MoveInGroup(false);" value="Move">
                                Move Volunteers</button>
                            <span class="pull-right"><button type="button" class="btn btn-link" title="Remove Attendee" onclick="return RemoveAttendee();" value="Close">
                                Check-Out</button>                      
                            <button type="button" class="btn btn-danger" title="Delete Attendance" onclick="return DeleteAttendee();" value="Close">
                                Delete Attendance</button></span>                    
                            <Rock:BootstrapButton ID="btnRemoveAttendee" style="display: none;" runat="server" ClientIDMode="Static" OnClick="OnClickRemoveAttendee" CausesValidation="false"></Rock:BootstrapButton>                                
                            <Rock:BootstrapButton ID="btnDeleteAttendee" style="display: none;" runat="server" ClientIDMode="Static" OnClick="OnClickDeleteAttendee" CausesValidation="false"></Rock:BootstrapButton>                                
                            <Rock:BootstrapButton ID="bootStrapBtnClose" style="display: none;" runat="server" ClientIDMode="Static" OnClick="BtnCloseGroup_Click" CausesValidation="false"></Rock:BootstrapButton>
                    </div>
                </div>
            </div>
        </div>


        <!-- Popup Window -->
        <div id="mdMoveAttendee" class="modal fade" role="dialog">
            <div class="modal-dialog">

                <!-- Modal content-->
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title" id="modelHeader">Move Attendee</h4>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <div class="control-wrapper" id="chkBoxList">
                            </div>
                        </div>
                    </div>
                    <div class="modal-body" id="popupBody">
                        <div class="form-group rock-drop-down-list " id="divGrpPositions">
                            <div class="control-wrapper">
                                <Rock:RockDropDownList ID="ddGroupSelect" ClientIDMode="Static" runat="server"></Rock:RockDropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <Rock:BootstrapButton ID="bootStrapBtn" Style="display: none;" runat="server" ClientIDMode="Static" OnClick="BtnMoveGroup_Click" CausesValidation="false"></Rock:BootstrapButton>
                        <Rock:BootstrapButton ID="bootStrapVolunteerBtn" Style="display: none;" runat="server" ClientIDMode="Static" OnClick="BtnMoveVolunteer_Click" CausesValidation="false"></Rock:BootstrapButton>
                        <button type="button" class="btn btn-default" id="btnMoveIntoGrp" onclick="return MoveIntoGroup();">Move</button>
                        <button type="button" class="btn btn-default" id="btnCloseGroup" data-dismiss="modal">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<input type="hidden" runat="server" id="hdSelectedPerson" />
<input type="hidden" runat="server" id="hdSelectedMoveToGroupVal" />
<input type="hidden" runat="server" id="hdRemoveAttendee" />

