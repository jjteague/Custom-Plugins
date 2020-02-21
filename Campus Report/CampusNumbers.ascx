<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusNumbers.ascx.cs" Inherits="RockWeb.Plugins.com_DTS.CampusReport.CampusNumbers" %>
<style>
    .table>thead>tr>th, .table>tbody>tr>th, .table>tfoot>tr>th, .table>thead>tr>td, .table>tbody>tr>td, .table>tfoot>tr>td {
        border: 0;
    }
    .location-header {
        text-align: center;
        font-size: 2em;
    }
    .info-header {
        text-align:center;
        font-size: 2em;
        background-color: #A3CAF3 !important;
    }
    .day-header {
        font-size: 1.2em;
        font-weight: bold;
        background-color: #FFEEA2 !important;
    }
    .font-italic {
        font-style: italic;
    }

    @media print
    {
		@page { margin: 0; }
		body { margin: 0; }
        .table > tbody > tr > td {
            font-size: 1em;
        }
        .table > tbody > tr {
            font-size: 1.1em;
        }
        .info-header {
            text-align:center;
            font-size: 3em;
            background-color: #A3CAF3 !important;
        }
        .day-header {
            font-size: 2em;
            font-weight: bold;
            background-color: #FFEEA2 !important;
        }

        .navbar .navbar-fixed-top .rock-top-header {
            display: none !important;
        }
        .navbar-default .navbar-static-side {
            display: none !important;
        }
         .navbar-default .navbar-static-side > li {
            display: none !important;
        }
         .nav .nav-stacked .navbar-side {
             display: none !important;  
         }
         .fa fa-book {
             display: none !important;
         }
        .panel-body {
            border: 0;
        }
        .page-title {
            display: none !important;
        }
        #content-wrapper {
            margin: 0 !important;
        }
        .panel .panel-body {
            border: 0;
        }
        .panel {
            box-shadow: none;
            border: 0;
        }
        .main-footer {
            display: none !important;
        }
		body
		{
		  padding: -10cm 0mm 0mm 0mm;
		}
		.book-date {
    page-break-after: always;
}

    }

</style>
<div class="row">
    <div class="col-xs-12 hidden-print">

        <div class="panel panel-block">
            <div class="panel-body">
                <div class="row">
                        
                    <div class="col-xs-4" style="margin:0px 0px 10px 0px">
                            <Rock:ModalAlert ID="maCampusScheduleWarning" runat="server" />
                            <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Select Campus" Help="Select a campus or All to report on" />
                            <Rock:RockDropDownList ID="ddlSundayDates" runat="server" Label="Sunday Dates" Help="Select a Sunday to show metrics" />
                        </div>
                    </div>
                </div>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-xs-12">
        <div class="panel panel-block">
            <div class="panel-heading hidden-print">
                <h1 class="panel-title">Campus Numbers Report</h1>
				
                    <span class="pull-right" style="padding-right:15px;">
					    <a href="#" class="btn btn-primary hidden-print" style="margin-right:15px" onClick="window.print();"><i class="fa fa-print"></i> Print Statement</a>
                        <Rock:BootstrapButton ID="btn_submit" runat="server" OnClick="btn_submit_Click" Text="Refresh" CssClass="btn btn-primary pull-right" ToolTip="Click here to adjust the date range" />



                    </span>
            </div>
            <div class="panel-body">
        
                    
                        <asp:Panel runat="server" ID="mainDiv" />

            </div>
        </div>

    </div>

</div>