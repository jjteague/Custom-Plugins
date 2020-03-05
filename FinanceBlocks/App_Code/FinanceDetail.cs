using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for FinanceDetail
/// </summary>
public class FinanceDetail: Rock.Web.UI.RockBlock
{

    public virtual void FilterData(string startDate, string endDate, int isTaxDeductable, string accountList) { }
}