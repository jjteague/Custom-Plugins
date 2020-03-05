using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using System.Data;

namespace Plugins.com_DTS.Misc
{

    public partial class SchedularOccurenceWebService : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [System.Web.Services.WebMethod]
        public static Dictionary<string, string> AddPositionInGroup(string grpId, string position, string desc) // string grpId,string personid
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("IsMember", "false");
            try
            {
                if (HttpContext.Current.Session["RockUserId"] != null)
                {                   
                        Int64 val = GetMaxId("_com_DTS_ServingSchedulerPositions") + 1;
                        string currDate = DateTime.Now.ToString("yyyy'-'MM'-'dd");
                        string sql = "INSERT INTO _com_DTS_ServingSchedulerPositions(Id, Active, GroupId, Value, Description, Guid,CreatedDateTime,ModifiedDateTime,IsVisable) VALUES ";
                        sql += "({0}, '{1}', '{2}','{3}','{4}','{5}', '{6}', '{7}', '{8}')";
                        sql = string.Format(sql, val, "1", grpId, position, desc, Guid.NewGuid().ToString(), currDate, currDate, 1);
                        DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                        dic.Add("Success", "true");
                        dic.Add("PositionId", Convert.ToString(val));
                        dic.Add("Name", position);                        
                        return dic;
                }
                else
                {
                    dic.Add("Message", "Session expire.");
                }
            }
            catch
            {
                dic.Add("Message", "Error occured.");
            }          
            dic.Add("Success", "false");
            return dic;
        }

        [System.Web.Services.WebMethod]
        public static Dictionary<string, string> AddMemberInGroup(string grpId, string personid, string name, string CurrentPersonAliasId) // string grpId,string personid
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("IsMember", "true");
            try {
                if (HttpContext.Current.Session["RockUserId"] != null)
                {
                    if (!IsExist(grpId, personid)) {
                        //Int64 val = GetMaxId("GroupMember") + 1;
                        CurrentPersonAliasId = string.IsNullOrEmpty(CurrentPersonAliasId) ? "NULL" : CurrentPersonAliasId;
                        string currDate = DateTime.Now.ToString("yyyy'-'MM'-'dd");
                        string sql = "INSERT INTO GroupMember(IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, Guid,CreatedDateTime,ModifiedDateTime,IsNotified, CreatedByPersonAliasId) VALUES ";
                        sql += "({0}, '{1}', '{2}','{3}','{4}','{5}', '{6}', '{7}', '{8}', {9})";
                        sql = string.Format(sql, "0", grpId, personid, "1", "1", Guid.NewGuid().ToString(), currDate, currDate, 0, CurrentPersonAliasId);
                        DbService.ExecuteCommand(sql, CommandType.Text, null, null);
                        
                        dic.Add("Success", "true");
                        Int64 val = GetInsertPersonGroupMemberId(grpId, personid, CurrentPersonAliasId);
                        if (val == -1)
                        {
                            val = GetMaxId("GroupMember") + 1;
                        }
                        dic.Add("MemberId", Convert.ToString(val));
                        dic.Add("PersonBlackoutDates", GetPersonBlackoutDates(personid, grpId));
                        dic.Add("Name", name);
                        dic.Add("Personid", personid);
                        return dic;
                    }

                    dic.Add("Message", "Person already exist in Group.");
                }
                else
                {
                    dic.Add("Message", "Session expire.");
                }
            }
            catch {
                dic.Add("Message", "Error occured.");
            }
            dic.Add("Name", name);
            dic.Add("Success", "false");
            return dic;
        }

        private static Int64 GetInsertPersonGroupMemberId(string GroupId, string PersonId, string AliasId)
        {
            string sql = "select top 1 id From GroupMember where GroupId = {0} And PersonId = {1}  And CreatedByPersonAliasId = {2}";
            AliasId = string.IsNullOrEmpty(AliasId) ? "NULL" : AliasId;
            sql = string.Format(sql, GroupId, PersonId, AliasId);
            DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null, null);
            if (dt != null)
            {
                while (dt.Rows.Count > 0)
                {
                    return Convert.ToInt64(dt.Rows[0][0]);
                }
            }
            return -1;
        }

        private static Int64 GetMaxId(string tableName)
        {
            string sql = "select MAX(id) From " + tableName;
            DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null, null);
            if (dt != null)
            {
                while (dt.Rows.Count > 0)
                {
                    return Convert.ToInt64(dt.Rows[0][0]);
                }
            }
            return -1;
        }

        private static bool IsExist(string grpId, string personid)
        {
            string sql = "select top 1 id From GroupMember where GroupId = '{0}' And PersonId = '{1}' ";
            sql = string.Format(sql, grpId, personid);
            DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null, null);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public const string RowSeperatorKey = "|~|~|";
        public const string MultiRowSeperatorKey = "|~~|~~|";

        private static string GetPersonBlackoutDates(string personidList, string groupId)
        {
            string PersonOnBlackoutVal = "";
            try
            {
                if (string.IsNullOrEmpty(personidList))
                {                 
                    return "";
                }
                
                // personidList            
                string startDate = DateTime.Now.ToString("yyyy-MM-dd");
                string sql = "SELECT StartDateTime,EndDateTime, gm.Id   From _com_DTS_ServingSchedulerBlackOut ss INNER JOIN PersonAlias p On p.AliasPersonId = ss.PersonAliasId And p.PersonId in ({0}) And ( (StartDateTime >= '{1}' OR EndDateTime >= '{1}') OR (StartDateTime <= '{1}' And EndDateTime >= '{1}') ) INNER JOIN GroupMember gm on  gm.GroupId = {2} And gm.PersonId = p.PersonId ORDER By p.PersonId ";
                sql = string.Format(sql, personidList, startDate, groupId);
                DataTable dt = DbService.GetDataTable(sql, CommandType.Text, null);
                string dates = "";                
                string prevPersonId = "";
                int count = dt.Rows.Count - 1;
                for (int i = 0; i <= count; i++)
                {
                    string currPersonId = Convert.ToString(dt.Rows[i][2]);
                    string sDate = Convert.ToString(dt.Rows[i][0]);
                    string endDate = Convert.ToString(dt.Rows[i][1]);
                    if (prevPersonId == "")
                    {
                        dates = currPersonId + ";" + sDate + "::" + endDate;
                    }
                    else if (currPersonId != prevPersonId)
                    {
                        PersonOnBlackoutVal += dates + MultiRowSeperatorKey;
                        dates = currPersonId + ";" + sDate + "::" + endDate;
                    }
                    else
                    {
                        // Prev person and current person ids are equal.
                        dates += RowSeperatorKey + sDate + "::" + endDate;
                        
                    }
                    prevPersonId = currPersonId;
                    if (i == count)
                        PersonOnBlackoutVal += dates;
                }
            }
            catch { }

            return PersonOnBlackoutVal;
        }

    }

}