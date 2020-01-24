/// Author: Ethan Widen
/// Email: ethan@dtschurch.com
/// Created Date: 4/8/2019

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

[DisplayName("Birthdays/Anniversaries by Group")]
[Category("com_DTS > Birthdays & Anniversaries By Group Selection")]
[Description("See Birthdays and Anniversaries based on a multi-select of groups")]
[GroupTypesField("Group Types", "Group types to appear in the multi select", false)]
public partial class BirthdaysAndAnniversaries : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            BindDropDown();
        }

    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        groupMemberGrid.GridRebind += GroupMemberGrid_GridRebind;
    }

    private void GroupMemberGrid_GridRebind(object sender, GridRebindEventArgs e)
    {
        BindGrid();
    }

    private void BindDropDown()
    {
        if (!string.IsNullOrWhiteSpace(GetAttributeValue("GroupTypes")))
        {
            var groupTypeService = new GroupTypeService(new RockContext());
            List<String> groupTypeGuids = GetAttributeValue("GroupTypes").Split(',').ToList();
            List<int> groupTypeIds = groupTypeService.GetListByGuids(groupTypeGuids.Select(Guid.Parse).ToList()).Select(gt => gt.Id).ToList();
            gpGroups.IncludedGroupTypeIds = groupTypeIds;
        }
    }

    protected void btn_submit_Click(object sender, EventArgs e)
    {
        BindGrid();
    }

    protected void btn_clear_birthday(object sender, EventArgs e)
    {
        ClearBirthday();
    }
    protected void btn_clear_anniv(object sender, EventArgs e)
    {
        ClearAnniversary();
    }
    private void ClearBirthday()
    {
        dateRangeFrom.SelectedIndex = 0;
        dateRangeTo.SelectedIndex = 0;

        BindGrid();
    }
    protected void ClearAnniversary()
    {
        annDateRangeFrom.SelectedIndex = 0;
        annDateRangeTo.SelectedIndex = 0;
    }

    private void BindGrid()
    {

        var rockContext = new RockContext();
        var groupMemberService = new GroupMemberService(rockContext).Queryable().AsNoTracking();

        List<int> groupIdsMulti = gpGroups.SelectedValuesAsInt().ToList();
        var groupMembers = groupMemberService.Where(gm => groupIdsMulti.Contains(gm.GroupId));
        SortProperty sortProperty = groupMemberGrid.SortProperty;

        groupMembers = FilterByBirthday(groupMembers);

        if (sortProperty != null)
        {
            groupMemberGrid.DataSource = groupMembers.Select(gm => new MemberData()
            {
                FirstName = gm.Person.FirstName,
                LastName = gm.Person.LastName,
                Id = gm.Person.Id,
                BirthMonth = gm.Person.BirthMonth,
                BirthDay = gm.Person.BirthDay,
                BirthYear = gm.Person.BirthYear,
                Birthdate = gm.Person.BirthDate,
                AnniversaryMonth = gm.Person.AnniversaryDate.HasValue ? gm.Person.AnniversaryDate.Value.Month : (int?)null,
                AnniversaryDay = gm.Person.AnniversaryDate.HasValue ? gm.Person.AnniversaryDate.Value.Day : (int?)null,
                AnniversaryYear = gm.Person.AnniversaryDate.HasValue ? gm.Person.AnniversaryDate.Value.Year : (int?)null,
                Anniversary = gm.Person.AnniversaryDate
            }).Distinct().Sort(sortProperty).ToList();
        }
        else
        {
            groupMemberGrid.DataSource = groupMembers.Select(gm => new MemberData()
            {
                FirstName = gm.Person.FirstName,
                LastName = gm.Person.LastName,
                Id = gm.Person.Id,
                BirthMonth = gm.Person.BirthMonth,
                BirthDay = gm.Person.BirthDay,
                BirthYear = gm.Person.BirthYear,
                Birthdate = gm.Person.BirthDate,
                AnniversaryMonth = gm.Person.AnniversaryDate.HasValue ? gm.Person.AnniversaryDate.Value.Month : (int?)null,
                AnniversaryDay = gm.Person.AnniversaryDate.HasValue ? gm.Person.AnniversaryDate.Value.Day : (int?)null,
                AnniversaryYear = gm.Person.AnniversaryDate.HasValue ? gm.Person.AnniversaryDate.Value.Year : (int?)null,
                Anniversary = gm.Person.AnniversaryDate
            }).Distinct().OrderBy(s => s.FirstName).ToList();
        }


        groupMemberGrid.DataBind();
        
    }

    /// <summary>
    /// Shows the modal alert message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="modalAlertType">Type of the modal alert.</param>
    private IQueryable<GroupMember> FilterByBirthday(IQueryable<GroupMember> groupMembers)
    {

        // When alldropdowns are selected with a value. Here it will show everyone in the date range selected for both birthday and anniversary
        if (dateRangeFrom.SelectedIndex > 0 && dateRangeTo.SelectedIndex > 0 && annDateRangeFrom.SelectedIndex > 0 && annDateRangeTo.SelectedIndex > 0)
        {
            int birthdayMonthFrom = int.Parse(dateRangeFrom.SelectedValue);
            int birthdayMonthTo = int.Parse(dateRangeTo.SelectedValue);

            int annBirthdayMonthFrom = int.Parse(annDateRangeFrom.SelectedValue);
            int annBirthdayMonthTo = int.Parse(annDateRangeTo.SelectedValue);

            groupMembers = groupMembers.Where(gm => (gm.Person.BirthDate.Value.Month >= birthdayMonthFrom && gm.Person.BirthDate.Value.Month <= birthdayMonthTo) || (gm.Person.AnniversaryDate.Value.Month >= annBirthdayMonthFrom && gm.Person.AnniversaryDate.Value.Month <= annBirthdayMonthTo) );

        }

        if (dateRangeFrom.SelectedIndex == 0 && dateRangeTo.SelectedIndex == 0)
        {

        }
        else if (dateRangeFrom.SelectedIndex != 0 && dateRangeTo.SelectedIndex <= 0)
        {
            if (dateRangeFrom.SelectedIndex != -1)
            {
                int birthdayMonthFrom = int.Parse(dateRangeFrom.SelectedValue);
                groupMembers = groupMembers.Where(gm => gm.Person.BirthDate.Value.Month == birthdayMonthFrom);
            }
        }
        else if (dateRangeFrom.SelectedIndex <= 0 && dateRangeTo.SelectedIndex != 0)
        {
            if (dateRangeTo.SelectedIndex != -1)
            {
                int birthdayMonthTo = int.Parse(dateRangeTo.SelectedValue);
                groupMembers = groupMembers.Where(gm => gm.Person.BirthDate.Value.Month == birthdayMonthTo);
            }
        }
        else if (dateRangeFrom.SelectedIndex != 0 && dateRangeTo.SelectedIndex != 0)
        {
            if (dateRangeFrom.SelectedIndex > 0 && dateRangeTo.SelectedIndex >= 0)
            {
                int birthdayMonthFrom = int.Parse(dateRangeFrom.SelectedValue);
                int birthdayMonthTo = int.Parse(dateRangeTo.SelectedValue);
                groupMembers = groupMembers.Where(gm => gm.Person.BirthDate.Value.Month >= birthdayMonthFrom && gm.Person.BirthDate.Value.Month <= birthdayMonthTo);
            }
        }


        if (annDateRangeFrom.SelectedIndex == 0 && annDateRangeTo.SelectedIndex == 0)
        {

        }
        else if (annDateRangeFrom.SelectedIndex != 0 && annDateRangeTo.SelectedIndex <= 0)
        {
            if (annDateRangeFrom.SelectedIndex != -1)
            {
                int birthdayMonthFrom = int.Parse(annDateRangeFrom.SelectedValue);
                groupMembers = groupMembers.Where(gm => gm.Person.AnniversaryDate.Value.Month == birthdayMonthFrom);
            }
        }
        else if (annDateRangeFrom.SelectedIndex <= 0 && annDateRangeTo.SelectedIndex != 0)
        {
            if (annDateRangeTo.SelectedIndex != -1)
            {
                int birthdayMonthTo = int.Parse(annDateRangeTo.SelectedValue);
                groupMembers = groupMembers.Where(gm => gm.Person.AnniversaryDate.Value.Month == birthdayMonthTo);
            }
        }
        else if (dateRangeFrom.SelectedIndex == 0 && dateRangeTo.SelectedIndex != 0)
        {
            int birthdayMonthTo = int.Parse(dateRangeTo.SelectedValue);
            groupMembers = groupMembers.Where(gm => gm.Person.AnniversaryDate.Value.Month == birthdayMonthTo);
        }
        else if (annDateRangeFrom.SelectedIndex != 0 && annDateRangeTo.SelectedIndex != 0)
        {
            if (annDateRangeFrom.SelectedIndex > 0 && annDateRangeTo.SelectedIndex >= 0)
            {
                int birthdayMonthFrom = int.Parse(annDateRangeFrom.SelectedValue);
                int birthdayMonthTo = int.Parse(annDateRangeTo.SelectedValue);
                groupMembers = groupMembers.Where(gm => gm.Person.AnniversaryDate.Value.Month >= birthdayMonthFrom && gm.Person.AnniversaryDate.Value.Month <= birthdayMonthTo);
            }
        }

        return groupMembers;
    }
    
    protected void groupMemberGrid_Sorting(object sender, GridViewSortEventArgs e)
    {
        BindGrid();
    }

    protected void gmFilters_ApplyFilterClick(object sender, EventArgs e)
    {
        //gmFilters.SaveUserPreference( "gmBirthdayDateRange", "Birthday Date Range", gmBirthdayDateRange.DelimitedValues );
        //gmFilters.SaveUserPreference( "gmAnniversaryDateRange", "Anniversary Date Range", gmAnniversaryDateRange.DelimitedValues );
        BindGrid();
    }
}

internal class MemberData
{
    public string FirstName { get; set; }
    public int Id { get; internal set; }
    public DateTime? Birthdate { get; internal set; }
    public DateTime? Anniversary { get; internal set; }
    public int? BirthMonth { get; internal set; }
    public int? BirthDay { get; internal set; }
    public int? BirthYear { get; internal set; }
    public int? AnniversaryMonth { get; internal set; }
    public int? AnniversaryDay { get; internal set; }
    public int? AnniversaryYear { get; internal set; }
    public string LastName { get; internal set; }

    public MemberData()
    {
    }
}