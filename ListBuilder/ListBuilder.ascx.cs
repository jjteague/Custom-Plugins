using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Plugins.com_DTS.Misc
{
    [LinkedPage("Related Page")]
    [DisplayName( "List Builder" )]
    [Category( "DTS > List Builder" )]
    [Description( "Allows you to import a list of known Id's in Rock to communicate or make new groups" )]
	[BooleanField( "Show Communicate", "Show Communicate button in grid footer?", true, "Grid Actions", order: 1 )]
    [BooleanField( "Show Merge Person", "Show Merge Person button in grid footer?", true, "Grid Actions", order: 2 )]
    [BooleanField( "Show Bulk Update", "Show Bulk Update button in grid footer?", true, "Grid Actions", order: 3 )]
    [BooleanField( "Show Excel Export", "Show Export to Excel button in grid footer?", true, "Grid Actions", order: 4 )]
    [BooleanField( "Show Merge Template", "Show Export to Merge Template button in grid footer?", true, "Grid Actions", order: 5 )]
	
	[ContextAware]
	public partial class ListBuilder : RockBlock
	{
		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			if ( !Page.IsPostBack )
			{
				BindGrid();
			}
            //gdPerson.Actions.BulkUpdateClick += Actions_BulkUpdateClick;
            gdPerson.GridRebind += GdPerson_GridRebind;
            //gdPerson.Actions.CommunicateClick += Actions_CommunicateClick;
        }

        private void GdPerson_GridRebind(object sender, GridRebindEventArgs e)
        {
            btnSubmit_Click(sender, null);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
			{
				if (comment.InnerText != string.Empty && comment.InnerText.Length > 0)
				{
					var ids = comment.InnerHtml.Split(',').Where(x => x.Trim() != string.Empty).Distinct().Select(s => int.Parse(s)).ToList();
					ids = ids.Distinct().ToList();
					var rockContext = new RockContext();
					var persons = rockContext.PersonAliases.Select(x => x.Person).Where(t => ids.Contains(t.Id)).Distinct().ToList();
                    gdPerson.DataKeyNames = new String[] { "Id"};

                    gdPerson.Actions.ShowCommunicate = GetAttributeValue( "ShowCommunicate" ).AsBoolean();
					gdPerson.Actions.ShowMergePerson = GetAttributeValue( "ShowMergePerson" ).AsBoolean();
					gdPerson.Actions.ShowBulkUpdate = GetAttributeValue( "ShowBulkUpdate" ).AsBoolean();
                


                    gdPerson.DataSource = persons;
					gdPerson.DataBind();
					///gdPerson.GridRebind += gdPerson_GridRebind; ///Used to add content to exporting
				}
		
			}

        private void Actions_CommunicateClick(object sender, EventArgs e)
        {
            for (int i = 0; i < gdPerson.Rows.Count; i++) {
                //gdPerson.Rows[i].Cells[0].se
            }
            var dict  = new Dictionary<string, string>();
            //NavigateToLinkedPage("RelatedPage", "PersonId", (int)e.RowKeyValues["Id"]);

        }

        private void Actions_BulkUpdateClick(object sender, EventArgs e)
        {
            NavigateToLinkedPage("RelatedPage", "Id", 1);
        }

        protected void OnRowBound(object sender, GridViewRowEventArgs e)
			{
				if (e.Row.RowType == DataControlRowType.DataRow)
				{
					var rockContext2 = new RockContext();
					Person id = (Person)e.Row.DataItem;
					Label lbl = (e.Row.FindControl("lblMobileNum") as Label);
					lbl.Text = id.PhoneNumbers.Where(x => x.NumberTypeValueId == 12).Select(x => x.NumberFormatted).FirstOrDefault();
				}
			}

			
			protected void gList_RowSelected( object sender, RowEventArgs e )
			{
					int personId = (int)e.RowKeyValue;
					Response.Redirect( string.Format( "~/Person/{0}", personId ), false);
			}
			protected void BindGrid( bool isExporting = false )
				{
					var ids = comment.InnerHtml.Split(',').Where(x => x.Trim() != string.Empty).Distinct().Select(s => int.Parse(s)).ToList();
					ids = ids.Distinct().ToList();
					var rockContext = new RockContext();
					var persons = rockContext.PersonAliases.Select(x => x.Person).Where(t => ids.Contains(t.Id)).Distinct().ToList();            
                    gdPerson.DataSource = persons;
					gdPerson.DataBind();
				}



	}
}