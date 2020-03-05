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
using DotLiquid;

[DisplayName("Serving Requests Volunteer")]
[Category("DTS > ServingRequestsVolunteerView")]
[Description("Block to show Serving Request Volunteer.")]
[WorkflowTypeField("Serving Requests Volunteer Workflow", "Activate the selected workflow for each individual schedule occurrence schedule record created (also fires if an scheduler date changes). The scheduler date is passed as the Entity to the workflow.")]
[CodeEditorField("Lava Template", "The lava template to show serving volunteer request.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~/Themes/Rock/Assets/Lava/ServingRequestVolunteerView.lava' %}", "", 2)]

public partial class ServingRequestsVolunteerView : RockBlock
{
    #region Page Event

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            DisplayServingRequestVolunteer();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        RouteAction();        
    }

    #endregion Page Eventhttp://localhost:49760/Themes/Rock/Assets/Lava/Calendar.lava

    #region Event
    #endregion Event

    #region Generate Html
    #endregion Generate Html

    #region Route Action

    /// <summary>
    /// Route the request to the correct panel
    /// </summary>
    private void RouteAction()
    {        
        var sm = ScriptManager.GetCurrent(Page);
        try {
            if (Request.Form["__EVENTARGUMENT"] != null)
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split('^');

                if (eventArgs.Length == 6)
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int ServingSchedulerId = 0;
                    int.TryParse(parameters, out ServingSchedulerId);

                    int status = 0;
                    int.TryParse(eventArgs[2], out status);

                    int GroupId = 0;
                    int.TryParse(eventArgs[3], out GroupId);

                    int weekId = -1;
                    int.TryParse(eventArgs[4], out weekId);

                    int personId = -1;
                    int.TryParse(eventArgs[5], out personId);
                    // javascript:__doPostBack('ctl00_main_ctl33_ctl02_ctl06_upGrid','EditStatus^'+ ServingSchedulerId +'^'+value +'^'+ groupId +'^'+ sWeekId +'^'+ personId);
                    switch (action)
                    {
                        case "EditStatus":
                            if (status >= 0 && status <= 3)
                            {
                                string sql = string.Format("UPDATE _com_DTS_ServingSchedulerDetail SET Status = '{0}' Where Id = '{1}'  And ServingWeekId = '{2}' ", status, ServingSchedulerId, weekId);
                                DbService.ExecuteCommand(sql, CommandType.Text, null);
                                GenerateWorkflow(ServingSchedulerId.ToString(), GroupId.ToString(), weekId.ToString());
                            }
                            break;
                    }
                }

                DisplayServingRequestVolunteer();
            }
        }
        catch { }        
    }

    #endregion Route Action

    #region Private Methods        

    /// <summary>
    /// Displays the view group  using a lava template
    /// </summary>
    private void DisplayServingRequestVolunteer()
    {
        RockContext rockContext = new RockContext();

        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, this.CurrentPerson);
        // Adding data
        mergeFields.Add("RequestVolunteer", GetLoginPersonServingRequestVolunteerDataList());
        string template = GetAttributeValue("LavaTemplate");
        ltrlRequestVolunteer.Text = template.ResolveMergeFields(mergeFields).ResolveClientIds(upGrid.ClientID);
    }

    #endregion Private Methods    

    #region Data

    private List<RequestVolunteer> GetLoginPersonServingRequestVolunteerDataList()
    {
        int personId = CurrentPerson == null ? -1 : CurrentPerson.Id;
        string sql = string.Format("SELECT ssd.Id,ssd.ServingWeekId as ServingWeekId,ssd.GroupId,ss.ServingDateTime,ssp.Value as Position,g.Name as GroupName, " +
					" REPLACE(REPLACE(REPLACE(REPLACE(ssd.Status, 0, 'Open'), 1, 'Accepted'), 2, 'Declined'), 3, 'Tentative') as Status,p.FirstName + ' ' + p.LastName as PersonName,p.Id as PersonId " +
					" From [_com_DTS_ServingSchedulerDetail] ssd " +
                    " INNER Join[_com_DTS_ServingSchedulerPositions] ssp ON ssp.Id = ssd.ServingPositionId " +
					" INNER JOIN [_com_DTS_ServingScheduler] ss ON ss.Id = ssd.ServingWeekId And CAST(ss.ServingDateTime as date) >= cast(GETDATE()-7 as date) " +
                    " INNER Join[Group] G on G.Id = ssd.GroupId " +
                    " INNER Join[GroupMember] gm on gm.Id = ssd.GroupMemberId And gm.PersonId = '{0}' " +
                    " INNER Join[Person] p on p.Id = gm.PersonId " +
					" ORDER BY ss.ServingDateTime DESC" , personId);

        DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null);
        List<RequestVolunteer> list = new List<RequestVolunteer>();
        RequestVolunteer volunteer;
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            // s.Id, s.ServingWeekId as ServingWeekId, s.GroupId, 
            // REPLACE(REPLACE(REPLACE(REPLACE(s.Status, 0, 'Open'), 1, 'Accepted'), 2, 'Declined'), 3, 'Tentative') as Status, ssp.Value as Position, 
            // p.FirstName + ' ' + p.LastName as PersonName, s.ServingDateTime, g.Name as GroupName, p.Id as PersonId
            volunteer = new RequestVolunteer();
            volunteer.Id = dt.Rows[i].Field<int>(0);
            volunteer.ServingWeekId = dt.Rows[i].Field<int>(1);
            volunteer.PersonId = dt.Rows[i].Field<int>("PersonId");
            volunteer.GroupId = dt.Rows[i].Field<int>("GroupId");
            volunteer.ServingDateTime = dt.Rows[i].Field<DateTime>("ServingDateTime");
            volunteer.GroupName = dt.Rows[i].Field<string>("GroupName");
            volunteer.PersonName = dt.Rows[i].Field<string>("PersonName");
            volunteer.Status = dt.Rows[i].Field<string>("Status");
            volunteer.Position = dt.Rows[i].Field<string>("Position");
            list.Add(volunteer);
        }

        return list;
    }

    #endregion Data

    #region Workflow

    private void GenerateWorkflow(string servingSchedulerDetailId, string grpId, string weekId)
    {
        try
        {            
            if (!string.IsNullOrEmpty(weekId) && !string.IsNullOrEmpty(grpId))
            {
                string sql = "Select s.Id as sId, s.ServingWeekId as sServingWeekId, s.GroupId,  " +
                          " REPLACE(REPLACE(REPLACE(REPLACE(s.Status, 0, 'Open'), 1, 'Accepted'), 2, 'Declined'), 3, 'Tentative') as Status, " +
                          " ssp.Value as sPosition, p.FirstName as pFName, p.LastName as pLName, s.ServingDateTime, grp.Guid as GrpGUID,gm.PersonId " +
                          "From [_com_DTS_ServingSchedulerDetail] s " +
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
        Guid? workflowTypeGuid = GetAttributeValue("ServingRequestsVolunteerWorkflow").AsGuidOrNull();
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

public class RequestVolunteer : Drop
{

    public DateTime ServingDateTime { get; set; }
    public int ServingWeekId { get; set; }
    public int GroupId { get; set; }
    public int PersonId { get; set; }
    public string Status { get; set; }
    public string GroupName { get; set; }
    public string PersonName { get; set; }
    public string Position { get; set; }
    public int Id { get; set; }
}