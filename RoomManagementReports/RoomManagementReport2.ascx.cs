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

[DisplayName("Room Management Report All Building")]
[Category("com_DTS > Room Management Reports")]
[Description("Room Management Report Will Print ALL Building List Combined")]
public partial class RoomManagementReport2 : RockBlock
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

        Label infoLabel = new Label();
        Panel infoPanel = new Panel();
        infoLabel.Text = "WHAT'S HAPPENING AT SB THIS WEEK";
        infoPanel.CssClass = "col-xs-12 info-header";
        infoPanel.Controls.Add( infoLabel );
        mainDiv.Controls.Add( infoPanel );

        foreach (DateTime day in EachDay( lowerDate, upperDate ))
        {

            int numOfRes = reservationSummaryList.Where( r => r.ReservationStartDateTime.Date == day.Date).Count();
            if (numOfRes > 0)
            {
                Label dayLabel = new Label();
                Panel dayPanel = new Panel();
                dayLabel.Text = String.Format( "{0}", day.ToShortDateString() );
                dayPanel.Controls.Add( dayLabel );
                dayPanel.CssClass = "col-xs-2 day-header";
                mainDiv.Controls.Add( dayPanel );


                Panel resDayPanel = new Panel();
                Table table = new Table();
                table.CssClass = "table table-sm";
                TableHeaderRow tableHeaderRow = new TableHeaderRow();
                TableHeaderCell thName = new TableHeaderCell();
                TableHeaderCell thEventStartEnd = new TableHeaderCell();
                TableHeaderCell thEventLocations = new TableHeaderCell();
                thName.Text = "Name";
				thName.CssClass = "col-xs-4";
                thEventStartEnd.Text = "Event Time";
				thEventStartEnd.CssClass = "col-xs-2";
                thEventLocations.Text = "Locations";
				thEventLocations.CssClass = "col-xs-6";
                tableHeaderRow.Cells.Add( thName );
                tableHeaderRow.Cells.Add( thEventStartEnd );
                tableHeaderRow.Cells.Add( thEventLocations );
                table.Rows.Add( tableHeaderRow );

                foreach (ReservationService.ReservationSummary reservation in reservationSummaryList)
                {

                    if (reservation.ReservationStartDateTime.Date == day.Date)
                    {


                        TableRow trReservation = new TableRow();
                        TableCell resName = new TableCell();
                        TableCell resStartEnd = new TableCell();
                        TableCell resEvent = new TableCell();


                        Label reservationNameLabel = new Label();
                        reservationNameLabel.Text = String.Format( "{0}", reservation.ReservationName );
                        resName.Controls.Add( reservationNameLabel );
                        trReservation.Cells.Add( resName );

                        Label reservationStartEndLabel = new Label();
                        reservationStartEndLabel.Text = String.Format( "{0} - {1}", reservation.ReservationStartDateTime.ToShortTimeString(), reservation.ReservationEndDateTime.ToShortTimeString() );
                        resStartEnd.Controls.Add( reservationStartEndLabel );
                        trReservation.Cells.Add( resStartEnd );

                        foreach (ReservationLocation rl in reservation.ReservationLocations)
                        {
                            Label reservationEventLabel = new Label();
                            reservationEventLabel.Text = String.Format( "{0}, ", rl.Location.Name );
                            resEvent.Controls.Add( reservationEventLabel );
                        }

                        
                        trReservation.Cells.Add( resEvent );



                        table.Rows.Add( trReservation );
                    }

                }
                resDayPanel.Controls.Add( table );
                resDayPanel.CssClass = "col-xs-12";
                mainDiv.Controls.Add( resDayPanel );
            }

        }
        
    }

    public IEnumerable<DateTime> EachDay( DateTime from, DateTime thru )
    {
        for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays( 1 ))
            yield return day;
    }
}