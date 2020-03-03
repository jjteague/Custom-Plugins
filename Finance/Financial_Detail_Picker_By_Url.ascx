<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Financial_Detail_Picker_By_Url.ascx.cs" Inherits="Financial_Detail_Picker_By_Url" %>
<div id="" data-zone-location="Page" class="block-instance">
    <div class="block-content">
        <div id="ctl00_main_ctl33_ctl01_ctl06_pnlDetails" class="js-group-panel">
            <input type="hidden" name="111" id="ctl00_main_ctl33_ctl01_ctl06_hfGroupId" value="147">
            <div class="panel panel-block">
                <div class="panel-heading panel-follow clearfix" data-toggle="collapse" href="#collapseExample">
                    <h1 class="panel-title pull-left">
                        <i class="fa fa-calendar"></i>
                        <span class="first-word" id="lblTitle" runat="server">Financial Date Picker</span>
                    </h1>
                    <div class="panel-labels">Close/Open <i class="fa fa-chevron-down"></i>
                    </div>
                </div>
                <!-- Start Body -->
				
                <asp:UpdatePanel ID="upDatePicker" runat="server">
                    <ContentTemplate>
					<div class="collapse in" id="collapseExample">
                        <div class="panel-body">
							<div class="col-sm-3">
								<div class="form-group date-picker">
									<label class="control-label">Start Date:</label>
									<div class="control-wrapper">
										<div class="input-group input-width-md date datetimepicker1" data-date-format="mm-dd-yyyy">
											<input name="StartDate" id="DateStartDate" runat="server" type="text" class="form-control"><span class="input-group-addon" type="date"><i class="fa fa-calendar"></i></span>
										</div>
									</div>
								</div>
								<div class="form-group date-picker">
									<input type="hidden" runat="server" id="hdDateDayDiff" value="7" />
									<label class="control-label">End Date:</label>
									<div class="control-wrapper">
										<div class="input-group input-width-md date datetimepicker1" data-date-format="mm-dd-yyyy">
											<input name="EndDate" id="DateEndDate" runat="server" type="text" class="form-control"><span class="input-group-addon" type="date"><i class="fa fa-calendar"></i></span>
										</div>
									</div>
								</div>
							</div>
							<div class="col-sm-3">
								<h5 id="h3OptionChkBox" runat="server">Deductable:</h5>
								<div class="form-group rock-text-box " id="chkStatusGroup" runat="server">
									<%--<input type="Checkbox" name="TaxDeductable" id="TaxDeductable" runat="server" />
									<label class="control-label" for="ctl00_main_ctl23_ctl01_ctl06_tbInput4">Tax Deductable</label>                                --%>
									<Rock:RockRadioButtonList name="ddlTaxDeductable" ID="ddlTaxDeductable" runat="server">
										<asp:ListItem Text="Tax Deductable" Value="1" Selected="True" />
										<asp:ListItem Text="Non Tax Deductable" Value="0" />
										<asp:ListItem Text="Both" Value="2"  />
									</Rock:RockRadioButtonList>                                
								</div>
							</div>
							<div class="col-sm-3">
								<div class="form-group date-picker ">
									<Rock:AccountPicker ID="apAccount" runat="server" Label="Account" AllowMultiSelect="true" />
								</div>
							</div>
							<div class="col-sm-3">
								<div class="form-group date-picker ">
									<Rock:DefinedValuesPicker CausesValidation="false" ID="defValuePicker" RepeatDirection="Horizontal" runat="server" Label="Currency" AutoPostBack="false" DefinedTypeId="10" DataTextField="Value" DataValueField="Id"></Rock:DefinedValuesPicker>
								</div>
							</div>	
							<div class="col-sm-12">
								<div>
									<input class="btn btn-primary" type="button" name="Submit" id="btnSubmitFilter" value="Submit" />                                
								</div>
							</div>
                            <script>
                                $(document).ready(function () {
                                    var ele = document.getElementById('apAccount');
                                    if (ele != null) {
                                        ele.className = 'picker picker-select rollover-container';
                                    }
                                    SetUrlValue();
                                });                                
                               
                                function SetUrlValue() {                                   
                                    var startDate;
                                    var endDate;
                                    var Amount;
                                    var Combine;
                                    var AccountIds;

                                    var url = window.location.href;
                                    var PerUrl = url;

                                    $("#DateStartDate").keyup(function (e) {
                                        if (e.keyCode == 13) {
                                            e.preventDefault();
                                            submitForm();
                                        }
                                    });
                                    $("#DateEndDate").keyup(function (e) {
                                        if (e.keyCode == 13) {
                                            e.preventDefault();
                                            submitForm();
                                        }
                                    });


                                    $(".datetimepicker1").datepicker({
                                        autoclose: true
                                    });

                                    $("#btnSubmitFilter").click(function () {
                                        submitForm();
                                    });

                                    function GetTaxableValue() {
                                        var open = '1';
                                        var ele = $('#ddlTaxDeductable_0');
                                        if (ele != undefined && ele[0].checked == true) {
                                            return ele.val();
                                        }
                                        ele = $('#ddlTaxDeductable_1');
                                        if (ele != undefined && ele[0].checked == true) {
                                            return ele.val();
                                        }
                                        ele = $('#ddlTaxDeductable_2');
                                        if (ele != undefined && ele[0].checked == true) {
                                            return ele.val();
                                        }
                                        return open;
                                    }

                                    function GetSelectedCurrencyValue() {
                                        //(defValuePicker_0
                                        var ctrl = undefined;
                                        var i = 0;
                                        var hasCurrency = false;
                                        var isContinue = false;
                                        var value = '';
                                        do {
                                            ctrl = $('#defValuePicker_' + i);
                                            if (ctrl != undefined && ctrl.length > 0 && ctrl[0].checked) {
                                                if (value != '')
                                                    value += ',';
                                                value += ctrl[0].value;
                                            }

                                            i = i + 1;
                                            if (hasCurrency == false && ctrl.length > 0)
                                                hasCurrency = true;

                                        } while (ctrl != undefined && ctrl.length > 0)

                                        if (hasCurrency == true && value == '')
                                            value = '-1';

                                        return value;
                                    }

                                    function submitForm() {
                                        
                                        startDate = $("#DateStartDate").val();
                                        endDate = $("#DateEndDate").val(); // ctl00$main$ctl15$ctl01$ctl06$ddlTaxDeductable
                                        var open = '&IsTax=' + GetTaxableValue();
                                        // ddlTaxDeductable
                                        var val = GetTaxableValue();
                                        var accList = $('#apAccount_hfItemId').val();

                                        var d = new Date();
                                        var date = d.getDate();
                                        var year = d.getFullYear();
                                        var month = d.getMonth() + 1;
                                        if (date < 10) {
                                            date = "0" + date;
                                        }
                                        if (month < 10) {
                                            month = "0" + month;
                                        }

                                        if (startDate == "") {
                                            startDate = month + "-01" + "-" + year;
                                        }
                                        if (endDate == "") {
                                            endDate = month + "-" + date + "-" + year;
                                        }
                                        var accounList = '';
                                        if (accList != '' && accList != '0') {
                                            accounList = '&AccountList=' + accList;
                                        }
                                        var cType = GetSelectedCurrencyValue();
                                        var currType = '';
                                        if (cType != '' && cType != '0') {
                                            currType = '&CurrencyType=' + cType;
                                        }
                                        location.href = "" + "?startDate=" + startDate + "&endDate=" + endDate + open + accounList + currType + "&IsPB=false";
                                    }
                                }
                            </script>
                        </div>
					</div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <!-- END Body -->
            </div>
        </div>
    </div>
</div>