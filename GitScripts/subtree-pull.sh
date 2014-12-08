# Use this script to pull all subtrees to remote repositoryou should never need to run it.
# Before running this script, make sure that the script remote-add.sh has already been run to create the remotes.

cd ..

git subtree pull --prefix DcmsMobile dcmsmobile master

git subtree pull --prefix BoxManager boxmanager master

git subtree pull --prefix BoxPick boxpick master

git subtree pull --prefix CartonAreas cartonareas master

git subtree pull --prefix CartonManager cartonmanager master

git subtree pull --prefix DcmsLibrary.Mvc dcmslibmvc master

git subtree pull --prefix DcmsLite dcmslite master

git subtree pull --prefix DcmsRights dcmsrights master

git subtree pull --prefix EclipseLibrary.Mvc libmvc master

git subtree pull --prefix EclipseLibrary.Oracle liboracle master

git subtree pull --prefix Inquiry inquiry master

git subtree pull --prefix packages packages master

git subtree pull --prefix PalletLocating palletlocating master

git subtree pull --prefix PickWaves pickwaves master

git subtree pull --prefix PieceReplenish piecereplenish master

git subtree pull --prefix Receiving receiving master

git subtree pull --prefix Repack repack master

git subtree pull --prefix REQ2 req2 master

git subtree pull --prefix Shipping shipping master