/// Author: Amit 
/// Email: toresolveissue@gmail.com
/// Created Date: 14 May 2018.
/// Altered/Updated By : Ethan Widen
/// Altered Date: 26 Nov 2018

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
using System.Linq;
using System.Data.Entity;
using Rock.Web.Cache;

[DisplayName("Move Checkin Attendance")]
[Category("com_DTS > Move Check-in Attendance")]
[Description("Move Children and Volunteers between rooms")]
[SchedulesField("Pick Schedules", "Pick the schedules you would like to be able to filter by", false)]
[CampusField("Pick Campus", "Pick the campus context", false,"","")]
[GroupTypesField("Children Group Types", "Select ALL Group Types that are for children", false)]
[GroupTypesField("Volunteer Group Types", "Select ALL Group Types that are for volunteers. Leave empty if none are needed.", false)]

public partial class MoveParticipantsLocation : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        participantsList.ClientIDMode = ClientIDMode.Static;
        hdSelectedGroupId.ClientIDMode = ClientIDMode.Static;
        hdSelectedPerson.ClientIDMode = ClientIDMode.Static;
        hdSelectedMoveToGroupVal.ClientIDMode = ClientIDMode.Static;
        bootStrapBtnClose.ClientIDMode = ClientIDMode.Static;
        btnMoveVolunteer.ClientIDMode = ClientIDMode.Static;
        btnMoveKids.ClientIDMode = ClientIDMode.Static;
        lblGroupName.ClientIDMode = ClientIDMode.Static;
        btnOnLoc.ClientIDMode = ClientIDMode.Static;
        btnOffLoc.ClientIDMode = ClientIDMode.Static;
        hdRemoveAttendee.ClientIDMode = ClientIDMode.Static;
        btnDeleteAttendee.ClientIDMode = ClientIDMode.Static;
        //gdGroupList.RowSelected += OnEditRowClick;
        gdGroupList.GridRebind += gdGroupList_GridRebind;
        gdGroupList.RowClickEnabled = true;
        gdGroupList.Actions.ShowAdd = false;
        gdGroupList.Actions.ShowExcelExport = false;
        gdGroupList.ShowConfirmDeleteDialog = false;
        BindEvent();

        //grpPicker.IncludedGroupTypeIds = new List<int>() { 19, 20, 34, 36, 41 };
        if (!IsPostBack)
        {
            schedulePicker_Init();
            //ChildrenGroupTypes_Init();
            BindGrid();
            divRightPanel.Visible = false;
            if (Session["DropDownSelection"] != null)
            {
                //the dropdown selection is saved in the session since we refresh the page for updates in numbers regularly
                //this code grabs that session variable and sets the selected index of the schedule dropdown to the appropriate, previous value
                ScheduleDropdown.SelectedIndex = ScheduleDropdown.Items.IndexOf(ScheduleDropdown.Items.FindByValue(Convert.ToString(Session["DropDownSelection"])));
                //Rebind the grid accordng to the dropdown selected value since the selection might change the grid
                //it might be possible to move this down out of the if and only have one grid bind
                BindGrid();
            }

        }
    }

    private void BindEvent()
    {
        //btnOnLoc.Click += BtnOnLoc_Click;
        //btnOffLoc.Click += BtnOffLoc_Click;
    }

    protected void BtnOffLoc_Click(object sender, EventArgs e)
    {
        //use location service to set to inactive / closed room
        RockContext rockContext = new RockContext();
        LocationService locationService = new LocationService(rockContext);
        Location locationToOff = locationService.Get(Convert.ToInt32(hdSelectedGroupId.Value));
        locationToOff.IsActive = false;
        rockContext.SaveChanges();
        RefreshToggleButton(false);
    }

    protected void BtnOnLoc_Click(object sender, EventArgs e)
    {
        //grab the location and set it to active
        RockContext rockContext = new RockContext();
        LocationService locationService = new LocationService(rockContext);
        Location locationToOff = locationService.Get(Convert.ToInt32(hdSelectedGroupId.Value));
        locationToOff.IsActive = true;
        rockContext.SaveChanges();
        RefreshToggleButton(true);
    }

    protected void BtnMoveGroup_Click(object sender, EventArgs e)
    {
        MoveToAnotherLocation(false);
    }

    protected void BtnMoveVolunteer_Click(object sender, EventArgs e)
    {
        MoveToAnotherLocation(true);
    }

    protected void OnUpdateCapacity(object sender, EventArgs e)
    {
        //update capacity of selected group - assuming we are talking about the soft room threshold - Ethan
        RockContext rockContext = new RockContext();
        LocationService locationService = new LocationService(rockContext);
        Location locationToUpdate = locationService.Get(Convert.ToInt32(hdSelectedGroupId.Value));
        locationToUpdate.SoftRoomThreshold = numCapacity.Value;
        rockContext.SaveChanges();
        BindGrid();
    }

    protected void OnClickRemoveAttendee(object sender, EventArgs e)
    {
        DeleteRemoveAttendee(true);
    }

    protected void OnClickDeleteAttendee(object sender, EventArgs e)
    {
        DeleteRemoveAttendee(false);
    }

    protected void ScheduleDropdown_SelectedIndexChanged(object sender, EventArgs e)
    {
        Session["DropDownSelection"] = ScheduleDropdown.SelectedValue;
        BindGrid();
    }

    #region 

    /// <summary>
    /// Method is used to fill dropdown by all members.
    /// Members dropdown will by filter by parameter GroupId.
    /// </summary>
    private void BindLocationDropDown(int locationId, List<int> childGroupTypeIds, List<int> volunteerGroupTypeIds, int scheduleId)
    {
        //clear the previous items
        ddGroupSelect.Items.Clear();
        divRightPanel.Visible = true;

        //grab the locations we want to bind
        RockContext rockContext = new RockContext();

       if (GetAttributeValue( "PickCampus" ).IsNotNullOrWhitespace())
        {
            CampusService campusService = new CampusService(rockContext);
            Campus campusContext = campusService.Get( Guid.Parse(GetAttributeValue( "PickCampus" )) );

            List<Location> locations = new List<Location>();
            GetLocationAndDescendents( locations, campusContext.Location );

            //locations = locations.Where( l => l.Id != locationId ).Where(l => l.LocationTypeValueId == 183).Where( l => l.GroupLocations.Where( gl => childGroupTypeIds.Contains( gl.Group.GroupTypeId ) || volunteerGroupTypeIds.Contains( gl.Group.GroupTypeId ) ).Count() > 0 ).ToList();
			locations = locations.Where( l => l.Id != locationId ).Where(l => l.LocationTypeValueId == 183).ToList();

            foreach (Location l in locations)
            {
                ddGroupSelect.Items.Add( new ListItem( l.Name, l.Id.ToString() ) );
            }
        }



        //IQueryable<IGrouping<string, GroupLocation>> groupLocations = rockContext.GroupLocations.Where(gl => gl.LocationId != locationId).Where(gl => gl.Schedules.Where( s => s.Id == scheduleId ).Count() > 0 ).GroupBy( gl => gl.Location.Name );
        ////loop through each location, grabbing the name and id
        //foreach (IGrouping<string, GroupLocation> groupLocation in groupLocations)
        //{
        //    foreach(GroupLocation gl in groupLocation)
        //    {

        //        if ((childGroupTypeIds.Contains( gl.Group.GroupType.Id ) || volunteerGroupTypeIds.Contains( gl.Group.GroupTypeId )))
        //        {
        //            ddGroupSelect.Items.Add( new ListItem( gl.Location.Name, gl.Location.Id.ToString() ) );
        //            break;
        //        }
        //    }

        //}
    }

    protected void GetLocationAndDescendents( List<Location> list, Location location )
    {
        list.Add( location );
        foreach (Location childLocation in location.ChildLocations)
        {
            GetLocationAndDescendents( list, childLocation );
        }
    }

    protected void schedulePicker_Init()
    {
        List<String> scheduleGuids = new List<String>();
        //grab the schedules and split them into a list of strings
        if (GetAttributeValue("PickSchedules").IsNotNullOrWhiteSpace())
        {
           scheduleGuids = GetAttributeValue("PickSchedules").Split(',').ToList();
            ScheduleDropdown.Warning = "";
        } else
        {
            ScheduleDropdown.Warning = "Set schedules in block settings....";
        }
        //spin up the schedule service, grab the actual schedule objects and assign that to the dropdown datasource
        var service = new ScheduleService(new RockContext());
        List<Schedule> schedules = service.GetListByGuids(scheduleGuids.Select(Guid.Parse).ToList());
        ScheduleDropdown.DataSource = schedules;
        ScheduleDropdown.DataValueField = "Id";
        ScheduleDropdown.DataTextField = "Name";
        ScheduleDropdown.DataBind();
    }

    protected List<int> GetChildrenGroupTypes_Init()
    {
        //this method can possibly be deprecated since there are other better ways to produce or search through lists of ids using EF
        string id = "";
        if (GetAttributeValue("ChildrenGroupTypes") != "")
        {
            List<String> childrenGroupTypesGuids = GetAttributeValue("ChildrenGroupTypes").Split(',').ToList();
            var service = new GroupTypeService(new RockContext());
            var childGroupTypes = service.GetListByGuids(childrenGroupTypesGuids.Select(Guid.Parse).ToList());
            var childGroupTypeIds = childGroupTypes.Select( x => x.Id ).ToList();

            return childGroupTypeIds;
        } else
        {
            return new List<int>();
        }
        
    }

    protected List<int> GetVolunteerGroupTypes_Init()
    {
        //this method can possibly be deprecated since there are other better ways to produce or search through lists of ids using EF
        string id = "";
        if (GetAttributeValue("VolunteerGroupTypes") != "")
        {
            List<String> volunteerGroupTypesGuids = GetAttributeValue("VolunteerGroupTypes").Split(',').ToList();
            var service = new GroupTypeService(new RockContext());
            var volunteerGroupTypes = service.GetListByGuids(volunteerGroupTypesGuids.Select(Guid.Parse).ToList());
            var volunteerGroupTypeIds = volunteerGroupTypes.Select( x => x.Id ).ToList();

            return volunteerGroupTypeIds;

        } else
        {
            return new List<int>();
        }
    }

    #endregion Init

    #region Grid Event

    protected void gdGroupList_GridRebind(object sender, GridRebindEventArgs e)
    {
        BindGrid();
    }

    protected void OnEditRowClick(object sender, RowEventArgs e)
    {
        try
        {
            string name = gdGroupList.Rows[e.RowIndex].Cells[0].Text;
            string capacityStr = gdGroupList.Rows[e.RowIndex].Cells[1].Text;
            int capacity = -1;
            if (capacityStr.IsNotNullOrWhitespace() && capacityStr != "0" && capacityStr != "&nbsp;")
            {
                capacity = Convert.ToInt32(capacityStr);
            }
            RefreshUI(e.RowKeyId, name, capacity);
        }
        catch (Exception ex)
        {
            string s = "";
        }
    }

    protected void BtnCloseGroup_Click(object sender, EventArgs e)
    {
        try
        {
            //not sure if this is used? I dont see it called right now - Ethan
            int currentGrpId = Convert.ToInt32(hdSelectedGroupId.Value);
            string sql = string.Format("UPDATE [Group] SET IsActive = 0 Where Id = {0} ", currentGrpId);
            DbService.ExecuteCommand(sql, CommandType.Text);
            BindGrid();
            divRightPanel.Visible = false;
        }
        catch (Exception ex)
        {
            string s = "";
        }
    }

    #endregion Grid Event

    #region Refresh UI

    private void RefreshUI(int locationId, string locName, int capacity)
    {
        int scheduleId = ScheduleDropdown.SelectedValue.AsInteger();
        var childGroupTypeIds = GetChildrenGroupTypes_Init();
        var volunteerGroupTypeIds = GetVolunteerGroupTypes_Init();// "41";
        BindLocationDropDown(locationId, childGroupTypeIds, volunteerGroupTypeIds, scheduleId);
        RockContext rockContext = new RockContext();
        hdSelectedGroupId.Value = Convert.ToString(locationId);
        Group grp = new GroupService(rockContext).Get(locationId);

        lblGroupName.InnerText = locName;
        if (capacity == -1)
        {
            numCapacity.Visible = false;
        }
        else
        {
            numCapacity.Visible = true;
            numCapacity.Value = capacity;
        }

        //grab all the attendenance info we need to refresh the UI
        List<Attendance> attendances = rockContext.Attendances
                .Where(a => a.Occurrence.ScheduleId == scheduleId)
                .Where(a => childGroupTypeIds.Contains(a.Occurrence.Group.GroupType.Id) || volunteerGroupTypeIds.Contains(a.Occurrence.Group.GroupTypeId))
                .Where(a => a.Occurrence.LocationId == locationId)
                .Where(a => a.DidAttend == true)
                .Where(a => a.StartDateTime > RockDateTime.Today)
                .Where(a => a.EndDateTime > RockDateTime.Now || a.EndDateTime == null)
                .OrderBy(a => a.PersonAlias.Person.LastName)
                .ToList();

        string html = "";
        int count = 0;
        int volunteerCount = 0;
        string volunteerHtml = "";
        string name;
        bool closeVolunteerDivTag = true;
        bool closeKidsDivTag = true;
        bool showVolunteer = true;
        bool showKids = true;
        bool hasKids = false;
        bool hasVolunteer = false;
        LocationService locationService = new LocationService( rockContext );
        Location location = locationService.Get( locationId );
        bool isActiveLocation = location.IsActive;
        
        foreach (Attendance a in attendances)
        {
            name = a.PersonAlias.Person.FullName;
            string id = Convert.ToString(a.Id);
            string gTypeId = Convert.ToString(a.Occurrence.Group.GroupTypeId);

            if (volunteerGroupTypeIds.Contains(a.Occurrence.Group.GroupTypeId))
            {
                hasVolunteer = true;
                if (volunteerCount == 0)
                {
                    volunteerHtml += "<div class='col-sm-6'>";
                }
                if (showVolunteer)
                {
                    volunteerHtml += "<div style='font-weight:bold;padding-right:0px !important;'>Volunteers</div>";
                    showVolunteer = false;
                }
                volunteerHtml += "";
                volunteerHtml += "<input type='checkbox' class='class volunteer' id='chkPerticipant" + id + "' title='" + name + "'>";
                volunteerHtml += "<label for='chkPerticipant" + id + "' style='font-weight:normal'>&nbsp;" + name + "</label><br />";
                volunteerHtml += "";
                volunteerCount += 1;
                closeVolunteerDivTag = true;
            }
            else
            {
                hasKids = true;
                if (count == 0)
                {
                    html += "<div class='col-sm-6'>";
                }
                if (showKids)
                {
                    html += "<div style='font-weight:bold;'>Child Participants</div>";
                    showKids = false;
                }
                html += "";
                html += "<input type='checkbox' class='class kids' id='chkPerticipant" + id + "' title='" + name + "'>";
                html += "<label for='chkPerticipant" + id + "'style='font-weight:normal'>&nbsp;" + name + "</label><br />";
                html += "";
                count += 1;
                closeKidsDivTag = true;
            }
        }

        if (!string.IsNullOrEmpty(html) && closeKidsDivTag)
            html += "</div>";
        if (!string.IsNullOrEmpty(volunteerHtml) && closeVolunteerDivTag)
            volunteerHtml += "</div>";

        btnMoveKids.Visible = hasKids;
        btnMoveVolunteer.Visible = hasVolunteer;
        participantsList.InnerHtml = html + volunteerHtml;
        RefreshToggleButton(isActiveLocation);
    }

    private void RefreshToggleButton(bool isActiveLocation)
    {
        lblOffLocation.Attributes.Remove("class");
        lblOnLocation.Attributes.Remove("class");
        if (isActiveLocation)
        {
            lblOffLocation.AddCssClass("btn btn-default btn-off btn-xs ");
            lblOnLocation.AddCssClass("btn btn-default btn-on btn-xs active");
        }
        else
        {
            lblOffLocation.AddCssClass("btn btn-default btn-off btn-xs active");
            lblOnLocation.AddCssClass("btn btn-default btn-on btn-xs");
        }
        BindGrid();
    }

    #endregion Refresh UI

    #region Bindgrid

    private void SetDisplayCount(DataTable dt)
    {
        int vTotalCount = 0;
        int cTotalCount = 0;
        for (int i = dt.Rows.Count - 1; i >= 0; i--) {            
            vTotalCount += Convert.ToInt32(dt.Rows[i]["VolunteerCount"]);
            cTotalCount += Convert.ToInt32(dt.Rows[i]["ChildCount"]);
        }
        lblChildCount.InnerText = Convert.ToString(cTotalCount);
        lblVolunteersCount.InnerText = Convert.ToString(vTotalCount);
        lblTotalCount.InnerText = Convert.ToString(cTotalCount + vTotalCount);
    }

    private DataTable BindGrid()
    {
        try
        {
            //grab location of person and define sql to run
            RockContext rockContext = new RockContext();

            //grab the current scheduleId, child group type ids, and volunteer group type ids to pass to the sql
            int scheduleId = ScheduleDropdown.SelectedValue.AsInteger();
            var childGroupTypeIds = GetChildrenGroupTypes_Init();
            var volunteerGroupTypeIds = GetVolunteerGroupTypes_Init();
            var today = DateTime.Today;

            //grab attendance occurrences grouped by location
            IQueryable<IGrouping<int?, AttendanceOccurrence>> attendanceOccurrences = rockContext.AttendanceOccurrences
                .Where(ao => DbFunctions.TruncateTime(ao.OccurrenceDate) == DbFunctions.TruncateTime( today ) )
                .Where(ao => ao.ScheduleId == scheduleId)
                .Where(ao => childGroupTypeIds.Contains(ao.Group.GroupTypeId) || volunteerGroupTypeIds.Contains(ao.Group.GroupTypeId))
                .GroupBy(ao => ao.LocationId);



            DataTable dt2 = new DataTable();

            DataColumn column;
            DataRow row;

            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Name";
            // Add the Column to the DataColumnCollection.
            dt2.Columns.Add(column);

            // Create second column.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Capacity";
            // Add the column to the table.
            dt2.Columns.Add(column);

            // Create third column.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Id";
            // Add the column to the table.
            dt2.Columns.Add(column);

            // Create fourth column.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "IsActive";
            // Add the column to the table.
            dt2.Columns.Add(column);

            // Create fifth column.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "ChildCount";
            // Add the column to the table.
            dt2.Columns.Add(column);

            // Create sixth column.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "VolunteerCount";
            // Add the column to the table.
            dt2.Columns.Add(column);

            foreach (IGrouping<int?, AttendanceOccurrence> locationGroup in attendanceOccurrences)
            {
                //create a new row and define child and volunteer count as 0 so nulls arent an issue
                row = dt2.NewRow();
                row["ChildCount"] = 0;
                row["VolunteerCount"] = 0;
                //loop through the occurrences for this location
                foreach (AttendanceOccurrence ao in locationGroup)
                {
                    //set the name of the location
                    row["Name"] = ao.Location.Name;
                    //and the threshold, if there is one
                    if (ao.Location.SoftRoomThreshold == null)
                    {
                        row["Capacity"] = -1;

                    }
                    else
                    {
                        row["Capacity"] = ao.Location.SoftRoomThreshold;

                    }
                    //and the id and whether the location is active
                    row["Id"] = ao.LocationId;
                    if (ao.Location.IsActive)
                    {
                        row["IsActive"] = "Open";

                    } else
                    {
                        row["IsActive"] = "Closed";
                    }
                    //count the child attendees if this attendance occurrence contains child records
                    if (childGroupTypeIds.Contains(ao.Group.GroupTypeId))
                    {
                        IEnumerable<Attendance> attChild = ao.Attendees
                            .Where(a => a.DidAttend == true)
                            .Where(a => childGroupTypeIds.Contains(a.Occurrence.Group.GroupTypeId))
                            .Where(a => a.StartDateTime > RockDateTime.Today)
                            .Where(a => a.EndDateTime > RockDateTime.Now || a.EndDateTime == null);
                        row["ChildCount"] = (int)row["ChildCount"] + attChild.Count();

                    } else
                    {
                        //and count the volunteer attendees if this attendance occurrence doesnt contain child records, because it must contain one of the two
                        IEnumerable<Attendance> attVolunteer = ao.Attendees
                            .Where(a => a.DidAttend == true)
                            .Where(a => volunteerGroupTypeIds.Contains(a.Occurrence.Group.GroupTypeId))
                            .Where(a => a.StartDateTime > RockDateTime.Today)
                            .Where(a => a.EndDateTime > RockDateTime.Now || a.EndDateTime == null);
                        row["VolunteerCount"] = (int)row["VolunteerCount"] + attVolunteer.Count();

                    }
                }
                //add the row to the datatable
                dt2.Rows.Add(row);
            }

            //define a data source that pulls from data table above
            gdGroupList.DataSource = dt2;
            gdGroupList.DataBind();
            //hide some columns we dont want
            SetDisplayCount(dt2);
            return dt2;
        }
        catch (Exception e)
        {
            string s = "";
        }
        return null;
    }

    #endregion Bindgrid

    #region Move To Location

    protected void MoveToAnotherLocation(bool isVolunteer)
    {
        try
        {
            //setup services
            string[] valueList = hdSelectedPerson.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            RockContext rockContext = new RockContext();
            GroupLocationService glService = new GroupLocationService(rockContext);
            AttendanceService attendanceService = new AttendanceService(rockContext);
            int moveTolocationId = Convert.ToInt32(hdSelectedMoveToGroupVal.Value);
            var selectedVolunteerGroupTypes = GetVolunteerGroupTypes_Init();
            var selectedChildrenGroupTypes = GetChildrenGroupTypes_Init();

            IQueryable<GroupLocation> groupLocations = null;
            //depending on if the move is a volunteer or not, we get the group locations viable for the move
            if (isVolunteer)
            {
                groupLocations = rockContext.GroupLocations
                    .Join(rockContext.Groups, gl => gl.GroupId, g => g.Id, (gl, g) => new { gl, g })
                    .Where(l => l.gl.LocationId == moveTolocationId)
                    .Where(l => selectedVolunteerGroupTypes.Contains(l.g.GroupTypeId))
                    .Select(l => l.gl);
            } else
            {
                groupLocations = rockContext.GroupLocations
                    .Join(rockContext.Groups, gl => gl.GroupId, g => g.Id, (gl, g) => new { gl, g })
                    .Where(l => l.gl.LocationId == moveTolocationId)
                    .Where(l => selectedChildrenGroupTypes.Contains(l.g.GroupTypeId))
                    .Select(l => l.gl);
            }

            if (groupLocations.Count() > 0)
            {
                //confirming the query returned something, then we grab all the attendances to be updated, make the appropriate corrections, and save
                int locId = groupLocations.FirstOrDefault().LocationId;
                int ggid = groupLocations.FirstOrDefault().GroupId;
                IQueryable<Attendance> attendancesToBeUpdated = attendanceService.GetByIds(valueList.Select(Int32.Parse).ToList());
                foreach (Attendance att in attendancesToBeUpdated)
                {
                    att.GroupId = ggid;
                    att.LocationId = moveTolocationId;
                }
                rockContext.SaveChanges();
                
                
                DataTable dtGrid = BindGrid();
                int currentGrpId = Convert.ToInt32(hdSelectedGroupId.Value);
                int newCapacity = -1;
                int currentCapacity = -1;
                int gid = 0;
                bool isFound = false;
                string grpName = lblGroupName.InnerText;
                string newGrpName = groupLocations.FirstOrDefault().Location.Name;
                for (int i = 0; i < dtGrid.Rows.Count; i++)
                {
                    gid = Convert.ToInt32(dtGrid.Rows[i]["Id"]);
                    int capacity = Convert.ToInt32(dtGrid.Rows[i]["Capacity"]);
                    if (gid == currentGrpId)
                    {
                        currentCapacity = capacity;
                        //isFound = true;
                        break;
                    }
                    else if (gid == moveTolocationId)
                    {
                        newCapacity = capacity;
                        newGrpName = Convert.ToString(dtGrid.Rows[i]["Name"]);
                    }

                }
                if (!isFound)
                {
                    currentCapacity = newCapacity;
                    currentGrpId = moveTolocationId;
                }

                RefreshUI(currentGrpId, newGrpName, currentCapacity);
            }
        }
        catch { }
    }

    #endregion Move To Location

    #region Support

    protected void DeleteRemoveAttendee(bool isRemove)
    {
        try
        {
            //this method should be split into two, methods should only ever perform one action / two verbs in the title of a method is a no-no - Ethan
            string[] valueList = hdRemoveAttendee.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            RockContext c = new RockContext();
            AttendanceService attendanceService = new AttendanceService(c);
            IQueryable<Attendance> attRecords = attendanceService.GetByIds(valueList.Select(Int32.Parse).ToList());
            //loop through the attendance records returned / removing or deleting them according to isRemove
            foreach (Attendance att in attRecords)
            {
                if (isRemove)
                {
                    att.EndDateTime = DateTime.Now;
                }
                else
                {
                    attendanceService.Delete(att);
                }
            }
            c.SaveChanges();

            if (valueList.Length > 0)
            {
                DataTable dtGrid = BindGrid();
                int currentGrpId = Convert.ToInt32(hdSelectedGroupId.Value);
                int newCapacity = 0;
                int currentCapacity = 0;
                int gid = 0;
                string grpName = lblGroupName.InnerText;
                bool isFound = false;
                for (int i = 0; i < dtGrid.Rows.Count; i++)
                {
                    gid = Convert.ToInt32(dtGrid.Rows[i]["Id"]);
                    int capacity = Convert.ToInt32(dtGrid.Rows[i]["Capacity"]);
                    if (gid == currentGrpId)
                    {
                        currentCapacity = capacity;
                        isFound = true;
                        break;
                    }

                }

                if (isFound)
                    RefreshUI(currentGrpId, grpName, currentCapacity);
            }
        }
        catch { }
    }

    #endregion Support
}