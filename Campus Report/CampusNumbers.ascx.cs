/// Author: Ethan Widen
/// Email: ethan@dtschurch.com
/// Created Date: 12/19/2019

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

namespace RockWeb.Plugins.com_DTS.CampusReport
{


    [DisplayName("Campus Numbers")]
    [Category("com_DTS > Metric Reports")]
    [Description("Reporting block that shows attendance and metrics")]
    [SchedulesField("Schedules", "Schedules to report on", true)]
    [MetricCategoriesField("Metrics", "Metrics to report on", true)]
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
            List<DateTime> sundayDates = getSundays();
            foreach(DateTime sunday in sundayDates)
            {
                ddlSundayDates.Items.Add(new ListItem(sunday.ToShortDateString(), sunday.ToShortDateString()));
            }
            

        }

        public static List<DateTime> getSundays()
        {
            List<DateTime> lstSundays = new List<DateTime>();
            DateTime twoMonthsAgo = DateTime.Now.Date.AddMonths(-2);
            DateTime now = DateTime.Now.Date;

            int intDaysPastTwoMonths = (int)(now - twoMonthsAgo).TotalDays;
            
            for (int i = 1; i < intDaysPastTwoMonths + 1; i++)
            {
                DateTime dayToTest = twoMonthsAgo.AddDays(i);
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

        private void BindGrid()
        {
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );
            var scheduleService = new ScheduleService(rockContext);
            var metricService = new MetricService(rockContext);
            var metricValueService = new MetricValueService(rockContext);
            //grab the metrics from the block setting that we want to print out
            List<MetricCategoriesFieldAttribute.MetricCategoryPair> metricsFromSettings = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs(GetAttributeValue("Metrics"));
            var schedulesFromSettings = GetAttributeValue("Schedules").Split(',').ToList();
            //grab the schedules from the block setting so we know how to restrict the metric values when we request them
            List<Schedule> schedules = new List<Schedule>();
            if (schedulesFromSettings != null)
            {
                schedules = scheduleService.Queryable().Where(s => schedulesFromSettings.Contains(s.Guid.ToString())).ToList();
            }

            //grab the sunday date from the dropdown so we know the date to ask for metric values
            DateTime sunday = Convert.ToDateTime(ddlSundayDates.SelectedValue.ToString());
            //and the previous 4 Sundays this year
            DateTime weekAgoSunday = sunday.AddDays(-7);
            DateTime twoWeekAgoSunday = weekAgoSunday.AddDays(-7);
            DateTime threeWeekAgoSunday = twoWeekAgoSunday.AddDays(-7);
            DateTime fourWeekAgoSunday = threeWeekAgoSunday.AddDays(-7);
            //also grab the 5 corresponding Sundays from a year ago
            DateTime sundayLastYear = sunday.AddYears(-1);
            DateTime weekAgoLastYear = sundayLastYear.AddDays(-7);
            DateTime twoWeeksAgoLastYear = weekAgoLastYear.AddDays(-7);
            DateTime threeWeeksAgoLastYear = twoWeeksAgoLastYear.AddDays(-7);
            DateTime fourWeeksAgoLastYear = threeWeeksAgoLastYear.AddDays(-7);
            //grab the campus or All
            string campus = ddlCampus.SelectedValue.ToString();


            //return the metrics we want to produce an HTML table/report for
            List<MetricValue> metricValues = metricValueService.Queryable().Where(mv => mv.MetricValueDateTime == sunday ||
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
            foreach (MetricCategoriesFieldAttribute.MetricCategoryPair metricPair in metricsFromSettings)
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

                //now add schedule rows and print out actual metric values and calculations
                foreach(Schedule schedule in schedules)
                {
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


                    foreach (MetricValue mv in metricValues)
                    {
                        //grab service time for this metric value
                        MetricValuePartition mp = mv.MetricValuePartitions.Single(mvp => mvp.MetricPartition.Label == "Service");
                        //if it matches the schedule we are looping through then print the row
                        if (schedule.Id == mp.EntityId && metric.Id == mv.MetricId)
                        {
                            if (mv.MetricValueDateTime == sunday)
                            {
                                thisWeekVal = Convert.ToInt32( mv.YValue);
                            }
                            if (mv.MetricValueDateTime == weekAgoSunday)
                            {
                                lastWeekVal = Convert.ToInt32(mv.YValue);
                            }
                            if (mv.MetricValueDateTime == twoWeekAgoSunday)
                            {
                                twoWeeksVal = Convert.ToInt32(mv.YValue);
                            }
                            if (mv.MetricValueDateTime == threeWeekAgoSunday)
                            {
                                threeWeeksVal = Convert.ToInt32(mv.YValue);
                            }
                            if (mv.MetricValueDateTime == fourWeekAgoSunday)
                            {
                                fourWeeksVal = Convert.ToInt32(mv.YValue);
                            }

                            if (mv.MetricValueDateTime == sundayLastYear)
                            {
                                thisWeekLastYearVal = Convert.ToInt32(mv.YValue);
                            }
                            if (mv.MetricValueDateTime == weekAgoLastYear)
                            {
                                lastWeekLastYearVal = Convert.ToInt32(mv.YValue);
                            }
                            if (mv.MetricValueDateTime == twoWeeksAgoLastYear)
                            {
                                twoWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                            }
                            if (mv.MetricValueDateTime == threeWeeksAgoLastYear)
                            {
                                threeWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                            }
                            if (mv.MetricValueDateTime == fourWeeksAgoLastYear)
                            {
                                fourWeeksLastYearVal = Convert.ToInt32(mv.YValue);
                            }
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
                    tr.Cells.Add(tcGrowth);

                    Label fiveWeekThisYearLabel = new Label();
                    double fiveWeekThisYearVal = ((double)(thisWeekVal + lastWeekVal + twoWeeksVal + threeWeeksVal + fourWeeksVal) / 5);
                    fiveWeekThisYearLabel.Text = String.Format("{0}", Math.Round(fiveWeekThisYearVal)); // average of last 5 weeks together
                    tcFiveWeekThisYear.Controls.Add(fiveWeekThisYearLabel);
                    tr.Cells.Add(tcFiveWeekThisYear);

                    Label fiveWeekLastYearLabel = new Label();
                    double fiveWeekLastYearVal = ((double)(thisWeekLastYearVal + lastWeekLastYearVal + twoWeeksLastYearVal + threeWeeksLastYearVal + fourWeeksLastYearVal) / 5);
                    fiveWeekLastYearLabel.Text = String.Format("{0}", Math.Round(fiveWeekLastYearVal)); // average of last 5 weeks last year together
                    tcFiveWeekLastYear.Controls.Add(fiveWeekLastYearLabel);
                    tr.Cells.Add(tcFiveWeekLastYear);

                    Label diffPeopleLabel = new Label();
                    diffPeopleLabel.Text = String.Format("{0}", (fiveWeekThisYearVal - fiveWeekLastYearVal));
                    tcDiffPeople.Controls.Add(diffPeopleLabel);
                    tr.Cells.Add(tcDiffPeople);

                    Label diffPercentLabel = new Label();
                    double diffPercentage = 0;
                    if (fiveWeekLastYearVal != 0)
                    {
                        diffPercentage = ((double)(fiveWeekThisYearVal - fiveWeekLastYearVal) / fiveWeekLastYearVal) * 100;
                    }
                    diffPeopleLabel.Text = String.Format("{0}%", Math.Round(diffPercentage));
                    tcDiffPercent.Controls.Add(diffPercentLabel);
                    tr.Cells.Add(tcDiffPercent);
                    table.Rows.Add(tr); 
                }

            }
            reportPanel.Controls.Add(table);
            reportPanel.CssClass = "col-xs-12";
            mainDiv.Controls.Add(reportPanel);


        }

    }
}