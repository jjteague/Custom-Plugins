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

//[LinkedPage("Related Page","",true, "/page/1717")]
[DisplayName("Scheduler Scheduler Occurrence Detail")]
[Category("DTS > Serving Scheduler")]
[Description("Block to show Serving Scheduler Occurrence Detail.")]
//[LinkedPage("Serving Scheduler Detail", "Serving Scheduler Detail", true, "~/Scheduler", "", 1, "Redirectonclick")]
[BooleanField("Show Export Occurrence Detail", "Export Occurence Detail", true, "", 1)]
//[BooleanField("New Occurence Detail", "New Occurence Detail", true, "", 3, "New")]
[BooleanField("Show Delete Occurrence Detail", "Delete", true, "", 2, "DeleteWeekDetail")]
[BooleanField("Show Progress Bar", "Show Percentage", true, "", 3, "showpercentage")]
[WorkflowTypeField("Serving Scheduler Detail Notification", "Activate the selected workflow for each individual schedule occurrence schedule record created (also fires if an scheduler date changes). The scheduler date is passed as the Entity to the workflow.")]

public partial class ServingSchedulerDetail : RockBlock
{
    #region Page Event

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindGrid();
        }
        hdPopupStatusSelectedVal.ClientIDMode = ClientIDMode.Static;
        ddPopupStatus.ClientIDMode = ClientIDMode.Static;
        hdPopupSelectedOccuIdVal.ClientIDMode = ClientIDMode.Static;
        string showAddNew = GetAttributeValue("New"); // AllowedActions       
        gdOccurenceDetailList.Columns[6].Visible = GetAttributeValue("DeleteWeekDetail").AsBoolean();
        bool export = GetAttributeValue("ShowExportOccurrenceDetail").AsBoolean();
        this.BlockUpdated += ServingSchedulerDetail_BlockUpdated;
        if (GetAttributeValue("showpercentage").AsBoolean())
        {            
            divProgressbarWb.Style.Remove("display");
        }
        else
            divProgressbarWb.Style.Add("display", "none");
        //if (VProgressPercent > 0)
        //    divProgressbarWb.Style.Remove("display");
        //else
        //    divProgressbarWb.Style.Add("display", "none");


        string weekid = Request.QueryString["WeekId"];
        gdOccurenceDetailList.Actions.ShowAdd = !string.IsNullOrEmpty(weekid);
        gdOccurenceDetailList.Actions.ShowExcelExport = export;
        gdOccurenceDetailList.Actions.AddClick += Actions_AddClick;

        gdOccurenceDetailList.GridRebind += gdOccurenceDetailList_GridRebind;
        gdOccurenceDetailList.RowSelected += OnEditRowClick;
        gdOccurenceDetailList.RowCommand += gdOccurenceDetailList_OnRowCommand;
        gdOccurenceDetailList.ShowConfirmDeleteDialog = false;
        gdOccurenceDetailList.RowDataBound += GdOccurenceDetailList_RowDataBound;
    }

    private void ServingSchedulerDetail_BlockUpdated(object sender, EventArgs e)
    {
        upGrid.Update();
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
        Rock.dialogs.confirm('Are you sure you want to delete this serving occurrence detail?', function (result) {        
            if (result)    {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }            
        });
    });
";
        ScriptManager.RegisterStartupScript(gdOccurenceDetailList, gdOccurenceDetailList.GetType(), "deleteInstanceScript", deleteScript, true);
        string rowClickEvent = "$(document).on('click', '#ctl00_main_ctl33_ctl02_ctl06_gdOccurenceDetailList td', function(e) {    GridRowClick(this); });";
        ScriptManager.RegisterStartupScript(gdOccurenceDetailList, gdOccurenceDetailList.GetType(), "rowClickEvent", rowClickEvent, true);
    }

    #endregion Page Event

    #region Grid Event

    /// <summary>
    /// It will check all Gridview rows and show the "Send Reminder" button for Open/Tentive status rows.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void GdOccurenceDetailList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        /*
        switch (e.Row.RowType)
        {
            case DataControlRowType.DataRow:
                DataRowView myDataRowView = (DataRowView)e.Row.DataItem;
                string strStatus = Convert.ToString(myDataRowView["Status"]);
                // Tentative
                if (strStatus == "Open" || strStatus == "Tentative")
                {
                    LinkButton btnEmail = (LinkButton)e.Row.FindControl("btnResendReminder");
                    if (btnEmail != null)
                    {
                        btnEmail.Visible = true;
                    }
                }
                break;
        } */
    }

    protected void OnEditRowClick(object sender, RowEventArgs e)
    {
        string expId = Request.QueryString["ExpandedIds"];
        if (string.IsNullOrEmpty(expId))
        {
            expId = "";
        }
        Response.Redirect("~/Scheduler/Edit?GroupId=" + Request.QueryString["GroupId"] + "&WeekId=" + Request.QueryString["WeekId"] + "&Action=EditDate&ExpandedIds=" + expId, true);
    }

    private void Actions_AddClick(object sender, EventArgs e)
    {
        string expId = Request.QueryString["ExpandedIds"];
        if (string.IsNullOrEmpty(expId))
        {
            expId = "";
        }
        //Response.Redirect("~/Scheduler/Edit?GroupId=" + Request.QueryString["GroupId"] + "&WeekId=" + Request.QueryString["WeekId"] + "&Action=AddDate&ExpandedIds=" + expId, true);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("GroupId", Request.QueryString["GroupId"]);
        dict.Add("WeekId", Request.QueryString["WeekId"]);
        dict.Add("Action", "AddDate");
        dict.Add("ExpandedIds", expId);
        NavigateToLinkedPage("RelatedPage",dict); 
    }

    protected void gdOccurenceDetailList_OnRowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "ResendReminder") return;
        //int id = Convert.ToInt32(e.CommandArgument);
        GenerateWorkflow(Convert.ToString(e.CommandArgument));
    }

    /// <summary>
    /// To delete occurence detail.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void DeleteField_Click(object sender, RowEventArgs e)
    {
        try
        {
            int value = (int)e.RowKeyValue;
            string dtlId = Convert.ToString(value);
            if (!string.IsNullOrEmpty(dtlId))
            {
                string sql = string.Format("DELETE FROM [_com_DTS_ServingSchedulerDetail] Where Id = '{0}' ", dtlId);
                DbService.ExecuteCommand(sql, CommandType.Text);
                string weekId = Request.QueryString["WeekId"];
                sql = string.Format("UPDATE _com_DTS_ServingScheduler SET TotalPositions = TotalPositions - 1 where Id = '{0}' And GroupId = '{1}' ", weekId, Request.QueryString["GroupId"]);
                DbService.ExecuteCommand(sql, CommandType.Text);
                //BindGrid();
                //upGrid.Update();
                string expId = Request.QueryString["ExpandedIds"];
                if (string.IsNullOrEmpty(expId))
                {
                    expId = "";
                }
                Response.Redirect("~/scheduler?GroupId=" + Request.QueryString["GroupId"] + "&ExpandedIds=" + expId + "&WeekId=" + weekId);
            }
        }
        catch { }
    }

    protected void gdOccurenceDetailList_GridRebind(object sender, GridRebindEventArgs e)
    {
        BindGrid();
    }

    protected void EditStatus(object sender, EventArgs e)
    {
        try
        {
            int val = Convert.ToInt32(hdPopupStatusSelectedVal.Value);
            string sql = string.Format("UPDATE _com_DTS_ServingSchedulerDetail SET Status = '{0}' Where Id = '{1}'  And ServingWeekId = '{2}' ", hdPopupStatusSelectedVal.Value, hdPopupSelectedOccuIdVal.Value, Request.QueryString["WeekId"]);
            DbService.ExecuteCommand(sql, CommandType.Text, null);

            string weekId = Request.QueryString["WeekId"];
            string expId = Request.QueryString["ExpandedIds"];
            if (string.IsNullOrEmpty(expId))
            {
                expId = "";
            }
            Response.Redirect("~/scheduler?GroupId=" + Request.QueryString["GroupId"] + "&ExpandedIds=" + expId + "&WeekId=" + weekId);
        }
        catch { }
    }

    #endregion Grid Event

    #region UI Binding

    private void BindGrid()
    {
        try
        {
            string grpId = Request.QueryString["GroupId"];
            string weekId = Request.QueryString["WeekId"];

            if (!string.IsNullOrEmpty(grpId) && !string.IsNullOrEmpty(weekId))
            {
                DataTable dt = GetServingSchedulerDetailDataList(grpId, weekId);

                gdOccurenceDetailList.DataSource = dt;
                gdOccurenceDetailList.DataBind();

                string strServing = GetServingDateTime(dt);
                lblServingDateTime.InnerText = strServing;
                // If show percentage is true then only calclulate the percentage and show it.
                if (GetAttributeValue("showpercentage").AsBoolean())
                {
                    CalculatePercentage(dt);
                }
                else
                    divProgressbarWb.Style.Add("display", "none");
            }
            else
            {
                string sql = "Select '' as Id, '' as sServingWeekId, '' as GroupId, NULL as Status, NULL as sPosition, NULL as Individual, NULL as ServingDateTime, 0 as ShowBtn where 1=0";
                DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null);
                gdOccurenceDetailList.DataSource = dt;
                gdOccurenceDetailList.DataBind();
                divProgressbarWb.Style.Add("display", "none");
            }
        }
        catch (Exception e)
        {
            string s = "";
        }
    }

    /// <summary>
    /// Calculating Progress Bar Values.
    /// </summary>
    /// <param name="dt"></param>
    private void CalculatePercentage(DataTable dt)
    {
        int VTotalAccepted = 0;
        double VProgressPercent = 0;
        int rowCount = dt.Rows.Count;
        for (int i = 0; i < rowCount; i++)
        {
            if (Convert.ToString(dt.Rows[i][3]) == "Accepted")
                VTotalAccepted += 1;
        }

        if (rowCount == 0 || VTotalAccepted == 0)
            VProgressPercent = 0;
        else if (rowCount > 0 && VTotalAccepted > 0)
            VProgressPercent = (VTotalAccepted * 100) / rowCount;

        string VProgressColor = "";
        if (VProgressPercent > 29 && VProgressPercent < 75)
            VProgressColor = "#ffbb33";
        else if (VProgressPercent >= 75)
            VProgressColor = "#28a745";
        else
            VProgressColor = "#ff4444";
        string vPrgBarInPercent = Convert.ToString(VProgressPercent) + "%";
        progressbar.Attributes.Add("aria-valuenow", Convert.ToString(VProgressPercent));
        progressbar.Style.Add("background-color", VProgressColor);
        progressbar.Style.Add("width", VProgressPercent == 0 ? "7%" : vPrgBarInPercent);
        progressbar.InnerText = vPrgBarInPercent;
        //if (VProgressPercent > 0)
        //    divProgressbarWb.Style.Remove("display");
        //else
        //    divProgressbarWb.Style.Add("display", "none");
    }

    #endregion UI Binding

    #region Data

    /// <summary>
    ///  Getting serving occurence detail by Groupid and weekid.
    /// </summary>
    /// <param name="GrpId"></param>
    /// <param name="WeekId"></param>
    /// <returns></returns>
    private DataTable GetServingSchedulerDetailDataList(string GrpId, string WeekId)
    {
        string expendedid = Request.QueryString["ExpandedIds"];
        string sql = string.Format("Select s.Id, s.ServingWeekId as sServingWeekId, s.GroupId, REPLACE(REPLACE(REPLACE(REPLACE(s.Status, 0, 'Open'), 1, 'Accepted'), 2, 'Declined'), 3, 'Tentative') as Status, ssp.Value as sPosition, p.FirstName + ' ' + p.LastName as Individual, ss.ServingDateTime,CASE WHEN [STATUS] = 0 OR [STATUS] = 3 Then 1 else 0 END as ShowBtn " +
                    " From _com_DTS_ServingScheduler ss " +
                    "INNER JOIN [_com_DTS_ServingSchedulerDetail] s on ss.Id = s.ServingWeekId  And s.ServingWeekId = '{0}' and s.GroupId = '{1}' " +
                    " Left Join[GroupMember] gm on gm.Id = s.GroupMemberId " +
                    " Left Join[Person] p on p.Id = gm.PersonId " +
                    " Left Join[_com_DTS_ServingSchedulerPositions] ssp ON ssp.Id = s.ServingPositionId ", WeekId, GrpId);

        return DbService.GetDataTable(sql, CommandType.Text, null);
    }

    private string GetServingDateTime(DataTable dt)
    {
        if (dt.Rows.Count > 0)
        {
            return Convert.ToDateTime(dt.Rows[0]["ServingDateTime"]).ToString("MM/dd/yyyy hh:mm tt");
        }
        return "";
    }

    #endregion Data

    #region Workflow

    private void GenerateWorkflow(string servingSchedulerDetailId)
    {
        try
        {
            string grpId = Request.QueryString["GroupId"];
            string weekId = Request.QueryString["WeekId"];
            if (!string.IsNullOrEmpty(weekId) && !string.IsNullOrEmpty(grpId))
            {
                string sql = "Select s.Id as sId, s.ServingWeekId as sServingWeekId, s.GroupId,  " +
                          " REPLACE(REPLACE(REPLACE(REPLACE(s.Status, 0, 'Open'), 1, 'Accepted'), 2, 'Declined'), 3, 'Tentative') as Status, " +
                          " ssp.Value as sPosition, p.FirstName as pFName, p.LastName as pLName, s.ServingDateTime, grp.Guid as GrpGUID,gm.PersonId " +
                          "From[_com_DTS_ServingSchedulerDetail] s " +
                          " INNER JOIN[_com_DTS_ServingScheduler] ss On ss.Id = s.ServingWeekId " +
                          " And s.ServingWeekId = '{0}' and s.GroupId = '{1}' And s.Id = '{2}' " +
                          "INNER JOIN[Group] grp On grp.Id = '{1}' And grp.Id = s.GroupId " +
                          "Left Join[GroupMember] gm on gm.Id = s.GroupMemberId " +
                          "Left Join[Person] p on p.Id = gm.PersonId " +
                          "Left Join[_com_DTS_ServingSchedulerPositions] ssp ON ssp.Id = s.ServingPositionId ";

                sql = string.Format(sql, weekId, grpId, servingSchedulerDetailId);
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

    protected void TriggerWorkflows(string occurenceid, string grpGuid, string fName, string status, string sPosition, string ServingDateTime, string PersonId)
    {
        WorkflowType workflowType = null;
        RockContext rockContext = new RockContext();
        WorkflowTypeService workflowTypeService = new WorkflowTypeService(rockContext);
        WorkflowService workflowService = new WorkflowService(rockContext);
        Guid? workflowTypeGuid = GetAttributeValue("ServingSchedulerDetailNotification").AsGuidOrNull();
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
                        string aliasId = GetPagePersonAliasGuidId(PersonId);
                        workflow.SetAttributeValue("Volunteer", aliasId);
                        workflow.SetAttributeValue("Group", grpGuid);
                        workflow.SetAttributeValue("OccurrenceDateTime", ServingDateTime);
                        workflow.SetAttributeValue("Position", sPosition);
                        workflow.SetAttributeValue("OccurrenceStatus", hdPopupStatusSelectedVal.Value);
                        workflow.SetAttributeValue("NotifyStatus", "Yes");
                        workflow.SetAttributeValue("RequestStatus", "Edit");
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
            string query = string.Format("select Guid from PersonAlias where PersonId = '{0}' ",personid);            
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