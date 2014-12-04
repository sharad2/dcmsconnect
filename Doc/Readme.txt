New Features-19th Feb 2013

PO Search:-
Provided feature to search PO within all UIs(Unrouted,Routing,Routed and BOL).
Redirects to proper ui in which PO exists. Incase PO belongs to some BOL redirect to report 110.21.
If PO has multiple status (i.e Unroted and Routing) a UI with list is displyed. 
Click on appropriate PO leads to proper UI with PO group being highlighted.

Include Shipped:-
Appointment screen has option to see shipped appointments as well.

Unavailable Buckets:-
Unrouted screen shows orders of unavailable buckets as well. Routing can't be done for these orders.

Unschedule Appointments:-
Showing unschedule appointments in appointment screen.
Logically every ATS date in routing screen denotes an unscehdule appointment.
Unschedule appointment remains so till BOLs are created for POs of this ATS date and assigned to some real appointment. 


Routing Use Cases
-----------------
Sharad 21 Nov 2012

We route only pure POs: POS which have same dc, carrier,shipping addresss, have no bols created yet??
1. Route unrouted orders whose cancel date is imminent.
 - Choose orders to route and assign ATS date.
 - System displays routing information for these orders.
 - Modify the displayed information to more closely match reality (not implemented)
 - Use this to enter info on customer site.

2. Enter routing information received from customer
 - Identify custome on routing summary. Go to Routing page for this customer.
 - Select the POs for which routing info is available and enter the info for those POs.

3. Create BOLs for routed POs. (Why are we making the user press a button for creatng BOLs)
 - Ideintify this on summary page and move on to ceating BOs.

3. Identify delayed routing. Does any customer need to be called because he has not sent the routing info.  (??)

4. Assign appointments to BOL and now you can ship them using ShipMgr.

Special schenarios:
1. BOL creation is not allowed untill all POs of certain ATS date don't have routing info(i.e. Load Id or PickUp date)
2. What happens if some orders in the EDI are not to be routed anymore?
  -Unroute these orders which don't have routing info yet and proceed with BOL creation. 
3. Routed Definition-Order must have LoadID or PickUp date.If Both are provided then LoadId is Boss.
4. If routing for some exixting ATS date the orders will be put in same EDI.
5. ATS date is considered as pickUpdate On BOL page if no pickup date is provided.For appointment creation only.
6. We are not populating prepaid code in SHIP table while creating bol, but you can enter it using ShipMgr UI. 
7. We show existing ATS date only  till BOL is not created for the POs assigned to this date.

Managing Appointments 
Special schenario
You can add bols of two different buidling in one appointment. The building of the appointment is the primary building where you need to assemble all the 
pallets. ScanToTruck UI also helps you to manage this. 


Guide truck loading 
1. The shipping clerk checks the staus of today's appointments using day view of Appointments screen. It is recommended that she moves the 
the next appointment to the door. 

Feedback:
 Routing Summary: Show sart date when it is in the future. Otherwise show it only in tool tip.
 All selections should be row level.
 Appointment becomes completed when it has at least one shipped BOL.

Design Goals
# We focus on routing unrouted POs of a customer.
# Incomplete POs can be routed with warning. 
# You will route full order which means a PO ships to only one destination(DC). We do not allow routing picklsip level. It might however happen that you do not route some pickslips of a order owing to 
  inventory shortage.In this case please cancel the pickslips.
# We do not allow routing cancelled pickslips in this release, even though it might be usefull for WC1 transfers. 

UI Design
Following tabs are available. 
#Customer Summary
Displays the status of open orders per customer. 
Unrouted:Number of unrouted orders. 
Routing: Number of orders for which EDI has been created. 
Routed: Number of orders for which pickup date has been entered. 
InBols: The number of orders which are part of any BOL. 

# Not Routed
Shows all unrouted POs of the customer. 
# You can even route cancelled orders. 
# We allow routing the boxes for which 

# Routing In Progress
Show PO level grid. with wt/vol and routing info. It stays here until a BOL is created.
One grid per ATS date. Multiple PO selection and ability to enter routing information. 
Checkbox to show only those POs where critical routing information is missing. Ability to undo routing. 

# Routed 
When the load info has been entered the POs are available in this screen for BOl creation. 
EDI 753 gets here when pickup date has been entered. 
One PO level grid per ATS date. Option to create BOLs for a specific ATS date.
If multiple EDIs are created for same ATS date multiple BOLS are created. The user can later merge them using ShipMgr.

Business Validations
1. One load can have only one carrier. 
2. Before creating BOL load id must be present in every pickslip.
3. 

# BOl
TODO: Display one row per BOL. Option to undo BOL.
Current ShipMgr program shows parent shipping Id on  screen. What should we display in BOL tab??

10-09-2012
------------
#. We are routing per PO. Does routing per bucket makes more sense??? Since number of orders can be very large. 

12-09-2012
------------
# TODO:- Show Top few orders in each page and explicitly tell user about this.
# Set reprint UCC flag. 

Business Validations
# What if the order being routed does not have weight and volume information?? Check in before creating EDI.
# ATS date should be 24 hours before.

UI:
Pass relevent fillters from one page to another so that the list shown is one that the user expects. 

Meeting between Sharad sir and rest of the team on 13th September 2012.
* Change customer should move to unrouted screen.
* UI to allow user to adjust weight and volume manually. 
* Show existing routing dates in routed tab. The user can either create EDI 753 for existing dates or can select a date of her choice. 
* Show the value of orders in dashboard. 
* Constraint on tbale EDI_753_754 should be dropped. 

Design Thinking 
* We will allow user to route even for those orders which are not yet available. 
* We will show weight and volume information from master_sku and add the box margin by looking into existing boxes in box table. 

------------------------------------------------------------------------------------------------------------------------------------
TODOs
1. Mechanism to update new shipping address (Update in PS and EDI_753_754 ???).
2. Update Customer DC. (Should we update in PS,dem_pickslip table also. Depends on how does export sends this information to Jesta).
3. If the carrier, shipping info is changed we need to set reprint UCC flag. 
4. Update Audit info(who create EDI753, who created EDI754 etc.
5. Do we need to update master_address just like PKG_EDI does today??
6. Query weight and volume from master_customer.

 
Backend 
1. Reprint UCC flag is both in EDI_753_754 table and in box table. We should support only one flag.
2. Drop index on customer_id from PS table??

--------------------------------------------------------------------------------------------------------------------------------------
TODOs: 20-09-2012
1. Show weight and volume below each grid in Routing page. (BB)
2. Radio buttons not working as expected in Unrouted orders UI. Also write a script to select the radiobutton 
   automatically as user enters something.
3. Show DC in Routed screen. (BB)
4. Keep one Create Bol button for each ATS date. 
5. In unrouted page short the grid on dc cancel and provide icon.
6. We create BOL even when no picking has been done. Will that be an issue???
7. TmsMgr shows the EDIs created through new screens. We should not show the manual EDI in TmsMgr. 
8. Work on EDI id instead of POS. 
9. Group by each PO query on PO,ITERATION,CUSTOMER.
10. Routed screen should allow BOL creation for one EDI.
11. Should I allow creating Load for multiple EDIs together (DB)??

----------------------------------------------------------------------------------------------------------------------------------------
TODO:25-09-2012
1. EDI ID should be passed while deleting PO.[Done]
2. Filter applied on unrouted page should be visible when apllied.[Done]
3  0s should not be clickable on main page. [Done]
4. Steps shown in side bar should be clickable.[Done]
5. Show short icon in header in unrouted page.
6. In routing/routed tab show one grid per EDI. If there are multiple ATS date we show a list. [Done] 
7. In unrouted page when I apply a filter on Dc cancel date the UI gives me the feel that building filter is also applied. 
   Please fix this. [Done]
8. If multiple carrier or multiple load, dcs, pickup date appear in one grid then highlight it. [Done]
9. In routing tab if i enter a value you should automatically check the check box. [Done]
10. Add selectable script in routing page. [Done]

Meeting between SS,DB,RV,RKaur 27-09-2012
1. Remove routed tab and allow BOl creation from Routing tab??? 
2. In routing summary change #BOLS to #POs for which the bols have been created. [Done]
3. Show a new column depicting the total orders with appointment focus. Ex. 8 of 50 orders scheduled.
4. Set the table width of grid so that zig zag grids do not happen.[Done]
5. If there are multiple loads, carriers, dc, pickup date .. per PO in routed tab highlight it with a suitable color scheme.[Done]

Appointment Related
1. Make a new screen for creating appointment.[Done]
2. Show appointment information in Appointments page. [Specs Changed]
3. In routing tab allow appointments to be associated with the EDI.[Specs Changed]

*----------------------------------------------------------------------
TODO:(ISSUES) 29-09-2012(Binay)
1. On Unrouted UI:It currently shows POs which are already in BOLS.(Put shipping_id filter in thr where clause.)[Done]
2. On Routed UI: We are updating CarrierId in edi_753_754_ps but during creation of BOL we are calling the package 'PKG_EDI.CREATE_BOL()' and reading CarrierId from PS.
	(It show massage 'Some Pickslips (e.g. 14008446)do not contain the shipping door area. ORA-06512: at "DCMS8.PKG_EDI", line 538 ORA-06512: at line 4')[Not required to handle it by Deepak sir.]
3. BOL screen shows BOL which are alreday shipped.(So problem occur when we going to delete BOL(It show massage 'Shipping id cannot be null for  Pickslip_id: 16449901  Carrier_id: 0390  Picking_status: SHIPPED  ')
4. FOR TESTING:For multiple Iteration case-- PO_Id:-7467529,CustomerId:-11160.(or PO_Id:-'2552542465',Customer_Id:-'23008')(It show massage'ORA-00001: unique constraint (DCMS8.EDP_PK) violated ORA-06512: at line 14')

-----------------------------------------------------------------------
*----------------------------------------------------------------------
TODO: Meeting between Deepk sir and me(Binay) on 01-10-2012
1. Show the date in formate 'Wed 9 Nov' instead of '2/4/2012'.[Done]
2. Show the progress bar in 'Green Color' instead of 'Blue Color'.[Done]
3. On UnRouted UI, Now user can perform filterisation on the basis of 'Building' or 'Start Date' or 'Cancel Date'.[Done]
4. Show StoreId,CarrierId,CustomerDcId with their corresponding name.[Done]
5. Implement AutoComplete for Carrier on Routing UI.[Done]
6. Show total POs with ',' separator(i.e.thousand separator).[Note:Can't implement with decimal values because it will round them.][Done]
7. Add massage for 'CASE: How to enable Create Bol button'.
8. Right now we are showing 'StatusSummary and ValidationSummary' on  both Routing and _routingPartial UI.[For Discussion]
9. How to implement Appointment Number generation logic? Currently we are using Appointment Id only.
-------------------------------------------------------------------------

--------------3rd October 2012 ----------------------
TODO: Appointment 
1. Where to show the appointment progress?
2. 

Backend
1. There is no foreign key on DC,Carrier in EDI_753_754_PS.
2. 

Manage appointment
1. Give an option to delete an appointment. 
2. Show Maidenform Holidays. 
3. What happens if a new carrier arrives??

--------------------------------------------------------
Meeting with Petra
--------------------------------------------------------
1. Do you want to see store in the routing info?
2. Any other information that you wold like to see?
3. What routing information do you receive from customer. 
   DC,Load,Carrier,Shipping address, Pickup date. 
4. If we try to route before buckets are made available, how do we handle the schenario when order is shot shipped. 
5. Provide building and lable in UI. 
6. Appointment number concept is required. 


Meeeting with Binod
-----------------------
1. Show highlighted only for recent selection. 
2. Appointment creation dialog buuton should either show edit or create not both. 
3. can we show the appointments of one customer in one color?

*-------------------------------------------------------
TODO: Meeting between Deepk sir and me(Binay) on 05-10-2012
1. Re-Implemented Routed UI.[Done]
2. Now Load Created for POs per EDI_ID.[Done]
3. Highlight those POs on Routing UI that's came from Routed UI for any modification.
-------------------------------------------------------- 

*-------------------------------------------------------
TODO: Meeting between sharad sir, Deepk sir,Ritesh sir and me(Binay) on 07-10-2012
1. On Routing UI now provide bind() for autochecked checkboxes on the keypress event on its corresponding textbox.[Done]
2. On Routing UI make separate form for delete PO.[Done]
3. On Routing UI strike the row of selected PO during deletion of that PO.[Done]
4. Remove all magic string.[Done]
5. Use ICollection or IList  instead of IEnumerable when you want to perform Count(),Any().[Done] 
6. Remove strike from UnChecked checkbox. [Done]
7. Now we will create Load for multiple POs and Edi.[Done]
8. Honor PO iteration. Currently we club the order with different iterations. We need to show them seperately. (?)[Done]
9. On Routing UI keyup event not work well  during deletion of contents of textbox.[Specs Changed]
-------------------------------------------------------- 

*-------------------------------------------------------
TODiscuss: Meeting with Deepk sir and me(Binay) on 09-10-2012
1. Should we have to restrict user to create BOL which contains some POs with multiple DC,Carrier,PickupDate and Load ?????????.[Done]
--------------------------------------------------------

*-------------------------------------------------------
TODO: Meeting with Deepk sir,Ritesh sir and me(Binay) on 11-10-2012
Changes done at Controller and ViewModel-----
1. Now we are added Iteration and customerId in key on RoutablPoModel and PoModel.[Done]
2. In CfreateEdi753(), now we are finding customerId from key.[Done]
3. Now during UndoRoute() we also pass Iteration and customerId from key.[Done]
  ****Question:Is EdiId nedded in currunt scenarios.

Changes done at Repository-----
1. In GetRoutingInProgress() we added customerId from ps.[Done]
2. In UndoRoute() now we also pass iteration and customerId.[Done]
3. In CreateEdi753() now we read customerId from key.[Done]

------------------------------------------------------------------------------------------------------------------------

FeedBacks
-----------------------------------------------------------------------------------------------------------------------
1. Distinguish POs with little or no routing info on routing screen.[Done]
2. Show DCCancel date of unrouted PO on main screen.[Done]
3. Bug: Routed pieces are more than ordered pieces.[Done]
4. Show building in BOL UI.[Done]
5. Grouping on ATS date ? if po is routed for ATS date mentioned in list ,it is shown in different group in routing UI.[Done]


Rajesh and Deepak
1. Cannot keep customer in master_appointment.[Done]
2. In AppointmentDetail UI, Show information per bol.[Done]

TODO list as of 15-10-2012
1. Update PS and dem_picklsip also when you update load info. (carrier,dc,shipping address).[Done]
2. Allow address to changed from Routing UI.[Done]

Ravneet and Deepak [Done]
1. PKG_EDI
2. Queries
3. Deprecation of existing columns. [To be done in phase 2]
4. Extract the repos functions to package. [Done]

Meeting with Raj
1. Show the Appointment Detail screen per BOL. Appointmnet info should be the first column. (Appointment 45 ships by Shiva travels at 8:30 PM) [Done]
2. Make BOL clickable and take it to Report 110.21 [Done]
3. Provide the date filter. [Done]
4. Provide appointment filter. [Done]
5. Possible Reports to link (110.19: Routed Orders Information). the goal is to link to all useful reports. We can ask John and Perta about it. [Not needed because the same info is avauilable in Routing UI]


Meeting RK,SG and DB 17-10-2012
-------------------------------
1. Where should we keep Appointment?
   In SHIP table. The design thinking is that that making an appointment should be independent of routing. [Done]
2. Show only those orders which are available. [Done]
3. Show only those EDIS which are manually routed. [We show all the EDIs because TmsMgr can also be routed from here.]

Meeting between Sharad Sir,Deepak Sir and Ravneet
2. How do we keep the loader info in database? Use productivity framework for it. [Done]
3. Provide the option to remove appointment from a BOL. Provide a radio button interface.[Done]
4. Show attribute customer in UI.[Not required as it is our internal concept.]

TODOs--22-10-2012
1. Update PS and dem_picklsip also when you update load info. (carrier,dc,shipping address).[Done]
4. Give an option to delete an appointment. [Done]
5. Should we have to restrict user to create BOL which contains some POs with multiple DC,Carrier,PickupDate and Load [Done]
6. Show DCCancel date of unrouted PO on main screen.[Done]
7. Grouping on ATS date ? if po is routed for ATS date mentioned in list ,it is shown in different group in routing UI [Done]


Meeting with Maidenform on 22 October.
1. Provide building filter in Customer Routing Summary screen also. [Done]
2. Make small shipments orders available for routing. [Done]
3. Provide option to schedule appointments for the automatically routed orders also, (for ex. JC Penny).  [Done]
4. Last six month appointment information must be available with the system.[Done]
5. Provide best picking sequence while retrieving pallets for  truck loading. [Phase 2]
6. We need a report which shows  how many pieces were ordered and how many actually shipped.
7. In ShipMgr allow shipping with warning if all pallets are not loaded to truck. If we short ship by three or more cartons, system should ask for manager override. 
8. Option to add scheduled and unscheduled appointments. A filter should be provided in Appointments details to see scheduled and unscheduled appointments separately.[Done]
9. In appointment detail screen provide the option to see the appointments by date range. Provide an option to see the appointments of current date.[Done]
10. Is it possible that 2 associates can be in the same load when retrieving pallets, the reason I ask is that we will have racks installed on the dock that will be only 
  accessible with a reach fork lift- which cannot enter a trailer- we may have to have the pallets set down for an electric pallet jack operator to run to the assigned door- 
  this electric pallet jack operator will be the actual loader( Physically placing the pallet on the trailer). [Done]
11. Can we have an entry for the Seal #, I see Door and Trailer Number- I ask for the seal because of CTPAT- I think that systemic recording of the seal number rather than
 hand writing on the bill may be more controlled.[Done]
12.  On the “report “ that we all wanted as an  encompassing report of status, can we have the option to run the particular status we seek? For example If I wanted to run 
all loads that show departed, this would indicate all that have left the facility, if I select shipped- this indicates what a BOL has been generated for. An date range ,  
And of course the option to run (ALL) status, I want this so that the Shipping Supervisor can print it off to carry with him or her on the dock to monitor loading.. rather than 
sitting glued to a PC and be able to have options shortens the amount of print out to create.
13.Is it possible to see the associates name in a transaction report- just to monitor the pallet movement as a form of productivity- could this be added and
 tweaked on the Pallet movement report on the DCMS inquiry [Done]

 Deepak, Binay and Dinesh
 1. Show the PO in multiple iterations by using an icon. Show the icon legend also. [Done]
 2. Remove the checkbox from BOL UI and disable the delete BOL option for those BOLs which are not created by manual EDI.[Done]
 3. Show all the orders including the orders of automatic EDI. Indicate to the user that he is working on an automatic EDI. [Phase 2]
 4. In BOL tab under Appointment Info use text like: Appt. 58 ships on 29/10/2012 [Done]
 5. In BOL tab show building description also. [Done]
 6. In routed page show building of the order.Show number of pieces and boxes in tooltip. [Done]


 TODOs left as on 31-10-2012
 Routing UI
 1. If a load has multiple pick up dates show them in red color and disable the button to create BOL. [Done]
 2. Show the number of POs selected in the routing information and address dialog. 
 5. Provide a link with the sucess message in routing and routed tabs. [Done]
 6. Create BOLs page must have a option to see unscheduled bols only.  [Done]
 7. Highlight just created apointment.[Done]
 8. In day appointment show carrier also(Shiva). [Done]
 9. Link the BOLs to Report 110.21. [Done]
 10. Option to remove a BOL from appointment. [Done]
 11. Option to delete appointment. [Done]

To Discuss
1. When user chooses an existing ATS date add the POs to existing EDI. [Done]
2. We are showing automatic EDIs now. We should give a message similar to "Thes POs are for dynamic routing ". [Phase 2]
3. ATS date must be two business day (48 hours or 72 hours) later from current date.What if the date is Sunday. [Not Required]


Current Philosphy of Address
1. PKG_PRINT_UCC reads SHIP TO address from MASTER_ADDRESS. so we need to modify the master as well. 
2. Should we consider adding master_addres_id instead of ADDRESS_T.

Future meeting with Petra and John
1. feedback of Bol page.
2. Feedback on Appointments.
3. Do they enter Appointment no. using ShipMgr screen? If not we will deprecate this column in SHIP table.

Team Meeting 06-11-2012
1. Show all appointments including transferred one. If the BOLs in the appointment has been transferred or shipped do not allow editing, deleting
 by normal user???
2. When an order was downloaded in one building and ships from another say something similar in UI:Ships From: Order belongs to FDC ships from ANX3 [Done]
3. If the pickup date is not available open up the dialog while creating automatic appointments. [Done]
4. When we create an appointment with a filter applied the created appointment does not show up. [Done]
5. Provide option to remove the BOLs from an appointment. [Done]
6. Highlight the appointment just created or updated by user.[Done]
7. When user adds an EDI to existing ATS date they should be added to same EDI?? [Done]
8. Scan to Truck should capture the arrival date if it has not been already put. 
9. Show the current time in appointment day view.[Done]
10: Show active appointments and archived appointments seperately. [Not needed]


 Backend
 1. While updating routing info we must update PS and dem_ps also. [Done]
 2. DC mixing should be allowed on the basis of $AMD not ASN and NON-ASN flag. We will populate the SHIP.DC_MIXING_FLAG for 
    audit purpose. (Not doing it for now. We keep the same specs.)
 3. Pickup_date also exists in PS table we should deprecate it. [Phase 2]
 4. Master_address philosphy. Why the master_address is changed while receiving the EDI 754. [Not to be done now]
 6. Reprint UCC flag should only be available in BOX table. Deprecate the one in EDI_753_754_PS table. [we keep it as audit]
 7. Make index on from pallet and to pallet in box productivity.
 8. Make trigger on update of appintment which will poulate modeified date and by. 
 9. Create table appointment with ROWDEPENDENCIES on. Frontend will pass the the timestamp to update appointment function and 
    backend API will check before updating. [Done]
 10. What happenes when no load is passed to Create bol function. [Done]
 
====================================================================================================
Feedback given by Rajesh Kandari
1: While creating a new apoointment by passing customer filter, the appointment does not show in calender for that customer.(Issue fixed)
2: To see a particular AppointmentNumber detail by passing appointmentNumber filter its giving an error. This error is comes when we pass an appoinment which has been created for 2039 onwards.(Issue fixed) 
3: In Unrouted page the po check box is not working properly.(Issue fixed)
4: While route and unroting a po the count in side bar is not updating.
5: By passing customer filter in appointment page it should be shown only appoinment of that customer but the page is showing all appointment. [Done]

=========================================================================================================
Inquiry 07-112012
=========================================================================================================
1. Show appointment number in shipment scan. 
2. Show BOL on pallet scan. 
3. Appointment number scan. 
-  List of Load, list of customers, carrier, appt. time, arrival time, #ctns, #pallets, #unloaded cartons, #unloaded pallets\

--------------------------------------------------------------------------------------------------------------------------------------------------------------
TODOS as of 07-11-2012
Appointments
1. Remark should have some reasonable limit. Right now there is no limit to it. 

Routing
1. When I go to routing,routed pages through DCMS connect home the page breaks. @if (Model.GroupedPoList.Count > 1): 
Please make sure that the list is not null before checking count. [Done]



Meeting with Ravneet 15-11-2012:
---------------------------------------------------------------------------------------------------------------------------------------------------------------
UI:
1. Check schenario when in a PO some pickslips have been routed and BOL for them has been created. 
The order will be visible in Unrouted screen. How do we handle it?
2. While unrouting we will make sure that either all the pickslips are unrouted or none are unrouted. 
3. Should we show a PO for which some pickslips already have BOLs created in Routing screen?
4. Show address also with the Routing Info. Maybe we can show it with current address. 
5. When address is updated we do not show any update status. [Done]
6. We have a reset routing info button also in routing page. The option to unroute and then route again just to reset 
  info looks tedious. 

Backend:
-------------------------------------------------------------------------------------------------------------------------------------
1. If we are not able to insert all the pickslips during create EDI function call. We will revert all. 
2. Why should I check AVAILABLE_FOR_PITCHING FLAG. Instead I should show all orders and proposed weight and volumne in Unrouted page. 

Unrouted Page
----------------------------------------------------------------------
1. If all the pieces have not been created we must highlight such POs. [Done]
2. Show the bucket of PO.  Link Inquiry with it. 
3. Show lable of PO. Link to Inquiry. 


BOL page:-

-Showing pickupdate, carrier, building of BOL, what if BOL is assigned to an appointment and these values get edited in appointment?


TODOs: 23-11-2012
Backend:
1. PKG_EDI_2.CREATE_BOL_FOR_EDI will now consider pickup date as load if the load has not been entered. (RKaur Done)
The definition of routed orders is load or pickup date. If both exist then load takes precedence. 
If the an EDI consists of any unrouted PO, the function should not allow to create BOL.(RKaur Done)
2. What about the arrival date which is inserted in SHIP table? Can we use it in Appointment? Can we get rid of it? 
   What is the current use??? (RKaur Done)
UI:
1. The definition of routed is load or pickup date. If an EDI consists of any unrouted PO, the Ui should not allow to create BOL. [Done]
2. In BOL page if there are multiple pickup dates show the list of dates. 
3. Highlight those orders on routing page  which are nearing ATS date and don't have routing info.Once ATS date is given preferrence once it has been 
   assigned to an order this is why we don't show DcCancle date routing and routed page. [Done]
4. UI should make it evident that different pickup dates will amount to deifferent bols.
5. Routing summary page must also honour routed definition.

26-11-2012

2. BOLs just created should be highlighted in BOLS page. [Done]
3. When I select an EDI customer it should be informed in the UI. [Phase 2]



Scan To Truck;
Show building of the suggested location in Scan To Truck.

Meeting with John:
1. Need SMS orders to be shown on screen. 
2. Need the pick path for picking multiple pallets in a single trip. 
3. Change Move Pallet to take 


Issues: 27-11-2012

1. In routing page sometimes the routing editor dialog does not appear. [Done]
2. Assigned ATS dates do not appear in the date picker. 
3. In day appointment page give me an option to remove the arrival time. 
4. In appointment day view Manage BOl link does not select the current BOLs associated with the appointment. Pass customerId also with the query string. 
5. When we delete an appointment from day view the Javascript breaks. [Done]
6. In routed page do not enable the CreateBol button if all the orders are not routed. 

Issues: 29-11-2012

1. Can I add a PO to existing ATS date for which BOL has already been created. 
2. Apply validations in all the input controls.[Done]
3. In BOL page do not allow deleting the BOL if the BOL has not been created through EDI, instead show icon with information.. 
4. In UnRouted UI disabled ATS date dialog if the customer has no any orders or UI has no customer Id.[Done]

===================================================================================TODO=====================================================================================

Done:
1. While unrouting we will make sure that either all the pickslips are unrouted or none are unrouted. [RK Done]
2. Show attribute customer in UI.[Not needed]
3. Show all appointments including transferred one. If the BOLs in the appointment has been transferred or shipped do not allow editing, deleting
   by normal user. [Not needed because appointment is an internal concept.]
4. Make trigger on update of appintment which will poulate modeified date and by. 
    Appointments. [RK] Done
5. Show door in Routed page. [RK] [Done]
6. Validate all input controls. [Binay][Done]
7. UI should make it evident that different pickup dates will amount to deifferent bols.[Binay][Done]
8. Remark should have some reasonable limit. Right now there is no limit to it. [Binay][Done]
9. When we click on change link to update truck arrival date page's focus get lost.[Binay][Done]
10.Assigned ATS dates do not appear in the date picker (earlier they were shown in orange color.). [Binay][Done]
11. On routing page if customer does not have any Open Order then hide Open Order Routing Summary tables header. (Not needed)
12. On BOL UI Assign Bol and Unassign Bol button not properly enabled or disabled.[Binay][Done] 
13. On BOL UI remove highlighted class as well as remove button after deletion of particular BOL.[Binay][Done] 
14.The date should be left alligned and numbers should be right in every page.(Ankit)[Done]
15.Total of pieces on Routing page is not below expected headers.(Ankit)[Done]
16.The Ship From ,DC ,DC Cancel Date ,Created On ,Created By  should be feft align on BOL page. (Ankit)[Done]
17.Error message is not in Red color when i tried to create new apponintment without providing building.(Ankit)[Done]
18. Show the bucket of PO in unrouted page.  [RK Done]
19.When appointment is scanned in the Inquiry show me how many pieces are ordered and how many shipped.[Ankit][Done]
20. Show the bucket of PO.  Link Inquiry with it. [Ankit][Done]
21. After unassigning a BOL I didnt get from which Appointment number and BOLs i have unassigned.[RK:We can unassign a long list therefore it is not feasible.]
22. Routing summary page must also honour routed definition.[RK/DB]
23. No BOL is present for customer but still we have "Assign to Appointment" button enabled.[DB: Not needed]
24. Added title for BOL link on day view of appointment.[Ankit][Done]
25. In BOL page if there are multiple pickup dates show the list of dates. [Ankit][Done]
26. On Routing UI highlight the PO whose currunt shipping address we want to see/change by click contact icon .[Binay][Done]
27. On Routing UI select all checkbox not working Properly .[Binay][Done]
28 On summary page corrected message[Ankit][Done]
29. Manage ROLE: Provide  grants.[Binay][Done]
30. Security: [Binay][Done]
31.Show address also with the Routing Info. Maybe we can show it with current address. [Binod][Done]


Issues 29-11-2012(Ankit Sharma)
1. When we click on any Unrouted,Routing or Routed Link the number of PO in list are different then expected through their Summary view for given customer.


Remaining Issues:
1. We need a report which shows  how many pieces were ordered and how many actually shipped.[To Do/Phase2]
2. ShipMgr allow shipping with warning if all pallets are not loaded to truck. If we short ship by three or more cartons, system should ask for manager override. [Manmohan]
3. Show the number of POs selected in the routing information and address dialog. [Binay/Phase2] 
4. Make index on "from pallet" and "to pallet" in box productivity.[RK,Phase 2]
6. In routed page do not enable the CreateBol button if all the orders are not routed. [DB/Phase2]
7. Assign to Appointment button on Appointment page is remembering the last selection of PO for automatic appointment creation.(Ankit/DB/TODO)
8. When we search an appointment number from Appointment page from its Find feature and then  click Assign BOL it is not opening pick appointment window for searched BOL. (DB)
9. When I try to filter an individual appointment in the day view the page does not move to the selected appointment. 
11. Convert the CheckBoxes for selection to button.

Phase 2
1. Provide best picking sequence while retrieving pallets for  truck loading. [Phase 2]
2. Scan to Truck should capture the arrival date if it has not been already put.
When pallet is already loaded on the truck ,ScanToPallet should not suggest this pallet.
3.  On the “report “ that we all wanted as an  encompassing report of status, can we have the option to run the particular status we seek? For example If I wanted to run 
all loads that show departed, this would indicate all that have left the facility, if I select shipped- this indicates what a BOL has been generated for. An date range ,  
And of course the option to run (ALL) status, I want this so that the Shipping Supervisor can print it off to carry with him or her on the dock to monitor loading.. rather than 
sitting glued to a PC and be able to have options shortens the amount of print out to create.
4. What if BOLS for some pickslips of an order are already created???[DB]
5. In day appointment page give me an option to remove the arrival time. [Already there]
6. Should there be checkbox or Button for selecting all rows?[Phase 2]

12-04-2012 Meeting with Manmohan. 
---------------------------------
1. In ShipMgr allow shipping with warning if all pallets are not loaded to truck.
2. If we short ship by three or more cartons, system should ask for manager override. 
Create a new wizard which shows the pallets which are not loaded to the truck yet. Provide option to Ship anyway. 


4 Dec 2012 Rajesh kandari
------------------------------
1: Known issues: In appointment page on Day View when we scan an appoinment it highlight the scanned appointment number but not navigate to that appoinment.[Phase 2]

06-12-2012
-----------------------
TODOs:
1. Code Review: DB
2. Release email: Binay, Dinesh
3. Documentation and presentation: Harry & Ritesh  (write normal use cases with the screenshots.)
4. Design thinking doc. Ritesh
6. Pkg_EDI_2 sets manual or automatic EDI flag.
7. Think about bringing TmsMgr in phase 2. 

Discussions with Sharad Sir:
1. Make partition in EDI_753_754_PS.


Backend Changes Phase2:
1. a new TEMP_PULL_PALLET table with following columns
-- Pallet_id:
   Assign_date:
   Module_code:
   Operator_name:
   Audit columns: 

Questions:
Should I keep the buiiding in temp_pull_pallet?
what if I want to pick multiple pallets in one go. 
Option one: Merge such pallets using MovePallet and then pull them. 

Sugesstion algo:
1. We suggest only when pallet at one go and user can skip to get another suggestion. The skipped pallet is put in suspense. Report .... dispalys these pallets.  

New functions in PKG_APPOINTMENT():
# GetSuggestedPallet(appointmentId, operatorName, buildingId)
# LoadPallet(PalletId)
  Update truck load date, productivity
# UnloadPallet(PalletId)
  RemovesTruckLoadDate.


  Phase 3:
  # Binod suggested that we can bring skip feature so that the case when a pallet is inaccessible is handeled. (DB)Good idea but sorry don't have time and budget
  to do it. 
  # 

  Backend Changes:
  # Partition on EDI_753_754_PS table. 

  Meeeting between Binod,Shiva, Rajesh and Deepak
  # Bring sounds
  # UI finishing, code comments, code quality
  # Ask confirmation for unloading of pallet. 
  # Once no suggestions can be made show the status. For example...
    1. All pallets have been sucessfully loaded. 
	2. 2 Pallets remaining
	P1234 -> In suspense
	P13456 -> Not all boxes verified. 
 # If I scan a pallet which contains any loaded box, treat whole pallet as loaded. The user can unload this pallet and then load 
 it again. Give a clear message that some boxes are already loaded. please scan the pallet to unload it and scan again to load it.

 Meeitng between Ravneet and Deepak
 # Make BOX_PRODUCTIVITY.Operation_code column nullable. 
 # 

 