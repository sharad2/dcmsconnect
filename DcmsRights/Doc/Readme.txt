DCMS Rights Application Readme
------------------------------
By Sharad Singhal. 27 March 2012.

By default, the user dcms8 can run this application.

Granting and Revoking Rights
Most commonly, you will be granting or revoking rights from users.
 - Select List of All Users from the home page.
 - Search for the user name in the list and click it. Alternatively, enter the user name for whom rights need to be added or removed.
 - You will see all DCMS rights which the user has been assigned. You can select some of these rights to revoke them. Note that indirectly granted roles cannot be removed.
 An error message will tell you the name of the parent role which must be removed.
 - Another tab on this page shows rights which have not been assigned to this user. You can select some of these and assign them.


Creating Users.
 The UI is optimized for creating multiple users who need the same rights. This scenario will occur when a batch of temps join the company.
 -Write user name one per line in the textbox on the home page.
 -Give initial password. This password is visible on the screen but this should not be an issue because users will be forced to change it at first login. The password must meet the password validation
 rules defined in the database
 -Select roles to be granted to the users.
 -Click the Create Users button. If all goes well, then the users will be created and they are ready to use the programs for which rights have been granted to them after
 changing their initial password.
 -All new users will have 'SO_DCMS_PROFILE' profile by default. The default profile is specified in web.config as shown below. The first value of the defaultProfile attribute is what is used here.

<membership defaultProvider="OracleMembershipProvider">
	<providers>
		<clear />
		<add name="OracleMembershipProvider" type="EclipseLibrary.Oracle.Web.Security.OracleMembershipProvider" connectionStringName="dcms8,dcms4" applicationName="DcmsWeb" 
			defaultProfile="SO_DCMS_PROFILE,SO_DCMS_SINGLE" />
	</providers>
</membership>

Managing Users
   After selecting a particular user, you can delete the user, lock or unlock the user, reset a forgotten password and kill session forcefully of a logged in user.

   The program has safety checks built in to ensure that only DCMS users are visible. A DCMS user is defined as a user who has one of the profiles specified in web.config. The following web.config entry specifies that a user is a DCMS user if he has any one of the profiles specified for defaultProfile.

<membership defaultProvider="OracleMembershipProvider">
	<providers>
		<clear />
		<add name="OracleMembershipProvider" type="EclipseLibrary.Oracle.Web.Security.OracleMembershipProvider" connectionStringName="dcms8,dcms4" applicationName="DcmsWeb" 
			defaultProfile="SO_DCMS_PROFILE,SO_DCMS_SINGLE" />
	</providers>
</membership>

This safety check implies that an existing non DCMS user cannot be converted to a DCMS user using this application. An admin can make this user a DCMS user simply by assigning one of the DCMS profiles to him.

   If you click on a role on the home page, you will have the option to remove multiple users from the clicked role.

Reviewing who can do what
This program is accessible to anonymous users in a read only mode. They can view all information but will not be able to modify anything.
 - The home page lists all DCMS programs and the roles required by them.
 - Click on any role to see the list of users who currently have that role.
 - You can also see the roles granted to a particular user by selecting the user on the User List Page or by clicking the user name wherever you see it.

 Audit Compliance
 For each user, the system audits all modifications made to the user. You can see this audit on the Manage user page on the Audit tab. This becomes very important since now multiple
 people will have the capability to manage user rights.

 Authorizing users to manage DCMS rights
 It is intended that all FDC supervisors should have the ability to manage rights of DCMS users. You can manually grant the DCMS_MANAGER role to the first user who should be
 authorized to manage rights. This user can then assign this right to other users.




