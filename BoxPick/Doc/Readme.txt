There are three controllers:

1. HomeController: Implements the mainstream path.
2. HelpController: Implements help screen.
3. ConfirmController: Implements those function which depends upon confirmation 


Help screen Management
----------------------
The JavaScript within each view redirects to the help controller when a special key is pressed.

Session Expiry
--------------
The base controller is decorated with BoxPickContextCheck which
checks for invalid model state. If the state is invalid due to no fault of posted values, then
it is presumed that session has expired and we redirect to home page.

Every action method which expects a valid environment must take the model as an action parameter.

Session Value Getting and Setting
---------------------------------
All view models take session as a constructor argument. They read and write their values directly to and from the session.
So changing a value in any model immediately changes the session value.

The Box pick home page clears to session to ensure a clean start.

Authorization
-------------
The base controller is decorated with AuthorizeExAttribute which prevents unauthorized access. This attribute passes
a reason query string to the LogonController so that it is able to display why login is being requested.

Database Querying
-----------------
This is the responsibility of each controller action.

Current Pallet lifetime
-----------------------
After a pallet is successfully scanned, it stays in the session until it gets full.
Whenever an action requeries the pallet, it removes it from the model (and thus the session) if the pallet turns out to be invalid.

Status Messages
---------------
You can add status messages like this:
this.AddStatusMessage(string.Format("Carton {0} associated with Box {1} picked.", model.LastCartonId, model.ScannedUccId));

The view can display the status messages using our ValidationSummaryEx helper method like this:
@Html.ValidationSummaryEx()

Both these status message helpers are available in  EclipseLibrary.Mvc.Helpers namespace.

This functionality will only be available if Controller is extended from EclipseLibrary.Mvc.EclipseController.

Sound Management
----------------
Success sound is played when state is valid otherwise Error sound is played. Success and Error cases are handled automatically by if
SoundType is Default.

Warning sound is played when we are navigation on alternate path which needs to be set.

ConfirmController sets the sound to warning in Get actions. 
HomeController sets the sound to warning only when a carton is scanned in UCC.






