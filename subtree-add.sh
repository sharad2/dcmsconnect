# This script was used to add subtrees to this dcmsmobile repository
# It exists here to serve as creation log. You should never need to run it.

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
