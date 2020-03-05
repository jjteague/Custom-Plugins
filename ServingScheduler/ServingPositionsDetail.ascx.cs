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


namespace Plugins.com_DTS.Misc
{

    
    [DisplayName("Serving Positions Detail")]
    [Category("DTS > ServingPositionsDetail")]
    [Description("Block to allow positions to be added to the group.")]

    [BooleanField("Show Add", "Show Add button in grid footer?", true, "Grid Actions", order: 1)]
    [BooleanField("Show Excel Export", "Show Export to Excel button in grid footer?", true, "Grid Actions", order: 2)]
    [BooleanField("Show Merge Template", "Show Export to Merge Template button in grid footer?", true, "Grid Actions", order: 3)]

    public partial class ServingPositionsDetail : RockBlock
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                SetFilter();
                BindGrid();
            }
            gdServing.ShowConfirmDeleteDialog = false;
            setPermission();
        }        

        #region Properties

        private const string addNewServing = "AddNewServing";

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
    $('table.js-grid-group-serving a.grid-delete-button').click(function( e ){
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this serving position?', function (result) {        
            if (result)    {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
            // return result;
        });
    });
";
            ScriptManager.RegisterStartupScript(gdServing, gdServing.GetType(), "deleteInstanceScript", deleteScript, true);
            //setPermission()
        }

        private void SetBlockPermission() {
        
}

        private void Actions_AddClick(object sender, EventArgs e)
        {
            hdServingId.Value = addNewServing;
            txtPosition.Text = "";
            txtDesc.Text = "";
            chkActive.Checked = false;
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
            string pos = txtPosition.Text.Trim();
            string desc = txtDesc.Text.Trim();
            if (!string.IsNullOrEmpty(pos) && !string.IsNullOrEmpty(desc))
            {
                string sql = "";
                string active = chkActive.Checked ? "1" : "0";
                string isvisable ="1";
                if (hdServingId.Value == addNewServing) {
                    sql = "INSERT INTO  _com_DTS_ServingSchedulerPositions(Value,Description, Guid, CreatedDateTime,ModifiedDateTime, GroupId, Active, IsVisable) Values('{0}' , '{1}', '{2}','{3}', '{4}', '{5}', {6}, {7}) ";
                    string date = DateTime.Now.ToString("yyyy'-'MM'-'dd");
                    string gid = Request.QueryString["GroupId"];
                    if (string.IsNullOrEmpty(gid))
                    {
                        gid = "";
                    }
                    sql = string.Format(sql, pos, desc, Guid.NewGuid().ToString(), date, date, gid, active, isvisable);
                }
                else
                {
                    sql = "UPDATE _com_DTS_ServingSchedulerPositions SET Value = '{0}' , Description = '{1}', Active = {2} Where Id = {3} ";
                    sql = string.Format(sql, pos, desc, active, hdServingId.Value);
                }
                
                DbService.ExecuteCommand(sql, CommandType.Text, null, null);             
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
            int isvisable = 0;
            //string deleteSql = "DELETE FROM _com_DTS_ServingSchedulerPositions Where Id = " + value.ToString() + " ";
            //DbService.ExecuteCommand(deleteSql, CommandType.Text, null, null);
            string updateVisableSql = "Update _com_DTS_ServingSchedulerPositions SET IsVisable = '0' Where Id = " + value.ToString() + " ";
            DbService.ExecuteCommand(updateVisableSql);
            BindGrid();
        }

        protected void OnEditRowClick(object sender, RowEventArgs e)
        {
            string pos = gdServing.Rows[e.RowIndex].Cells[1].Text;
            string desc = gdServing.Rows[e.RowIndex].Cells[2].Text;
            hdServingId.Value = e.RowKeyId.ToString();
            txtPosition.Text = pos;
            txtDesc.Text = desc;
            chkActive.Checked = gdServing.Rows[e.RowIndex].Cells[3].Text.Contains(" fa-check");
            pnlEditModel.Visible = true;
            mdEdit.Visible = true;
            mdEdit.Show();
            upnlEdit.Update();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue(object sender, GridFilter.DisplayFilterValueArgs e)
        {
            switch (e.Key)
            {
                // Handle each user preference as needed
                // ...

                case "Active":
                    if (string.IsNullOrEmpty(e.Value))
                    {
                        e.Value = "";
                    }
                    else {
                        e.Value = e.Value == "1" ? "Yes" : "No";
                    }
                    break;
                case "Position":
                    if (string.IsNullOrEmpty(e.Value))
                    {
                        e.Value = "";
                    }
                    break;
            }
        }

        bool isFilterApply = false;
        protected void rFilter_ApplyFilterClick(object sender, EventArgs e) {
            isFilterApply = true;            
            rFilter.SaveUserPreference("Position", txtFilterNamePos.Text.Trim());            
            rFilter.SaveUserPreference("Active", ddFilterActive.SelectedValue != "Active/Inactive" ?
              ddFilterActive.SelectedValue : string.Empty);
            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick(object sender, EventArgs e)
        {
            isFilterApply = false;
            rFilter.DeleteUserPreferences();
            BindGrid();
            SetFilter();
        }

        #endregion EventHandler

        #region Internal Methods

        private void setPermission()
        {
            bool canEditBlock = IsUserAuthorized(Authorization.EDIT);
            gdServing.Actions.ShowAdd = canEditBlock;
            gdServing.IsDeleteEnabled = canEditBlock;
            gdServing.RowClickEnabled = canEditBlock;
            gdServing.GridRebind += GdPerson_GridRebind;
            if (canEditBlock)
            {
                gdServing.Actions.AddClick += Actions_AddClick;
            }
            else
            {
                gdServing.RowSelected -= OnEditRowClick;
            }
        }

        private void SetFilter() {
            string pos = rFilter.GetUserPreference("Position");            
            txtFilterNamePos.Text = string.IsNullOrEmpty(pos) ? "" : pos;
            string activeVal = rFilter.GetUserPreference("Active");
            if (string.IsNullOrEmpty(activeVal))
                ddFilterActive.SelectedIndex = 0;
            else
                ddFilterActive.SelectedValue = activeVal;
            //txtFilterNamePos.Text = string.IsNullOrEmpty(pos) ? "" : pos;
        }

        protected void BindGrid(bool isExporting = false)
        {
            string eMsg = "";
            DataSet ds = GetData(out eMsg);
            if (ds.Tables.Count > 0)
                gdServing.DataSource = ds.Tables[0];
            else
                gdServing.DataSource = ds;
            gdServing.DataBind();
            upGrid.Update();
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

            string query = GetAttributeValue("Query");
            query = "Select Id,ssp.Value AS Position, ssp.Description AS Description, ssp.CreatedDateTime AS CreatedDate, ";
            query += "REPLACE(REPLACE(ssp.Active, 0, 'No'), 1, 'Yes') AS Active,'Edit' AS Edit, 'X' AS[Delete] ";
            query += "From _com_DTS_ServingSchedulerPositions ssp ";
            query += "Where GroupId = '{{ PageParameter.GroupId }}' ";
            query += "AND IsVisable = 1 ";

            if (isFilterApply) {
                string value = rFilter.GetUserPreference("Position");      //txtFilterNamePos.Text.Trim();
                if (!string.IsNullOrEmpty(value)) {
                    value = value.Replace("'", "''");
                    query += " And ssp.Value Like  ('%" + value + "%') ";
                }
                string activeVal = rFilter.GetUserPreference("Active");
                if (!string.IsNullOrEmpty(activeVal))
                {
                    value = activeVal; //chkFilterActive.Checked ? "1" : "0";
                    query += " And ssp.Active = '" + value + "' ";
                }
            }

            query += "Order By ssp.Active DESC, ssp.Value";
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

        #endregion Internal Methods

    }
}