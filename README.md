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
/UserRoles and /UserRoles/Manage?ParticipantId={id}: Organiser
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



*Features*

*Website*

Registration, login and role-based redirect (Organisers to the dashboard, Participants to their profile)
Profile page with name, gender, phone number and profile image upload
Organiser-only user and role management at /UserRoles
Navbar with avatar, first name and an API Docs link



*REST API*

JWT authentication with Participant and Organiser roles
Profile, events, categories, enrolments and results endpoints
Role-based access and ownership checks (an Organiser can only change their own events)
Swagger UI with descriptions, request shapes and response codes for every endpoint
Tech Stack
ASP.NET Core 9 (MVC, Razor Pages, Web API)
ASP.NET Core Identity with JWT bearer authentication
Entity Framework Core with SQL Server
Swashbuckle (Swagger / OpenAPI)
Bootstrap
Getting Started
Set DefaultConnection in appsettings.json.
Set the Jwt section in appsettings.json (Key must be 32+ characters; use User Secrets for anything beyond local development):
json
   "Jwt": { "Key": "your-long-random-key", "Issuer": "RaceDay", "Audience": "RaceDayUsers" }
Add a fallback avatar at wwwroot/images/default-avatar.png.
Make sure the project has a migration for the current model: Add-Migration <Name> in Package Manager Console.
Run the app. On startup it applies migrations, creates the Organiser and Participant roles and seeds an Organiser account.

If you change a model, add a new migration, otherwise startup fails with PendingModelChangesWarning.

Seeded Organiser (development only)
Email: organiser@guy.com
Password: P@55w0rd

Change or remove these before deploying.



*Using Swagger*

Open /swagger (or click API Docs in the navbar).
Expand POST /api/auth/login, click Try it out, enter an email and password, and Execute.
Copy the token from the response.
Click Authorize, paste the token only (Swagger adds "Bearer") and confirm.
Call any protected endpoint. To switch roles, use Authorize → Logout and paste the other user's token.

Tokens expire after 2 hours.

*API Endpoints*

*Auth*

POST /api/auth/register: register as Participant or Organiser (role field)
POST /api/auth/login: returns a JWT and the user's roles



*Profile (both roles)*

GET /api/profile: view your own profile
PUT /api/profile: update your own profile



*Events*

GET /api/events and GET /api/events/{id}: view events (both roles)
POST /api/events, PUT /api/events/{id}, DELETE /api/events/{id}: manage your own events (Organiser)
Each event has a name, description, date, location, distanceKm and eventType (Run, Walk or Cycle)



*Categories*

GET /api/events/{eventId}/categories: view categories (both roles)
POST and DELETE /api/events/{eventId}/categories: define categories such as "Under 20" or "21km" (Organiser)



*Enrolments*

POST /api/events/{eventId}/enrol: enrol under a chosen categoryId (Participant)
GET /api/enrolments/mine: your own enrolments (Participant)
GET /api/events/{eventId}/enrolments: all enrolments for your event (Organiser)



*Results*

POST and PUT /api/events/{eventId}/results: record finish time and position (Organiser)
GET /api/events/{eventId}/results: results for your event (Organiser)
GET /api/results/mine: your own results (Participant)
Response Codes
401: no or invalid token
403: wrong role, or not the owner of the event
404: event, category, enrolment or result not found
409: duplicate (already enrolled, category name exists, result already recorded)
400: invalid data
Website Usage

Assign a role: log in as an Organiser, open /UserRoles, click Manage Roles on a user, tick the roles and click Update Roles.

Edit your profile: click your name in the navbar, update your details, optionally upload an image (.jpg, .jpeg, .png, .webp, max 2 MB) and click Save.



*Project Structure*

Data/: ApplicationUser, ApplicationDbContext, migrations
Models/: Event, Category, Enrolment, Result, API request and response DTOs, profile image service
Controllers/Api/: AuthController, ProfileController, EventsController, CategoriesController, EnrolmentsController, ResultsController
Controllers/: HomeController, UserRoles
Areas/Identity/Pages/Account/: login with role redirect, profile page
Views/: website pages and navbar
Security Notes
API endpoints use JWT bearer auth, the website uses cookies.
Role rules use [Authorize(Roles = ...)], and event ownership is checked in the controllers.
Anyone can register as an Organiser through the API. Restrict this before real use.
Roles are stored in the token or cookie, so log in again after a role change.
Profile image uploads are checked by extension and size only.




--PART 3--
Overview
RaceDay is an ASP.NET Core MVC web application designed to manage racing events. The system allows organisers to create and manage events while participants can browse events, view categories, and register for races.

The application implements secure authentication and authorisation using ASP.NET Core Identity and JWT authentication, supports image uploads through Azure Blob Storage, and has been containerised using Docker for simplified deployment.




Features:

Authentication and Authorisation
ASP.NET Core Identity integration
JWT-based authentication
Secure login and registration
Role-based access control
Supported roles:
Organiser
Participant
Event Management




Organisers can:

Create events
Edit events
Delete events
Manage event details
Upload event banner images




Participants can:

View available events
View event information
Browse event categories
Event Categories
Categories linked to specific events
View categories for each race
Organised race registration structure
Azure Blob Storage Integration

Implemented Azure Blob Storage for media management.





Features:

Upload event banner images
Store images securely in Azure
Generate image URLs for display
Automatic image retrieval in event views





Swagger API Documentation:

Swagger has been configured to provide:
Interactive API testing
Endpoint documentation
JWT authentication testing
XML comments support

This enables developers to view and test available API endpoints directly through the browser.

Automated Testing
Implemented integration testing using xUnit.





Test Coverage Includes:

Authentication Tests
User registration
User login
Invalid role handling
Role assignment validation
Event Tests
Event creation
Event updates
Authorisation checks
Organiser ownership validation


Continuous Integration






GitHub Actions has been configured to automate:

Build validation
Test execution
Continuous integration workflow

This ensures application changes are validated automatically before deployment.

Technologies Used
Backend
ASP.NET Core MVC
ASP.NET Core Identity
Entity Framework Core
SQL Server
Security
JWT Authentication
Role-based Authorisation
Cloud Services
Azure Blob Storage
Documentation
Swagger / OpenAPI
Testing
xUnit
Integration Testing
DevOps
Docker
GitHub Actions
Database

The application uses SQL Server as the primary database.






Entity Framework Core is used for:

Database migrations
Data access
Relationship management
Future Improvements





Potential future enhancements include:

Online race registration
Payment integration
Email notifications
Race result tracking
Participant dashboards
HTTPS Docker configuration
Author





Developed as part of the RaceDay project to demonstrate:

ASP.NET Core MVC development
Authentication and authorisation
Cloud storage integration
API documentation
Automated testing
Containerisation using Docker
CI/CD practices



Run Docker Container:

docker run -d -p 8080:8080 --name RaceDay my-mvc-app

