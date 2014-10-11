Scan To Pallet Specifications.

 Scanning Pallet makes it the active pallet. All future box scans apply to this pallet.
 Scanning box is equivalent to scanning pallet if box is on a pallet.
 If the box is not on a pallet:
 If the box can be placed on the active pallet, we do so. 
 If it cannot be placed on the active pallet, then we ask the user to scan the pallet on which this box can be placed. 
 We propose a pallet if possible. While looking for pallets to propose, we are looking for pallets matching the criteria of the box in the area in which user is working. In case of multiple choices, we suggest the most recently created pallet.

-----------------------------------------------------
What is Context Area:

Context area is only used for suggesting pallets for new boxes. It needs to be provided only if suggestions are expected. If this program is only being used to relocate pallets, the context area is irrelevant.

-----------------------------------------------------
When do we ask for pallet location?
 - The user can scan a pallet location at any time. The scanned location will apply to the active pallet.



 ----------------------------------------------------
 How are pallets suggested for a scanned box?

 All pallets matching the criteria of the box are candidates for suggestions. The pallet which has been most recently created is suggested first.
 
 A pallet never gets closed. Therefore the pallet will keep getting suggested, but the user is free to ignore the suggestion and start a new pallet.

------------------------------------------------------
How do we validate that user has put correct boxes on the pallet? Ans. The system will never enforce validating the contents of a pallet. The user will have the option of emptying the pallet. After this is done, he can then scan each box on the pallet.


We must give user flexibility of removing the desired boxes from a pallet. Scanning a box on another pallet will automatically remove it from the previous pallet.

-----------------------------------------------------------
In which area should a newly created pallet be?
The pallet locations in ialoc will be globally unique. We will know the area of the pallet only after the location is scanned.
When the program begins, the user is asked to scan any pallet location near him. This provides the context for pallet suggestions. 
It also opens up the possibility of having multiple FDK since IACONFIG is no longer being used.

-------------------------------------------------------
What is next ia_id?
next_ia_id column is being used to decide whether to take the pallets to door area or not. Pallets having next_ia_id as door area are the candidate pallets for moving to door area through scantodoor program. Now as we have createad a new report R120_21 for solving this purpose so
we are not using the next_ia_id concept any more in the new scantopallet program.


------------------------------------------------------------------
Support for Master Pack
This program is completely oblivious to master pack requirements.

------------------------------------------------------------------
Mail from John Campos 20 Apr 2012
 
Here is what the group came together and complied as  a list of needs to make Scan to dock more comprehensive of what we need.

Additional columns added to the 110.21
1.	Add ship /cancel date. We can do this 
2.	Add option to filter by building. This means filter by location area. Each building will have its own forward dock.
3.	Add load ID column. 
4.	Add filter for Wave column.
5.	Add Bill of Lading column.
6.	Create query for the ability to run history for every dock location. (Inquiry) (Why?)
7.	Filter to sort all information by specific building.  
8.	Ability to zero out pallets ID that is in a location.
9.	Ability to sort / filter all columns.
10.	Ability to scan ADR pallets to the dock / door. Currently the pallet ID is retired and not visible to locate to the dock or door.
11.	Ability to limit the number of pallets that can be scanned in to a location, with a limited number of locations per building that will have no limit or a higher limit of pallets that can be scan to these dock locations. 
12.	Specific pallet locations for every pallet location at the door. (=> ADR should be numbered).

We will need to sit down and have discussion on each item listed for proper clarification.

Proposed Process

Objective:
Minimize wrong shipments so that customer chargebacks are avoided.

Goal: 
- To assemble pallets which can be loaded directly to the truck.
- To determine which pallets need to be loaded on the truck.
- Easily pull these pallets from wherever they are and load them.

to a truck. 

- Use BoxManager to palletize boxes. At the outset, the program will require a location scan which will indicate the area in which the user is working. Then all pallets must be scanned within the same area.

- Use 110.21 to monitor status of pallets at the dock and use it to decide what should be pulled to ADR.

- Use BoxManager again to relocate pallets to ADR. Simply scan the new pallet and the new location.

- When a pallet is shipped, it will be removed from ADR.

- Scan dock location in Inquiry to see what it currently has.


What we will not do:
 - Space management. This means we will not suggest which location the pallet should be located at. The process is not optimized for the case when number of available locations are very scarce.
 - Routing management. 

 --------------------
 14 May 2012

 DB and Manmohan
 - DefiniPALLET table tion of empty pallet: A pallet having no boxes is an empty pallet. 
 - ADR will be numbered.(Is scanning a location mandatory.) 

 Open Work
 - Merging.
 - Spot checking.
 - Suggestions for nearby empty locations.
 - In ShipMgr we look for ASN customer and do not allow DC mixing. 
 - DB suggested that location scan should not be mandatory. 
 - If the location scan is not mandatory we might loose the visibility of where the pallet is located.
 - Sounds similar to Pitching should be introduced. 

UI1: Pallet Creation

- Includes palelt merging


UI2: Pallet Moving

- A pallet can be placed at a location or on another pallet (merging)


UI2. Spot check will happen when pallet moved to different area (Will ask for carton count and force spot check if count does not match).

Pallet table must go

When no pallet is active
 - The scanned boxes are presumed to be on a temporary pallet. As long as a temporary pallet exists, only boxes of that pallet will be accepted. 
   This temporary pallet can be made permanent by scanning a pallet id and a location. The system will ask for carton count before cartons are transferred to the 
   physical pallet. In case of count mismatch, the cartons must be transferred individually.
 
   This makes the temporary pallet empty.

   When temporary pallet is made physical, carton count is asked for. In case of mismatch, the user is asked to keep cartons one by one.

---------------- Meeting notes 14-05-2012 ---------------------------
Shiva, Binay, Manmohan and Deepak
# Why temp pallet: It will increase the speed of scanning. 
# Concurrency issues. What if we suggest same pallet to multiple users.
# If I scan a pallet the UI will show the details but this pallet will not be made active.
  You can still scan the new boxes to a temp pallet and update them later. 
# I scanned a few boxes and kept them on a pallet but forgot to scan the pallet. What should I do now?
  Just scan all the boxes again followed by pallet.   
# Should we change location numbering scheme. 

------------------- ScanToPallet TODO --------------------
# Change GUID of application.
# If a customer is having attribute customer and pallet creation rules are set for both then program will 
  honor the rule set for the attribute customer.
  
------------------- MovePallet TODO --------------------

Sharad 19 May 2012
------------------

View Stage Pallet
- This view allows boxes to be placed on a staging pallet. It can be initiated without a pallet, or with a pallet. 
  if no pallet is passed, a new staging pallet is created when the first box is scanned. The view displays summary information about the staging pallet.

It accepts box scans. The scanned box is immediately placed on the pallet as long as the sort criteria matches. The first box is always accepted.

Special case: If the scanned box must is already on a pallet there are 2 case.
Case 1: There is no active pallet => the pallet of the box will become active. 
Case 2: There is already an active pallet => The box will leave its old pallet and become part of the active pallet.

If the sort criteria does not match, the box is rejected and the user is encouraged to transform the staging pallet into a real pallet.

The user use can simply scan a compatible pallet (i.e. pallet with matching sort criteria) which immediately empties the staging pallet and moves the boxes to the scanned pallet.
Now the rejected box can be scanned again to create a new staging pallet and the process repeats.

The view constantly displays a list of similar pallets along with their locations. This list can be used by the user as a guide to which physical pallet to use.

When the staging pallet is converted to a physical pallet, a dialog asks him to scan a location for the pallet. Location scan is optional but highly recommended. The new pallet will then get assigned to the scanned location.


 Pallet Suggestions: 22-05-2012
 --------------------------------------------
 How are pallets suggested for a scanned box?

 All pallets matching the criteria of the box are candidates for suggestions. The pallet which has been most recently created by the user is suggested first. 
 Pallets which have already crossed the limit should not be in the suggestions.


MOVE PALLET 22-05-2012
--------------------------
1. Allows pallets to be moved from one location to the other.
2. Suggest door locations. This will help in clustering different shipments so that pallets of the same shipment are likely to be near each other.
3. Allows two pallets to be merged.
4. Pallet Validation: While moving a pallet to door location we should ask user to enter number of boxes on pallet and if a mismatch is found provide option to 
   rescan each box. 
5. Door area has to be numbered. 

Issue 24-05-2012
-------------------
1: When user scan both empty pallet, how to handle this situation?
ANS: (MBisht) Move pallet does not accept empty pallets.
 
Issues 28-5-2012(MBisht)
---------------------
1. Issue in pallet suggestion based on the set criteria.(DB)
2. Colors of message should be discussed.


------------------ Team Meeting 30-05-2012 ------------------------- 
--------------------------------------------------------------------
1. Ability to limit the number of pallets that can be scanned in to a location, with a limited number of locations per building that will have no limit or a higher limit of pallets that can be scan to these dock locations. 
6. Create query for the ability to run history for every dock location. (Inquiry) (Why?)

 
Scan To Pallet
1. Issue in pallet suggestion based on the set criteria.


		MB, DB :- 31 May 2012.
		--------------------------
		Currently the query that is retreving suggestions for both scantopallet and move pallet UI
		can suggest impure pallets. Our expectation is that this will happen very rarely but even then 
		for correcting this we have only way which is to write the query in the following way:-

		But this query is performing full scan(PARTAION_RANGE_SCAN) on the table box. As per our discussion
		we should not use this query as it will get executed on almost every scan. So we have decided to
		not to use this query. We are waiting for user's feedback on this. 



		 WITH PALLET_DETAILS AS
					 (SELECT B.PALLET_ID AS PALLET_ID,
							 PS.CUSTOMER_ID AS CUSTOMER_ID,
							 PS.PO_ID AS PO_ID,
							 PS.CUSTOMER_DC_ID AS CUSTOMER_DC_ID,
							 PS.BUCKET_ID AS BUCKET_ID,
							 B.LOCATION_ID AS LOCATION_ID,
							 B.IA_ID AS IA_ID,
							 MAX(B.IA_CHANGE_DATE) OVER(PARTITION BY B.PALLET_ID) AS IA_CHANGE_DATE,
							 SUM(SCASE.OUTER_CUBE_VOLUME) OVER(PARTITION BY B.PALLET_ID) AS PALLET_VOLUME,
							 COUNT(DISTINCT B.UCC128_ID) OVER(PARTITION BY B.PALLET_ID) AS TOTAL_PALLET_BOXES,
							 COUNT(DISTINCT PS.PO_ID) OVER(PARTITION BY B.PALLET_ID) AS PO_COUNT,
							 COUNT(DISTINCT PS.CUSTOMER_DC_ID) OVER(PARTITION BY B.PALLET_ID) AS DC_COUNT,
							 COUNT(DISTINCT PS.BUCKET_ID) OVER(PARTITION BY B.PALLET_ID) AS BUCKET_COUNT,
							 COUNT(DISTINCT B.LOCATION_ID) OVER(PARTITION BY B.PALLET_ID) AS LOCATION_COUNT,
							 COUNT(DISTINCT B.IA_ID) OVER(PARTITION BY B.PALLET_ID) AS AREA_COUNT
						FROM <proxy />BOX B
					   INNER JOIN <proxy />PS PS
						  ON B.PICKSLIP_ID = PS.PICKSLIP_ID
						LEFT OUTER JOIN <proxy />SKUCASE SCASE
						  ON B.CASE_ID = SCASE.CASE_ID
					   WHERE PS.CUSTOMER_ID = :customer_id
						 AND B.PALLET_ID IS NOT NULL
						 AND PS.TRANSFER_DATE IS NULL
						 AND B.STOP_PROCESS_DATE IS NULL
						 AND B.LOCATION_ID IS NOT NULL
						<if>   
						  AND B.IA_ID != :excludeareaid
						</if>
						  )
					SELECT PD.PALLET_ID AS PALLET_ID,
					  MAX(PD.LOCATION_ID) AS LOCATION_ID,
					  MAX(PD.IA_CHANGE_DATE) AS IA_CHANGE_DATE,
					  MAX(PD.IA_ID) AS IA_ID,
					  MAX(PD.PALLET_VOLUME) AS PALLET_VOLUME,
					  MAX(PD.TOTAL_PALLET_BOXES) AS TOTAL_PALLET_BOXES
					  FROM PALLET_DETAILS PD 
					 WHERE PD.LOCATION_COUNT = 1
					AND PD.AREA_COUNT = 1
					<if>
					   AND PD.PO_ID = :po_id 
					   AND PD.PO_COUNT = 1
					</if> 
					<if>
					   AND PD.CUSTOMER_DC_ID = :customer_dc_id 
					   AND PD.DC_COUNT = 1
					</if> 
					<if>
					   AND PD.BUCKET_ID = :bucket_id 
					   AND PD.BUCKET_COUNT = 1
					</if>
					<if>
					AND PD.PALLET_VOLUME &lt; :palletvolumelimit 
					</if>
					GROUP BY PD.PALLET_ID
					 ORDER BY
					<if c='not($orderbytouchdate)'> 
					 MAX(PD.IA_ID),MAX(PD.LOCATION_ID)
					</if> 
					<else> 
					MAX(PD.IA_CHANGE_DATE) DESC,
							 MAX(PD.PALLET_VOLUME) 
					</else>

2. Colors of message should be discussed.(We will wait for user's feedback)
3. Same pallet is being suggested in the pallet suggestions.
4. Should we allow location scan in ScanToPallet when user scans an existing pallet.
5. Change GUID for the program.
6. Can we show attribute customer in the UI.

TODO: 4 June 2012(shiva)
1: When user scan a box first time, Pallet percent full is not seen in ring scanner.
2. When showing location in UI we should show last four charecters in bold and first four characters a little dimmed.

TODO: 6th June 2012 (MB) 
1. On scanning a destination pallet display the total volume in a message. So that user can decide whether the merging on the scanned destination pallet is 
correct or not.(done : Shiva).

2. Desktop UI could be more informative. Need discussion.

Issues by shiva (7 june 2012)
----------------------------------
1: Should we validate boxes when user move pallet in same area's location?
2: In which order suggession list shows in both UI?




Following are the scenarios which we will have to discuss and indcorporate in the move pallet UI.(MBisht 16th June 2012)
-----------------------------------------------------------------------------------
1. Should we allow pallet verification while moving the pallets within an area or it should be asked when the pallet movement is
  being done across areas?


MBisht(June 20th 2012)
------------------------------------------------------------------------------
Move Pallet
1.	In the move pallet UI on scanning a pallet the indicator which indicates whether to take away the pallet from location or not is coming after the pallet box verification which is incorrect. It should be displayed before verification of the pallet.’
2.	Correct Message  Box count you entered is incorrect. Please scan all boxes on pallet {0} one by one to validate.
3.	For verifying a pallet when I pressed enter without scanning any box the program is throwing unhandled exception error.
4.  Pallet boxes should be verified in a single transaction.

ScanToPallet
1. After merging of two real pallets ScanToPallet UI should ask for pallet verification.

Shiva : 25 june 2012
----------------------
1: Use area short name to show in UI.
2: check Sound.(In MovePallet)(done).
3: If user scan empty source pallet to merge.Solve this bug.(In ScanToPallet)(Done in trunk).

Deepak 26 June 2012
----------------------

1. The suggested pallet list should display the percent full for each pallet based on cubic volume limit. This will help the operator in choosing the least full pallet.(Done) 
2. The list of suggested pallets should include only those pallets which have sufficient remaining capacity to accommodate all cartons on the staging pallet. (Done)
5. When a pallet is moved from one area to another, we should ask for carton count. The user should get two opportunities to get the count right.
   If he does not get it right, then the system must force a scan of each carton on the pallet.The goal here is to ensure accuracy when a pallet is moved from forward dock to door. (Done)
6. The box should be placed in suspense while it is on a staging pallet. It should come out of suspense once it is placed on a real pallet. (Done)
7. Oddball boxes. Some small orders will have only one or two boxes. Creating a pallet for these boxes will be too much overhead. There should be some special locations in forward dock which must be willing to accept non-palletized cartons.(Done)

Not completed
1. We should allow only one pallet per forward dock location. This will lead to accountability because users will not be able to cheat the system by locating all pallets into a single location.
2. Dock Location Inquiry must display history of the location. It should indicate which boxes moved in and out of it, and also the user who moved the box to and from the location.
3. There should be some special locations in forward dock which must be willing to accept non-palletized cartons for Oddball boxes.

Sharad 19 Oct 2012: Modifications for VAS
-----------------------------------------
Each view will post a hidden field indicating whether the UI is for VAS or for STP.
Controller For VAS: When a box is sscanned
- box verification status is irrelevant (STP requires boxes to be verified).
- In addition to updating box pallet, update the box VAS flag as well.
- While suggesting pallets, look only for those pallets which have unverified boxes (STP must look for pallets which have verified boxes).
