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

[DisplayName("Serving Scheduler List")]
[Category("DTS > ServingSchedulerListLava")]
[Description("Block to show Serving Scheduler List.")]
[LinkedPage("Serving Scheduler Detail", "Serving Scheduler Detail", true, "~/Scheduler", "", 1, "Redirectonclick")]
[CodeEditorField("Lava Template", "The lava template to show serving scheduler list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~/Plugins/com_DTS/ServingScheduler/LavaServingSchedulerListLava.lava' %}", "", 2)]
public partial class ServingSchedulerListLava : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            DisplayServingSchedulerList();
    }

    /// <summary>
    /// Displays the view group  using a lava template
    /// </summary>
    private void DisplayServingSchedulerList()
    {
        RockContext rockContext = new RockContext();

        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, this.CurrentPerson);
        // Adding data
        mergeFields.Add("ServingSchedulerList", GetLoginPersonServingRequestVolunteerDataList());
        string template = GetAttributeValue("LavaTemplate");
        ltrlServingSchedulerList.Text = template.ResolveMergeFields(mergeFields).ResolveClientIds(upGrid.ClientID);
    }

    #region Data

    private List<Dictionary<string, object>> GetLoginPersonServingRequestVolunteerDataList()
    {
        int personId = CurrentPerson == null ? -1 : CurrentPerson.Id;
        string sql = string.Format("Select s.Id, s.GroupId, s.ServingWeekId as ServingWeekId, REPLACE(REPLACE(REPLACE(REPLACE(s.Status, 0, 'Open'), 1, 'Accepted'), 2, 'Declined'), 3, 'Tentative') as Status, s.ServingDateTime, g.Name as GroupName " +
                    " From [_com_DTS_ServingSchedulerDetail] s " +
                    " INNER Join[Group] G on G.Id = s.GroupId And CAST(s.ServingDateTime as date) >= cast(GETDATE() as date)  " + //  
                    " INNER Join[GroupMember] gm on gm.Id = s.GroupMemberId And gm.PersonId = '{0}' ", personId);

        // Group By s.ServingWeekId, SSD.ServingDateTime, ssp.Value, G.Name, s.Status, p.FirstName, p.LastName

        DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null);
        string redirecturl = "";

        redirecturl = LinkedPageUrl("Redirectonclick");        
        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        string url = "";
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Id", dt.Rows[i].Field<int>(0));
            int grpId = dt.Rows[i].Field<int>("GroupId");
            dict.Add("GroupId", grpId);
            dict.Add("ServingDateTime", dt.Rows[i].Field<DateTime>("ServingDateTime"));
            dict.Add("GroupName", dt.Rows[i].Field<string>("GroupName"));
            dict.Add("Status", dt.Rows[i].Field<string>("Status"));
            int weekId = dt.Rows[i].Field<int>("ServingWeekId");
            dict.Add("ServingWeekId", weekId);
            url = string.Format("{0}?GroupId={1}&WeekId={2}", redirecturl, grpId, weekId);
            dict.Add("Url", url);

            list.Add(dict);
        }

        return list;
    }

    #endregion Data


}