using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Shipping")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems")]
[assembly: AssemblyCopyright("Copyright © Eclipse Systems 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

// Initial version 1.0.0.0
// Backend Dependency
// 1. Removal of constraint named NO_OF_BOXES_CHK from EDI_753_754_PS table.
// 2. Index on SHIP.APPOINTMENT_NUMBER
// 3. Column TRUCK_LOAD_DATE in BOX table.
// 4. Appointment table and related sequences.
// 5. Add column appoimtment_id in edi_753_754_ps and create foreign key on appoinment_id,Create index on appoinment_id of edi_753_754_ps and 
//    Drop Po_id is not null and TOTAL_PIECES_CHK check constraints from edi_753_754_ps. 
// 6. Using packages PKG_EDI_2 and PKG_APPOINTMENT also PKG_SHIP.DELETE_BOL
// 7. Trigger TRG_APPOINTMENT_AUDIT_BUR.trg
// 8. Appointment_id column in ship table and foreign key constraints on APPOINTMENT_ID of ship table.
// 9. Add column ROUTING_TYPE in edi_753_754 and check constraints on ROUTING_TYPE also foreign key on edi_753_745 customer id.
// 10.Make foreign key of ps on shipping_id as on delete set null.


// Change log from 1.0.0.0 to to 1.1.0.0(Ritesh Verma 15th Dec 2012)
// Provided ScanToTruck UI
// Backend Dependency
// 1.Make operation_code columns Nullable in Box_Productivity 
// 2.Role dcms8_scantotruck . 

// Change log from 1.1.0.0 to to 1.2.0.0(Deepak 18th Dec 2012)
// Major Changes: Now we route for PO and DC. Earlier we routed for a PO only.  
// Backend Dependency
// 1. PKG_EDI_2 
// 2. PKG_SHIP. 

// Change log from 1.2.0.0 to to 1.3.0.0(Ritesh 19th Dec 2012)
// Changes in Routing UI
// 1. Showing Start date,DcCance date and Dollars
// 2. DC is first column
// 3. Introduced filters of Start date and DcCancel date
// 4. Showing Please wiat while routing page loads.
// 5. Changed sorting.
// 6. Showing info icon if DC or carrier is changed.
// Changes in Backend
// 1. PKG_EDI_2 version 4728
// 2. PKG_APPOINTMENT version 4727
// 3. TRG_BOX_P_BUIDR version 4726

// Change log from 1.3.0.0 to to 1.4.0.0(Ritesh 1st Jan 2013)
// Unrouted UI
// 1. Provided building filter.
// 2. Showing start date(ascending order).
// 3. DC is first column in grid.
// 4. Removed Created Pieces % column.
// Routing UI
// 1. Provided building filter.
// 2. Showing count of selected POs.
// 3.Showing icon if DC,Carrier is changed.
// 4. Showing icon on ATS dates for which no routing info is available on schedule ATS date.
// Routed UI
// 1.Shwing icon if DC ,Carrier has changed.
// BOL UI
// 1. Showing parent shipping id.
// 2. Shipping ID is clickable.
// 3. Feature to delete multiple BOLs.
// 4. Icon to unassign appointment from BOLs.
// Appointment Dialog in BOL UI
// 1. Displaying Customer name, remarks with appointment.
// 2. Showing count of POs and BOXs.
// 3. Unassign button removed from appointment dialog.
// Appointment UI
// 1. Login is not required to see appointments.
// 2. Week view is defulat view.
// 3. Showing unschedule appointments.
// 4. Refersh in day view.
// 5. Showing Appointment customer(s) in month/week view.
// Scan To truck
// 1. Asking for login as soon as user enter scan to truck.
// 2. Chanhges is layout and styling.
// 3. Not asking for building scan.
// Backend Dependency
// 1. PKG_APPOINTMENT
// 2. PKG_EDI_2

// Change log from 1.4.0.0 to 1.5.0.0(Branched by Ritesh on 11th Jan 2013)
// 1. Remembering customer in Unrouted,Routing,Routed and BOL UI.
// 2. Showing details of unschedule appointment in day view of appointment.
// 3. Removed building from group and showing it in column in Routed UI.
// 4. Refresh icon in header of day view instead with each appointment.
// 5. Showing BOL,BOX and PO count with appointment.
// 6. Showing schedule/unschedule appointment in different color scheme.

// Change log from 1.5.0.0 to 1.6.0.0(Branched by Ritesh on 18th Feb 2013)
// 1. Showing Unavailable buckets in Unrouted UI
// 2. Validating DC in Routing UI.
// 3. Provided PO search feature .
// 4. Feature to see shipped appointments
// 5. Notify appointment time if time zone of server and client is not same.
// Other Dependencies
// 1. Using jquery-ui-1.10.0.js
// 2. Using theme Start1.10.0 
// Backend Dependency
// 1.GRANT SELECT ON dem_pickslip TO DCMS8_TMSMGR
// 2.GRANT SELECT ON CUSTDC TO DCMS8_TMSMGR
// 3.PKG_EDI_2

// Change log from 1.6.0.0 to 1.7.0.0(Branched by Ritesh on 20th Feb 2013)
// 1. Showing Unavailable buckets in Unrouted UI
// 2. Validating DC in Routing UI.
// 3. Provided PO search feature .
// 4. Feature to see shipped appointments
// 5. Validating Carrier in Routing UI
// Other Dependencies
// 1. Using jquery-ui-1.10.0.js
// 2. Using theme Start1.10.0 
// Backend Dependency
// 1.GRANT SELECT ON dem_pickslip TO DCMS8_TMSMGR
// 2.GRANT SELECT ON CUSTDC TO DCMS8_TMSMGR
// 3.PKG_EDI_2

// Change log from 1.7.0.0 to 1.7.1.0 (Branch created by Ritesh on 20th April 2013)
// Changes of branch version 1.7.0.1 have been merged.
// Change log of branch version 1.7.0.1
// Backend Dependency 
// 1.PKG_EDI_2
// 2.PKG_APPOINTMENT
// General
// 1.Sorting icon in all UIs
// Summary
// 1.PO is considered unrouted even if one pickslip of PO is unrouted. same philosophy is applied to Routing and Routed POs.
// Unrouted
// 1.Disabled POs having no Boxes
// 2.Changed External link icon.
// 3.Fixed.Issue:-If PO with same DC and iteration exist in multiple buildings, UI shows max building only.
// Routing
// 1.Set values to original in case check box is checked and no value is provided in routing info dialog.
// 2.Remove option for edit/set address against the orders.
// Routed
// 1.Users knows in advance how many BOLs will be created for each ATS date
// 2.Remove grouping on EDI.
// Appointment
// 1.Notify if timezone of appointment and user is not same.


// Change log from 1.7.1.0 ?
// Merged changes of branch 1.7.1.1
// Change log of branch 1.7.1.1
// 1. Fixed issue in Routed Ui where same ATS date was displayed twice.
// Merged changes of branch 1.7.1.2
// Change log of branch 1.7.1.2
// 1. Updated assembly product info




[assembly: AssemblyVersion("1.7.1.0")]
[assembly: AssemblyFileVersion("1.7.1.0")]
[assembly: AssemblyProduct(@"<ol><b>Unrouted</b>
                        <li>
                           Showing Unavailable buckets.
                        </li>
                        <li>
                            Disabled POs having no Boxes.
                        </li>
                        <li>
                            Fixed.Issue:-If PO with same DC and iteration exist in multiple buildings, UI shows max building only.
                        </li>
                        </ol>
                        <ol><b>Routing</b>
                        <li>Set values to original in case check box is checked and no value is provided in routing info dialog.</li>
                        <li>Remove option for edit/set address against the orders.</li>
                        <li>Showing icon if DC,Carrier is changed.</li>
                        <li>Showing icon on ATS dates for which no routing info is available on schedule ATS date.</li>
                        </ol>
<ol><b>Routed</b></ol>
<li>You know in advance how many BOLs will be created for each ATS date</li>
<ol><b>Appointment</b>
<li>Week view is defulat view.</li>
<li>Showing unschedule appointments.</li>
<li>Feature to see shipped appointments</li>
li>Notify if timezone of appointment and user is not same.</li>
</ol>
<ol><b>Appointment Dialog in BOL UI</b></ol>
<li>Displaying Customer name, remarks with appointment.</li>
<li>Showing count of POs and BOXs.</li>
<li>Unassign button removed from appointment dialog.</li>
<ol>Provided PO search feature</ol>")]
