# This script was used to add subtrees to this dcmsmobile repository
# It exists here to serve as creation log. You should never need to run it.
# Before running this script, make sure that the current directory is the repository root.

git remote add dcmsmobile http://server/git/dcmsconnect/apps/dcmsmobile.git
git subtree add --prefix DcmsMobile dcmsmobile master --squash
git commit -a -m "Added DcmsMobile as subtree"

git remote add packages http://server/git/dcmsconnect/libs/packages.git
git subtree add --prefix packages packages master --squash
git commit -m "Added packages as subtree"

git remote add dcmslibmvc http://server/git/dcmsconnect/libs/dcmslibmvc.git
git subtree add --prefix DcmsLibrary.Mvc dcmslibmvc master --squash
git commit -a -m "Added DcmsLibrary.Mvc as subtree"

git remote add libmvc http://server/git/dcmsconnect/libs/libmvc.git
git subtree add --prefix EclipseLibrary.Mvc libmvc master --squash
git commit -a -m "Added EclipseLibrary.Mvc as subtree"

git remote add liboracle http://server/git/dcmsconnect/libs/liboracle.git
git subtree add --prefix EclipseLibrary.Oracle liboracle master --squash
git commit -a -m "Added EclipseLibrary.Oracle as subtree"

git remote add inquiry http://server/git/dcmsconnect/apps/inquiry.git
git subtree add --prefix Inquiry inquiry master --squash
git commit -a -m "Added Inquiry as subtree"

git remote add boxmanager http://server/git/dcmsconnect/apps/boxmanager.git
git subtree add --prefix BoxManager boxmanager master --squash
git commit -a -m "Added BoxManager as subtree"

git remote add boxpick http://server/git/dcmsconnect/apps/boxpick.git
git subtree add --prefix BoxPick boxpick master --squash
git commit -a -m "Added BoxPick as subtree"

git remote add cartonareas http://server/git/dcmsconnect/apps/cartonareas.git
git subtree add --prefix CartonAreas cartonareas master --squash
git commit -a -m "Added CartonAreas as subtree"

git remote add cartonmanager http://server/git/dcmsconnect/apps/cartonmanager.git
git subtree add --prefix CartonManager cartonmanager master --squash
git commit -a -m "Added CartonManager as subtree"

git remote add dcmslite http://server/git/dcmsconnect/apps/dcmslite.git
git subtree add --prefix DcmsLite dcmslite master --squash
git commit -a -m "Added DcmsLite as subtree"

git remote add dcmsrights http://server/git/dcmsconnect/apps/dcmsrights.git
git subtree add --prefix DcmsRights dcmsrights master --squash
git commit -a -m "Added DcmsRights as subtree"

git remote add palletlocating http://server/git/dcmsconnect/apps/palletlocating.git
git subtree add --prefix PalletLocating palletlocating master --squash
git commit -a -m "Added PalletLocating as subtree"

git remote add piecereplenish http://server/git/dcmsconnect/apps/piecereplenish.git
git subtree add --prefix PieceReplenish piecereplenish master --squash
git commit -a -m "Added PieceReplenish as subtree"

git remote add receiving http://server/git/dcmsconnect/apps/receiving.git
git subtree add --prefix Receiving receiving master --squash
git commit -a -m "Added Receiving as subtree"

git remote add repack http://server/git/dcmsconnect/apps/repack.git
git subtree add --prefix Repack repack master --squash
git commit -a -m "Added Repack as subtree"

git remote add req2 http://server/git/dcmsconnect/apps/req2.git
git subtree add --prefix REQ2 req2 master --squash
git commit -a -m "Added REQ2 as subtree"

git remote add shipping http://server/git/dcmsconnect/apps/shipping.git
git subtree add --prefix Shipping shipping master --squash
git commit -a -m "Added Shipping as subtree"

git remote add pickwaves http://server/git/dcmsconnect/apps/pickwaves.git
git subtree add --prefix PickWaves pickwaves master --squash
git commit -a -m "Added PickWaves as subtree"