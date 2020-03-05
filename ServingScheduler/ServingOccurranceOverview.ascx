<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServingOccurranceOverview.ascx.cs" Inherits="Plugins.com_DTS.Misc.ServingOccurranceOverview" %>
<style type="text/css">
    .LastMenuCSS { position:fixed !important; top:unset;left:auto;}
    .dropdown-menu {overflow:auto;}
</style>
<script type="text/javascript">
    $(document).ready(function () {
        var tbl = document.getElementById('ctl00_main_ctl33_ctl01_ctl06_gdServing');
        if (tbl != null) {
            var tr = tbl.getElementsByTagName("tr");
            if (tr != null && tr.length > 0) {
                var menu = tr[tr.length - 3].getElementsByClassName('dropdown-menu');
                if (menu != null && menu.length > 0) {
                    menu[0].className = 'dropdown-menu LastMenuCSS';
                }

            }
        }
    });
    function OnChange(sender, e) {
        if ($(sender).val() == 'Action') {
            e.preventDefault();
            return false;
        }
        return true;
    }
    $('#ddAction').click(function (e) {
        alert('click');
        e.preventDefault();
    });
    function OnClickAction(e) {
        e.preventDefault();
    }
    function OnDeleteClick(sender,e) {
        // a > li > ul > div > td > tr
        var tr = $(sender).parent().parent().parent().parent().parent();
        var ele = tr.find('a.grid-delete-button');
        ele.click();
        e.preventDefault();
    }
    function SendReminder(e, weekid) {
        e.preventDefault();
        Rock.dialogs.confirm('Do you want to send reminder?', function (result) {
            if (result) {
                document.getElementById('hdWeekId').value = weekid;
                document.getElementById('btnSendReminder').click();
            }           
        });        
    }
</script>
<asp:UpdatePanel runat="server" ID="upGrid" UpdateMode="Conditional">
    <ContentTemplate>

        <div class="row">
            <div class="col-md-12">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h3 class="panel-title pull-left"><b><i class="fa fa-calendar"></i>&nbsp;&nbsp;Serving Schedule</b>&nbsp;-&nbsp;<a href='~/page/113?GroupId={{PageParameter.GroupId}}&ExpandedIds={{PageParameter.ExpandedIds}}' id="anchorGroup" runat="server"></a></h3>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lContent" runat="server"></asp:Literal>
                            <asp:Button ID="btnSendReminder" style="display:none;" runat="server" ClientIDMode="Static" OnClick="OnClickSendReminder" />
                            <input type="hidden" runat="server" id="hdWeekId" />
                            <Rock:Grid ID="gdServing" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No occurrance Found" DataKeyNames="Id" RowItemText="Serving Overview" CssClass="js-grid-group-serving" AllowPaging="true" PageSize="2" EnableSortingAndPagingCallbacks="true">
                                <Columns>
                                    <%--Serving Ovcurance	  Total Positions	  Accepted	  Open	  Declined	  Notify Team--%>
                                    <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" HtmlEncode="false" />                                    
                                    <asp:HyperLinkField DataTextField="ServingDateTime" HeaderText="Serving Occurrance" DataNavigateUrlFields="Id,GroupId,ExpandedIds" DataNavigateUrlFormatString="~/scheduler?GroupId={1}&ExpandedIds={2}&WeekId={0}" />
                                    <%--NavigateUrl='<%# Eval("Id",@"~/scheduler?GroupId={{PageParameter.GroupId }}&ExpandedIds={{ PageParameter.ExpandedIds }}&WeekId={0}") %>' --%>
                                    <Rock:RockBoundField DataField="TotalPositions" HeaderText="Total Positions" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="AcceptedCount" HeaderText="Accepted" HtmlEncode="false"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="OpenCount" HeaderText="Open" HtmlEncode="false"></Rock:RockBoundField>
                                    <Rock:RockBoundField DataField="RejectedCount" HeaderText="Declined" HtmlEncode="false"></Rock:RockBoundField>
                                    <Rock:RockTemplateField ShowHeader="true" HeaderText="Action">
                                        <ItemTemplate>
                                            <%-- <asp:DropDownList ID="ddAction" ClientIDMode="Static" runat="server" CssClass="btn" OnSelectedIndexChanged="gvr_SelectedIndexChanged" onchange="return OnChange(this,event);">
                                                <asp:ListItem Selected="True" Value="Action" class="btn btn-secondary" Text="Action"></asp:ListItem>
                                                <asp:ListItem Text="Edit" class="btn btn-secondary" Value="Edit"></asp:ListItem>
                                                <asp:ListItem Text="Delete" class="btn btn-secondary" Value="Delete"></asp:ListItem>
                                                <asp:ListItem Text="Send Reminder" class="btn btn-secondary" Value="Send Reminder"></asp:ListItem>
                                            </asp:DropDownList>--%>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary">Action</button>
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown">
                                                    <span class="caret"></span>
                                                    <span class="sr-only">Toggle Dropdown</span>
                                                </button>
                                                <ul class="dropdown-menu" role="menu">
                                                    <li><a href="#"></a><a title="Copy" href="<%=RelatedPage%>?GroupId=<%#Eval("GroupId")%>&amp;CopyWeekId=<%#Eval("Id")%>&amp;Action=AddDate"><i class="fa fa-copy"></i>&nbsp;Copy Occurrence</a></li>
                                                    <li><a href="#"></a><a title="Edit" href="<%=RelatedPage%>?GroupId=<%#Eval("GroupId")%>&amp;WeekId=<%#Eval("Id")%>&amp;Action=EditDate"><i class="fa fa-edit"></i>&nbsp;Edit Occurrence</a></li>
                                                    <li><a href="#" onclick="javascript:SendReminder(event,'<%#Eval("Id")%>')"><i class="fa fa-envelope"></i>&nbsp;Send Reminder</a></li>
                                                    <li class="divider"></li>
                                                    <li><a href="#" onclick="javascript:OnDeleteClick(this,event);"><i class="fa fa-times"></i>&nbsp;Delete Occurrence</a></li>
                                                </ul>
                                            </div>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
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
