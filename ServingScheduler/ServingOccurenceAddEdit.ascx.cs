/// Author: Amit 
/// Email: toresolveissue@gmail.com
/// Created Date: 20 Feberuary 2018.

using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Plugins.com_DTS.Misc
{

    [DisplayName("Serving Occurence Add/Edit")]
    [Category("DTS > Serving Scheduler")]
    [Description("Add or edit a serving occurrence")]
    [WorkflowTypeField("Serving Scheduler", "Activate the selected workflow for each individual schedule occurrence schedule record created (also fires if an scheduler date changes). The scheduler date is passed as the Entity to the workflow.")]
    [BooleanField("Notify Volunteer Now?", "The Default value of the Notify Volunteers check-box", false)]

    public partial class ServingOccurence : RockBlock
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                Init();
                SetData();
            }
            chkNotifyVolunteer.Checked = GetAttributeValue("NotifyVolunteerNow?").AsBoolean();
            btnSave.Click += ddSubmit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnCancel.Click += BtnCancel_Click;
            //btnSave.Attributes.Add("onclick", "return false;");
        }

        #region Internal Methods

        // Using the key to seperate a row occurence value by Row Seperator key.
        public const string RowSeperatorKey = "|~|~|";

        // Using the key to seperate multi row occurence seperated by below key.
        // For example if there are 2 oocurence rows then seperating like Rows1 Values|~~|~~|Row2 Values.
        public const string MultiRowSeperatorKey = "|~~|~~|";

        private void Init()
        {
            BindPositionDropDown();
            BindMemberDropDown();
            dtScheduerPick.SelectedDateTime = DateTime.Now;
            btnDelete.Visible = false;
            if (this.CurrentPersonAliasId != null)
                hdCurrentPersonAliasIdVal.Value = Convert.ToString(this.CurrentPersonAliasId);
        }

        /// <summary>
        /// Method is used to fill dropdown by available positions.
        /// All Active with visible status positoon will be available in dropdown.
        /// </summary>
        private void BindPositionDropDown()
        {
            string query = "";
            query = "Select ssp.Id,ssp.Value AS Position ";
            query += "From _com_DTS_ServingSchedulerPositions ssp ";
            query += "Where ssp.GroupId = '{{ PageParameter.GroupId }}' ";
            query += "AND ssp.IsVisable = 1 And ssp.Active = 1 ";
            query += "Order By ssp.Value ";
            string errMsg;
            DataTable dt = GetData(out errMsg, query);
            ListItem item;
            item = new ListItem(" ", "");
            ddPosition1.Items.Add(item);
            //ddPosition2.Items.Add(item);
            //ddPosition3.Items.Add(item);
            if (dt != null)
            {
                int count = dt.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    item = new ListItem(Convert.ToString(dt.Rows[i][1]), Convert.ToString(dt.Rows[i][0]));
                    ddPosition1.Items.Add(item);
                    //item = new ListItem(Convert.ToString(dt.Rows[i][1]), Convert.ToString(dt.Rows[i][0]));
                    //ddPosition2.Items.Add(item);
                    //item = new ListItem(Convert.ToString(dt.Rows[i][1]), Convert.ToString(dt.Rows[i][0]));
                    //ddPosition3.Items.Add(item);
                }
                if (count > 0)
                {
                    ddPosition1.SelectedIndex = 1;
                    //ddPosition2.SelectedIndex = count > 1 ? 2 : 1;
                    //ddPosition3.SelectedIndex = count > 2 ? 3 : 1;
                }
            }
        }

        /// <summary>
        /// Find the position text value from positionid and return it.
        /// </summary>
        /// <param name="positionId"></param>
        /// <returns></returns>
        private string GetPositionTextValueFromId(string positionId)
        {
            for (int i = 0; i < ddPosition1.Items.Count; i++)
            {
                if (ddPosition1.Items[i].Value == positionId)
                    return ddPosition1.Items[i].Text;
            }

            return positionId;
        }

        /// <summary>
        /// Method is used to fill dropdown by all members.
        /// Members dropdwon will by filter by parameter GroupId.
        /// </summary>
        private void BindMemberDropDown()
        {
            string query = "";
            query = "select  p.FirstName, p.LastName, gm.Id, p.id from GroupMember gm ";
            query += "Inner Join Person p on p.id = gm.PersonId And gm.GroupId =  '{{ PageParameter.GroupId }}' ";
            query += "Order By p.FirstName, p.LastName ";
            string errMsg;
            DataTable dt = GetData(out errMsg, query);
            ListItem item;
            // Adding a blank item.
            item = new ListItem(" ", "");
            ddMember1.Items.Add(item);
            string personidList = "";
            if (dt != null)
            {
                int count = dt.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    item = new ListItem(Convert.ToString(dt.Rows[i][0]) + " " + Convert.ToString(dt.Rows[i][1]), Convert.ToString(dt.Rows[i][2]));
                    ddMember1.Items.Add(item);
                    personidList += Convert.ToString(dt.Rows[i][3]);
                    if (i != count - 1)
                    {
                        // Adding person list. It needs to get avaiable person blackut dates.
                        personidList += ",";
                    }
                }
                if (count > 0)
                {
                    ddMember1.SelectedIndex = 1;
                }
            }
            // Getting all avaialble group person blackout dates.
            GetPersonBlackoutDates(personidList);
        }

        private void BindNonGroupMemberDropDown(string personidList)
        {

            string query = "";
            // 	select  p.FirstName, p.LastName, gm.GroupId from GroupMember gm
            // Inner Join Person p on p.id = gm.PersonId And gm.GroupId = 57
            query = "select DISTINCT p.FirstName, p.LastName, p.id from GroupMember gm ";
            query += "Inner Join Person p on p.id = gm.PersonId And gm.GroupId <>  '{{ PageParameter.GroupId }}' And p.Id not in (" + personidList + ") ";
            query += "Order By p.FirstName, p.LastName ";
            string errMsg;
            DataTable dt = GetData(out errMsg, query);
            ListItem item;
            if (dt != null)
            {
                int count = dt.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    item = new ListItem(Convert.ToString(dt.Rows[i][0]) + " " + Convert.ToString(dt.Rows[i][1]), Convert.ToString(dt.Rows[i][2]));
                    //ddNonGrpPerson.Items.Add(item);

                }
                if (count > 0)
                {
                    //ddNonGrpPerson.SelectedIndex = 1;
                }
            }

        }

        List<string> servingSchedulerDetailIdList = new List<string>();

        /// <summary>
        /// Setting the UI by Group, Group members, position data.
        /// </summary>
        private void SetData()
        {
            string query = "";
            string errMsg;
            string grpId = Request.QueryString["GroupId"];
            // Setting the Gorup information in UI.
            try
            {
                RockContext rockContext = new RockContext();
                Rock.Model.Group grp = new GroupService(rockContext).Get(Convert.ToInt32(grpId));
                if (grp != null)
                {
                    hdGroupGuidIdVal.Value = grp.Guid.ToString();
                    hlGroupName.Text = grp.Name;
                    if (grp.Campus != null && !string.IsNullOrEmpty(grp.Campus.Name))
                        hlCampusName.Text = grp.Campus.Name;
                    else
                        hlCampusName.Visible = false;
                }
            }
            catch { }


            string wId = Request.QueryString["WeekId"];
            bool copyOccurence = false;
            if (string.IsNullOrEmpty(wId))
            {
                wId = Request.QueryString["CopyWeekId"];
                if (!string.IsNullOrEmpty(wId))
                    copyOccurence = true;
            }
            if (!string.IsNullOrEmpty(wId))
            {
                // Setting the Group week information in UI if week id is not empty.                
                btnDelete.Visible = copyOccurence == false;
                if (copyOccurence)
                {
                    query = "select ServingDateTime, TotalPositions from ";
                    query += "_com_DTS_ServingScheduler ssd WHERE ssd.Id =  '{0}' ";
                    query = string.Format(query, wId);
                }
                else
                {
                    query = "select ServingDateTime, TotalPositions from ";
                    query += "_com_DTS_ServingScheduler ssd WHERE ssd.Id =  '{{ PageParameter.WeekId }}' ";
                }
                // Setting the ServingScheduler date and adding positions.
                DataTable dt = GetData(out errMsg, query);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string dateTime = Convert.ToString(dt.Rows[0][0]);
                    if (!string.IsNullOrEmpty(dateTime))
                    {
                        dtScheduerPick.SelectedDateTime = Convert.ToDateTime(dateTime);
                    }
                    string totalPos = Convert.ToString(dt.Rows[0][1]);
                    if (!string.IsNullOrEmpty(totalPos))
                        numTotalPos.Value = Convert.ToInt32(totalPos);

                }

                if (!copyOccurence)
                {
                    query = "select Id, ServingPositionId, GroupMemberId,ServingDateTime, ssd.Status from ";
                    query += "_com_DTS_ServingSchedulerDetail ssd WHERE ssd.GroupId =  '{{ PageParameter.GroupId }}'  And ssd.ServingWeekId =  '{{ PageParameter.WeekId }}' ";
                }
                else
                {
                    query = "select Id, ServingPositionId, GroupMemberId,ServingDateTime, ssd.Status from ";
                    query += "_com_DTS_ServingSchedulerDetail ssd WHERE ssd.GroupId =  '{0}'  And ssd.ServingWeekId =  '{1}' ";
                    query = string.Format(query, grpId, wId);
                }


                // Getting the ServingScheduler detail data.
                dt = GetData(out errMsg, query);
                int rowCount = dt != null ? dt.Rows.Count : 0;
                string statusDefaultValue = "0";
                if (dt != null && rowCount > 0)
                {
                    // Setting the ServingScheduler meber/status and positions.
                    for (int i = 0; i < rowCount; i++)
                    {
                        string id = Convert.ToString(dt.Rows[i][0]);
                        string posId = Convert.ToString(dt.Rows[i][1]);
                        string memId = Convert.ToString(dt.Rows[i][2]);
                        string statusId = copyOccurence ? statusDefaultValue : Convert.ToString(dt.Rows[i][4]);
                        if (string.IsNullOrEmpty(posId))
                        {
                            posId = "";
                        }
                        if (string.IsNullOrEmpty(memId))
                        {
                            memId = "";
                        }
                        if (string.IsNullOrEmpty(statusId))
                        {
                            statusId = "";
                        }
                        // Filling the hidden variable by value because UI is generate from javascript on pageload.
                        // once UI generated then setting the value from hidden variables.
                        hdSchedulerOccurenceVal.Value += memId + RowSeperatorKey + posId + RowSeperatorKey + statusId;
                        if (!copyOccurence)
                            // Id are unique id of SchedularDetail table, It is used to update  respactive row.
                            hdSchedulerOccurenceIdVal.Value += id;
                        if (i != rowCount - 1)
                        {
                            hdSchedulerOccurenceVal.Value += MultiRowSeperatorKey;
                            if (!copyOccurence)
                                hdSchedulerOccurenceIdVal.Value += RowSeperatorKey;
                        }
                    }
                }
            }
        }

        private void SetPositionMemberId(DataRow dr, RockDropDownList posDropDown, RockDropDownList memDropDown)
        {
            string posId = Convert.ToString(dr[1]);
            string memId = Convert.ToString(dr[2]);
            if (!string.IsNullOrEmpty(posId))
            {
                posDropDown.SelectedValue = posId;
            }
            if (!string.IsNullOrEmpty(memId))
            {
                memDropDown.SelectedValue = memId;
            }

        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="schemaOnly">if set to <c>true</c> [schema only].</param>
        /// <returns></returns>
        private DataTable GetData(out string errorMessage, string sql)
        {
            errorMessage = string.Empty;

            string query = sql;
            if (!string.IsNullOrWhiteSpace(query))
            {
                try
                {
                    var mergeFields = GetDynamicDataMergeFields();

                    // NOTE: there is already a PageParameters merge field from GetDynamicDataMergeFields, but for backwords compatibility, also add each of the PageParameters as plain merge fields
                    foreach (var pageParam in PageParameters())
                    {
                        mergeFields.AddOrReplace(pageParam.Key, pageParam.Value);
                    }

                    query = query.ResolveMergeFields(mergeFields);

                    var parameters = GetParameters();
                    int timeout = GetAttributeValue("Timeout").AsInteger();
                    return DbService.GetDataTable(query, CommandType.Text, null);

                }
                catch (System.Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the dynamic data merge fields.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDynamicDataMergeFields()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, this.CurrentPerson);
            if (CurrentPerson != null)
            {

                mergeFields.Add("Person", CurrentPerson);
            }

            mergeFields.Add("RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber());
            mergeFields.Add("CurrentPage", this.PageCache);

            return mergeFields;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetParameters()
        {
            string[] queryParams = GetAttributeValue("QueryParams").SplitDelimitedValues();
            if (queryParams.Length > 0)
            {
                var parameters = new Dictionary<string, object>();
                foreach (string queryParam in queryParams)
                {
                    string[] paramParts = queryParam.Split('=');
                    if (paramParts.Length == 2)
                    {
                        string queryParamName = paramParts[0];
                        string queryParamValue = paramParts[1];

                        // Remove leading '@' character if was included
                        if (queryParamName.StartsWith("@"))
                        {
                            queryParamName = queryParamName.Substring(1);
                        }

                        // If a page parameter (query or form) value matches, use it's value instead
                        string pageValue = PageParameter(queryParamName);
                        if (!string.IsNullOrWhiteSpace(pageValue))
                        {
                            queryParamValue = pageValue;
                        }
                        else if (queryParamName.ToLower() == "currentpersonid" && CurrentPerson != null)
                        {
                            // If current person id, use the value of the current person id
                            queryParamValue = CurrentPerson.Id.ToString();
                        }

                        parameters.Add(queryParamName, queryParamValue);
                    }
                }

                return parameters;
            }

            return null;
        }

        /// <summary>
        /// It is used to get max auto generated unique id.
        /// </summary>
        /// <param name="tableName">It is the name of sql table.</param>
        /// <returns></returns>
        private Int64 GetMaxSchedularId(string tableName)
        {
            string sql = "select MAX(id) From " + tableName;
            DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null, null);
            if (dt != null)
            {
                while (dt.Rows.Count > 0)
                {
                    return Convert.ToInt64(dt.Rows[0][0]);
                }
            }
            return -1;
        }

        /// <summary>
        /// Method is used to save ServingScheduler and ServingSchedulerDetail data.
        /// </summary>
        /// <param name="gid">It is Groupid</param>
        /// <param name="ServingWeekId">It is the week id, If UI in add mode then it is a unique id of ServingScheduler table. </param>
        /// <param name="currDate">Current date of add/update.</param>
        /// <param name="ServingDateTime">Selected date from selected ServingDateTime dropdown.</param>
        /// <param name="memberid">Selected memberid(GroupMemberId)</param>
        /// <param name="posId">sqlqcted position id</param>
        /// <param name="serveDtlId">Unique id of _com_DTS_ServingSchedulerDetail table. In case of add mode, It will be empty or null.</param>
        /// <param name="statusId">Selected status id</param>
        private void SaveServingSchedulerDetail(string gid, Int64 ServingWeekId, string currDate, string ServingDateTime, string memberid, string posId, string serveDtlId, string statusId)
        {
            memberid = string.IsNullOrEmpty(memberid) ? "NULL" : memberid;
            posId = string.IsNullOrEmpty(posId) ? "NULL" : posId;
            bool isAdd = true;
            string servingDateTime = string.IsNullOrEmpty(ServingDateTime) ? "NULL" : "'" + ServingDateTime + "'";
            if (string.IsNullOrEmpty(serveDtlId))
            {
                Int64 id = GetMaxSchedularId("_com_DTS_ServingSchedulerDetail") + 1;
			string sql = "INSERT INTO  _com_DTS_ServingSchedulerDetail( GroupId, ServingDateTime, ServingWeekId, CreatedDateTime,ModifiedDateTime, Guid, GroupMemberId,ServingPositionId, Status ) Values({0}, {1}, {2}, {3}, '{4}', NewId(), {5}, {6}, {7}) ";
                sql = string.Format(sql, gid, servingDateTime, ServingWeekId, currDate, currDate, memberid, posId, statusId);
                DbService.ExecuteCommand(sql, CommandType.Text, null, null);
            }
            else
            {
                string sql = "UPDATE  _com_DTS_ServingSchedulerDetail SET ServingDateTime = {0}, ModifiedDateTime = '{1}' , GroupMemberId = {2},ServingPositionId =  {3}, Status = {4} Where Id = {5}";
                sql = string.Format(sql, servingDateTime, currDate, memberid, posId, statusId, serveDtlId);
                DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                isAdd = false;
            }
            TriggerWorkflows(ServingDateTime, posId, statusId, memberid, isAdd);
        }

        /// <summary>
        /// Method is used create workflow.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="position"></param>
        /// <param name="operationStatus"></param>
        /// <param name="GroupMemberId"></param>
        /// <param name="isAdd"></param>
        protected void TriggerWorkflows(string ServingDateTime, string position, string operationStatus, string GroupMemberId, bool isAdd)
        {
            WorkflowType workflowType = null;
            RockContext rockContext = new RockContext();
            WorkflowTypeService workflowTypeService = new WorkflowTypeService(rockContext);
            WorkflowService workflowService = new WorkflowService(rockContext);
            Guid? workflowTypeGuid = GetAttributeValue("ServingScheduler").AsGuidOrNull();
            Workflow workflow;
            Person _person = this.CurrentPerson;
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
                            string aliasId = GetPagePersonAliasGuidId(GroupMemberId);
                            workflow.SetAttributeValue("NotifyStatus", chkNotifyVolunteer.Checked ? "true" : "false");
                            workflow.SetAttributeValue("GroupMemberId", GroupMemberId);
                            workflow.SetAttributeValue("OccurrenceDateTime", ServingDateTime); // Needs fixed to show the occurrence DateTime and sends '2018-09-09 08:45:000' with the '' around the date.  
                            if (this.CurrentPerson != null && this.CurrentPerson.Guid != null)
                                workflow.SetAttributeValue("Requestor", this.CurrentPerson.Guid.ToString()); // Does not send anything over to the workflow
                            else if (this.CurrentPersonAlias != null && this.CurrentPersonAlias.AliasPersonGuid != null)
                                workflow.SetAttributeValue("Requestor", this.CurrentPersonAlias.AliasPersonGuid.ToString()); // Does not send anything over to the workflow
                            workflow.SetAttributeValue("RequestStatus", isAdd ? "New" : "Edit");
                            workflow.SetAttributeValue("Group", hdGroupGuidIdVal.Value);
                            workflow.SetAttributeValue("PositionId", position);
                            string positionText = GetPositionTextValueFromId(position);
                            workflow.SetAttributeValue("PositionText", positionText); // Sends the table Id of the position and needs to see the text value of the position
                            workflow.SetAttributeValue("OccurrenceStatus", operationStatus);
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

        /// <summary>
        /// Getting selected group member person alias id.
        /// </summary>
        /// <param name="groupMemberId">Unique id of GroupMember</param>
        /// <returns></returns>
        /*private string GetPagePersonAliasGuidId(string GroupMemberId)*/
        private string GetPagePersonAliasGuidId(string GroupMemberId)
        {
            try
            {
                string query = "";
                query = "select  p.Guid from GroupMember gm ";
                query += "Inner Join Person p on p.id = gm.PersonId And gm.Id =  {0} ";
                query = string.Format(query, GroupMemberId);

                DataTable dt = DbService.GetDataTable(query, CommandType.Text, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string val = Convert.ToString(dt.Rows[0][0]);
                    return val;
                }

            }
            catch (Exception ex) { }


            return this.CurrentPerson.PrimaryAlias.AliasPersonGuid != null ? ((Guid)this.CurrentPerson.PrimaryAlias.AliasPersonGuid).ToString() : Guid.Empty.ToString();
        }

        /// <summary>
        /// Getting all group members blackout dates if any.
        /// It is used to show Red exclamation mark if selected mebmer not available or selected person blackout date set as serving date.
        /// Method will collact all person blackout dates and keep in hidden variable.
        /// If user select a person then javascript method will start showing Red exclamation mark if person blackout date is similar to selected ServingDate.
        /// </summary>
        /// <param name="personidList"></param>
        private void GetPersonBlackoutDates(string personidList)
        {
            try
            {
                if (string.IsNullOrEmpty(personidList))
                {
                    hdPersonOnBlackoutVal.Value = "";
                    return;
                }
                // personidList            
                string startDate = DateTime.Now.ToString("yyyy-MM-dd");
                string sql = "SELECT StartDateTime,EndDateTime, gm.Id   From _com_DTS_ServingSchedulerBlackOut ss INNER JOIN PersonAlias p On p.AliasPersonId = ss.PersonAliasId And p.PersonId in ({0}) And ( (StartDateTime >= '{1}' OR EndDateTime >= '{1}') OR (StartDateTime <= '{1}' And EndDateTime >= '{1}') ) INNER JOIN GroupMember gm on  gm.GroupId = {2} And gm.PersonId = p.PersonId ORDER By p.PersonId ";
                sql = string.Format(sql, personidList, startDate, Request.QueryString["GroupId"]);
                DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null);
                string dates = "";
                string prevPersonId = "";
                int count = dt.Rows.Count - 1;
                for (int i = 0; i <= count; i++)
                {
                    string currPersonId = Convert.ToString(dt.Rows[i][2]);
                    string sDate = Convert.ToString(dt.Rows[i][0]);
                    string endDate = Convert.ToString(dt.Rows[i][1]);
                    if (prevPersonId == "")
                    {
                        dates = currPersonId + ";" + sDate + "::" + endDate;
                    }
                    else if (currPersonId != prevPersonId)
                    {
                        hdPersonOnBlackoutVal.Value += dates + MultiRowSeperatorKey;
                        dates = currPersonId + ";" + sDate + "::" + endDate;
                    }
                    else
                    {
                        // Prev person and current person ids are equal.
                        dates += RowSeperatorKey + sDate + "::" + endDate;
                    }
                    prevPersonId = currPersonId;
                    if (i == count)
                        hdPersonOnBlackoutVal.Value += dates;
                }
            }
            catch { }
        }

        #endregion Internal Methods

        #region Event

        /// <summary>
        /// Method will execute on save button of occurence form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string gid = Request.QueryString["GroupId"];
                string currDate = DateTime.Now.ToString("yyyy'-'MM'-'dd");
                string date = null;
                if (dtScheduerPick.SelectedDateTime != null)
                    date = Convert.ToDateTime(dtScheduerPick.SelectedDateTime).ToString("yyyy'-'MM'-'dd hh:mm:ss.fff");
                string totPos = Convert.ToString(numTotalPos.Value);
                string weekId = Request.QueryString["WeekId"];
                Int64 wId = -1;
                string serveDate = string.IsNullOrEmpty(date) ? "NULL" : "'" + date + "'";
                if (!string.IsNullOrEmpty(weekId))
                {
                    wId = Convert.ToInt64(weekId);
                    string sql = "UPDATE [_com_DTS_ServingScheduler] SET ServingDateTime = {0}, TotalPositions = {1} Where Id = {2} ";
                    sql = string.Format(sql, serveDate, totPos, wId);
                    DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                }
                else
                {
                    Int64 id = GetMaxSchedularId("_com_DTS_ServingScheduler") + 1;
                    wId = id;
                    string sql = "INSERT INTO [_com_DTS_ServingScheduler](GroupId, ServingDateTime, Guid, CreatedDateTime,ModifiedDateTime, TotalPositions,CreatedByPersonAliadId) Values({0},{1}, NewId(), '{2}', '{3}', {4}, {5}) ";
                    sql = string.Format(sql, gid, serveDate, currDate, currDate, totPos, this.CurrentPersonAliasId);
                    DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                }
                // All Scheduler detail ids are seperated by RowSeperatorKey. variable will alaways be empty in add mode.
                string[] idList = hdSchedulerOccurenceIdVal.Value.Split(new string[] { RowSeperatorKey }, StringSplitOptions.RemoveEmptyEntries);
                int listCount = idList.Length;

                // All dynamic generated occurence values are seperated and restore in hidden variable when click on save button using javascript method.
                string[] multiValue = hdSchedulerOccurenceVal.Value.Split(new string[] { MultiRowSeperatorKey }, StringSplitOptions.RemoveEmptyEntries);

                // Loop through all dynamic generated Scheduler detail and saving.
                for (int i = 0; i < multiValue.Length; i++)
                {
                    string[] value = multiValue[i].Split(new string[] { RowSeperatorKey }, StringSplitOptions.None);
                    if (value.Length == 3)
                    {
                        SaveServingSchedulerDetail(gid, wId, currDate, date, value[0], value[1], listCount > i ? idList[i] : "", value[2]);
                    }
                    else
                    {
                        SaveServingSchedulerDetail(gid, wId, currDate, date, "", "", listCount > i ? idList[i] : "", "0");
                    }
                }

                if (idList.Length > 0 && idList.Length > multiValue.Length)
                {
                    for (int i = multiValue.Length; i < idList.Length; i++)
                    {
                        string sql = string.Format("DELETE FROM _com_DTS_ServingSchedulerDetail WHERE ID = {0} ", idList[i]);
                        DbService.ExecuteCommand(sql, CommandType.Text);
                    }
                }

                Response.Redirect("~/Scheduler?GroupId=" + gid+"&WeekId="+wId);
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// Used to delete occurence.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                string weekId = Request.QueryString["WeekId"];
                if (!string.IsNullOrEmpty(weekId))
                {
                    string gid = Request.QueryString["GroupId"];
                    string sql = "DELETE From [_com_DTS_ServingScheduler] WHERE Id = " + weekId;
                    DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                    sql = "DELETE From _com_DTS_ServingSchedulerDetail WHERE ServingWeekId = " + weekId;
                    DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                    Response.Redirect("~/Scheduler?GroupId=" + gid);
                }
            }
            catch { }

        }

        /// <summary>
        /// To  go back the occurence list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            string gid = Request.QueryString["GroupId"];
            Response.Redirect("~/Scheduler?GroupId=" + gid);
        }

        #endregion Event

    }

}