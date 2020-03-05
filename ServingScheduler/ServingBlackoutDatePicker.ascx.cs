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

[DisplayName("Serving Blackout Report Datepicker")]
[Category("DTS > ServingBlackoutDatePickerFilter")]
[Description("Block to show Serving blackout Report Date Picker.")]

[TextField("Date Picker", "Date Picker", true, "Date Picker", "", 1)]
[IntegerField("Date Differences", "Date Differences",false, 28, "", 6)]
public partial class ServingBlackoutDatePicker : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
       
        hdDateDayDiff.ClientIDMode = System.Web.UI.ClientIDMode.Static;
      
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