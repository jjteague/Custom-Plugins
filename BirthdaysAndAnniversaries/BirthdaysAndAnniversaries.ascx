<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BirthdaysAndAnniversaries.ascx.cs" Inherits="BirthdaysAndAnniversaries" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class=" col-xs-3">
            <div class="panel panel-block">
                <div class="panel-body">
                    <div class="row">
                        <div class="col-xs-12" style="margin: 10px 0px 0px 0px">
                            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                            <Rock:GroupPicker ID="gpGroups" runat="server" Label="Select Group(s)" AllowMultiSelect="true" />

                        </div>
                    </div>

                </div>
            </div>

            <%--DropDowns --%>
            <div class="panel panel-block">
                <div class="panel-body">
                    <div class="row">
                        <div class="col-xs-12" style="margin: 0px 0px 0px 0px">
                            <asp:Label Text="Birthday Month Range " runat="server" Font-Bold="true" />

                        </div>

                        <div class="col-xs-12" style="margin: 10px 0px 10px 0px">
                            <Rock:ButtonDropDownList ID="dateRangeFrom" runat="server" Title="From">
                                <asp:ListItem Text="All" Value="0" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="January" Value="1"></asp:ListItem>
                                <asp:ListItem Text="February" Value="2"></asp:ListItem>
                                <asp:ListItem Text="March" Value="3"></asp:ListItem>
                                <asp:ListItem Text="April" Value="4"></asp:ListItem>
                                <asp:ListItem Text="May" Value="5"></asp:ListItem>
                                <asp:ListItem Text="June" Value="6"></asp:ListItem>
                                <asp:ListItem Text="July" Value="7"></asp:ListItem>
                                <asp:ListItem Text="August" Value="8"></asp:ListItem>
                                <asp:ListItem Text="September" Value="9"></asp:ListItem>
                                <asp:ListItem Text="October" Value="10"></asp:ListItem>
                                <asp:ListItem Text="November" Value="11"></asp:ListItem>
                                <asp:ListItem Text="December" Value="12"></asp:ListItem>

                            </Rock:ButtonDropDownList>
                        </div>

                        <div class="col-xs-12" style="margin: 0px 0px 10px 0px">
                            <Rock:ButtonDropDownList ID="dateRangeTo" runat="server" Title="To">
                                <asp:ListItem Text="All" Value="0" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="January" Value="1"></asp:ListItem>
                                <asp:ListItem Text="February" Value="2"></asp:ListItem>
                                <asp:ListItem Text="March" Value="3"></asp:ListItem>
                                <asp:ListItem Text="April" Value="4"></asp:ListItem>
                                <asp:ListItem Text="May" Value="5"></asp:ListItem>
                                <asp:ListItem Text="June" Value="6"></asp:ListItem>
                                <asp:ListItem Text="July" Value="7"></asp:ListItem>
                                <asp:ListItem Text="August" Value="8"></asp:ListItem>
                                <asp:ListItem Text="September" Value="9"></asp:ListItem>
                                <asp:ListItem Text="October" Value="10"></asp:ListItem>
                                <asp:ListItem Text="November" Value="11"></asp:ListItem>
                                <asp:ListItem Text="December" Value="12"></asp:ListItem>

                            </Rock:ButtonDropDownList>
                        </div>
                    </div>
                    <Rock:BootstrapButton ID="btn_clear" runat="server" OnClick="btn_clear_birthday" Text="Clear" CssClass="btn btn-primary pull-right" ToolTip="Click here to clear all birthday and anniversary months" />
                </div>
            </div>

            <div class="panel panel-block">
                <div class="panel-body">
                    <div class="row">
                        <div class="col-xs-12" style="margin: 0px 0px 0px 0px">
                            <asp:Label Text="Anniversary Month Range " runat="server" Font-Bold="true" />
                        </div>

                        <div class="col-xs-12" style="margin: 10px 0px 10px 0px">
                            <Rock:ButtonDropDownList ID="annDateRangeFrom" runat="server" Title="From">
                                <asp:ListItem Text="All" Value="0" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="January" Value="1"></asp:ListItem>
                                <asp:ListItem Text="February" Value="2"></asp:ListItem>
                                <asp:ListItem Text="March" Value="3"></asp:ListItem>
                                <asp:ListItem Text="April" Value="4"></asp:ListItem>
                                <asp:ListItem Text="May" Value="5"></asp:ListItem>
                                <asp:ListItem Text="June" Value="6"></asp:ListItem>
                                <asp:ListItem Text="July" Value="7"></asp:ListItem>
                                <asp:ListItem Text="August" Value="8"></asp:ListItem>
                                <asp:ListItem Text="September" Value="9"></asp:ListItem>
                                <asp:ListItem Text="October" Value="10"></asp:ListItem>
                                <asp:ListItem Text="November" Value="11"></asp:ListItem>
                                <asp:ListItem Text="December" Value="12"></asp:ListItem>

                            </Rock:ButtonDropDownList>

                        </div>

                        <div class="col-xs-12" style="margin: 0px 0px 10px 0px">
                            <Rock:ButtonDropDownList ID="annDateRangeTo" runat="server" Title="To">
                                <asp:ListItem Text="All" Value="0" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="January" Value="1"></asp:ListItem>
                                <asp:ListItem Text="February" Value="2"></asp:ListItem>
                                <asp:ListItem Text="March" Value="3"></asp:ListItem>
                                <asp:ListItem Text="April" Value="4"></asp:ListItem>
                                <asp:ListItem Text="May" Value="5"></asp:ListItem>
                                <asp:ListItem Text="June" Value="6"></asp:ListItem>
                                <asp:ListItem Text="July" Value="7"></asp:ListItem>
                                <asp:ListItem Text="August" Value="8"></asp:ListItem>
                                <asp:ListItem Text="September" Value="9"></asp:ListItem>
                                <asp:ListItem Text="October" Value="10"></asp:ListItem>
                                <asp:ListItem Text="November" Value="11"></asp:ListItem>
                                <asp:ListItem Text="December" Value="12"></asp:ListItem>

                            </Rock:ButtonDropDownList>
                        </div>
                    </div>
                    <Rock:BootstrapButton ID="btn_clear2" runat="server" OnClick="btn_clear_anniv" Text="Clear" CssClass="btn btn-primary pull-right" ToolTip="Click here to clear all birthday and anniversary months" />
                </div>
            </div>
			<div class="col-xs-12 text-muted">V1.1</div>
        </div>

        <div class="col-xs-9">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">Group Member Birthdays & Anniversaries</h1>
                    <Rock:BootstrapButton ID="btn_submit" runat="server" OnClick="btn_submit_Click" Text="Refresh" CssClass="btn btn-primary pull-right" ToolTip="Click here to refresh group member grid on the right" />
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <%-- <Rock:GridFilter ID="gmFilters" runat="server" OnApplyFilterClick="gmFilters_ApplyFilterClick">
                            <Rock:DateRangePicker ID="gmBirthdayDateRange" runat="server" Label="Birthday Date Range" />
                             <Rock:DateRangePicker ID="gmAnniversaryDateRange" runat="server" Label="Anniversary Date Range" />
                        </Rock:GridFilter>--%>
                        <Rock:Grid ID="groupMemberGrid" runat="server" AllowSorting="true" AutoPostBack="false" OnSorting="groupMemberGrid_Sorting">
                            <Columns>
                                <Rock:RockBoundField DataField="Id" Visible="false" />
                                <Rock:RockBoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName" />
                                <Rock:RockBoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                                <Rock:RockBoundField DataField="BirthMonth" HeaderText="Birth Month" SortExpression="BirthMonth" />
                                <Rock:RockBoundField DataField="BirthDay" HeaderText="Birth Day" SortExpression="BirthDay" />
                                <Rock:RockBoundField DataField="BirthYear" HeaderText="Birth Year" SortExpression="BirthYear" />
                                <Rock:DateField DataField="Birthdate" HeaderText="Birth Date" SortExpression="Birthdate" />
                                <Rock:RockBoundField DataField="AnniversaryMonth" HeaderText="Anniversary Month" SortExpression="AnniversaryMonth" />
                                <Rock:RockBoundField DataField="AnniversaryDay" HeaderText="Anniversary Day" SortExpression="AnniversaryDay" />
                                <Rock:RockBoundField DataField="AnniversaryYear" HeaderText="Anniversary Year" SortExpression="AnniversaryYear" />
                                <Rock:DateField DataField="Anniversary" HeaderText="Anniversary Date" SortExpression="Anniversary" />
                                <Rock:RockBoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                                <Rock:RockBoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

        </div>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btn_submit" EventName="Click" />
    </Triggers>
</asp:UpdatePanel>
