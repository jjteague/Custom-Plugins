/// Author: Amit 
/// Email: toresolveissue@gmail.com
/// Created Date: 02 March 2018.

using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Web.UI;

namespace Plugins.com_DTS.Misc
{
    [LinkedPage("Related Page", "", true, "/scheduler")]
    [DisplayName("Serving Occurrance Overview")]
    [Category("DTS > Serving Scheduler")]
    [Description("Block to show ServingOccurranceOverview.")]
    [IntegerField("Rows to Display", "", true, 8, "", 0, "RowstoDisplay")]
    [LinkedPage("Serving Scheduler Detail", "Serving Scheduler Detail", true, "~/Scheduler", "", 1, "Redirectonclick")]
    //[CodeEditorField("Lava Template", "The lava template to use for showing occurrance Overview.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "{% include '~~/Assets/Lava/ServingOccurranceOverviewLava' %}", "", 2)]
    [BooleanField("Export Occurence Overview", "Export Occurence Overview", true, "", 2)]
    [BooleanField("New Occurence Overview", "New Occurence Overview", true, "", 3, "New")]
    [BooleanField("Delete Occurence Overview", "Delete", true, "", 4, "DeleteOccurence")]
    [BooleanField("Show Total Positions", "Show Total Positions", true, "", 5, "showtotalpositions")]
    [WorkflowTypeField("Serving Scheduler Notification", "Activate the selected workflow for each individual schedule occurrence schedule record created (also fires if an scheduler date changes). The scheduler date is passed as the Entity to the workflow.")]

    public partial class ServingOccurranceOverview : RockBlock
    {
        #region Page Event

        public string RelatedPage = "/Scheduler/Edit?GroupId=";

        protected void Page_Load(object sender, EventArgs e)
        {
            gdServing.PageSize = GetAttributeValue("RowstoDisplay").AsInteger();

            if (!Page.IsPostBack)
            {
                BindGrid();
            }
            string RelatedRedirectPage = this.LinkedPageUrl("RelatedPage");
            
            if (!string.IsNullOrEmpty(RelatedRedirectPage))
                this.RelatedPage = RelatedRedirectPage;
            string canDelete = GetAttributeValue("DeleteOccurence"); // AllowedActions            
            string showAddNew = GetAttributeValue("New"); // AllowedActions
            if (canDelete == "false")
            {
                gdServing.Columns[7].Visible = false;
            }
            gdServing.Actions.ShowAdd = GetAttributeValue("New").AsBoolean();
            if (gdServing.Actions.ShowAdd)
                gdServing.Actions.AddClick += Actions_AddClick;
            bool totalPos = GetAttributeValue("showtotalpositions").AsBoolean();
            if (!totalPos)
                gdServing.Columns[2].Visible = false;
            else
                gdServing.Columns[2].Visible = true;
            bool export = GetAttributeValue("ExportOccurenceOverview").AsBoolean();
            gdServing.Actions.ShowExcelExport = export;

            gdServing.ShowConfirmDeleteDialog = false;
            gdServing.RowDataBound += GdServing_RowDataBound;
            gdServing.GridRebind += GdServing_GridRebind;
            gdServing.ShowConfirmDeleteDialog = false;
            hdWeekId.ClientIDMode = ClientIDMode.Static;
            //gdServing.RowCommand += GdServing_RowCommand;
        }

        protected void OnClickSendReminder(object sender, EventArgs e)
        {
            GenerateWorkflow();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string deleteScript = @"
    $('table.js-grid-group-serving a.grid-delete-button').click(function( e ){
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this serving occurrence overview?', function (result) {        
            if (result)    {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
            // return result;
        });
    });
";
            ScriptManager.RegisterStartupScript(gdServing, gdServing.GetType(), "deleteInstanceScript", deleteScript, true);
        }

        protected void gvr_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow gvr = (GridViewRow)(((System.Web.UI.Control)sender).NamingContainer);
            DropDownList action = (DropDownList)gvr.FindControl("ddAction");
            switch (action.SelectedValue)
            {
                case "Edit":
                    //int.Parse(gvr.Cells[0].Text);
                    //weekId = int.Parse(parameters);
                    break;
                case "Delete":
                    //weekId = int.Parse(parameters);
                    break;
                case "Send Reminder":
                    //weekId = int.Parse(parameters);
                    break;
            }
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //RouteAction();
        }

        #endregion Page Event

        #region Grid Events

        protected void Actions_AddClick(object sender, EventArgs e)
        {
            // &ExpandedIds=&Action=AddDate
            string expId = Request.QueryString["ExpandedIds"];
            if (string.IsNullOrEmpty(expId))
            {
                expId = "";
            }
            //Response.Redirect("~/Scheduler/Edit?GroupId=" + Request.QueryString["GroupId"] + "&Action=AddDate&ExpandedIds=" + expId);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("GroupId", Request.QueryString["GroupId"]);            
            dict.Add("Action", "AddDate");
            dict.Add("ExpandedIds", expId);
            NavigateToLinkedPage("RelatedPage", dict);
        }

        private void GdServing_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // reducing the banner size?            
            //gdServing.Rows[e.Row.RowIndex].Cells[0].Controls[0] as LinkButton).
        }

        protected void DeleteField_Click(object sender, RowEventArgs e)
        {
            try
            {
                int value = (int)e.RowKeyValue;
                string weekId = Convert.ToString(value);
                if (!string.IsNullOrEmpty(weekId))
                {
                    string gid = Request.QueryString["GroupId"];
                    string sql = "DELETE From [_com_DTS_ServingScheduler] WHERE Id = " + weekId;
                    DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                    sql = "DELETE From _com_DTS_ServingSchedulerDetail WHERE ServingWeekId = " + weekId;
                    DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                    BindGrid();
                    upGrid.Update();
                }
            }
            catch { }
        }

        protected void OnClickLinkButton(object sender, RowEventArgs e)
        {
            //throw new NotImplementedException();
        }

        protected void OnEditRowClick(object sender, RowEventArgs e)
        {

        }

        protected void GdServing_GridRebind(object sender, GridRebindEventArgs e)
        {
            BindGrid();
        }

        #endregion Grid Events

        #region Private Methods        

        /// <summary>
        /// Displays the view group  using a lava template
        /// </summary>
        private void DisplayViewGroup()
        {
            string grpId = Request.QueryString["GroupId"];
            int _groupId = Convert.ToInt32(grpId);
            if (_groupId > 0)
            {
                RockContext rockContext = new RockContext();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, this.CurrentPerson);

                // add linked pages
                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add("PersonDetailPage", LinkedPageRoute("PersonDetailPage"));
                linkedPages.Add("RosterPage", LinkedPageRoute("RosterPage"));
                linkedPages.Add("AttendancePage", LinkedPageRoute("AttendancePage"));
                linkedPages.Add("CommunicationPage", LinkedPageRoute("CommunicationPage"));
                mergeFields.Add("LinkedPages", linkedPages);

                // Adding data
                mergeFields.Add("WeekList", GetWeekList(grpId));
                mergeFields.Add("GroupName", GetGroupName(grpId));
                mergeFields.Add("AcceptedStatus", GetAcceptedStatus(grpId));
                mergeFields.Add("DateCount", GetDateCount(grpId));
                mergeFields.Add("AcceptedCount", GetAcceptedCount(grpId));
                mergeFields.Add("OpenCount", GetOpenCount(grpId));
                mergeFields.Add("TentativeCount", GetTentativeCount(grpId));
                mergeFields.Add("RejectedCount", GetRejectedCount(grpId));

                // add collection of allowed security actions
                Dictionary<string, object> securityActions = new Dictionary<string, object>();
                string canDelete = GetAttributeValue("DeleteOccurence"); // AllowedActions
                securityActions.Add("ShowDelete", canDelete != null && canDelete == "true");

                string showAddNew = GetAttributeValue("New"); // AllowedActions
                securityActions.Add("AddNew", showAddNew != null && showAddNew == "true");

                mergeFields.Add("AllowedActions", securityActions);

                Dictionary<string, object> currentPageProperties = new Dictionary<string, object>();
                currentPageProperties.Add("Id", RockPage.PageId);
                currentPageProperties.Add("Path", Request.Path);
                mergeFields.Add("CurrentPage", currentPageProperties);

                string template = GetAttributeValue("LavaTemplate");

                //lContent.Text = template.ResolveMergeFields(mergeFields).ResolveClientIds(upnlContent.ClientID);
            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group occurences was available from the querystring.</div>";
            }
        }

        #endregion Private Methods

        #region Data Methods

        private void BindGrid()
        {
            try
            {
                string grpId = Request.QueryString["GroupId"];
                DataTable dt = GetServingOccurranceDataList(grpId);

                gdServing.DataSource = dt;
                gdServing.DataBind();

                DataTable dtGrp = GetGroupName(grpId);
                if (dtGrp.Rows.Count > 0)
                {
                    anchorGroup.InnerText = Convert.ToString(dtGrp.Rows[0][0]);
                }
            }
            catch (Exception e)
            {
                string s = "";
            }
        }

        private void FillServingOccurranceOverviewGrid()
        {
            string grpId = Request.QueryString["GroupId"];
            DataTable dt = GetDateCount(grpId);
            if (dt.Rows.Count > 0)
            {
                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {

                }
            }
        }

        private DataTable GetServingOccurranceDataList(string GrpId)
        {
            string expendedid = Request.QueryString["ExpandedIds"];
            string sql = " WITH ss_detail(COUNTS, ServingWeekId, Status) As(SELECT count(a.Status) TotalCount, a.ServingWeekId, a.Status " +
             "FROM [_com_DTS_ServingSchedulerDetail] a " +
             " where a.GroupId = '" + GrpId + "' " +
             "Group By a.ServingWeekId, a.Status " +
            " Having a.Status in (0, 1, 2) ) " +
            " SELECT ss.ServingDateTime, ISNULL(ss.TotalPositions, 0 ) as TotalPositions,ss.Id,ISNULL(ssdOpenStatus.COUNTS,0) as OpenCount, ISNULL(ssdAcpStatus.COUNTS, 0) as AcceptedCount,  " +
            " ISNULL(ssdRejStatus.COUNTS, 0) as RejectedCount, ss.GroupId, '" + expendedid + "' as ExpandedIds  FROM( " +
            " SELECT * FROM[_com_DTS_ServingScheduler] ss where ss.GroupId = " + GrpId + ") as ss " +
            " LEFT JOIN ss_detail ssdOpenStatus On ssdOpenStatus.ServingWeekId = ss.Id And ssdOpenStatus.Status = 0 " +
            " LEFT JOIN ss_detail ssdAcpStatus On ssdAcpStatus.ServingWeekId = ss.Id And ssdAcpStatus.Status = 1 " +
            " LEFT JOIN ss_detail ssdRejStatus On ssdRejStatus.ServingWeekId = ss.Id And ssdRejStatus.Status = 2 " +
            "Order By ServingDateTime Desc";
            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetWeekList(string grpId)
        {
            string noOfRows = GetAttributeValue("RowstoDisplay"); // AllowedActions
            int numberOfRows = Convert.ToInt32(noOfRows);
            if (numberOfRows <= 0)
            {
                numberOfRows = 8;
            }
            string sql = "SELECT " + " * FROM[_com_DTS_ServingScheduler] WHERE [Id] > '0' and GroupId = '" + grpId + "' Order By ServingDateTime Desc ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetGroupName(string grpId)
        {
            string sql = "SELECT [Name] FROM [Group] WHERE [Id] = '" + grpId + "' ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetAcceptedStatus(string grpId)
        {
            string sql = " SELECT count(*) TotalCount, b.Id, a.ServingWeekId FROM [_com_DTS_ServingSchedulerDetail] a  Inner Join [_com_DTS_ServingScheduler] b on a.ServingWeekId = b.Id ";
            sql += " WHERE a.Status = '0' and b.GroupId = '{{ PageParameter.GroupId }}'  Group By b.Id, a.ServingWeekId ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetDateCount(string grpId)
        {
            string sql = "Select count(Id) as TDates From [_com_DTS_ServingScheduler] ";
            sql += " WHERE GroupId = '" + grpId + "' ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetAcceptedCount(string grpId)
        {
            string sql = " Select count(a.Id) as 'AcceptedTotal', a.ServingWeekId as 'Id' From [_com_DTS_ServingSchedulerDetail] a ";
            sql += " Where a.Status = 1 Group By a.ServingWeekId ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetOpenCount(string grpId)
        {
            string sql = "     Select count(a.Id) as 'OpenTotal', a.ServingWeekId as 'Id' From [_com_DTS_ServingSchedulerDetail] a ";
            sql += " Where a.Status = 0  Group By a.ServingWeekId ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetTentativeCount(string grpId)
        {
            string sql = "     Select count(a.Id) as 'TentativeTotal', a.ServingWeekId as 'Id' From [_com_DTS_ServingSchedulerDetail] a ";
            sql += " Where a.Status = 3  Group By a.ServingWeekId ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        private DataTable GetRejectedCount(string grpId)
        {
            string sql = "     Select count(a.Id) as 'RejectedTotal', a.ServingWeekId as 'Id' From [_com_DTS_ServingSchedulerDetail] a ";
            sql += " Where a.Status = 2  Group By a.ServingWeekId ";

            return DbService.GetDataTable(sql, CommandType.Text, null);
        }

        #endregion Data Methods

        #region Workflow

        private void GenerateWorkflow()
        {
            try
            {
                string grpId = Request.QueryString["GroupId"];
                string weekId = hdWeekId.Value;
                if (!string.IsNullOrEmpty(weekId) && !string.IsNullOrEmpty(grpId))
                {
                    string sql = "Select s.Id as sId, s.ServingWeekId as sServingWeekId, s.GroupId,  " +
                              " REPLACE(REPLACE(REPLACE(REPLACE(s.Status, 0, 'Open'), 1, 'Accepted'), 2, 'Declined'), 3, 'Tentative') as Status, " +
                              " ssp.Value as sPosition, p.FirstName as pFName, p.LastName as pLName, s.ServingDateTime, grp.Guid as GrpGUID,gm.PersonId " +
                              "From[_com_DTS_ServingSchedulerDetail] s " +
                              " INNER JOIN[_com_DTS_ServingScheduler] ss On ss.Id = s.ServingWeekId " +
                              " And s.ServingWeekId = '{0}' and s.GroupId = '{1}' " +
                              "INNER JOIN[Group] grp On grp.Id = '{1}' And grp.Id = s.GroupId " +
                              "Left Join[GroupMember] gm on gm.Id = s.GroupMemberId " +
                              "Left Join[Person] p on p.Id = gm.PersonId " +
                              "Left Join[_com_DTS_ServingSchedulerPositions] ssp ON ssp.Id = s.ServingPositionId ";

                    sql = string.Format(sql, weekId, grpId);
                    DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string occurenceid = Convert.ToString(dt.Rows[i][0]);
                        string grpGuid = Convert.ToString(dt.Rows[i]["GrpGUID"]);
                        string fName = Convert.ToString(dt.Rows[i]["pFName"]) + " " + Convert.ToString(dt.Rows[i]["pLName"]);
                        string status = Convert.ToString(dt.Rows[i]["Status"]);
                        string sPosition = Convert.ToString(dt.Rows[i]["sPosition"]);
                        string ServingDateTime = Convert.ToString(dt.Rows[i]["ServingDateTime"]);
                        string PersonId = Convert.ToString(dt.Rows[i]["PersonId"]);
                        TriggerWorkflows(occurenceid, grpGuid, fName, status, sPosition, ServingDateTime, PersonId);
                    }
                }
            }
            catch { }
        }

        protected void TriggerWorkflows(string occurenceid, string grpGuid, string fName, string status, string sPosition, string ServingDateTime, string personId)
        {
            WorkflowType workflowType = null;
            RockContext rockContext = new RockContext();
            WorkflowTypeService workflowTypeService = new WorkflowTypeService(rockContext);
            WorkflowService workflowService = new WorkflowService(rockContext);
            Guid? workflowTypeGuid = GetAttributeValue("ServingSchedulerNotification").AsGuidOrNull();
            Workflow workflow;
            Person _person = this.CurrentPerson;
            //
            // Process per-GroupMember workflow requests.
            //
            if (workflowTypeGuid.HasValue)
            {
                workflowType = workflowTypeService.Get(workflowTypeGuid.Value);

                if (workflowType != null && workflowType.Id != 0)
                {
                    //
                    // Walk each GroupMember object and fire off a Workflow for each.
                    //

                    try
                    {
                        workflow = Workflow.Activate(workflowType, _person.FullName, rockContext);
                        if (workflow != null)
                        {
                            List<string> workflowErrors;
                            string aliasId = GetPagePersonAliasGuidId(personId);                            
                            workflow.SetAttributeValue("Volunteer", aliasId);
                            workflow.SetAttributeValue("Group", grpGuid);
                            workflow.SetAttributeValue("OccurrenceDateTime", ServingDateTime);
                            workflow.SetAttributeValue("Position", sPosition);
                            workflow.SetAttributeValue("OccurrenceStatus", status);
                            workflowService.Process(workflow, null, out workflowErrors);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionLogService.LogException(ex, this.Context);
                    }

                }
            }
        }

        private string GetPagePersonAliasGuidId(string personid)
        {
            try
            {
                string query = string.Format("select Guid from PersonAlias where PersonId = '{0}' ", personid);
                DataTable dt = DbService.GetDataTable(query, CommandType.Text, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string val = Convert.ToString(dt.Rows[0][0]);
                    return val;
                }

            }
            catch (Exception) { }


            return this.CurrentPerson.PrimaryAlias.AliasPersonGuid != null ? ((Guid)this.CurrentPerson.PrimaryAlias.AliasPersonGuid).ToString() : Guid.Empty.ToString();
        }

        #endregion Workflow

    }
}