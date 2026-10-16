*RaceDay*
Authentication, role-based access and user profiles for RaceDay, built on ASP.NET Core MVC, ASP.NET Core Identity and Entity Framework Core (SQL Server).

*Features*
Custom ApplicationUser (extends IdentityUser) with first name, last name, gender and profile image
Two roles: Organiser and Participant
Role-based login redirect: Organisers go to the dashboard, Participants to their profile
Organiser-only page to view users and assign or remove roles
Profile page with name, gender, phone number and image upload
Navbar shows the user's avatar and first name
Seeded Organiser account on a fresh database

*Key Files*
Data/ApplicationUser.cs: user model and Gender enum
Data/ApplicationDbContext.cs: Identity DbContext (Gender stored as a string)
Models/Interface/: IProfileImageService and ProfileImageService
Models/ViewModels/: UserRolesViewModel, ManageUserRolesViewModel
Controllers/UserRoles.cs: user and role management (Organiser only)
Controllers/HomeController.cs: includes OrganiserDashboard
Views/UserRoles/: Index and Manage views
Views/Shared/_LoginPartial.cshtml: navbar login area
Areas/Identity/Pages/Account/Login.cshtml.cs: role-based redirect
Areas/Identity/Pages/Account/Manage/Index.cshtml(.cs): profile page

*Getting Started*
1)Check DefaultConnection in appsettings.json.
2)Make sure a migration exists for the current model (Add-Migration <Name> in Package Manager Console).
3)Add a fallback avatar at wwwroot/images/default-avatar.png.
4)Run the app. On startup it applies migrations, creates the roles and seeds the Organiser.

If you change ApplicationUser or the DbContext, add a new migration or startup will fail with PendingModelChangesWarning.
Seeded Organiser (development only)
Email: organiser@guy.com
Password: P@55w0rd

Change or remove these before deploying, and move them to User Secrets or environment variables for anything beyond local use.

*Usage*
Assign a role: log in as an Organiser, open /UserRoles, click Manage Roles on a user, tick the roles and click Update Roles.
Edit your profile: click your name in the navbar, update your details, optionally upload an image (.jpg, .jpeg, .png, .webp, max 2 MB) and click Save.

*Routes*
/Identity/Account/Login and /Identity/Account/Register: public
/Identity/Account/Manage: signed-in users
/Home/OrganiserDashboard: Organiser
/UserRoles and /UserRoles/Manage?userId={id}: Organiser
Profile Images

*ProfileImage*
Service (scoped, registered in Program.cs) accepts only .jpg, .jpeg, .png and .webp files up to 2 MB, saves them to wwwroot/uploads/profiles/ under a GUID name, stores the relative URL in ProfileImagePath, and deletes the old image on replacement.

*Security Notes*
UserRoles uses [Authorize(Roles = "Organiser")] and anti-forgery validation on POST.
UseAuthentication() must come before UseAuthorization() in Program.cs.
Roles live in the login cookie, so log out and back in after a role change.

*Known Limitations*
Role updates remove all roles then re-add the selected ones (not atomic), and an Organiser can remove their own role.
Register page doesn't collect first name, last name or gender yet.
Image validation checks extension and size only, not file contents.

*Troubleshooting*
PendingModelChangesWarning: add a new migration.
Not redirected after login: log out and back in, and confirm the user has a row in AspNetUserRoles.
Access Denied on /UserRoles: the user isn't in the Organiser role.
Broken navbar avatar: default-avatar.png is missing.
"Hello !" with no name: FirstName is empty; update it on the profile page.
ApplicationUser not found: add using RaceDay.Data; (or @using RaceDay.Data in views).
Image upload arrives as null: the form needs enctype="multipart/form-data".
