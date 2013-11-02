Sharad 7/11/2011

Conversion becomes property of request. For conversion requests, target SKU is required, otherwise it is not available.
Ensure that an SKU can only be added once per request.

Add option to change carton quality on assignment. Prompt for new quality and propose the highest unshippable quality.


Back end EditCarton will provide the option to pass reason code.
--------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------
Rajesh and Ravneet:(11 July 2013)
Analysis what is most case in Conersion.
SELECT R.REQ_PROCESS_ID,

       R.REQ_MODULE_CODE,

       R.STYLE,

       R.COLOR,

       R.DIMENSION,

       R.SKU_SIZE,

       R.VWH_ID,

       R.CONVERSION_STYLE,

       R.CONVERSION_COLOR,

       R.CONVERSION_DIMENSION,

       R.CONVERSION_sku_size,

       R.CONVERSION_VWH_ID,

       r.insert_date

  FROM SRC_REQ_DETAIL R

WHERE R.REQ_MODULE_CODE IN ('REQ', 'REQ2')

   AND (R.CONVERSION_STYLE IS NOT NULL)

     AND (r.style != r.conversion_style AND  r.color != r.conversion_color AND r.dimension = r.conversion_dimension AND r.sku_size = r.conversion_sku_size )

   AND R.INSERT_DATE BETWEEN TO_DATE('01/01/2013', 'dd/mm/yyyy') AND

       TO_DATE('10/07/2013', 'dd/mm/yyyy')

ORDER BY R.INSERT_DATE DESC ,r.req_process_id,r.style ,r.color;

 

--CLAIM : In most of the requests style is always changed .The requests found where style has not changed are the requests for the conversion

-- of virtual warehouse where sku remains same.In most of the requests they change both the style and color where dim and sku_size remains same.

--In some requests they change style,color and dim where sku_Size is not changed.Also few requests are found where style,color and sku_size is changed

--where dimension is not changed. In very few requests they request conversion of all style,color,dim and sku_size.

--There is no request for conversion of only color or only dim or only sku_size .However there are some requests where only style is changed.

 

--Total records - 3751

 

--Total records where only virtaul warehouse is changed - 1332

 

--Total records where sku is changed  = 2419

 

--No of requests where both style and color has changed = 1094 (45%)

 

--No of requests where style,color,dimension has changed = 719 (30%)

 

--No of requests where style,color,sku_size has changed = 134 (6)

 

--No of requests where style,dimension,sku_size has changed = 0

 

--No of requests where color,dim and sku_size is changed = 0

 

--No of requests where style,color,dim,size has changed = 20 (rarely)

 

--No of requests where only style has changed = 288 ( 12%)

 

--No of requests where only color has changed = 0

 

--No of requests where only dim has changed = 0

 

--No of requests where only sku_size has changed = 1


In most of cases they change style and color both.
Also there is many -> one mapping. Means in one request they change multiple style and color to single style and color.


***************************************************PUL********************************************************

REQ3

1. When an existing request has changed and pulling is also in progress, we need an option to refresh the pulling staus. (TO discuss: Will it be similar to Piece Replenishment??)
2. When would the request expire ? Request will expire within 72 hours unless overridden. 
3. A job will cleanup expired requests. This includes cleanup of reserved cartons. this will make request framework self managing.  
3. When is a request closed ?  Request is deemed closed as it expires. 


REQ design changes
1. Create a request using new REQ3 framework. 
2. A new table src_req_detail_2 will keep only the REQ3 requests. PKG_CTNRESV_2 now inserts the request in new table. These requests cannot be opened in REQ2. 
3. A new table named src_pull_carton will keep the details of cartons pulled against a request. This not is not audit. Audit will be captured as it is done today. 
4. Assign cartons will also have to exclude those cartons which are in CTNRESV_PUL_CARTON. 



Reporting:
Reporting: 30.06: SKUs to be pulled.  SRC_REQ_DETAIL UNION SRC_REQ_DETIAL_2.


Questions

# New Pulling

Brain storming

Features:
 #Allows supervisors to monitor/ control pulling activities in a warehouse. This is similar to Piece Replenishment UI. The information displayed will be per area/ per SKU. 
   Provide filter to show SKUs to be pulled for a request. 
   - User can increase/decrease SKU priority. 
   - Refersh the picture of cartons to be pulled in case a major change has been done in an existing request. 
   - User can stop pulling a request/SKU for an area. When this option is selected the cartons pending pulling are released immediately. 
   - Carton Substitution is allowed. 
   - Suspense feature -- User will be asked to put carton in suspense if he suspects that carton has been lost.  
   - Manual Pulling?? (Can pallet locating help here.)
   - Pallet Pulling?? ANX3 type of warehouse where pallets are SKU pure.


PUL UI

1. Can show SKUs to be pulled per SKU (Pallet???).
2. Shows the cartons to be pulled to user in a way simialr to today's piece Replenishment. User can stop pulling of a SKU/request, refresh or increase/decrease priority. 
 

PUL design

1. Keeps the cartons pulled against a request in src_pull_carton.
2. TEMP_PUL_CARTON remains. 
3. Concurrency will be managed using TEMP_PUL_CARTON
 

Questions:
# Can I pull cartons without a pallet. 
# Do we want to support kitting?? 


#################################################################################################

PUL meeting
1. FDC:BIR => FDC:AWL 40 cartons.
2. Show high priority requests.

Create_pull_pallet -- If no request passed.
# Tries to find cartons for you based on following specifications. 
  1. No pullers, High priority request, oldest request, amount of work. 
  2. If request passed -- No issues.
 

To decide:

1: How many cartons to be placed on pallet. 
2: Should we try to keep similar SKUs together. Order by sTY/COL/DIM/SIZE and not SKU_ID
3: Who manges the exipry: Remove all the expired cartons from table. 
4: Swap carton: Do we need to keep the actual pulled carton.If the suggested carton is no longer valid, find similar carton and suggest it. 
5: Skip carton.
6: Should I stamp carton when I suggest it for pulling. 


ctnresv_pul_carton

# Needs pallet_id,suggested_carton, pulled_carton. 
# Swap cartons introduces major issues. We are keeping it on hold for now and we will revisit it later.

PUL UI
# Shows a list of areas where pulling can be done. Order by priority/work.
# Optimize for arbitrary scan.   Puller seem to be scanning multiple wrong cartons before they receive a correct one. 

Agreements with Ravneet,SS and DB
# Show requests order by priority, and user would choose the higest priority request of his building. 
# Use quantum of work for suggesting requests. 


Meeting on 08/08/2013 (SS,RK,DB)
# Req will not reserve cartons. we will follow subversion model of first come first serve.  

Meeting Ravneet & Deepak 14th August 2013
Home screen specs:
# Index view shows the areas where pulling is possible. User can choose the area where he wishes to pull cartons. 
# Press enter in this UI to let system suggest the request to pull. 


To discuss:
What about pallet pulling??
We have agreed not to provide it for now since it will not be used and we don't want to put efforts into it. 


Meeting Ravneet & Deepak 19th August 2013
# Skip carton will ask to put carton in suspense. Similar to PieceReplenishment.
# Swap carton: to be handeled by pull carton. Pull carton will now take PalletId, ReqId, CartonId. 