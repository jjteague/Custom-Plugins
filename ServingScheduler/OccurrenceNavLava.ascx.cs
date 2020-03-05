using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Security;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Plugins.com_DTS.Misc
{
    [DisplayName("Group Navigation")]
    [Category("DTS > GroupNavigation")]    
    [CodeEditorField("Lava Template", "The lava template to use for the Group Navigation.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "{% include '~~/Plugins/com_DTS/ServingScheduler/Lava/OccurrenceNavLava' %}", "", 0)]
    public partial class OccurrenceNavLava : RockBlock
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                DisplayViewGroup();
        }

        #region Internal Methods

        ////
        //// Group Methods

        /// <summary>
        /// Displays the view group  using a lava template
        /// </summary>
        private void DisplayViewGroup()
        {
            try
            {
                string grpid = Request.QueryString["GroupId"];
                string weekid = Request.QueryString["WeekId"];
                string servingDateTime = Convert.ToDateTime(DateTime.Now).ToString("MM/dd/yyyy");
                string nextVal = "";
                string prevVal = "";
                if (!string.IsNullOrEmpty(grpid) && !string.IsNullOrEmpty(weekid))
                {
                    RockContext rockContext = new RockContext();
                    string sql = "SELECT top 1 * from(SELECT "
                    + "LEAD(ss.Id) OVER(ORDER BY ss.ServingDateTime DESC) PreviousValue, "
                    + "ss.Id, ss.ServingDateTime,"
                    + "LAG(ss.Id) OVER(ORDER BY ss.ServingDateTime DESC) NextValue "
                    + "FROM _com_DTS_ServingScheduler ss where  ss.GroupId = {0} ) as nextPrev where nextPrev.Id = {1} Order By ServingDateTime Desc";
                    sql = string.Format(sql, grpid, weekid);
                    DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null);

                 
                    if (dt.Rows.Count > 0)
                    {
                        
                        servingDateTime = Convert.ToString(dt.Rows[0]["ServingDateTime"]);
                        nextVal = Convert.ToString(dt.Rows[0]["NextValue"]);
                        prevVal = Convert.ToString(dt.Rows[0]["PreviousValue"]);
                        if (string.IsNullOrEmpty(servingDateTime))
                            servingDateTime = "";
                        if (string.IsNullOrEmpty(nextVal))
                            nextVal = "";
                        if (string.IsNullOrEmpty(prevVal))
                            prevVal = "";                        
                    }
                }
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, this.CurrentPerson);               

                Dictionary<string, object> grpInfo = new Dictionary<string, object>();
                grpInfo.Add("ServingDateTime", servingDateTime);
                grpInfo.Add("NextValue", nextVal);
                grpInfo.Add("PreviousValue", prevVal);
                grpInfo.Add("GroupId", grpid);

                mergeFields.Add("item", grpInfo);

                Dictionary<string, object> currentPageProperties = new Dictionary<string, object>();
                currentPageProperties.Add("Id", RockPage.PageId);
                currentPageProperties.Add("Path", Request.Path);
                mergeFields.Add("CurrentPage", currentPageProperties);

                string template = GetAttributeValue("LavaTemplate");

                lContent.Text = template.ResolveMergeFields(mergeFields);
            }
            catch(Exception e) {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }

        }

        #endregion Internal Methods

    }
}