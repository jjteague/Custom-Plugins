using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Security;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Plugins.com_DTS.ServingScheduler
{


    [DisplayName("Blackout Dates Detail Internal")]
    [Category("DTS > Serving Scheduler")]
    [Description("Allows Admins to set Blackout Dates.")]

    [BooleanField("Show Add", "Show Add button in grid footer?", true, "Grid Actions", order: 1)]
    [BooleanField("Show Excel Export", "Show Export to Excel button in grid footer?", true, "Grid Actions", order: 2)]
    [BooleanField("Show Merge Template", "Show Export to Merge Template button in grid footer?", true, "Grid Actions", order: 3)]
    [WorkflowTypeField("Individual Workflow", "Activate the selected workflow for each individual Blackoutdate record created (also fires if an Blackoutdate changes from Inactive to Pending or Active). The Blackoutdate is passed as the Entity to the workflow.", category: "Grid Actions", order: 4)]

    public partial class BlackoutDates : RockBlock
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                
                BindGrid();
            }
            gdBlackoutDates.ShowConfirmDeleteDialog = false;
            setPermission();
        }

        #region Properties

        private const string addNewBlackoutDate = "AddNewBlackoutDate";
        //private string FormStatus;

        #endregion Properties

        #region EventHandler

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            AddConfigurationUpdateTrigger(upnlEdit);
            //this.BlockUpdated += GroupMemberList_BlockUpdated;        
            // a.grid-delete-button    
            string deleteScript = @"
    $('table.js-grid-group-blackoutdates a.grid-delete-button').click(function( e ){
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this Blackout date?', function (result) {        
            if (result)    {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
            // return result;
        });
    });
";
            ScriptManager.RegisterStartupScript(gdBlackoutDates, gdBlackoutDates.GetType(), "deleteInstanceScript", deleteScript, true);
            //setPermission()
        }

        private void SetBlockPermission()
        {

        }

        private void Actions_AddClick(object sender, EventArgs e)
        {
            hdBlackoutDateId.Value = addNewBlackoutDate;

            txtDesc.Text = "";
            dtStartDate.SelectedDate = DateTime.Now;
            dtEndDate.SelectedDate = DateTime.Now;
            pnlEditModel.Visible = true;
            mdEdit.Visible = true;
            mdEdit.Show();
            upnlEdit.Update();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click(object sender, EventArgs e)
        {            
            string startDate = ((DateTime)dtStartDate.SelectedDate).ToString("yyyy-MM-dd");
            string endDate = ((DateTime)dtEndDate.SelectedDate).ToString("yyyy-MM-dd");
            string desc = txtDesc.Text.Trim();
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                string sql = "";
                string isvisable = "1";
                bool isEdit = false;
                if (hdBlackoutDateId.Value == addNewBlackoutDate)
                {
                    sql = "INSERT INTO  _com_DTS_ServingSchedulerBlackOut(StartDateTime,EndDateTime,Notes, PersonAliasId, Guid, CreatedDateTime,ModifiedDateTime, IsVisable) Values('{0}' , '{1}', '{2}',{3}, '{4}', '{5}', {6}, {7}) ";
                    string date = DateTime.Now.ToString("yyyy'-'MM'-'dd");
                    string gid = Request.QueryString["GroupId"];
                    if (string.IsNullOrEmpty(gid))
                    {
                        gid = "";
                    }
                    string aid = GetPagePersonAliasId();
                    string pAliasId = string.IsNullOrEmpty(aid) ? "NULL" : aid;
                    sql = string.Format(sql, startDate, endDate, desc, pAliasId, Guid.NewGuid().ToString(), date, date, isvisable);
                    

                }
                else
                {
                    sql = "UPDATE _com_DTS_ServingSchedulerBlackOut SET StartDateTime = '{0}' , Notes = '{1}', EndDateTime = '{2}' Where Id = {3} ";
                    sql = string.Format(sql, startDate, desc, endDate, hdBlackoutDateId.Value);
                    isEdit = true;
                }

                DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                TriggerWorkflows(startDate, endDate, desc, isEdit ? "Edit" : "New");
                pnlEditModel.Visible = false;
                mdEdit.Visible = false;
                mdEdit.Hide();
                upnlEdit.Update();
                BindGrid();
            }
        }

        protected void GdPerson_GridRebind(object sender, GridRebindEventArgs e)
        {
            BindGrid();
        }

        protected void DeleteField_Click(object sender, RowEventArgs e)
        {
            int value = (int)e.RowKeyValue;            
            string deleteEntrySql = "DELETE FROM _com_DTS_ServingSchedulerBlackOut Where Id = " + value.ToString() + " ";
            DbService.ExecuteCommand(deleteEntrySql);
            try
            {
                string start = gdBlackoutDates.Rows[e.RowIndex].Cells[2].Text;
                string end = gdBlackoutDates.Rows[e.RowIndex].Cells[3].Text;
                string desc = gdBlackoutDates.Rows[e.RowIndex].Cells[4].Text;
                TriggerWorkflows(start, end, desc, "Delete");
            }
            catch { }
            BindGrid();
        }

        protected void OnEditRowClick(object sender, RowEventArgs e)
        {
            string start = gdBlackoutDates.Rows[e.RowIndex].Cells[2].Text;
            string end = gdBlackoutDates.Rows[e.RowIndex].Cells[3].Text;
            string desc = gdBlackoutDates.Rows[e.RowIndex].Cells[4].Text;
            hdBlackoutDateId.Value = e.RowKeyId.ToString();
            dtStartDate.SelectedDate = Convert.ToDateTime(start);
            dtEndDate.SelectedDate = Convert.ToDateTime(end);
            txtDesc.Text = desc;
            pnlEditModel.Visible = true;
            mdEdit.Visible = true;
            mdEdit.Show();
            upnlEdit.Update();
        }        

        #endregion EventHandler

        #region Internal Methods

        private void setPermission()
        {
            bool canEditBlock = IsUserAuthorized(Authorization.EDIT);
            gdBlackoutDates.Actions.ShowAdd = canEditBlock;
            gdBlackoutDates.IsDeleteEnabled = canEditBlock;
            gdBlackoutDates.RowClickEnabled = canEditBlock;
            gdBlackoutDates.GridRebind += GdPerson_GridRebind;
            if (canEditBlock)
            {
                gdBlackoutDates.Actions.AddClick += Actions_AddClick;
            }
            else
            {
                gdBlackoutDates.RowSelected -= OnEditRowClick;
            }


            gdBlackoutDates.Actions.ShowAdd = GetAttributeValue("ShowAdd").AsBoolean();            
            gdBlackoutDates.Actions.ShowMergeTemplate = GetAttributeValue("ShowMergeTemplate").AsBoolean();
            gdBlackoutDates.Actions.ShowExcelExport = GetAttributeValue("ShowExcelExport").AsBoolean();
        }

        protected void BindGrid(bool isExporting = false)
        {
            string eMsg = "";
            DataSet ds = GetData(out eMsg);
            if (ds.Tables.Count > 0)
                gdBlackoutDates.DataSource = ds.Tables[0];
            else
                gdBlackoutDates.DataSource = ds;
            gdBlackoutDates.DataBind();
            upGrid.Update();
        }

        private string GetPagePersonAliasId()
        {
            string value = hdPagePersonAliasId.Value.Trim();
            if (!string.IsNullOrEmpty(value) && value != "-1")
            {
                return value;
            }
            string aliasId = "-1";
            try
            {
                string query = "select AliasPersonId from PersonAlias where PersonId = {{ PageParameter.PersonId }} ";
                var mergeFields = GetDynamicDataMergeFields();

                // NOTE: there is already a PageParameters merge field from GetDynamicDataMergeFields, but for backwords compatibility, also add each of the PageParameters as plain merge fields
                foreach (var pageParam in PageParameters())
                {
                    mergeFields.AddOrReplace(pageParam.Key, pageParam.Value);
                }

                query = query.ResolveMergeFields(mergeFields);
                DataTable dt = DbService.GetDataTable(query, CommandType.Text, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string val = Convert.ToString(dt.Rows[0][0]);
                    if (!string.IsNullOrEmpty(val))
                    {
                        hdPagePersonAliasId.Value = val;
                        return val;
                    }
                }

            }
            catch (Exception) { }


            return aliasId;
        }

        private string GetPagePersonAliasGuidId()
        {                        
            try
            {
                string query = "select AliasPersoGuid from PersonAlias where PersonId = {{ PageParameter.PersonId }} ";
                var mergeFields = GetDynamicDataMergeFields();

                // NOTE: there is already a PageParameters merge field from GetDynamicDataMergeFields, but for backwords compatibility, also add each of the PageParameters as plain merge fields
                foreach (var pageParam in PageParameters())
                {
                    mergeFields.AddOrReplace(pageParam.Key, pageParam.Value);
                }

                query = query.ResolveMergeFields(mergeFields);
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

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="schemaOnly">if set to <c>true</c> [schema only].</param>
        /// <returns></returns>
        private DataSet GetData(out string errorMessage, bool schemaOnly = false)
        {
            errorMessage = string.Empty;

            string query = "";
            //string aliasId = this.CurrentPersonAliasId != null ? this.CurrentPersonAliasId.ToString() : "-0"; 
            string aliasId = this.CurrentPersonAlias.AliasPersonId == null ? "-0" : this.CurrentPersonAlias.AliasPersonId.ToString();
            query = "Select ssp.Id, concat(p.FirstName, ' ' , p.Lastname) as Name, CONVERT(varchar,ssp.StartDateTime, 101) as StartDateTime, CONVERT(varchar,ssp.EndDateTime, 101) as EndDateTime, ssp.Notes, ";
            query += "'Edit' AS Edit, 'X' AS[Delete] ";
            query += "From _com_DTS_ServingSchedulerBlackOut ssp ";
            query += "INNER JOIN PersonAlias pAlias  on ssp.PersonAliasId = pAlias.AliasPersonId ";
            query += "INNER JOIN Person p  on pAlias.PersonId = p.Id ";
            query += "Where pAlias.PersonId = " + GetPagePersonAliasId();
            query += "Order By ssp.StartDateTime DESC";

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

                    if (schemaOnly)
                    {
                        try
                        {
                            // GetDataSetSchema won't work in some cases, for example, if the SQL references a TEMP table.  So, fall back to use the regular GetDataSet if there is an exception or the schema does not return any tables
                            var dataSet = DbService.GetDataSetSchema(query, GetAttributeValue("StoredProcedure").AsBoolean(false) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout);
                            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
                            {
                                return dataSet;
                            }
                            else
                            {
                                return DbService.GetDataSet(query, GetAttributeValue("StoredProcedure").AsBoolean(false) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout);
                            }
                        }
                        catch
                        {
                            return DbService.GetDataSet(query, GetAttributeValue("StoredProcedure").AsBoolean(false) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout);
                        }
                    }
                    else
                    {
                        return DbService.GetDataSet(query, GetAttributeValue("StoredProcedure").AsBoolean(false) ? CommandType.StoredProcedure : CommandType.Text, parameters, timeout);
                    }
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
                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
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

        protected void TriggerWorkflows(string startDate, string enddate, string desc, string operationStatus)
        {
            WorkflowType workflowType = null;
            RockContext rockContext = new RockContext();
            WorkflowTypeService workflowTypeService = new WorkflowTypeService(rockContext);
            WorkflowService workflowService = new WorkflowService(rockContext);
            Guid? workflowTypeGuid = GetAttributeValue("IndividualWorkflow").AsGuidOrNull();
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
                                string aliasId = GetPagePersonAliasGuidId();
                                workflow.SetAttributeValue("Person", aliasId);                               
                                workflow.SetAttributeValue("StartDate", startDate); 
                                workflow.SetAttributeValue("EndDate", enddate);  
                                workflow.SetAttributeValue("Notes", desc);
                                workflow.SetAttributeValue("FormStatus", operationStatus); //can you add this?  need FormStatus = Edit if the form was edited and FormStatus = New if its a new request
                                workflowService.Process(workflow, null, out workflowErrors);
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionLogService.LogException(ex, this.Context);
                        }
                    
                }
            }

            //
            // Activate an optional workflow for the entire submission.
            //
            /*workflowTypeGuid = GetAttributeValue("SubmissionWorkflow").AsGuidOrNull();
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

                            if (workflow.Attributes.ContainsKey(GetAttributeValue("SubmissionAttribute")))
                            {
                                string guids = membership.Select(gm => gm.Guid.ToString()).ToList().AsDelimited(",");
                                workflow.SetAttributeValue(GetAttributeValue("SubmissionAttribute"), guids);
                            }
                            workflowService.Process(workflow, _person, out workflowErrors);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionLogService.LogException(ex, this.Context);
                    }
                }
            }*/
        }

        #endregion Internal Methods
    }

}
