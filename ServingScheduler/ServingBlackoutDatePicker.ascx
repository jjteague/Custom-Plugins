﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServingBlackoutDatePicker.ascx.cs" Inherits="ServingBlackoutDatePicker" %>

<div id="" data-zone-location="Page" class="block-instance">
    <div class="block-content">
        <div id="ctl00_main_ctl33_ctl01_ctl06_pnlDetails" class="js-group-panel">
            <input type="hidden" name="111" id="ctl00_main_ctl33_ctl01_ctl06_hfGroupId" value="147">
            <div class="panel panel-block">
                <div class="panel-heading panel-follow clearfix">
                    <h1 class="panel-title pull-left">
                        <i class="fa fa-calendar"></i>
                        <span class="first-word" id="lblTitle" runat="server">Date Picker</span>
                    </h1>
                    <div class="panel-labels">
                    </div>
                </div>
                <!-- Start Body -->
                <asp:UpdatePanel ID="upDatePicker" runat="server">
                    <ContentTemplate>
                        <div class="panel-body">
                            <div class="form-group date-picker ">
                                <label class="control-label">Start Date:</label>
                                <div class="control-wrapper">
                                    <div class="input-group input-width-md date datetimepicker1" data-date-format="mm-dd-yyyy">
                                        <input name="StartDate" id="DateStartDate" type="text" class="form-control"><span class="input-group-addon" type="date"><i class="fa fa-calendar"></i></span>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group date-picker ">
                                <input type="hidden" runat="server" id="hdDateDayDiff" value="28" />
                                <label class="control-label">End Date:</label>
                                <div class="control-wrapper">
                                    <div class="input-group input-width-md date datetimepicker1" data-date-format="mm-dd-yyyy">
                                        <input name="EndDate" id="DateEndDate" type="text" class="form-control"><span class="input-group-addon" type="date"><i class="fa fa-calendar"></i></span>
                                    </div>
                                </div>
                            </div>
                           
                            <div class="checkbox">
                                <input class="btn btn-primary" type="button" name="Submit" id="btnSubmit" value="Submit" />
                            </div>

                            <script>
                                $(document).ready(function () {
                                    SetValue("");
                                });
                                function SetValue(title) {
                                    if (title != '') {
                                        var lbl = document.getElementById('ctl00_main_ctl24_ctl01_ctl06_lblTitle');
                                        if (lbl != null)
                                            lbl.innerText = title;
                                    }
                                    var startDate;
                                    var endDate;                                    

                                    var url = window.location.href;
                                    var PerUrl = url;

                                    if (url.indexOf("=") == -1) {
                                        var d = new Date();
                                        var date = d.getDate();
                                        var month = d.getMonth() + 1;
                                        var year = d.getFullYear();
                                        if (date < 10) {
                                            date = "0" + date;
                                        }
                                        if (month < 10) {
                                            month = "0" + month;
                                        }
                                        startDate = month + "-" + date + "-" + year;
                                        var numberOfDaysToAdd = $('#hdDateDayDiff').val();
                                        var addDays = parseInt(date) + parseInt(numberOfDaysToAdd);
                                        d.setDate(addDays);
                                        date = d.getDate();
                                        month = d.getMonth() + 1;
                                        year = d.getFullYear();
                                        if (date < 10) {
                                            date = "0" + date;
                                        }
                                        if (month < 10) {
                                            month = "0" + month;
                                        }

                                        endDate = month + "-" + date + "-" + year;
                                    }
                                    else {
                                        url = url.substr(url.indexOf("=") + 1);
                                        startDate = url.substr(0, 10);
                                        url = url.substr(url.indexOf("=") + 1);
                                        endDate = url.substr(0, 10);                                      
                                    }

                                    $("#DateStartDate").val(startDate);
                                    $("#DateEndDate").val(endDate);                                                                        

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

                                    $("#btnSubmit").click(function () {
                                        submitForm();
                                    });

                                    function submitForm() {
                                        startDate = $("#DateStartDate").val();
                                        endDate = $("#DateEndDate").val();
                                        Amount = $("#txtAmount").val();                                        

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

                                        location.href = "" + "?startDate=" + startDate + "&endDate=" + endDate;
                                    }
                                }
                                
                    </script>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <!-- END Body -->
            </div>
        </div>
    </div>
</div>
