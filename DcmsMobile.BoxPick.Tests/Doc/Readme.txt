Bulk Tests: These tests ensure the behavior of action invocation by controlling the input environment.

1. Authentication Tests. These ensure that unauthenticated requests are redirected to login page.
2. Session tests. These ensure that if session data is corrupted, home page is displayed. These tests also verify the minimum requirement
for valid session data.

All other tests can now assume a proper environment and check more specific behavior.


HomeController
--------------
Building Action - If valid building provided, displays pallet screen, otherwise displays building screen.

Pallet Action - Not invoked if Building invalid. 

GET displays Carton screen if valid pallet is provided, else redisplays pallet screen.
  POST ensures that Pallet in form is valid and shows carton screen, else redirects to building.

Carton Action - Not invoked if pallet invalid.
  GET displays Carton screen and clears current carton.
  POST ensures that posted carton is valid and makes it the current carton. If posted carton invalid, redisplays carton screen.
    - If current pallet scanned then partial picking process invoked else ignored with error.
	- If current UCC scanned, skip ucc process invoked, else ignored with error.

Ucc Action - Not invoked if Pallet/Carton invalid.
 GET - Reprpmpts for UCC
 POST - For Valid
- What sound, if any, is expected when the user presses empty enter ? Sharad suggests that empty scan should not 

