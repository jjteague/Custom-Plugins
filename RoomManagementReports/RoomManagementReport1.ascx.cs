/// Author: Ethan Widen
/// Email: ethan@dtschurch.com
/// Created Date: 5/15/2019

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using com.centralaz.RoomManagement.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

[DisplayName("Room Management Report Per Location")]
[Category("com_DTS > Room Management Reports")]
[Description("Room Management Report To Print Each Location On Seperate Pages")]
public partial class RoomManagementReport1 : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            
        }

    }



    protected void btn_submit_Click( object sender, EventArgs e )
    {
        BindGrid();
    }

    private void BindGrid()
    {
        var rockContext = new RockContext();
        var reservationService = new ReservationService( rockContext );
        var qry = reservationService.Queryable();
        var locationsService = new LocationService( rockContext ).Queryable().AsNoTracking();
        DateTime lowerDate = dateRangePicker.LowerValue ?? DateTime.Now;
        DateTime upperDate = dateRangePicker.UpperValue ?? DateTime.Now;
        DateTime now = DateTime.Now;
        var allLocations = locationsService.ToList().Where( l => l.IsNamedLocation == true && l.CampusId == 1);

        var reservationSummaryList = reservationService.GetReservationSummaries( qry, lowerDate, upperDate, false );

        foreach (Location location in allLocations)
        {
            int numOfResInLocation = reservationSummaryList.Where( r => r.ReservationLocations.Where( rl => rl.LocationId == location.Id ).Count() > 0 ).Count();

            if (numOfResInLocation > 0)
            {

                Panel locationPanel = new Panel();
                Label locationLabel = new Label();
                locationLabel.Text = String.Format( "Welcome to {0}", location.Name );
                locationPanel.CssClass = "col-xs-12 location-header";
                locationPanel.Controls.Add( locationLabel );
                mainDiv.Controls.Add( locationPanel );

                Label infoLabel = new Label();
                Panel infoPanel = new Panel();
                infoLabel.Text = "This week's activities:";
                infoPanel.CssClass = "col-xs-12 info-header";
                infoPanel.Controls.Add( infoLabel );
                mainDiv.Controls.Add( infoPanel );

                foreach (DateTime day in EachDay( lowerDate, upperDate ))
                {

                    int numOfRes = reservationSummaryList.Where( r => r.ReservationStartDateTime.Date == day.Date && r.ReservationLocations.Where( rl => rl.LocationId == location.Id ).Count() > 0 ).Count();
                    if (numOfRes > 0)
                    {
                        Label dayLabel = new Label();
                        Panel dayPanel = new Panel();
                        dayLabel.Text = String.Format( "{0}", day.ToShortDateString() );
                        dayPanel.Controls.Add( dayLabel );
                        dayPanel.CssClass = "col-xs-12 day-header";
                        mainDiv.Controls.Add( dayPanel );


                        Panel resDayPanel = new Panel();
                        Table table = new Table();
                        table.CssClass = "table table-borderless table-sm";
                        TableHeaderRow tableHeaderRow = new TableHeaderRow();
                        TableHeaderCell thName = new TableHeaderCell();
                        TableHeaderCell thResStart = new TableHeaderCell();
                        TableHeaderCell thResEnd = new TableHeaderCell();
                        TableHeaderCell thEventStart = new TableHeaderCell();
                        TableHeaderCell thEventEnd = new TableHeaderCell();
                        thName.Text = "";
						thName.CssClass = "col-xs-4";
                        thResStart.Text = "START";
						thResStart.CssClass = "col-xs-2";
                        thResEnd.Text = "END";
						thResEnd.CssClass = "col-xs-2";
                        thEventStart.Text = "";
						thEventStart.CssClass = "col-xs-2";
                        thEventEnd.Text = "";
						thEventEnd.CssClass = "col-xs-2";
                        tableHeaderRow.Cells.Add( thName );
                        tableHeaderRow.Cells.Add( thResStart );
                        tableHeaderRow.Cells.Add( thResEnd );
                        tableHeaderRow.Cells.Add( thEventStart );
                        tableHeaderRow.Cells.Add( thEventEnd );
                        table.Rows.Add( tableHeaderRow );

                        foreach (ReservationService.ReservationSummary reservation in reservationSummaryList)
                        {

                            if (reservation.ReservationStartDateTime.Date == day.Date && reservation.ReservationLocations.Where( rl => rl.LocationId == location.Id ).Count() > 0)
                            {


                                TableRow trReservation = new TableRow();
                                TableCell resName = new TableCell();
                                TableCell resStart = new TableCell();
                                TableCell resEnd = new TableCell();
                                TableCell setupStart = new TableCell();
                                TableCell setupEnd = new TableCell();


                                Label reservationNameLabel = new Label();
                                reservationNameLabel.Text = String.Format( "{0}", reservation.ReservationName );
                                resName.Controls.Add( reservationNameLabel );
                                trReservation.Cells.Add( resName );

                                Label reservationStartLabel = new Label();
                                reservationStartLabel.Text = String.Format( "{0}", reservation.EventStartDateTime.ToShortTimeString() );
                                resStart.Controls.Add( reservationStartLabel );
                                trReservation.Cells.Add( resStart );

                                Label reservationEndLabel = new Label();
                                reservationEndLabel.Text = String.Format( "{0}", reservation.EventEndDateTime.ToShortTimeString() );
                                resEnd.Controls.Add( reservationEndLabel );
                                trReservation.Cells.Add( resEnd );

                                Label reservationSetupStartLabel = new Label();
                                reservationSetupStartLabel.Text = String.Format( "{0} set up time", reservation.ReservationStartDateTime.ToShortTimeString() );
                                setupStart.Controls.Add( reservationSetupStartLabel );
                                setupStart.CssClass = "font-italic";
                                trReservation.Cells.Add( setupStart );

                                Label reservationSetupEndLabel = new Label();
                                reservationSetupEndLabel.Text = String.Format( "{0} exit time", reservation.ReservationEndDateTime.ToShortTimeString() );
                                setupEnd.Controls.Add( reservationSetupEndLabel );
                                setupEnd.CssClass = "font-italic";
                                trReservation.Cells.Add( setupEnd );

                                table.Rows.Add( trReservation );
                            }

                        }
                        resDayPanel.Controls.Add( table );
                        resDayPanel.CssClass = "col-xs-12";
                        mainDiv.Controls.Add( resDayPanel );
                    }


                }

                Panel dividerDiv = new Panel();
                dividerDiv.Style.Add( "page-break-after", "always;padding-bottom:50px;" );
                mainDiv.Controls.Add( dividerDiv );
            }
        }
    }

    public IEnumerable<DateTime> EachDay( DateTime from, DateTime thru )
    {
        for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays( 1 ))
            yield return day;
    }
}