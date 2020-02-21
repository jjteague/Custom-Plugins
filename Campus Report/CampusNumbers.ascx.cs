/// Author: Ethan Widen
/// Email: ethan@dtschurch.com
/// Created Date: 12/19/2019

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_DTS.CampusReport
{


    [DisplayName("Campus Numbers")]
    [Category("com_DTS > Metric Reports")]
    [Description("Reporting block that shows attendance and metrics")]
    [SchedulesField("Schedules", "Pick all schedules to report on (serving schedule or normal schedule)", true)]
    [MetricCategoriesField("Non Serving Team Metrics", "Metrics to report on", true, "", "", 0, "NonServingMetrics")]
    [MetricCategoriesField("Serving Team Metrics", "Metrics to report on", false, "", "", 0, "ServingMetrics")]
    [MetricCategoriesField("Attendance Metric", "This metric will be used to calculate the percentage of the church serving if any serving team metrics are defined", false, "", "", 0, "AttendanceMetric")]
    [TextField("Name of Service/Schedule Partition", "Set the partition name for the schedule or service (since the naming of this partition varies)", true, "", "", 0, "NameOfServiceTypePartition")]
    [AttributeField("0b2c38a7-d79c-4f85-9757-f1b045d32c8a", "Attribute on Schedule for the Type of Schedule (Normal/Serving Team", "This block setting defines the entity attribute on a Schedule that" +
        "indicates the intent of the schedule (serving teams or normal).", false)]
    [IntegerField("Sundays back", "This block setting defines how many sundays back the dropdown on the report shows. Defaulted to 8, so the dropdown will show this upcoming Sunday and 8 previous Sundays", false, 8, "", 0, "SundaysBack")]
    public partial class CampusNumbers : RockBlock
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadSundayDatesDropDown();
                LoadCampusDropDown();
            }

        }

        private void LoadCampusDropDown()
        {
            ddlCampus.Items.Clear();
            List<Campus> campuses = getCampuses();
            foreach(Campus campus in campuses)
            {
                ddlCampus.Items.Add(new ListItem(campus.Name, campus.Id.ToString()));
            }
            ddlCampus.Items.Add(new ListItem("All", "0"));
        }

        private List<Campus> getCampuses()
        {
            RockContext rockContext = new RockContext();
            var campusService = new CampusService(rockContext);
            List<Campus> campuses = campusService.Queryable().ToList();
            return campuses;
        }

        private void LoadSundayDatesDropDown()
        {
             ddlSundayDates.Items.Clear();
            int weeksBack = GetAttributeValue("SundaysBack").AsInteger();
            List<DateTime> sundayDates = getSundays(weeksBack);
            foreach(DateTime sunday in sundayDates)
            {
                ddlSundayDates.Items.Add(new ListItem(sunday.ToShortDateString(), sunday.ToShortDateString()));
            }
            

        }

        public static List<DateTime> getSundays(int weeksBack)
        {
            List<DateTime> lstSundays = new List<DateTime>();

            DateTime timeBack = DateTime.Now.Date.AddDays((0 - (weeksBack * 7)));
            DateTime nextWeekNow = DateTime.Now.Date.AddDays(7);

            int intDaysPastTimeBack = (int)(nextWeekNow - timeBack).TotalDays;
            
            for (int i = 1; i < intDaysPastTimeBack + 1; i++)
            {
                DateTime dayToTest = timeBack.AddDays(i);
                if (dayToTest.DayOfWeek == DayOfWeek.Sunday)
                {
                    lstSundays.Add(dayToTest);
                }
            }
            return lstSundays;
        }

        protected void btn_submit_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        public static DateTime FirstDayOfWeek(DateTime date)
        {
            DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int offset = fdow - date.AddDays(7).DayOfWeek;
            DateTime fdowDate = date.AddDays(offset);
            return fdowDate;
        }

        private void BindGrid()
        {
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );
            var scheduleService = new ScheduleService(rockContext);
            var metricService = new MetricService(rockContext);
            var metricValueService = new MetricValueService(rockContext);
            //grab the metrics from the block setting that we want to print out
            List<MetricCategoriesFieldAttribute.MetricCategoryPair> nonServingMetricsFromSettings = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs(GetAttributeValue("NonServingMetrics"));
            List<MetricCategoriesFieldAttribute.MetricCategoryPair> servingMetricsFromSettings = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs(GetAttributeValue("ServingMetrics"));
            List<MetricCategoriesFieldAttribute.MetricCategoryPair> attendanceMetricFromSettings = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs(GetAttributeValue("AttendanceMetric"));
            try
            {
                if (attendanceMetricFromSettings.Count > 1)
                {
                    throw new Exception();
                }
            } catch (Exception e)
            {
                maCampusScheduleWarning.Show("There is more than one attendance metric defined in the block settings. Only a single metric should be defined. " +
                    "This attendance metric is used to calculate the percentage of people serving in the church and can be seen as a row on a serving metric type in the report", ModalAlertType.Alert);
            }
            var schedulesFromSettings = GetAttributeValue("Schedules").Split(',').ToList();

            //grab the sunday date from the dropdown so we know the date to ask for metric values
            DateTime sunday = Convert.ToDateTime(ddlSundayDates.SelectedValue.ToString());
            //and the previous 4 Sundays this year
            DateTime weekAgoSunday = sunday.AddDays(-7);
            DateTime twoWeekAgoSunday = weekAgoSunday.AddDays(-7);
            DateTime threeWeekAgoSunday = twoWeekAgoSunday.AddDays(-7);
            DateTime fourWeekAgoSunday = threeWeekAgoSunday.AddDays(-7);
            //also grab the 5 corresponding Sundays from a year ago
            DateTime sundayLastYear = FirstDayOfWeek( sunday.AddYears(-1));
            DateTime weekAgoLastYear = sundayLastYear.AddDays(-7);
            DateTime twoWeeksAgoLastYear = weekAgoLastYear.AddDays(-7);
            DateTime threeWeeksAgoLastYear = twoWeeksAgoLastYear.AddDays(-7);
            DateTime fourWeeksAgoLastYear = threeWeeksAgoLastYear.AddDays(-7);

            //grab the campus or All
            int campusId = Convert.ToInt32(ddlCampus.SelectedValue.ToString());
            Campus campus = campusService.Queryable().First(c => c.Id == campusId);

            //set variables for grabbing attendance for the last three weeks to use if any serving metrics are defined and an attendance metric is defined
            int thisWeekAttendanceNum = 0;
            int lastWeekAttendanceNum = 0;
            int twoWeeksAgoAttendanceNum = 0;


            //grab the schedules from the block setting so we know how to restrict the metric values when we request them
            List<Schedule> schedules = new List<Schedule>();
            List<Schedule> schedulesByCampus = new List<Schedule>();
            if (schedulesFromSettings != null)
            {
                schedules = scheduleService.Queryable().Where(s => schedulesFromSettings.Contains(s.Guid.ToString())).ToList();
                foreach (Schedule schedule in schedules)
                {
                    try
                    {

                        schedule.LoadAttributes();
                        Rock.Web.Cache.AttributeValueCache value;
                        schedule.AttributeValues.TryGetValue("Campus", out value);
                        if (value.Value == campus.Guid.ToString())
                        {
                            schedulesByCampus.Add(schedule);
                        }
                    } catch (Exception e)
                    {
                        maCampusScheduleWarning.Show("There was an issue getting the campus associated with the \"" + schedule + "\" schedule. Be sure the schedules have a Campus entity attribute for this block to work.", ModalAlertType.Warning);
                        return;
                    }
                }
            }


            //return the metrics we want to produce an HTML table/report for
            List<MetricValue> metricValues = metricValueService.Queryable().Where(mv => mv.MetricValueType == MetricValueType.Measure).Where(mv => mv.MetricValueDateTime == sunday ||
            mv.MetricValueDateTime == weekAgoSunday ||
            mv.MetricValueDateTime == twoWeekAgoSunday ||
            mv.MetricValueDateTime == threeWeekAgoSunday ||
            mv.MetricValueDateTime == fourWeekAgoSunday ||
            mv.MetricValueDateTime == sundayLastYear ||
            mv.MetricValueDateTime == weekAgoLastYear ||
            mv.MetricValueDateTime == twoWeeksAgoLastYear ||
            mv.MetricValueDateTime == threeWeeksAgoLastYear ||
            mv.MetricValueDateTime == fourWeeksAgoLastYear).OrderBy(mv => mv.Metric.Title).OrderBy(mv => mv.MetricValueDateTime).ToList();

            Panel reportPanel = new Panel();
            Table table = new Table();
            table.CssClass = "table table-sm";

            //row to show campus name at top of report
            TableHeaderRow tableCampusHeaderRow = new TableHeaderRow();
            TableHeaderCell thCampus = new TableHeaderCell();
            thCampus.Text = campus.Name.ToUpper();
            thCampus.CssClass = "col-xs-12 text-center";
            thCampus.ColumnSpan = 9;

            tableCampusHeaderRow.Cells.Add(thCampus);
            tableCampusHeaderRow.BackColor = System.Drawing.Color.DarkGray;
            table.Rows.Add(tableCampusHeaderRow);

            //loop through the non serving metrics
            foreach (MetricCategoriesFieldAttribute.MetricCategoryPair metricPair in nonServingMetricsFromSettings)
            {
                //loop starts here for each metric in the table
                //grab metric to spin through and print out header
                Metric metric = metricService.Get(metricPair.MetricGuid);
                //check to see if this metric is the attendance metric defined in the block setting
                //if so we flag it to then later save the attendance numbers for this week, last week, and two weeks ago
                Boolean matchAttendanceMetric = false;
                if (attendanceMetricFromSettings != null)
                {
                    matchAttendanceMetric = metric.Guid.Equals(attendanceMetricFromSettings.FirstOrDefault().MetricGuid);

                }
                


                TableHeaderRow tableHeaderRow = new TableHeaderRow();
                TableHeaderCell thDate = new TableHeaderCell();
                TableHeaderCell thMetricName = new TableHeaderCell();
                TableHeaderCell thGrowth = new TableHeaderCell();
                TableHeaderCell thFiveWeekLook = new TableHeaderCell();
                TableHeaderCell thLastFiveDiff = new TableHeaderCell();
                thDate.Text = String.Format("{0}", sunday.ToShortDateString());
                thDate.CssClass = "col-xs-2";
                //these may need to be dynamic by all campus or specific campus. for now defaulted to specific campus
                thDate.ColumnSpan = 1;
                thMetricName.Text = String.Format("{0}", metric.Title);
                thMetricName.CssClass = "col-xs-2";
                thMetricName.ColumnSpan = 3;
                thGrowth.Text = "Growth";
                thGrowth.CssClass = "col-xs-2";
                thGrowth.ColumnSpan = 1;
                thFiveWeekLook.Text = "5 Week Look (Last 5)";
                thFiveWeekLook.CssClass = "col-xs-2";
                thFiveWeekLook.ColumnSpan = 2;
                thLastFiveDiff.Text = "Last 5 Difference";
                thLastFiveDiff.CssClass = "col-xs-2";
                thLastFiveDiff.ColumnSpan = 2;
                tableHeaderRow.Cells.Add(thDate);
                tableHeaderRow.Cells.Add(thMetricName);
                tableHeaderRow.Cells.Add(thGrowth);
                tableHeaderRow.Cells.Add(thFiveWeekLook);
                tableHeaderRow.Cells.Add(thLastFiveDiff);
                table.Rows.Add(tableHeaderRow);

                TableHeaderRow tableSubHeaderRow = new TableHeaderRow();
                TableHeaderCell thServiceTime = new TableHeaderCell();
                TableHeaderCell thThisWeekAtt = new TableHeaderCell();
                TableHeaderCell thLastWeekAtt = new TableHeaderCell();
                TableHeaderCell thTwoWeekAgoAtt = new TableHeaderCell();
                TableHeaderCell thWeekToWeek = new TableHeaderCell();
                TableHeaderCell thFiveWeekThisYear = new TableHeaderCell();
                TableHeaderCell thFiveWeekLastYear = new TableHeaderCell();
                TableHeaderCell thLastFivePeople = new TableHeaderCell();
                TableHeaderCell thLastFivePercent = new TableHeaderCell();

                thServiceTime.Text = "Service Time";
                thServiceTime.CssClass = "col-xs-1";
                thServiceTime.ColumnSpan = 1;

                thThisWeekAtt.Text = "This Week";
                thThisWeekAtt.CssClass = "col-xs-1";
                thThisWeekAtt.ColumnSpan = 1;

                thLastWeekAtt.Text = "Last Week";
                thLastWeekAtt.CssClass = "col-xs-1";
                thLastWeekAtt.ColumnSpan = 1;

                thTwoWeekAgoAtt.Text = "2 Weeks Ago";
                thTwoWeekAgoAtt.CssClass = "col-xs-1";
                thTwoWeekAgoAtt.ColumnSpan = 1;

                thWeekToWeek.Text = "Week to Week %";
                thWeekToWeek.CssClass = "col-xs-1";
                thWeekToWeek.ColumnSpan = 1;

                thFiveWeekThisYear.Text = String.Format("{0}", sunday.Year);
                thFiveWeekThisYear.CssClass = "col-xs-1";
                thFiveWeekThisYear.ColumnSpan = 1;

                thFiveWeekLastYear.Text = String.Format("{0}", sundayLastYear.Year);
                thFiveWeekLastYear.CssClass = "col-xs-1";
                thFiveWeekLastYear.ColumnSpan = 1;

                thLastFivePeople.Text = "People";
                thLastFivePeople.CssClass = "col-xs-1";
                thLastFivePeople.ColumnSpan = 1;

                thLastFivePercent.Text = "Growth Rate %";
                thLastFivePercent.CssClass = "col-xs-1";
                thLastFivePercent.ColumnSpan = 1;


                tableSubHeaderRow.Cells.Add(thServiceTime);
                tableSubHeaderRow.Cells.Add(thThisWeekAtt);
                tableSubHeaderRow.Cells.Add(thLastWeekAtt);
                tableSubHeaderRow.Cells.Add(thTwoWeekAgoAtt);
                tableSubHeaderRow.Cells.Add(thWeekToWeek);
                tableSubHeaderRow.Cells.Add(thFiveWeekThisYear);
                tableSubHeaderRow.Cells.Add(thFiveWeekLastYear);
                tableSubHeaderRow.Cells.Add(thLastFivePeople);
                tableSubHeaderRow.Cells.Add(thLastFivePercent);
                table.Rows.Add(tableSubHeaderRow);



                //total variables for this metric
                int totalThisWeekVal = 0;
                int totalLastWeekVal = 0;
                int totalTwoWeeksVal = 0;
                int totalThreeWeeksVal = 0;
                int totalFourWeeksVal = 0;
                    
                int totalThisWeekLastYearVal = 0;
                int totalLastWeekLastYearVal = 0;
                int totalTwoWeeksLastYearVal = 0;
                int totalThreeWeeksLastYearVal = 0;
                int totalFourWeeksLastYearVal = 0;



                //now add schedule rows and print out actual metric values and calculations
                foreach (Schedule schedule in schedulesByCampus)
                {
                    try
                    {

                        schedule.LoadAttributes();
                        Rock.Web.Cache.AttributeValueCache value;
                        if (schedule.AttributeValues.ContainsKey("ServingTeamorNormalSchedule"))
                        {
                            schedule.AttributeValues.TryGetValue("ServingTeamorNormalSchedule", out value);
                            if (value.Value != "Normal")
                            {
                                continue;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        maCampusScheduleWarning.Show("There was an issue getting the serving team or normal schedule attribute associated with the \"" + schedule + "\" schedule." +
                            " Be sure the schedules have a ServingTeamorNormalSchedule entity correctly setup. The attribute must be a Single-Select with two values. Normal and Serving Team, with the default set to Normal", ModalAlertType.Warning);
                        return;
                    }

                    TableRow tr = new TableRow();

                    int thisWeekVal = 0;
                    int lastWeekVal = 0;
                    int twoWeeksVal = 0;
                    int threeWeeksVal = 0;
                    int fourWeeksVal = 0;

                    int thisWeekLastYearVal = 0;
                    int lastWeekLastYearVal = 0;
                    int twoWeeksLastYearVal = 0;
                    int threeWeeksLastYearVal = 0;
                    int fourWeeksLastYearVal = 0;

                    int numOfMeetingsInPastFive = 0;
                    int numOfMeetingsInPastFiveYearAgo = 0;


                    foreach (MetricValue mv in metricValues)
                    {
                        //grab service time for this metric value
                        try
                        {

                            string scheduleTypePartitionName = GetAttributeValue("NameOfServiceTypePartition");
                            MetricValuePartition mp = mv.MetricValuePartitions.Single(mvp => mvp.MetricPartition.Label == scheduleTypePartitionName);
                            //if it matches the schedule we are looping through then print the row
                            if (schedule.Id == mp.EntityId && metric.Id == mv.MetricId)
                            {
                                if (mv.MetricValueDateTime == sunday)
                                {
                                    thisWeekVal = Convert.ToInt32( mv.YValue);
                                    totalThisWeekVal += thisWeekVal;
                                    numOfMeetingsInPastFive++;
                                    if (matchAttendanceMetric)
                                    {
                                        thisWeekAttendanceNum += Convert.ToInt32(mv.YValue);
                                    }
                                }
                                if (mv.MetricValueDateTime == weekAgoSunday)
                                {
                                    lastWeekVal = Convert.ToInt32(mv.YValue);
                                    totalLastWeekVal += lastWeekVal;
                                    numOfMeetingsInPastFive++;
                                    if (matchAttendanceMetric)
                                    {
                                        lastWeekAttendanceNum += Convert.ToInt32(mv.YValue);
                                    }
                                }
                                if (mv.MetricValueDateTime == twoWeekAgoSunday)
                                {
                                    twoWeeksVal = Convert.ToInt32(mv.YValue);
                                    totalTwoWeeksVal += twoWeeksVal;
                                    numOfMeetingsInPastFive++;
                                    if (matchAttendanceMetric)
                                    {
                                        twoWeeksAgoAttendanceNum += Convert.ToInt32(mv.YValue);
                                    }
                                }
                                if (mv.MetricValueDateTime == threeWeekAgoSunday)
                                {
                                    threeWeeksVal = Convert.ToInt32(mv.YValue);
                                    totalThreeWeeksVal += threeWeeksVal;
                                    numOfMeetingsInPastFive++;
                                }
                                if (mv.MetricValueDateTime == fourWeekAgoSunday)
                                {
                                    fourWeeksVal = Convert.ToInt32(mv.YValue);
                                    totalFourWeeksVal += fourWeeksVal;
                                    numOfMeetingsInPastFive++;
                                }

                                if (mv.MetricValueDateTime == sundayLastYear)
                                {
                                    thisWeekLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalThisWeekLastYearVal += thisWeekLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == weekAgoLastYear)
                                {
                                    lastWeekLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalLastWeekLastYearVal += lastWeekLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == twoWeeksAgoLastYear)
                                {
                                    twoWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalTwoWeeksLastYearVal += twoWeeksLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == threeWeeksAgoLastYear)
                                {
                                    threeWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalThreeWeeksLastYearVal += threeWeeksLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == fourWeeksAgoLastYear)
                                {
                                    fourWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalFourWeeksLastYearVal += fourWeeksLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                            
                            
                            
                            }

                        }
                        catch (Exception e)
                        {
                            maCampusScheduleWarning.Show("There was an issue accessing the schedule/service partitions. Be sure to set a value in the block settings and confirm your metrics have schedule/service partitions<br>" + e.StackTrace, ModalAlertType.Warning);
                            return;
                        }


                    }
                    TableCell tcServiceTime = new TableCell();
                    TableCell tcThisWeek = new TableCell();
                    TableCell tcLastWeek = new TableCell();
                    TableCell tcTwoWeeks = new TableCell();
                    TableCell tcGrowth = new TableCell();
                    TableCell tcFiveWeekThisYear = new TableCell();
                    TableCell tcFiveWeekLastYear = new TableCell();
                    TableCell tcDiffPeople = new TableCell();
                    TableCell tcDiffPercent = new TableCell();

                    Label serviceTimeLabel = new Label();
                    serviceTimeLabel.Text = String.Format("{0}", schedule.Name);
                    tcServiceTime.Controls.Add(serviceTimeLabel);
                    tr.Cells.Add(tcServiceTime);

                    Label thisWeekLabel = new Label();
                    thisWeekLabel.Text = String.Format("{0}", thisWeekVal);
                    tcThisWeek.Controls.Add(thisWeekLabel);
                    tr.Cells.Add(tcThisWeek);

                    Label lastWeekLabel = new Label();
                    lastWeekLabel.Text = String.Format("{0}", lastWeekVal);
                    tcLastWeek.Controls.Add(lastWeekLabel);
                    tr.Cells.Add(tcLastWeek);

                    Label twoWeeksLabel = new Label();
                    twoWeeksLabel.Text = String.Format("{0}", twoWeeksVal);
                    tcTwoWeeks.Controls.Add(twoWeeksLabel);
                    tr.Cells.Add(tcTwoWeeks);

                    Label growthLabel = new Label();
                    double growthPercentage = 0;
                    if (lastWeekVal != 0)
                    {
                        growthPercentage = ((double)(thisWeekVal - lastWeekVal) / lastWeekVal) * 100;
                    }
                    growthLabel.Text = String.Format("{0}%", Math.Round(growthPercentage)); // =((C6-D6)/D6*1)
                    tcGrowth.Controls.Add(growthLabel);
                    SetBackgroundColorByAboveOrBelowZero(tcGrowth, Math.Round(growthPercentage));
                    
                    tr.Cells.Add(tcGrowth);

                    Label fiveWeekThisYearLabel = new Label();
                    double fiveWeekThisYearVal = 0;
                    try
                    {
                        if (numOfMeetingsInPastFive != 0)
                        {
                            fiveWeekThisYearVal = ((double)(thisWeekVal + lastWeekVal + twoWeeksVal + threeWeeksVal + fourWeeksVal) / numOfMeetingsInPastFive);
                        }
                    } catch (Exception e)
                    {
                        maCampusScheduleWarning.Show("Division By 0 during calculation of the 5 Week Look of this year for "+ metric.Title +". Be sure there is at least one metric value entered in the past 5 weeks", ModalAlertType.Alert);
                    }
                    fiveWeekThisYearLabel.Text = String.Format("{0}", Math.Round(fiveWeekThisYearVal)); // average of last 5 weeks together
                    tcFiveWeekThisYear.Controls.Add(fiveWeekThisYearLabel);
                    SetBackgroundColorByAboveOrBelowZero(tcFiveWeekThisYear, Math.Round(fiveWeekThisYearVal - thisWeekVal));
                    
                    tr.Cells.Add(tcFiveWeekThisYear);

                    Label fiveWeekLastYearLabel = new Label();
                    double fiveWeekLastYearVal = 0;

                    try
                    {
                        if (numOfMeetingsInPastFiveYearAgo != 0)
                        {
                            fiveWeekLastYearVal = ((double)(thisWeekLastYearVal + lastWeekLastYearVal + twoWeeksLastYearVal + threeWeeksLastYearVal + fourWeeksLastYearVal) / numOfMeetingsInPastFiveYearAgo);
                        }
                    } catch (Exception e)
                    {
                        maCampusScheduleWarning.Show("Division By 0 during calculation of the 5 Week Look of last year for " + metric.Title + ". Be sure there is at least one metric value entered last year in this 5 week time frame", ModalAlertType.Alert);
                    }
                    fiveWeekLastYearLabel.Text = String.Format("{0}", Math.Round(fiveWeekLastYearVal)); // average of last 5 weeks last year together
                    tcFiveWeekLastYear.Controls.Add(fiveWeekLastYearLabel);
                    SetBackgroundColorByAboveOrBelowZero(tcFiveWeekLastYear, Math.Round(fiveWeekLastYearVal - lastWeekLastYearVal));
                   
                    tr.Cells.Add(tcFiveWeekLastYear);

                    Label diffPeopleLabel = new Label();
                    diffPeopleLabel.Text = String.Format("{0}", (Math.Round(fiveWeekThisYearVal) - Math.Round(fiveWeekLastYearVal)));
                    tcDiffPeople.Controls.Add(diffPeopleLabel);
                    tcDiffPeople.AddCssClass("diffPeople");
                    SetBackgroundColorByAboveOrBelowZero(tcDiffPeople, (Math.Round(fiveWeekThisYearVal) - Math.Round(fiveWeekLastYearVal)));
                    tr.Cells.Add(tcDiffPeople);

                    Label diffPercentLabel = new Label();
                    double diffPercentage = 0;
                    if (fiveWeekLastYearVal != 0)
                    {
                        diffPercentage = ((double)(Math.Round(fiveWeekThisYearVal) - Math.Round(fiveWeekLastYearVal)) / Math.Round(fiveWeekLastYearVal)) * 100;
                    }
                    diffPercentLabel.Text = String.Format("{0}%", Math.Round(diffPercentage));
                    tcDiffPercent.Controls.Add(diffPercentLabel);
                    tcDiffPercent.AddCssClass("diffPercent");
                    SetBackgroundColorByAboveOrBelowZero(tcDiffPercent, diffPercentage);
                   
                    tr.Cells.Add(tcDiffPercent);

                    table.Rows.Add(tr); 
                }
                TableRow totalTr = new TableRow();
                //now add another row for totals
                TableCell totalTcServiceTime = new TableCell();
                TableCell totalTcThisWeek = new TableCell();
                TableCell totalTcLastWeek = new TableCell();
                TableCell totalTcTwoWeeks = new TableCell();
                TableCell totalTcGrowth = new TableCell();
                TableCell totalTcFiveWeekThisYear = new TableCell();
                TableCell totalTcFiveWeekLastYear = new TableCell();
                TableCell totalTcDiffPeople = new TableCell();
                TableCell totalTcDiffPercent = new TableCell();

                Label TotalserviceTimeLabel = new Label();
                TotalserviceTimeLabel.Text = "Total";
                totalTcServiceTime.Controls.Add(TotalserviceTimeLabel);
                totalTr.Cells.Add(totalTcServiceTime);

                Label TotalthisWeekLabel = new Label();
                TotalthisWeekLabel.Text = String.Format("{0}", totalThisWeekVal);
                totalTcThisWeek.Controls.Add(TotalthisWeekLabel);
                totalTr.Cells.Add(totalTcThisWeek);

                Label TotallastWeekLabel = new Label();
                TotallastWeekLabel.Text = String.Format("{0}", totalLastWeekVal);
                totalTcLastWeek.Controls.Add(TotallastWeekLabel);
                totalTr.Cells.Add(totalTcLastWeek);

                Label TotaltwoWeeksLabel = new Label();
                TotaltwoWeeksLabel.Text = String.Format("{0}", totalTwoWeeksVal);
                totalTcTwoWeeks.Controls.Add(TotaltwoWeeksLabel);
                totalTr.Cells.Add(totalTcTwoWeeks);

                Label TotalgrowthLabel = new Label();
                double TotalgrowthPercentage = 0;
                if (totalLastWeekVal != 0)
                {
                    TotalgrowthPercentage = ((double)(totalThisWeekVal - totalLastWeekVal) / totalLastWeekVal) * 100;
                }
                TotalgrowthLabel.Text = String.Format("{0}%", Math.Round(TotalgrowthPercentage)); // =((C6-D6)/D6*1)
                totalTcGrowth.Controls.Add(TotalgrowthLabel);
                SetBackgroundColorByAboveOrBelowZero(totalTcGrowth, TotalgrowthPercentage);
                totalTr.Cells.Add(totalTcGrowth);

                Label TotalfiveWeekThisYearLabel = new Label();
                double TotalfiveWeekThisYearVal = ((double)(totalThisWeekVal + totalLastWeekVal + totalTwoWeeksVal + totalThreeWeeksVal + totalFourWeeksVal) / 5);
                TotalfiveWeekThisYearLabel.Text = String.Format("{0}", Math.Round(TotalfiveWeekThisYearVal)); // average of last 5 weeks together
                totalTcFiveWeekThisYear.Controls.Add(TotalfiveWeekThisYearLabel);
                SetBackgroundColorByAboveOrBelowZero(totalTcFiveWeekThisYear, TotalfiveWeekThisYearVal);
                totalTr.Cells.Add(totalTcFiveWeekThisYear);

                Label TotalfiveWeekLastYearLabel = new Label();
                double TotalfiveWeekLastYearVal = ((double)(totalThisWeekLastYearVal + totalLastWeekLastYearVal + totalTwoWeeksLastYearVal + totalThreeWeeksLastYearVal + totalFourWeeksLastYearVal) / 5);
                TotalfiveWeekLastYearLabel.Text = String.Format("{0}", Math.Round(TotalfiveWeekLastYearVal)); // average of last 5 weeks last year together
                totalTcFiveWeekLastYear.Controls.Add(TotalfiveWeekLastYearLabel);
                SetBackgroundColorByAboveOrBelowZero(totalTcFiveWeekLastYear, TotalfiveWeekLastYearVal);
                totalTr.Cells.Add(totalTcFiveWeekLastYear);

                Label TotaldiffPeopleLabel = new Label();
                TotaldiffPeopleLabel.Text = String.Format("{0}", (Math.Round(TotalfiveWeekThisYearVal) - Math.Round(TotalfiveWeekLastYearVal)));
                totalTcDiffPeople.Controls.Add(TotaldiffPeopleLabel);
                totalTcDiffPeople.AddCssClass("diffPeople");
                SetBackgroundColorByAboveOrBelowZero(totalTcDiffPeople, (Math.Round(TotalfiveWeekThisYearVal) - Math.Round(TotalfiveWeekLastYearVal)));
                totalTr.Cells.Add(totalTcDiffPeople);

                Label TotaldiffPercentLabel = new Label();
                double TotaldiffPercentage = 0;
                if (TotalfiveWeekLastYearVal != 0)
                {
                    TotaldiffPercentage = ((double)(Math.Round(TotalfiveWeekThisYearVal) - Math.Round(TotalfiveWeekLastYearVal)) / Math.Round(TotalfiveWeekLastYearVal)) * 100;
                }
                TotaldiffPercentLabel.Text = String.Format("{0}%", Math.Round(TotaldiffPercentage));
                totalTcDiffPercent.Controls.Add(TotaldiffPercentLabel);
                totalTcDiffPercent.AddCssClass("diffPercent");
                SetBackgroundColorByAboveOrBelowZero(totalTcDiffPercent, TotaldiffPercentage);
                totalTr.Cells.Add(totalTcDiffPercent);
                totalTr.BackColor = System.Drawing.Color.Yellow;
                table.Rows.Add(totalTr);


            }
            //loop through the serving metrics
            foreach (MetricCategoriesFieldAttribute.MetricCategoryPair metricPair in servingMetricsFromSettings)
            {
                //loop starts here for each metric in the table
                //grab metric to spin through and print out header
                Metric metric = metricService.Get(metricPair.MetricGuid);



                TableHeaderRow tableHeaderRow = new TableHeaderRow();
                TableHeaderCell thDate = new TableHeaderCell();
                TableHeaderCell thMetricName = new TableHeaderCell();
                TableHeaderCell thGrowth = new TableHeaderCell();
                TableHeaderCell thFiveWeekLook = new TableHeaderCell();
                TableHeaderCell thLastFiveDiff = new TableHeaderCell();
                thDate.Text = String.Format("{0}", sunday.ToShortDateString());
                thDate.CssClass = "col-xs-2";
                //these may need to be dynamic by all campus or specific campus. for now defaulted to specific campus
                thDate.ColumnSpan = 1;
                thMetricName.Text = String.Format("{0}", metric.Title);
                thMetricName.CssClass = "col-xs-2";
                thMetricName.ColumnSpan = 3;
                thGrowth.Text = "Growth";
                thGrowth.CssClass = "col-xs-2";
                thGrowth.ColumnSpan = 1;
                thFiveWeekLook.Text = "5 Week Look (Last 5)";
                thFiveWeekLook.CssClass = "col-xs-2";
                thFiveWeekLook.ColumnSpan = 2;
                thLastFiveDiff.Text = "Last 5 Difference";
                thLastFiveDiff.CssClass = "col-xs-2";
                thLastFiveDiff.ColumnSpan = 2;
                tableHeaderRow.Cells.Add(thDate);
                tableHeaderRow.Cells.Add(thMetricName);
                tableHeaderRow.Cells.Add(thGrowth);
                tableHeaderRow.Cells.Add(thFiveWeekLook);
                tableHeaderRow.Cells.Add(thLastFiveDiff);
                table.Rows.Add(tableHeaderRow);

                TableHeaderRow tableSubHeaderRow = new TableHeaderRow();
                TableHeaderCell thServiceTime = new TableHeaderCell();
                TableHeaderCell thThisWeekAtt = new TableHeaderCell();
                TableHeaderCell thLastWeekAtt = new TableHeaderCell();
                TableHeaderCell thTwoWeekAgoAtt = new TableHeaderCell();
                TableHeaderCell thWeekToWeek = new TableHeaderCell();
                TableHeaderCell thFiveWeekThisYear = new TableHeaderCell();
                TableHeaderCell thFiveWeekLastYear = new TableHeaderCell();
                TableHeaderCell thLastFivePeople = new TableHeaderCell();
                TableHeaderCell thLastFivePercent = new TableHeaderCell();

                thServiceTime.Text = "Service Time";
                thServiceTime.CssClass = "col-xs-1";
                thServiceTime.ColumnSpan = 1;

                thThisWeekAtt.Text = "This Week";
                thThisWeekAtt.CssClass = "col-xs-1";
                thThisWeekAtt.ColumnSpan = 1;

                thLastWeekAtt.Text = "Last Week";
                thLastWeekAtt.CssClass = "col-xs-1";
                thLastWeekAtt.ColumnSpan = 1;

                thTwoWeekAgoAtt.Text = "2 Weeks Ago";
                thTwoWeekAgoAtt.CssClass = "col-xs-1";
                thTwoWeekAgoAtt.ColumnSpan = 1;

                thWeekToWeek.Text = "Week to Week %";
                thWeekToWeek.CssClass = "col-xs-1";
                thWeekToWeek.ColumnSpan = 1;

                thFiveWeekThisYear.Text = String.Format("{0}", sunday.Year);
                thFiveWeekThisYear.CssClass = "col-xs-1";
                thFiveWeekThisYear.ColumnSpan = 1;

                thFiveWeekLastYear.Text = String.Format("{0}", sundayLastYear.Year);
                thFiveWeekLastYear.CssClass = "col-xs-1";
                thFiveWeekLastYear.ColumnSpan = 1;

                thLastFivePeople.Text = "People";
                thLastFivePeople.CssClass = "col-xs-1";
                thLastFivePeople.ColumnSpan = 1;

                thLastFivePercent.Text = "Growth Rate %";
                thLastFivePercent.CssClass = "col-xs-1";
                thLastFivePercent.ColumnSpan = 1;


                tableSubHeaderRow.Cells.Add(thServiceTime);
                tableSubHeaderRow.Cells.Add(thThisWeekAtt);
                tableSubHeaderRow.Cells.Add(thLastWeekAtt);
                tableSubHeaderRow.Cells.Add(thTwoWeekAgoAtt);
                tableSubHeaderRow.Cells.Add(thWeekToWeek);
                tableSubHeaderRow.Cells.Add(thFiveWeekThisYear);
                tableSubHeaderRow.Cells.Add(thFiveWeekLastYear);
                tableSubHeaderRow.Cells.Add(thLastFivePeople);
                tableSubHeaderRow.Cells.Add(thLastFivePercent);
                table.Rows.Add(tableSubHeaderRow);



                //total variables for this metric
                int totalThisWeekVal = 0;
                int totalLastWeekVal = 0;
                int totalTwoWeeksVal = 0;
                int totalThreeWeeksVal = 0;
                int totalFourWeeksVal = 0;

                int totalThisWeekLastYearVal = 0;
                int totalLastWeekLastYearVal = 0;
                int totalTwoWeeksLastYearVal = 0;
                int totalThreeWeeksLastYearVal = 0;
                int totalFourWeeksLastYearVal = 0;



                //now add schedule rows and print out actual metric values and calculations
                foreach (Schedule schedule in schedulesByCampus)
                {

                    try
                    {

                        schedule.LoadAttributes();
                        Rock.Web.Cache.AttributeValueCache value;
                        if (schedule.AttributeValues.ContainsKey("ServingTeamorNormalSchedule"))
                        {
                            schedule.AttributeValues.TryGetValue("ServingTeamorNormalSchedule", out value);
                            if (value.Value != "Serving Team")
                            {
                                continue;
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        maCampusScheduleWarning.Show("There was an issue getting the serving team or normal schedule attribute associated with the \"" + schedule + "\" schedule." +
                            " Be sure the schedules have a ServingTeamorNormalSchedule entity correctly setup. The attribute must be a Single-Select with two values. Normal and Serving Team, with the default set to Normal.", ModalAlertType.Warning);
                        return;
                    }
                    TableRow tr = new TableRow();

                    int thisWeekVal = 0;
                    int lastWeekVal = 0;
                    int twoWeeksVal = 0;
                    int threeWeeksVal = 0;
                    int fourWeeksVal = 0;

                    int thisWeekLastYearVal = 0;
                    int lastWeekLastYearVal = 0;
                    int twoWeeksLastYearVal = 0;
                    int threeWeeksLastYearVal = 0;
                    int fourWeeksLastYearVal = 0;

                    int numOfMeetingsInPastFive = 0;
                    int numOfMeetingsInPastFiveYearAgo = 0;

                    foreach (MetricValue mv in metricValues)
                    {
                        //grab service time for this metric value
                        try
                        {

                            string scheduleTypePartitionName = GetAttributeValue("NameOfServiceTypePartition");
                            MetricValuePartition mp = mv.MetricValuePartitions.Single(mvp => mvp.MetricPartition.Label == scheduleTypePartitionName);
                            //if it matches the schedule we are looping through then print the row
                            if (schedule.Id == mp.EntityId && metric.Id == mv.MetricId)
                            {
                                if (mv.MetricValueDateTime == sunday)
                                {
                                    thisWeekVal = Convert.ToInt32(mv.YValue);
                                    totalThisWeekVal += thisWeekVal;
                                    numOfMeetingsInPastFive++;
                                }
                                if (mv.MetricValueDateTime == weekAgoSunday)
                                {
                                    lastWeekVal = Convert.ToInt32(mv.YValue);
                                    totalLastWeekVal += lastWeekVal;
                                    numOfMeetingsInPastFive++;
                                }
                                if (mv.MetricValueDateTime == twoWeekAgoSunday)
                                {
                                    twoWeeksVal = Convert.ToInt32(mv.YValue);
                                    totalTwoWeeksVal += twoWeeksVal;
                                    numOfMeetingsInPastFive++;
                                }
                                if (mv.MetricValueDateTime == threeWeekAgoSunday)
                                {
                                    threeWeeksVal = Convert.ToInt32(mv.YValue);
                                    totalThreeWeeksVal += threeWeeksVal;
                                    numOfMeetingsInPastFive++;
                                }
                                if (mv.MetricValueDateTime == fourWeekAgoSunday)
                                {
                                    fourWeeksVal = Convert.ToInt32(mv.YValue);
                                    totalFourWeeksVal += fourWeeksVal;
                                    numOfMeetingsInPastFive++;
                                }

                                if (mv.MetricValueDateTime == sundayLastYear)
                                {
                                    thisWeekLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalThisWeekLastYearVal += thisWeekLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == weekAgoLastYear)
                                {
                                    lastWeekLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalLastWeekLastYearVal += lastWeekLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == twoWeeksAgoLastYear)
                                {
                                    twoWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalTwoWeeksLastYearVal += twoWeeksLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == threeWeeksAgoLastYear)
                                {
                                    threeWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalThreeWeeksLastYearVal += threeWeeksLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }
                                if (mv.MetricValueDateTime == fourWeeksAgoLastYear)
                                {
                                    fourWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                                    totalFourWeeksLastYearVal += fourWeeksLastYearVal;
                                    numOfMeetingsInPastFiveYearAgo++;
                                }



                            }

                        }
                        catch (Exception e)
                        {
                            maCampusScheduleWarning.Show("There was an issue accessing the schedule/service partitions. Be sure to set a value in the block settings and confirm your metrics have schedule/service partitions<br>" + e.StackTrace, ModalAlertType.Warning);
                            return;
                        }


                    }
                    TableCell tcServiceTime = new TableCell();
                    TableCell tcThisWeek = new TableCell();
                    TableCell tcLastWeek = new TableCell();
                    TableCell tcTwoWeeks = new TableCell();
                    TableCell tcGrowth = new TableCell();
                    TableCell tcFiveWeekThisYear = new TableCell();
                    TableCell tcFiveWeekLastYear = new TableCell();
                    TableCell tcDiffPeople = new TableCell();
                    TableCell tcDiffPercent = new TableCell();

                    Label serviceTimeLabel = new Label();
                    serviceTimeLabel.Text = String.Format("{0}", schedule.Name);
                    tcServiceTime.Controls.Add(serviceTimeLabel);
                    tr.Cells.Add(tcServiceTime);

                    Label thisWeekLabel = new Label();
                    thisWeekLabel.Text = String.Format("{0}", thisWeekVal);
                    tcThisWeek.Controls.Add(thisWeekLabel);
                    tr.Cells.Add(tcThisWeek);

                    Label lastWeekLabel = new Label();
                    lastWeekLabel.Text = String.Format("{0}", lastWeekVal);
                    tcLastWeek.Controls.Add(lastWeekLabel);
                    tr.Cells.Add(tcLastWeek);

                    Label twoWeeksLabel = new Label();
                    twoWeeksLabel.Text = String.Format("{0}", twoWeeksVal);
                    tcTwoWeeks.Controls.Add(twoWeeksLabel);
                    tr.Cells.Add(tcTwoWeeks);

                    Label growthLabel = new Label();
                    double growthPercentage = 0;
                    if (lastWeekVal != 0)
                    {
                        growthPercentage = ((double)(thisWeekVal - lastWeekVal) / lastWeekVal) * 100;
                    }
                    growthLabel.Text = String.Format("{0}%", Math.Round(growthPercentage)); // =((C6-D6)/D6*1)
                    tcGrowth.Controls.Add(growthLabel);
                    SetBackgroundColorByAboveOrBelowZero(tcGrowth, growthPercentage);
                    tr.Cells.Add(tcGrowth);

                    Label fiveWeekThisYearLabel = new Label();
                    double fiveWeekThisYearVal = 0;
                    try
                    {
                        if (numOfMeetingsInPastFive != 0)
                        {
                            fiveWeekThisYearVal = ((double)(thisWeekVal + lastWeekVal + twoWeeksVal + threeWeeksVal + fourWeeksVal) / numOfMeetingsInPastFive);
                        }
                    }
                    catch (Exception e)
                    {
                        maCampusScheduleWarning.Show("Division By 0 during calculation of the 5 Week Look of this year for " + metric.Title + ". Be sure there is at least one metric value entered in the past 5 weeks", ModalAlertType.Alert);
                    }
                    fiveWeekThisYearLabel.Text = String.Format("{0}", Math.Round(fiveWeekThisYearVal)); // average of last 5 weeks together
                    tcFiveWeekThisYear.Controls.Add(fiveWeekThisYearLabel);
                    SetBackgroundColorByAboveOrBelowZero(tcFiveWeekThisYear, fiveWeekThisYearVal);
                    tr.Cells.Add(tcFiveWeekThisYear);

                    Label fiveWeekLastYearLabel = new Label();
                    double fiveWeekLastYearVal = 0;

                    try
                    {
                        if (numOfMeetingsInPastFiveYearAgo != 0)
                        {
                            fiveWeekLastYearVal = ((double)(thisWeekLastYearVal + lastWeekLastYearVal + twoWeeksLastYearVal + threeWeeksLastYearVal + fourWeeksLastYearVal) / numOfMeetingsInPastFiveYearAgo);
                        }
                    }
                    catch (Exception e)
                    {
                        maCampusScheduleWarning.Show("Division By 0 during calculation of the 5 Week Look of last year for " + metric.Title + ". Be sure there is at least one metric value entered last year in this 5 week time frame", ModalAlertType.Alert);
                    }
                    fiveWeekLastYearLabel.Text = String.Format("{0}", Math.Round(fiveWeekLastYearVal)); // average of last 5 weeks last year together
                    tcFiveWeekLastYear.Controls.Add(fiveWeekLastYearLabel);
                    SetBackgroundColorByAboveOrBelowZero(tcFiveWeekLastYear, fiveWeekLastYearVal);
                    tr.Cells.Add(tcFiveWeekLastYear);

                    Label diffPeopleLabel = new Label();
                    diffPeopleLabel.Text = String.Format("{0}", (Math.Round(fiveWeekThisYearVal) - Math.Round(fiveWeekLastYearVal)));
                    tcDiffPeople.Controls.Add(diffPeopleLabel);
                    tcDiffPeople.AddCssClass("diffPeople");
                    SetBackgroundColorByAboveOrBelowZero(tcDiffPeople, (Math.Round(fiveWeekThisYearVal) - Math.Round(fiveWeekLastYearVal)));
                    tr.Cells.Add(tcDiffPeople);

                    Label diffPercentLabel = new Label();
                    double diffPercentage = 0;
                    if (fiveWeekLastYearVal != 0)
                    {
                        diffPercentage = ((double)(Math.Round(fiveWeekThisYearVal) - Math.Round(fiveWeekLastYearVal)) / Math.Round(fiveWeekLastYearVal)) * 100;
                    }
                    diffPercentLabel.Text = String.Format("{0}%", Math.Round(diffPercentage));
                    tcDiffPercent.Controls.Add(diffPercentLabel);
                    tcDiffPercent.AddCssClass("diffPercent");
                    SetBackgroundColorByAboveOrBelowZero(tcDiffPercent, diffPercentage);
                    tr.Cells.Add(tcDiffPercent);

                    table.Rows.Add(tr);
                }


                TableRow percentServingTr = new TableRow();
                //now add another row for church serving percentage
                TableCell tcPercentServing = new TableCell();
                TableCell tcPercentServingThisWeek = new TableCell();
                TableCell tcPercentServingLastWeek = new TableCell();
                TableCell tcPercentServingTwoWeeksAgo = new TableCell();
                TableCell tcEmptyCell = new TableCell();

                Label tcPercentServingLabel = new Label();
                tcPercentServingLabel.Text = "Percentage of the church Serving";
                tcPercentServing.Controls.Add(tcPercentServingLabel);
                percentServingTr.Cells.Add(tcPercentServing);

                Label tcPercentServingThisWeekLabel = new Label();
                double percentOfServingThisWeek = 0;
                if (thisWeekAttendanceNum != 0)
                {
                    percentOfServingThisWeek = ((double)totalThisWeekVal / thisWeekAttendanceNum) * 100;
                }
                tcPercentServingThisWeekLabel.Text = String.Format("{0}%", Math.Round(percentOfServingThisWeek));
                tcPercentServingThisWeek.Controls.Add(tcPercentServingThisWeekLabel);
                tcPercentServingThisWeek.AddCssClass("diffPercent");
                SetBackgroundColorByAboveOrBelowZero(tcPercentServingThisWeek, percentOfServingThisWeek);
                percentServingTr.Cells.Add(tcPercentServingThisWeek);

                Label tcPercentServingLastWeekLabel = new Label();
                double percentOfServingLastWeek = 0;
                if (lastWeekAttendanceNum != 0)
                {
                    percentOfServingLastWeek = ((double)totalLastWeekVal / lastWeekAttendanceNum) * 100;
                }
                tcPercentServingLastWeekLabel.Text = String.Format("{0}%", Math.Round(percentOfServingLastWeek));
                tcPercentServingLastWeek.Controls.Add(tcPercentServingLastWeekLabel);
                tcPercentServingLastWeek.AddCssClass("diffPercent");
                SetBackgroundColorByAboveOrBelowZero(tcPercentServingLastWeek, percentOfServingLastWeek);
                percentServingTr.Cells.Add(tcPercentServingLastWeek);

                Label tcPercentServingtwoWeeksAgoLabel = new Label();
                double percentOfServingTwoWeeksAgo = 0;
                if (twoWeeksAgoAttendanceNum != 0)
                {
                    percentOfServingTwoWeeksAgo = ((double)totalTwoWeeksVal / twoWeeksAgoAttendanceNum) * 100;
                }
                tcPercentServingtwoWeeksAgoLabel.Text = String.Format("{0}%", Math.Round(percentOfServingTwoWeeksAgo));
                tcPercentServingTwoWeeksAgo.Controls.Add(tcPercentServingtwoWeeksAgoLabel);
                tcPercentServingTwoWeeksAgo.AddCssClass("diffPercent");
                SetBackgroundColorByAboveOrBelowZero(tcPercentServingTwoWeeksAgo, percentOfServingTwoWeeksAgo);
                percentServingTr.Cells.Add(tcPercentServingTwoWeeksAgo);

                percentServingTr.BackColor = System.Drawing.Color.Yellow;
                table.Rows.Add(percentServingTr);

                TableRow totalTr = new TableRow();
                //now add another row for totals
                TableCell totalTcServiceTime = new TableCell();
                TableCell totalTcThisWeek = new TableCell();
                TableCell totalTcLastWeek = new TableCell();
                TableCell totalTcTwoWeeks = new TableCell();
                TableCell totalTcGrowth = new TableCell();
                TableCell totalTcFiveWeekThisYear = new TableCell();
                TableCell totalTcFiveWeekLastYear = new TableCell();
                TableCell totalTcDiffPeople = new TableCell();
                TableCell totalTcDiffPercent = new TableCell();

                Label TotalserviceTimeLabel = new Label();
                TotalserviceTimeLabel.Text = "Total";
                totalTcServiceTime.Controls.Add(TotalserviceTimeLabel);
                totalTr.Cells.Add(totalTcServiceTime);

                Label TotalthisWeekLabel = new Label();
                TotalthisWeekLabel.Text = String.Format("{0}", totalThisWeekVal);
                totalTcThisWeek.Controls.Add(TotalthisWeekLabel);
                totalTr.Cells.Add(totalTcThisWeek);

                Label TotallastWeekLabel = new Label();
                TotallastWeekLabel.Text = String.Format("{0}", totalLastWeekVal);
                totalTcLastWeek.Controls.Add(TotallastWeekLabel);
                totalTr.Cells.Add(totalTcLastWeek);

                Label TotaltwoWeeksLabel = new Label();
                TotaltwoWeeksLabel.Text = String.Format("{0}", totalTwoWeeksVal);
                totalTcTwoWeeks.Controls.Add(TotaltwoWeeksLabel);
                totalTr.Cells.Add(totalTcTwoWeeks);

                Label TotalgrowthLabel = new Label();
                double TotalgrowthPercentage = 0;
                if (totalLastWeekVal != 0)
                {
                    TotalgrowthPercentage = ((double)(totalThisWeekVal - totalLastWeekVal) / totalLastWeekVal) * 100;
                }
                TotalgrowthLabel.Text = String.Format("{0}%", Math.Round(TotalgrowthPercentage)); // =((C6-D6)/D6*1)
                totalTcGrowth.Controls.Add(TotalgrowthLabel);
                SetBackgroundColorByAboveOrBelowZero(totalTcGrowth, TotalgrowthPercentage);
                totalTr.Cells.Add(totalTcGrowth);

                Label TotalfiveWeekThisYearLabel = new Label();
                double TotalfiveWeekThisYearVal = ((double)(totalThisWeekVal + totalLastWeekVal + totalTwoWeeksVal + totalThreeWeeksVal + totalFourWeeksVal) / 5);
                TotalfiveWeekThisYearLabel.Text = String.Format("{0}", Math.Round(TotalfiveWeekThisYearVal)); // average of last 5 weeks together
                totalTcFiveWeekThisYear.Controls.Add(TotalfiveWeekThisYearLabel);
                SetBackgroundColorByAboveOrBelowZero(totalTcFiveWeekThisYear, TotalfiveWeekThisYearVal);
                totalTr.Cells.Add(totalTcFiveWeekThisYear);

                Label TotalfiveWeekLastYearLabel = new Label();
                double TotalfiveWeekLastYearVal = ((double)(totalThisWeekLastYearVal + totalLastWeekLastYearVal + totalTwoWeeksLastYearVal + totalThreeWeeksLastYearVal + totalFourWeeksLastYearVal) / 5);
                TotalfiveWeekLastYearLabel.Text = String.Format("{0}", Math.Round(TotalfiveWeekLastYearVal)); // average of last 5 weeks last year together
                totalTcFiveWeekLastYear.Controls.Add(TotalfiveWeekLastYearLabel);
                SetBackgroundColorByAboveOrBelowZero(totalTcFiveWeekLastYear, TotalfiveWeekLastYearVal);
                totalTr.Cells.Add(totalTcFiveWeekLastYear);

                Label TotaldiffPeopleLabel = new Label();
                TotaldiffPeopleLabel.Text = String.Format("{0}", (Math.Round(TotalfiveWeekThisYearVal) - Math.Round(TotalfiveWeekLastYearVal)));
                totalTcDiffPeople.Controls.Add(TotaldiffPeopleLabel);
                totalTcDiffPeople.AddCssClass("diffPeople");
                SetBackgroundColorByAboveOrBelowZero(totalTcDiffPeople, (Math.Round(TotalfiveWeekThisYearVal) - Math.Round(TotalfiveWeekLastYearVal)));
                totalTr.Cells.Add(totalTcDiffPeople);

                Label TotaldiffPercentLabel = new Label();
                double TotaldiffPercentage = 0;
                if (TotalfiveWeekLastYearVal != 0)
                {
                    TotaldiffPercentage = ((double)(Math.Round(TotalfiveWeekThisYearVal) - Math.Round(TotalfiveWeekLastYearVal)) / Math.Round(TotalfiveWeekLastYearVal)) * 100;
                }
                TotaldiffPercentLabel.Text = String.Format("{0}%", Math.Round(TotaldiffPercentage));
                totalTcDiffPercent.Controls.Add(TotaldiffPercentLabel);
                totalTcDiffPercent.AddCssClass("diffPercent");
                SetBackgroundColorByAboveOrBelowZero(totalTcDiffPercent, TotaldiffPercentage);
                totalTr.Cells.Add(totalTcDiffPercent);

                totalTr.BackColor = System.Drawing.Color.Yellow;
                table.Rows.Add(totalTr);


            }
            reportPanel.Controls.Add(table);
            reportPanel.CssClass = "col-xs-12";
            mainDiv.Controls.Add(reportPanel);


        }

        private static void SetBackgroundColorByAboveOrBelowZero(TableCell incomingTableCell, double incomingDouble)
        {
            if (incomingDouble > 0)
            {
                incomingTableCell.BackColor = System.Drawing.Color.FromArgb(198, 239, 206);
            }
            else
            {
                incomingTableCell.BackColor = System.Drawing.Color.FromArgb(255, 199, 206);
            }
        }
    }
}