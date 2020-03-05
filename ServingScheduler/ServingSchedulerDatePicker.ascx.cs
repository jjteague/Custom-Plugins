/// Author: Amit 
/// Email: toresolveissue@gmail.com
/// Created Date: 13 April 2018.

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

[DisplayName("Serving Report Datepicker")]
[Category("DTS > ServingScheduler")]
[Description("Block to show Serving Report Date Picker.")]

[BooleanField("StatusFilter", "Show status checkbox filter", true, "", 2)]
[BooleanField("OpenPositions", "OpenPositions", true, "", 3)]
[BooleanField("AcceptedPositions", "Accepted Positions", true, "", 4)]
[BooleanField("DeclinedPositions", "Declined Positions", true, "", 5)]
[TextField("Date Picker", "Date Picker", true, "Date Picker", "", 1)]
[IntegerField("Date Differences", "Date Differences",false, 28, "", 6)]
public partial class ServingSchedulerDatePicker : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (GetAttributeValue("StatusFilter").AsBoolean())
        {
            chkStatusGroup.Visible = true;
            h3OptionChkBox.Visible = true;
        }
        else
        {
            chkStatusGroup.Visible = false;
            h3OptionChkBox.Visible = false;
        }
        OpenPositions.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        DeclinedPositions.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        AcceptedPositions.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        hdDateDayDiff.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        string isPB = Request.QueryString["isPB"];
        if (string.IsNullOrEmpty(isPB) && !IsPostBack)
        {
            if (GetAttributeValue("DeclinedPositions").AsBoolean())
            {
                DeclinedPositions.Checked = true;
            }
            
            if (GetAttributeValue("OpenPositions").AsBoolean())
            {
                OpenPositions.Checked = true;
            }
            
            if (GetAttributeValue("AcceptedPositions").AsBoolean())
            {
                AcceptedPositions.Checked = true;
            }            
            
        }
        int dateDiff = GetAttributeValue("DateDifferences").AsInteger();
        hdDateDayDiff.Value = Convert.ToString(dateDiff);
        string title = GetAttributeValue("DatePicker");
        if (!string.IsNullOrEmpty(title))
        {
            lblTitle.InnerText = title;
        }
        this.AddConfigurationUpdateTrigger(upDatePicker);
        //ClientScript.RegisterStartupscript(this.GetType(), "myFunc", "myFunction();", true);
        System.Web.UI.ScriptManager.RegisterStartupScript(this.upDatePicker,this.upDatePicker.GetType(), "RefreshSc", "SetValue('"+ title + "');", true);
    }

    /// <summary>
    /// Route the request to the correct panel
    /// </summary>
    private void RouteAction()
    {
        int weekId = 0;

        if (Request.Form["__EVENTARGUMENT"] != null)
        {
            string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split('^');

            if (eventArgs.Length == 2)
            {
                string action = eventArgs[0];
                string parameters = eventArgs[1];

                int argument = 0;
                int.TryParse(parameters, out argument);

                switch (action)
                {
                    case "CopyOccurrance":
                        weekId = int.Parse(parameters);
                        break;
                    case "EmailAllUnconfirmed":
                        weekId = int.Parse(parameters);
                        break;
                    case "DeleteOccurence":
                        weekId = int.Parse(parameters);
                        break;
                }
            }
        }
    }

}