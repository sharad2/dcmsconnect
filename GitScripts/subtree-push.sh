# Use this script to push all subtrees to remote repositoryou should never need to run it.
# Before running this script, make sure that the script remote-add.sh has already been run to create the remotes.

cd ..

git subtree push --prefix DcmsMobile dcmsmobile master

git subtree push --prefix BoxManager boxmanager master

git subtree push --prefix BoxPick boxpick master

git subtree push --prefix CartonAreas cartonareas master

git subtree push --prefix CartonManager cartonmanager master

git subtree push --prefix DcmsLibrary.Mvc dcmslibmvc master

git subtree push --prefix DcmsLite dcmslite master

git subtree push --prefix DcmsRights dcmsrights master

git subtree push --prefix EclipseLibrary.Mvc libmvc master

git subtree push --prefix EclipseLibrary.Oracle http://server/git/dcmsconnect/libs/liboracle.git master

git subtree push --prefix Inquiry inquiry master

git subtree push --prefix packages packages master

git subtree push --prefix PalletLocating palletlocating master

git subtree push --prefix PickWaves pickwaves master

git subtree push --prefix PieceReplenish piecereplenish master

git subtree push --prefix Receiving receiving master

git subtree push --prefix Repack repack master

git subtree push --prefix REQ2 req2 master

git subtree push --prefix Shipping shipping master