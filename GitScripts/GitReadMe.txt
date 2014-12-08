Sharad/Hemant 8 Dec 2014
------------------------
Steps to work on DCMSConnect from Git.

1. Clone the repository from http://server/git/dcmsconnect/solutions/dcmsmobile.git
2. Run the script remote-add.sh. This will add a remote for each app. These remotes will be needed when you push your changes to individual apps.

3. You are welcome to use DcmsMobile-All.sln which will load all projects in the solution. If you find that this is too slow for you, you should load DcmsMobile-Min.sln. This solution has all apps unloaded. You can just load the app you are interested in.
If you are going to be working on this app for a long time, make copy of this solution (e.g. DcmsMobile-PickWaves.sln) and check it in.


FAQ
----
1. Where was this repository initially cloned from ?
  Check the URL value of remote origin by running "git remote -v" in Git Bash

$ git remote -v
boxmanager      http://server/git/dcmsconnect/apps/boxmanager.git (fetch)
boxmanager      http://server/git/dcmsconnect/apps/boxmanager.git (push)
boxpick http://server/git/dcmsconnect/apps/boxpick.git (fetch)
boxpick http://server/git/dcmsconnect/apps/boxpick.git (push)
cartonareas     http://server/git/dcmsconnect/apps/cartonareas.git (fetch)
cartonareas     http://server/git/dcmsconnect/apps/cartonareas.git (push)
cartonmanager   http://server/git/dcmsconnect/apps/cartonmanager.git (fetch)
cartonmanager   http://server/git/dcmsconnect/apps/cartonmanager.git (push)
dcmslibmvc      http://server/git/dcmsconnect/libs/dcmslibmvc.git (fetch)
dcmslibmvc      http://server/git/dcmsconnect/libs/dcmslibmvc.git (push)
dcmslite        http://server/git/dcmsconnect/apps/dcmslite.git (fetch)
dcmslite        http://server/git/dcmsconnect/apps/dcmslite.git (push)
dcmsmobile      http://server/git/dcmsconnect/apps/dcmsmobile.git (fetch)
dcmsmobile      http://server/git/dcmsconnect/apps/dcmsmobile.git (push)
dcmsrights      http://server/git/dcmsconnect/apps/dcmsrights.git (fetch)
dcmsrights      http://server/git/dcmsconnect/apps/dcmsrights.git (push)
inquiry http://server/git/dcmsconnect/apps/inquiry.git (fetch)
inquiry http://server/git/dcmsconnect/apps/inquiry.git (push)
libmvc  http://server/git/dcmsconnect/libs/libmvc.git (fetch)
libmvc  http://server/git/dcmsconnect/libs/libmvc.git (push)
liboracle       http://server/git/dcmsconnect/libs/liboracle.git (fetch)
liboracle       http://server/git/dcmsconnect/libs/liboracle.git (push)
origin  http://server/git/dcmsconnect/solutions/dcmsmobile.git (fetch)
origin  http://server/git/dcmsconnect/solutions/dcmsmobile.git (push)
packages        http://server/git/dcmsconnect/libs/packages.git (fetch)
packages        http://server/git/dcmsconnect/libs/packages.git (push)
palletlocating  http://server/git/dcmsconnect/apps/palletlocating.git (fetch)
palletlocating  http://server/git/dcmsconnect/apps/palletlocating.git (push)
pickwaves       http://server/git/dcmsconnect/apps/pickwaves.git (fetch)
pickwaves       http://server/git/dcmsconnect/apps/pickwaves.git (push)
piecereplenish  http://server/git/dcmsconnect/apps/piecereplenish.git (fetch)
piecereplenish  http://server/git/dcmsconnect/apps/piecereplenish.git (push)
receiving       http://server/git/dcmsconnect/apps/receiving.git (fetch)
receiving       http://server/git/dcmsconnect/apps/receiving.git (push)
repack  http://server/git/dcmsconnect/apps/repack.git (fetch)
repack  http://server/git/dcmsconnect/apps/repack.git (push)
req2    http://server/git/dcmsconnect/apps/req2.git (fetch)
req2    http://server/git/dcmsconnect/apps/req2.git (push)
shipping        http://server/git/dcmsconnect/apps/shipping.git (fetch)
shipping        http://server/git/dcmsconnect/apps/shipping.git (push)

ssing_000@MERKEL /c/GitWork/dcmsmobile (master)
$

2. How to get the latest updates from the server?
  -- Use pull from any GUI

3. How to sumbit my changes to the server?
  -- Use push from any GUI.
  -- Then run the script subtree-push.sh to update individual projects in app repositories.

