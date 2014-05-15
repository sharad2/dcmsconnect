Sharad 10 Jan 2012
------------------
Create and manage pick waves.
For creating, provide visibility to decide what waves should be created. DC Cancel date, routing status, invenory availability,...
For managing visibility to decide what needs attention.

Management means
- Freezing/Unfreezing.
- Adjusting priority
- Prefer ADR pulling over more expensive pitching.
- Ability to chase shortages


New type of pick wave?
 - When creating, choose how you want to process it.
  * ADR Pulling ? If yes, then specify multiple areas to pull from. Also intelligently suggest areas from which inventory can be pulled. Across buildings?
  * What to do after ADR Pulling?
    - Ignore (ADR Exclusive). Boxes will not be created for remaining inventory. Order will be undeshipped.
	- Pitching (most common). Boxes will be created for pitching.
	- Checking. Boxes will not be created for remaining pieces. Instead, they will be created during the checking process.
	- Quick Pick (DCMS Lite concept)
  
  followed by pitching (most common case)
  * ADR Pulling with no pitching (equivalent to ADR Exclusive.)

Goals
 - Recreate boxes whould be rarely necessary
 - Shortage management built in
 - Support for DCMS Lite (i.e. bulk printing)

TO Think
- Integrate shortage management
- Integrate routing status
- Support for multiple building shipments

TODO:
What is bucket.available_for_mpc? CreateMPC uses this flag to show bucket list. Want to default this to Y which amounts to deprecating it.
Would like to deprecate bucket.status = READYFORMPC. Who looks at  READYFORPUL and READYFORCHECKING status?



Sharad 30 Mar 2012
------------------

Pick Wave manager aims to enable bulk management of pick waves. Currently it focuses on ADR Exclusive pickwaves, but we intend to expand the scope of this application to all types
of waves very soon.

Managing ADR Exclusive Waves
============================
On the opening screen you see a link for each pick carton area for which Pick Waves exists. Normally you will see only a single link for CPK, but the possibility exists for
the existence of multiple carton picking areas.

You click on this link to see a list of all ADR Exclusive pick waves for CPK which are in progress. A Pick wave is considered to be in the "In Progress" state after it has been made available and until all ordered pieces have been picked. The system will automatically mark the pick wave as completed after all pieces have been picked. When inventory shortages exist, you will have to manually mark the pick wave as completed using the button provided. 
For each pick wave, the number of not assigned cartons is displayed. You can click this number to diagnose why the system is having trouble in creating boxes. The click will lead to Report 130.28 which will display all shortages for the bucket in the CPK area. The report will also indicate how many pieces need to be pulled from CFD to enable creation of all boxes. If after looking at this report you make the determination that it is not possible to ship this bucket complete, then you can forcefully mark it as complete using the UI.

When you mark a bucket as complete, all unpicked boxes are deleted; the cartons reserved against these boxes are released; The pickslip status is set to validated.

Under rare circumstances, you can decide to make an in progress wave unavailable as well.

Options are available to see completed and unavailable pick waves as well. 

Completed pick waves can be marked as in progress although this should be rarely necessary.
You will do this if you have discovered some inventory which can be shipped against this wave. This simply updates the status of the bucket to in progress. Pickslips of this bucket will stay completed. When you recreate boxes of this bucket, the pickslips for which boxes could be created are automatically marked as in progress.


Unavailable waves can be marked as available at any time. When a wave is made available, the system automatically tries to create boxes for the wave.

The UI makes it convenient to manage multiple waves at the same time. You select as many waves as you want and then tell the system what to do with them.

You will probably be spending most of your time in managing the progress of pick waves.
 - You should recreate boxes when you believe that inventory has reached CPK. Remember that boxes can only be created if suitable cartons are available in CPK.
 - As a precaution, if additional boxes do get created, the pickslip is marked as in progress. Of course, normally you will be recreating boxes of in progress pickslips only.

The system goes through a lot of effort to minimize the need to manually recreate boxes, but it does need help.
 - Boxes are automatically created when a pick wave is made available.
 - Boxes are automatically recreated during box expediting, just before pallet stickers are printed.
 - Ideally, the system should recreate boxes each time a pallet is located in CPK. However, this s not currently implemented.



Binod Kumar 29 Mar 2012
-------------------------
** CURRENT FEATURES OF PICKWAVES APPLICATION **

1.	This module is aimed for warehouse order managers to provide the facility to manage waves pool, and floor users can also use this to manage some wave related task.
2.	This module provides an UI to check Unavailable, In progress, Completed waves status.
3.	User can freeze or unfreeze the waves
4.	User can manually mark In progress waves as completed, if there is no hope to recreate more boxes for them and also can mark uncomplete to start work on those waves.
5.	In progress waves can be made unavailable too.
6.	Various report Urls given which will help out the resolve the problem with pick waves
	a.	Given report 130.28 url to show the shortage of inventory against bucket.
	b.	130.28. Inventory Shortages. This report will identify those SKUs which cannot be shipped due to insufficient inventory in CPK and CFD combined.
		It will also tell you which areas these SKUs can be pulled from.
	c.	130.28. Replenishment Workload. Boxes can only be created if inventory exists in CPK. You can check the list of SKUs which need to move from CFD to CPK in this report.
	d.	130.33. Replenishment Exception Report. This make sure that replenishment is happening efficiently. This report lists those SKUs which cannot be replenished 
		due to some abnormal circumstance. You need to fix these abnormalities.
7.	Box creation is potentially a time consuming task. This is performed here in the background and the UI is not blocked.
	So that user can perform further tasks rather than waiting that task to be completed.



	 
	 ***********************  UPCOMING FEATURES ******************
=>  Functionality to Bulk Pick Waves Creation, will be soon available more over here will be more powerful features to manage pick waves.


Sharad 7 Apr 2012
-----------------
Thoughts on the personality of Pick Wave manager

Home page displays imported orders by customer and some stats related to them
Users get to pick a customer and choose to create pick waves for that customer. They must decide how the wave is to be shipped.
They could allow ADR pulling, Pitching or both.

ADR Pulling only = ADR Exclusive wave of today
ADR Pulling + Pitching = ADR wave of today
Pitching only = Pitching wave of today.

They will have the opttion to customize both the pulling experience as well as the pitching experience.

For Pulling they must decide whether labels will be printed via the box expediting process (= ADREPPWSS) or after the boxes have been pulled (ADR process). They must also specify the pallet limit, pull area, etc.

For Pitching they must decide whether z-bar will be used for pitching or an MPC or checking. Options similar to what SelectO provides today will be available.

Box Picking will be the program to use to perform the pulling.

There will be no concept of making a bucket available. Box creation will happen during the box expediting of the mpc expediting process.


-------------------------------
Sharad 14 Aug 2012

Ability to pitch an ADRE bucket

Occasionally it may not be possible to get cartons for all the pieces needed by a bucket. In such situations, you can open up an ADRE bucket for pitching.
This is done from the Manage ADR Exclusive Pick Waves screen. A new option "Pitch remaining boxes" will be introduced.

This can be selected at any time. When chosen, it will prompt for the picking area. It will ensure that locations have been assigned in the pick area for all the SKUs involved.
Once all this is successful, the status of the bucket wll be set to READYFORMPC and pitch boxes will be created. 
This will make it visible in CreateMPC and it can be pitched as usual. This bucket will be visible to all the pitching based tools and reports.


You will be able to manage these buckets using this pick wave manager. Options will be available to adjust priority, mark complete.

 You will also have the option to revert the pick wave to ADR exclusive. This will delete all unpitched boxes. Once this is done, it will look line any other ADRE bucket.

Sharad 17 Apr 2013:
-------------------
This Pick Wave Manager will require:
- Building specific areas. Building id column in ia and tab_inventory_area must be not null.
- building of carton area and pick area location will not be looked at.
- pickslip building will be ignored.
- SKU mapping to building will be ignored.

Sharad 19 Apr 2013
------------------
Buckets created by SelectPO will not be visible to this pick wave manager, and vice versa.
bucket.pick_mode column will be null for buckets created by this UI. This UI will only select buckets where this colun is null.
SelectPO will be changed to only select buckets where this column is not null.

-----------------------------
Better query

SELECT ps.BUCKET_ID AS BUCKET_ID,
       /*       SUM(CASE
             WHEN BOX.CARTON_ID IS NULL AND BOX.STOP_PROCESS_DATE IS NOT NULL THEN
              NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
           END) AS CAN_EXP_PCS_PITCH,
       SUM(CASE
             WHEN BOX.CARTON_ID IS NULL AND BOX.STOP_PROCESS_DATE IS NOT NULL THEN
              BD.CURRENT_PIECES
           END) AS CAN_CUR_PCS_PITCH,*/
       SUM(CASE
             WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NOT NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
           END) AS VRFY_EXP_PCS_PITCH,
       SUM(CASE
             WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NOT NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              BD.CURRENT_PIECES
           END) AS VRFY_CUR_PCS_PITCH,
       SUM(CASE
             WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
           END) AS UNVRFY_EXP_PCS_PITCH,
       SUM(CASE
             WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              BD.CURRENT_PIECES
           END) AS UNVRFY_CUR_PCS_PITCH,
       COUNT(UNIQUE CASE
               WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND
                    BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NOT NULL THEN
                BOX.UCC128_ID
             END) AS INPROGRESS_BOXES_PITCH,
       COUNT(UNIQUE CASE
               WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NOT NULL AND
                    BOX.STOP_PROCESS_DATE IS NULL THEN
                BOX.UCC128_ID
             END) AS VALIDATED_BOXES_PITCH,
       /*       COUNT(UNIQUE CASE
         WHEN BOX.CARTON_ID IS NULL AND BOX.STOP_PROCESS_DATE IS NOT NULL THEN
          BOX.UCC128_ID
       END) AS CANCELLED_BOXES_PITCH,*/
       /*       SUM(CASE
         WHEN BOX.CARTON_ID IS NOT NULL AND
              BOX.STOP_PROCESS_DATE IS NOT NULL THEN
          NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
       END) AS CAN_EXP_PCS_PULL,*/
       /*       SUM(CASE
         WHEN BOX.CARTON_ID IS NOT NULL AND
              BOX.STOP_PROCESS_DATE IS NOT NULL THEN
          BD.CURRENT_PIECES
       END) AS CAN_CUR_PCS_PULL,*/
       SUM(CASE
             WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NOT NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
           END) AS VRFY_EXP_PCS_PULL,
       SUM(CASE
             WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NOT NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              BD.CURRENT_PIECES
           END) AS VRFY_CUR_PCS_PULL,
       SUM(CASE
             WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
           END) AS UNVRFY_EXP_PCS_PULL,
       SUM(CASE
             WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NULL AND
                  BOX.STOP_PROCESS_DATE IS NULL THEN
              BD.CURRENT_PIECES
           END) AS UNVRFY_CUR_PCS_PULL,
       COUNT(UNIQUE CASE
               WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NULL AND
                    BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NOT NULL THEN
                BOX.UCC128_ID
             END) AS INPROGRESS_BOXES_PULL,
       COUNT(UNIQUE CASE
               WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NOT NULL AND
                    BOX.STOP_PROCESS_DATE IS NULL THEN
                BOX.UCC128_ID
             END) AS VALIDATED_BOXES_PULL,
       /*       COUNT(UNIQUE CASE
         WHEN BOX.CARTON_ID IS NOT NULL AND
              BOX.STOP_PROCESS_DATE IS NOT NULL THEN
          BOX.UCC128_ID
       END) AS CANCELLED_BOXES_PULL,*/
       COUNT(UNIQUE CASE
               WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NULL AND
                    BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NULL THEN
                BOX.UCC128_ID
             END) AS NONPHYSICAL_BOXES_PULL,
       COUNT(UNIQUE CASE
               WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND
                    BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NULL THEN
                BOX.UCC128_ID
             END) AS NONPHYSICAL_BOXES_PITCH,
       MAX(CASE
             WHEN BOX.CARTON_ID IS NULL THEN
              BOX.PITCHING_END_DATE
           END) AS MAX_PITCHING_END_DATE,
       MIN(CASE
             WHEN BOX.CARTON_ID IS NULL THEN
              BOX.PITCHING_END_DATE
           END) AS MIN_PITCHING_END_DATE,
       MAX(CASE
             WHEN BOX.CARTON_ID IS NOT NULL THEN
              BOX.PITCHING_END_DATE
           END) AS MAX_PULLING_END_DATE,
       MIN(CASE
             WHEN BOX.CARTON_ID IS NOT NULL THEN
              BOX.PITCHING_END_DATE
           END) AS MIN_PULLING_END_DATE
  FROM PS PS
/* INNER JOIN BUCKET BKT
ON PS.BUCKET_ID = BKT.BUCKET_ID*/
 INNER JOIN BOX BOX
    ON PS.PICKSLIP_ID = BOX.PICKSLIP_ID
 INNER JOIN BOXDET BD
    ON BOX.PICKSLIP_ID = BD.PICKSLIP_ID
   AND BOX.UCC128_ID = BD.UCC128_ID
 WHERE PS.TRANSFER_DATE IS NULL
      
   AND PS.CUSTOMER_ID = '23008'
   and box.stop_process_date is null
   and bd.stop_process_date is null
 GROUP BY ps.BUCKET_ID
