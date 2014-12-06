# This script was used to add subtrees to this dcmsmobile repository
# It exists here to serve as creation log. You should never need to run it.
# Before running this script, make sure that
#   1. The current directory is the repository root.
#   2. The script remote-add.sh has already been run to create the remotes.

git subtree add --prefix DcmsMobile dcmsmobile master --squash
git commit -a -m "Added DcmsMobile as subtree"

git subtree add --prefix packages packages master --squash
git commit -m "Added packages as subtree"

git subtree add --prefix DcmsLibrary.Mvc dcmslibmvc master --squash
git commit -a -m "Added DcmsLibrary.Mvc as subtree"

git subtree add --prefix EclipseLibrary.Mvc libmvc master --squash
git commit -a -m "Added EclipseLibrary.Mvc as subtree"

git subtree add --prefix EclipseLibrary.Oracle liboracle master --squash
git commit -a -m "Added EclipseLibrary.Oracle as subtree"

git subtree add --prefix Inquiry inquiry master --squash
git commit -a -m "Added Inquiry as subtree"

git subtree add --prefix BoxManager boxmanager master --squash
git commit -a -m "Added BoxManager as subtree"

git subtree add --prefix BoxPick boxpick master --squash
git commit -a -m "Added BoxPick as subtree"

git subtree add --prefix CartonAreas cartonareas master --squash
git commit -a -m "Added CartonAreas as subtree"

git subtree add --prefix CartonManager cartonmanager master --squash
git commit -a -m "Added CartonManager as subtree"

git subtree add --prefix DcmsLite dcmslite master --squash
git commit -a -m "Added DcmsLite as subtree"

git subtree add --prefix DcmsRights dcmsrights master --squash
git commit -a -m "Added DcmsRights as subtree"

git subtree add --prefix PalletLocating palletlocating master --squash
git commit -a -m "Added PalletLocating as subtree"

git subtree add --prefix PieceReplenish piecereplenish master --squash
git commit -a -m "Added PieceReplenish as subtree"

git subtree add --prefix Receiving receiving master --squash
git commit -a -m "Added Receiving as subtree"

git subtree add --prefix Repack repack master --squash
git commit -a -m "Added Repack as subtree"

git subtree add --prefix REQ2 req2 master --squash
git commit -a -m "Added REQ2 as subtree"

git subtree add --prefix Shipping shipping master --squash
git commit -a -m "Added Shipping as subtree"

git subtree add --prefix PickWaves pickwaves master --squash
git commit -a -m "Added PickWaves as subtree"