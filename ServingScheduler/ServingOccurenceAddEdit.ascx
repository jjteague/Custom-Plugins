<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServingOccurenceAddEdit.ascx.cs" Inherits="Plugins.com_DTS.Misc.ServingOccurence" %>
<script runat="server">

    protected void ddSubmit_Click(object sender, EventArgs e)
    {

    }
</script>
<script type="text/javascript">

    var rowSeperatorKey = "|~|~|";
    var multiRowSeperatorKey = "|~~|~~|";

    function OnSaveBtnClick() {
        // ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_lblNumber
        var eValue = parseInt(document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber').value);
        var elementValues = "";
        var eBlackout = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdPersonOnBlackoutVal');
        var arrBlackout;
        if (eBlackout != null) {
            arrBlackout = eBlackout.value.split(multiRowSeperatorKey);
        }
        var blackoutDate = "";
        for (var i = 0; i < eValue ; i++) {
            elementValues += $('#ddMem' + i).val() + rowSeperatorKey + $('#ddPos' + i).val() + rowSeperatorKey + $('#ddStatus' + i).val();
            if (i != (eValue - 1))
                elementValues += multiRowSeperatorKey;
            var selDate = $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbDate').val() + " " + $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbTime').val();
            var name = Validation(arrBlackout, $('#ddMem' + i).val(), $("#ddMem" + i + " option:selected").text(), new Date(selDate), i);
            blackoutDate += blackoutDate == "" ? name : ", " + name;
        }
        $('#hdSchedulerOccurenceVal').val(elementValues);

        var el = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdSchedulerOccurenceVal');
        if (el != null) {
            el.value = elementValues;
        }
        if (blackoutDate == "" || confirm("'" + blackoutDate + "' members are on vacation, Do you still want to continue?")) {
            return true;
        }
        return false;
    }


    function OnSaveBtnClick11() {
        // ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_lblNumber
        var eValue = parseInt(document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber').value);
        var elementValues = "";
        var eBlackout = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdPersonOnBlackoutVal');
        var arrBlackout;
        if (eBlackout != null) {
            arrBlackout = eBlackout.value.split(multiRowSeperatorKey);
        }
        var blackoutDate = "";
        var i = 0;
        var totalItem = 0;
        for (var j = 0; j < eValue ; j++) {
            if (j > i) {
                i = j;
            }
            var rowEle = document.getElementById('row' + i);
            while (rowEle == null) {
                i = i + 1;
                rowEle = document.getElementById('row' + i);
            }
            elementValues += $('#ddMem' + i).val() + rowSeperatorKey + $('#ddPos' + i).val() + rowSeperatorKey + $('#ddStatus' + i).val();
            if (totalItem != (eValue - 1))
                elementValues += multiRowSeperatorKey;
            var selDate = $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbDate').val() + " " + $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbTime').val();
            var name = Validation(arrBlackout, $('#ddMem' + i).val(), $("#ddMem" + i + " option:selected").text(), new Date(selDate), i);
            blackoutDate += blackoutDate == "" ? name : ", " + name;
            totalItem = totalItem + 1;
            i = i + 1;
        }
        $('#hdSchedulerOccurenceVal').val(elementValues);

        var el = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdSchedulerOccurenceVal');
        if (el != null) {
            el.value = elementValues;
        }
        if (blackoutDate == "" || confirm("'" + blackoutDate + "' members are on vacation, Do you still want to continue?")) {
            return true;
        }
        return false;
    }

    function OnChangeDate() {
        var eValue = parseInt(document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber').value);
        var eBlackout = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdPersonOnBlackoutVal');
        var arrBlackout;
        if (eBlackout != null) {
            arrBlackout = eBlackout.value.split(multiRowSeperatorKey);
        }

        var selDate = $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbDate').val() + " " + $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbTime').val();
        for (var i = 0; i < eValue ; i++) {
            Validation(arrBlackout, $('#ddMem' + i).val(), $("#ddMem" + i + " option:selected").text(), new Date(selDate), i);
        }
    }

    function Validation(arrBlackout, groupMemId, memName, selDate, index) {

        for (var i = 0; i < arrBlackout.length; i++) {
            var id = arrBlackout[i].split(';');
            if (id.length > 0 && id[0] == groupMemId) {
                var dates = id[1].split(rowSeperatorKey);
                for (var j = 0; j < dates.length; j++) {
                    var date12 = dates[j].split("::");
                    if (date12.length == 2 && date12[0] != "" && date12[1] != "") {
                        var dt1 = new Date(date12[0]);
                        var dt2 = new Date(date12[1]);
                        if (dt1 <= selDate && (dt2 >= selDate)) { // || dt2 <= selDate
                            var warning = document.getElementById('warningLi' + index);
                            warning.style.display = "";
                            warning.title = getSelectedText('ddMem' + i) + ' has a schedule restriction and should be cleared with the volunteer before scheduling!';
                            return memName;
                        }
                    }
                }
            }
        }
        document.getElementById('warningLi' + index).style.display = "none";
        return "";
    }

    function getSelectedText(elementId) {
        var elt = document.getElementById(elementId);

        if (elt.selectedIndex == -1)
            return null;

        return elt.options[elt.selectedIndex].text;
    }


    function OnSelectMember(index) {
        var eBlackout = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdPersonOnBlackoutVal');
        var arrBlackout;
        if (eBlackout != null) {
            arrBlackout = eBlackout.value.split(multiRowSeperatorKey);
        }
        var selDate = $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbDate').val() + " " + $('#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbTime').val();
        var name = Validation(arrBlackout, $('#ddMem' + index).val(), $("#ddMem" + index + " option:selected").text(), new Date(selDate), index);

    }

    var prevValue = 0;

    var deleteRowExec = 0;
    function OnTotalPositionChange(sender) {
        if (deleteRowEvent) {
            if (deleteRowExec == 1) {
                deleteRowEvent = false;
                deleteRowExec = 0;
            }
            else
                deleteRowExec += 1;
            deleteRowEvent = false;
            return;
        }
        var eValue = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber').value;
        GenerateRow(parseInt(eValue));
    }

    function SetOccurenceRows() {
        var totalOccurenceRow = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber').value;
        var el = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdSchedulerOccurenceVal');
        var arr = "";
        if (el != null) {
            arr = el.value.split(multiRowSeperatorKey);
        }
        for (var i = 0; i < arr.length; i++) {
            var valArr = arr[i].split(rowSeperatorKey);
            $('#ddMem' + i).val(valArr[0]);
            $('#ddPos' + i).val(valArr[1]);
            $('#ddStatus' + i).val(valArr[2]);
        }
    }

    function GenerateRow(value) {
        if (prevValue == 0) {
            GenerateOccurenceRows(true, 0, value);
        }
        else if (prevValue > value) {
            // Remove rows
            var val = prevValue - value;
            val = prevValue - val;
            GenerateOccurenceRows(false, val, prevValue);
        } else if (prevValue < value) {
            // Add rows
            GenerateOccurenceRows(true, prevValue, value);
        }
        prevValue = value;
    }
    var deleteRowEvent = false;
    function DeleteRow(rowid) {
        var row = document.getElementById('row'+rowid);
        if (row != null) {
            row.remove();
            var ele = document.getElementsByClassName('js-number-down');
            if (ele != null && ele.length == 1) {
                var ctrl = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber');
                if (ctrl != null) {
                    deleteRowEvent = true;
                    var prevCount = parseInt(ctrl.value);
                    Rock.controls.numberUpDown.adjust(this, -1, '');
                    ctrl.value = parseInt(ctrl.value) - 1;
                    document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_lblNumber').innerText = ctrl.value;
                    RenameNextRowsId(rowid, prevCount);
                    self.prevValue = ctrl.value;
                }
            }
        }
    }

    function RenameNextRowsId(deleterowid, prevCount) {
        var dRow = parseInt(deleterowid);
        for (var i = dRow + 1; i < prevCount; i++) {
            var row = document.getElementById('row' + i);
            if (row != null) {
                var val = (i - 1);
                row.id = 'row' + val;
                var mem = document.getElementById('ddMem' + i);
                mem.id = 'ddMem' + val;
                var pos = document.getElementById('ddPos' + i);
                pos.id = 'ddPos' + val;
                var status = document.getElementById('ddStatus' + i);
                status.id = 'ddStatus' + val;
                var warn = document.getElementById('warningLi' + i);
                warn.id = 'warningLi' + val;
                var del = document.getElementById('aDelete' + i);
                del.id = "aDelete" + val;
                del.click = 'DeleteRow("' + val + '")';
            }
        }
    }

    function GenerateOccurenceRows(add, startFrom, totalOccurenceRow) {
        for (var i = startFrom; i < totalOccurenceRow; i++) {
            if (add) {
                var content = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_ddMember1').innerHTML;
                var memId = 'ddMem' + i;
                var html = '';
                var className = '';
                var firstRowWarningClass = ''
                if (i > 0) {
                    className = ' visible-xs';                    
                } else
                    firstRowWarningClass = 'margin-top:20px;';
                html = '<div class="row" id="row' + i + '">';
                // Start generating member column.
                html += '<div class="col-sm-3">';
                html += '<div class="form-group">';
                html += '<div class="form-group rock-drop-down-list ">';
                html += '<label class="control-label ' + className + '" for="' + memId + '">Serving Member</label>';
                html += '<div class="control-wrapper">';
                html += '<select id="' + memId + '" class="form-control" onchange="OnSelectMember(' + "'" + i + "'" + ')">';
                html += content;
                html += '</select>';
                html += '</div></div></div></div>';
                // End generating member column.
                // Start generating position column         

                content = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_ddPosition1').innerHTML;
                var posId = 'ddPos' + i;
                html += '<div class="col-sm-3">';
                html += '<div class="form-group">';
                html += '<div class="form-group rock-drop-down-list ">';
                html += '<label class="control-label ' + className + '" for="' + posId + '">Position</label>';
                html += '<div class="control-wrapper">';
                html += '<select id="' + posId + '" class="form-control">';
                html += content;
                html += '</select>';
                html += '</div></div></div></div>';
                // End generating position column       
                // Original Amit Code to close row html += '<div class="col-sm-3" />';
                // Mike Sample HTML
                html += '<div class="col-sm-2"><div class="form-group"><div class="form-group rock-drop-down-list "><label class="control-label ' + className + '" for="Status">Status</label><div class="control-wrapper"><select id="ddStatus' + i + '" class="form-control"><option value="0">Open</option><option value="1">Accepted</option><option value="2">Declined</option></select></div></div></div></div>';
                html += '<div class="col-sm-1" style="font-size:2.3em; color:#d9534f;height:72px;"><div class="form-group"><div class="form-group rock-drop-down-list "><label class="control-label ' + className + '" for="Remove row"></label><div class="control-wrapper"><a href="#" style="vertical-align:middle;" id="aDelete' + i + '" onclick=DeleteRow("' + i + '") class="btn btn-sm btn-danger key-value-remove"><i class="fa fa-minus-circle"></i></a></div></div></div></div>';
                html += '<div class="col-sm-1" style="font-size:2.3em; color:#d9534f;vertical-align:bottom;"><div class="control-wrapper rock-drop-down-list"><i id="warningLi' + i + '" style="display:none;' + firstRowWarningClass + '" class="fa fa-exclamation-triangle" title="Clint has a schedule restriction and should be cleared with the volunteer before scheduling!"></i></div><div>';
                // It is for delete row button.
                // End row close.
                html += '</div>';
                // rowLast
                //$('#rowLast').append(html);
                var element = document.getElementById('row' + (i - 1));
                if (element != null) {
                    element.insertAdjacentHTML('afterend', html);
                } else
                    document.getElementById('rowLast').insertAdjacentHTML('afterend', html);
                var length = $('#' + memId + ' > option').length;
                var prevMemEle = document.getElementById('ddMem' + (i - 1));
                if (prevMemEle != null) {
                    var index = prevMemEle.selectedIndex;
                    if (index != null && length > (index + 1)) {
                        $("#" + memId).prop('selectedIndex', (index + 1));
                    }
                }
                /// It is used to show warning indicator as per selected member.
                OnSelectMember(i);
            }
            else {
                var ele = document.getElementById('row' + i);
                ele.remove();
            }
        }
    }

    $(document).ready(function () {
        var e = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_lblNumber');
        // Event will be fire on changing the number of occurence.
        e.addEventListener("DOMSubtreeModified", function () {
            // Method will generate the occurence row like Member, Position and Status.
            OnTotalPositionChange();
        });

        $("#ctl00_main_ctl23_ctl01_ctl06_dtScheduerPick_tbDate").on('change', function () {
            // It will fire on selecting the date, It is used to show the Red exclemenation mark if any selecting member Blackout date is set.
            OnChangeDate();
        });
        // Event will be fire on selecting the member from PersonPicker.
        $('#ctl00_main_ctl23_ctl01_ctl06_rockPersonPicker_btnSelect').on('click', function () {
            // It is used to rest the height of personpicker after selecting the member.
            document.getElementById('popupBody').style = '';
        });
        OnTotalPositionChange(null);
        SetOccurenceRows();
    });

    // Method will execute on opening the Add member/position popup.
    function AddPersonOrPositionInGroup(modelName) {
        // divGrpPerson, divGrpPositions
        switch (modelName) {
            case "Person":
                // Showing the Add member related fields and hiding the position popup.
                document.getElementById('divGrpPerson').style.display = '';
                document.getElementById('divGrpPositions').style.display = 'none';
                document.getElementById('modelHeader').innerText = "Person";
                break;
            case "Position":
                // Showing the Add position related fields and hiding the person fields.
                document.getElementById('divGrpPerson').style.display = 'none';
                document.getElementById('divGrpPositions').style.display = '';
                document.getElementById('positionDesc').value = '';
                document.getElementById('positionTxt').value = '';
                document.getElementById('modelHeader').innerText = "Position";
                break;
        }

        var model = $('#mdAddPersionPosition');
        // SHowing the model window.
        model.modal('show');
        document.getElementById('popupBody').style = '';
        model.show();
    }

    // Method will executing on click submit button of Add member/position popup.
    function OnAddInGroup() {
        // If position is not visible, that means added popup is for Person.
        var isMember = document.getElementById('divGrpPositions').style.display == 'none';
        Add(isMember);
    }

    // Method is used to get query string value.
    // Name parameter is the key.
    // For examle if want to get GroupId query string value then pass name as 'GroupId'.
    function GetParameterByName(name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }

    function Add(isMember) {
        //called when user clicks a check box
        var pathname = window.location.origin;
        var jsondata = '';
        var url = pathname + "/Plugins/com_DTS/ServingScheduler/SchedularOccurenceWebService.aspx/";
        var grpid = GetParameterByName("GroupId", null);
        var aliasIdVal = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdCurrentPersonAliasIdVal').value;
        if (isMember) {
            var personid = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_rockPersonPicker_hfPersonId').value; //$('#ddNonGrpPerson').val();
            var membername = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_rockPersonPicker_hfPersonName').value; //$("#ddNonGrpPerson" + " option:selected").text()        
            if (membername == "") {
                alert('Person is required.');
                return;
            }
            // Preparing the json data to send over the web service.
            jsondata = '{ "grpId": "' + grpid + '" , "personid": "' + personid + '", "name": "' + membername + '" , "CurrentPersonAliasId": "' + aliasIdVal + '" }';
            url += "AddMemberInGroup";
        }
        else {
            var desc = $.trim(document.getElementById('positionDesc').value);
            var posTxt = $.trim(document.getElementById('positionTxt').value);
            // Preparing the json data to send over the web service.
            jsondata = '{ "grpId": "' + grpid + '" , "position": "' + posTxt + '", "desc": "' + desc + '"  }';
            url += "AddPositionInGroup";
            if (posTxt == "") {
                alert('Postion is required.');
                return;
            }
        }
        var model = $('#mdAddPersionPosition');
        model.modal('hide');

        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: jsondata,
            url: url,
            success: function (data) {
                if (data.d.Success == "true") {
                    if (data.d.IsMember == 'true') {
                        PapulateDropdownOnAddmember(data.d);
                        alert('Member added successfully.');
                    }
                    else {
                        alert('Position added successfully.');
                        RefreshPositionDropdown(data.d);
                    }
                }
                else {
                    alert(data.d.Message);
                }
            },
            error: function (msg) {
                alert('Error occured in adding.');
            }
        });
    }

    // After successfully adding the member in Group, Member's dropdown are papulating with new member.
    function PapulateDropdownOnAddmember(data) {
        // This is case: When added member has some blackout dates set.
        if (data.PersonBlackoutDates != "") {
            var eBlackout = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_hdPersonOnBlackoutVal');
            if (eBlackout != null) {
                if (eBlackout.value != "") {
                    eBlackout.value += multiRowSeperatorKey + data.PersonBlackoutDates;
                }
                else {
                    eBlackout.value = data.PersonBlackoutDates;
                }
            }
        }

        var content = document.getElementById('ctl00_main_ctl23_ctl01_ctl06_ddMember1');
        if (data.Personid != "") {

        }
        if (data.MemberId != "") {
            $('#ctl00_main_ctl23_ctl01_ctl06_ddMember1').append($('<option></option>').val(data.MemberId).html(data.Name));
            var eValue = parseInt(document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber').value);
            for (var i = 0; i < eValue; i++) {
                $('#ddMem' + i).append($('<option></option>').val(data.MemberId).html(data.Name));
            }
        }
    }

    // After successfully adding the position in Group, refresh position.
    function RefreshPositionDropdown(data) {
        if (data.PositionId != "") {
            $('#ctl00_main_ctl23_ctl01_ctl06_ddPosition1').append($('<option></option>').val(data.PositionId).html(data.Name));
            var eValue = parseInt(document.getElementById('ctl00_main_ctl23_ctl01_ctl06_numTotalPos_numTotalPos_hfNumber').value);
            for (var i = 0; i < eValue; i++) {
                $('#ddPos' + i).append($('<option></option>').val(data.PositionId).html(data.Name));
            }
        }
    }

</script>

<div class="panel panel-block">
    <div class="panel panel-heading">
        <i class="fa fa-bar-chart-o fa-fw"></i>Serving Occurrence Editor
        <div class="pull-right">
            <div class="btn-group">
                <Rock:HighlightLabel ID="hlGroupName" runat="server" LabelType="Type" Text="Group Name" />
                <Rock:HighlightLabel ID="hlCampusName" runat="server" LabelType="Campus" Text="Campus Name" />
            </div>
            <div class="btn-group">
                <button type="button" class="btn btn-default btn-xs dropdown-toggle" data-toggle="dropdown">
                    Actions
                    <span class="caret"></span>
                </button>
                <ul class="dropdown-menu pull-right" role="menu">
                    <li><a href="/Scheduler/Groups">Goto Group Viewer</a>
                    </li>
                    <li><a href="#" onclick="return AddPersonOrPositionInGroup('Person')">Add Member</a>
                    </li>
                    <li><a href="#" onclick="return AddPersonOrPositionInGroup('Position')">Add Position</a>
                    </li>
                </ul>
            </div>
        </div>
    </div>
    <div class="panel-body">
        <input type="hidden" runat="server" id="hdSchedulerOccurenceVal" />
        <input type="hidden" runat="server" id="hdSchedulerOccurenceIdVal" />
        <input type="hidden" runat="server" id="hdCurrentPersonAliasIdVal" />
        <!-- It will keep blackout dates with person id -->
        <input type="hidden" runat="server" id="hdPersonOnBlackoutVal" />
        <input type="hidden" runat="server" id="hdGroupGuidIdVal" />
        <div class="row">
            <div class="col-sm-6">
                <div class="form-group schedule-picker ">
                    <div class="control-wrapper">
                        <Rock:DateTimePicker ID="dtScheduerPick" runat="server" Required="true" RequiredErrorMessage="Please select a date & time." Label="Occurrence Date & Time" />
                    </div>
                </div>
            </div>
            <div class="col-sm-4">
                <div class="form-group">
                    <Rock:NumberUpDown ID="numTotalPos" runat="server" Required="true" Minimum="0" onchage="OnTotalPositionChange(this);" RequiredErrorMessage="Enter total position" Label="Total Position" />
                </div>
            </div>
        </div>

        <div class="row" id="rowLast">
            <Rock:RockDropDownList ID="ddMember1" runat="server" Style="display: none;"></Rock:RockDropDownList>
            <Rock:RockDropDownList ID="ddPosition1" runat="server" Style="display: none;"></Rock:RockDropDownList>

        </div>
        <div class=" col-sm-12 row">
            &nbsp;<asp:CheckBox ID="chkNotifyVolunteer" runat="server" Text="Notify Volunteers Now?" />
        </div>
        <div class="col-sm-12 row">
            <br />
            <br />
        </div>
        <div class="col-sm-12 row">
            <div class="pull-left">
                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Submit" CssClass="btn btn-primary" OnClientClick="return OnSaveBtnClick();" />
                <Rock:BootstrapButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn  btn-link" CausesValidation="false"></Rock:BootstrapButton>
            </div>
            <div class="pull-right">
                <Rock:BootstrapButton ID="btnDelete" runat="server" Text="Delete Occurence" CssClass="btn btn-danger" CausesValidation="false"></Rock:BootstrapButton>
            </div>
        </div>
    </div>
</div>

<div id="mdAddPersionPosition" class="modal fade" role="dialog">
    <div class="modal-dialog">

        <!-- Modal content-->
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
                <h4 class="modal-title" id="modelHeader">Person</h4>
            </div>
            <div class="modal-body" id="popupBody">
                <div class="form-group rock-drop-down-list " id="divGrpPerson">
                    <div class="control-wrapper">
                        <asp:DropDownList ID="ddNonGrpPerson" Visible="false" runat="server" ClientIDMode="Static"></asp:DropDownList>
                        <Rock:PersonPicker ID="rockPersonPicker" runat="server" Width="450px" Height="350px" Label="Person" />
                    </div>
                </div>
                <div class="form-group rock-drop-down-list " id="divGrpPositions" style="display: none;">
                    <div class="control-wrapper">
                        <Rock:RockTextBox ID="positionTxt" runat="server" ClientIDMode="Static" Label="Position"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="positionDesc" runat="server" ClientIDMode="Static" Label="Description"></Rock:RockTextBox>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-default" onclick="OnAddInGroup()">Submit</button>
                <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
            </div>
        </div>

    </div>
</div>

