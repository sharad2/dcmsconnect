Pre-requiste
1. All areas must be within building.
2. Identify existing impacts if any.
   # Receiving: How will the UIs behave.
   # Repack: 
   # PKG_REQ will be obsolete. We go through CTNRESV.   

---------------------------------------------------------PUL notes---------------------------------------------------------------------------------
1.  Remove hardwiring of source area. Get the source area from request.
2.  Remove ADR specific code from package. 
3.  Prinitng of carton tickets is not required therefore that code must go. 
4.  We use redesigned TEMP_PUL_CARTON with pallet_id in place of PULL_PROCESS_ID 
5.  For pallet areas can we pull whole pallet. Do we need a seperate UI for this or it can be done in the same UI.
7.  Manual Pulling: Would it be a seperate screen or same as guided pulling?
8.  Do they need subsitution for internal pulling???
9.  Mark Cartons in suspense. 
10. Do we need to print shortage report. 
11. PUL productivity.
12. How can I show the request information should I query src_req_detail. Ans: Read source area from src_carton. No need to request src_req_detail.
13. Keep the pull priority in src_carton.
14. Swap_cartons: Should allow swapping cartons which are not requested for ADR pulling.   
15. Pallet Pulling.
16. Is shortage report really required? Can't we show that through Inquiry or report. 
17. Should we deduce pallets to pull based on source area. User will enter a source area and we will search the pallets which can be pulled based on 
request.
18. Should we pull cartons for RST based actual order. We will not need current request framework. Another idea is to request cartons when we CreateMPC.
19. While pulling for RST should we design process similar to Guided Locating. 
20. Current REQ2 allows request to be reassigned and it can possibly change the cartons in the request. How to avoid this issue???
21. When cartons are short pulled we print shortage report. Should we go for a more advance option like alerting user with an email.

---------------------------------------------------------------------------------------------------------------------------------------------------

PKG_PUL
   
  FUNCTION PKG_PUL_GET_CARTONS(AREA_ID              IN VARCHAR2,
                               PALLET_ID            IN VARCHAR2
)
RETURN NO_OFCARTONS

{
//TODOS
// Remove hardwiring of source area. Get the source area from request.
// Remove ADR specific code from package. 
// Try removing temp_pul_carton.Keep expiry in src_carton instead of using temp_pul_carton. Update the cartons list with pallet in src_carton.
// Keep a new column for expiry in src_carton.
// Should we consider travel sequence while creating pallet. 
 
}

 PROCEDURE PKG_PUL_PULL_CARTON(ACARTON_ID        IN VARCHAR2,
                                ADESTINATION_AREA IN VARCHAR2) IS
{
// Puts the passed carton to destination area. 
// If the destination area is an SKU area call open carton.

}

FUNCTION GET_PALLET_LIMIT(ADESTINATION_AREA IN VARCHAR2) RETURN NUMBER IS should be independent function. This will be called by both PKG_PUL_2 and Pkg_BoxExpediate


        TODO List:
++++++++++++++++++++++++++

1. Populate the some useful info on Index page.
2. If user scans another carton instead of suggested carton, can program put that on pallet or can accept alternate carton?
3. When a carton is stolen after it was added to the list of puller, what should be our behaviour?
4. What is the creteria of Pullable cartons, a part from those which are already in temp_pull_carton?