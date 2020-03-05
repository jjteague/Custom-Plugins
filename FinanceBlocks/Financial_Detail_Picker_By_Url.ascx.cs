using Rock.Model;
using Rock.Web.UI;
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

[DisplayName("Finance Detail Picker By Url")]
[Category("DTS > Financial")]
[Description("Block to filter by start and end date and financial accounts")]

public partial class Financial_Detail_Picker_By_Url : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ddlTaxDeductable.ClientIDMode = ClientIDMode.Static;
        hdDateDayDiff.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        DateStartDate.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        DateEndDate.ClientIDMode = System.Web.UI.ClientIDMode.Static;
        apAccount.ClientIDMode = ClientIDMode.Static;
        defValuePicker.ClientIDMode = ClientIDMode.Static;
        //string isPB = Request.QueryString["isPB"];
        string startDate = Request.QueryString["startDate"];
        if (!string.IsNullOrEmpty(startDate))
        {
            DateStartDate.Value = startDate;
            DateEndDate.Value = Request.QueryString["endDate"];
            string isTax = Request.QueryString["IsTax"];
            if (!string.IsNullOrEmpty(isTax))
                ddlTaxDeductable.SelectedValue = isTax;
            apAccount.SetValues(GetSelectedAccountIds());
            SelectCurrency();
        }
        else
            Init();
        this.AddConfigurationUpdateTrigger(upDatePicker);
    }


    private void Init()
    {
        DateTime dt = GetWeekMonday();
        DateStartDate.Value = dt.ToString("MM-dd-yyyy");
        DateEndDate.Value = DateTime.Now.ToString("MM-dd-yyyy");
        Rock.Model.DefinedValueService valueService = new DefinedValueService(new Rock.Data.RockContext());
        var list = valueService.GetByDefinedTypeId(10).ToList();
        for (int i = 0; i < defValuePicker.Items.Count; i++)
        {
            defValuePicker.Items[i].Selected = true;
        }
    }

    private void SelectCurrency()
    {
        string cType = Request.QueryString["CurrencyType"];                      
        try
        {
            if (!string.IsNullOrEmpty(cType))
            {
                string[] strIds = cType.Split(new char[] { ',' });
                for (int i = 0; i < strIds.Length; i++)
                {
                    for (int j = 0; j < defValuePicker.Items.Count; j++)
                    {
                        if (defValuePicker.Items[j].Value == strIds[i])
                        {
                            defValuePicker.Items[j].Selected = true;
                            break;
                        }
                    }
                }
            }
        }
        catch { }
    }

    private int[] GetSelectedAccountIds()
    {
        string accountIds = Request.QueryString["AccountList"];
        List<int> listId = new List<int>();
        try
        {
            if (!string.IsNullOrEmpty(accountIds))
            {
                string[] strIds = accountIds.Split(new char[] { ',' });
                for (int i = 0; i < strIds.Length; i++)
                {
                    listId.Add(Convert.ToInt32(strIds[i]));
                }
            }
        }
        catch { }

        return listId.ToArray();
    }

    private DateTime GetWeekMonday()
    {
        DateTime dt = DateTime.Now;
        switch (dt.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                return dt.AddDays(-6);
            default:
                return dt.AddDays(-((int)dt.DayOfWeek - 1));
        }
    }
}