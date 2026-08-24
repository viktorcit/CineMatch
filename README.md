# CineMatch.Api

CineMatch.Api is the backend for CineMatch, a service that helps two users
choose a movie or TV show to watch together.

Users create a session, add movies from TMDB,
vote with likes or dislikes, and the API finds matches
that both participants liked.

## Features

- User registration and login with ASP.NET Core Identity.
- Access and refresh token generation.
- Refresh token storage in the database.
- Movie and TV show search through the TMDB API:
  - by title;
  - by TMDB link;
  - with filtering by content type and year.
- Saving movies to the local database and adding them to an active session.
- Creating a session with a unique code.
- Joining a session as the second participant by code.
- Leaving a session and ending a session by its creator.
- Voting for movies inside a session.
- Getting:
  - all movies in a session;
  - matched movies;
  - a random movie from the matched list.
- Swagger UI for manual API testing.

## Tech Stack

- C#
- ASP.NET Core 8
- Entity Framework Core 8
- ASP.NET Core Identity
- SQLite
- JWT
- Swagger / Swashbuckle
- TMDB API

## Project Structure

- `Controllers/` - API endpoints for authentication, movies,
  sessions, and voting.
- `Services/` - application business logic.
- `Data/` - `AppDbContext`, DTOs, contracts, and role/admin seeding.
- `Entity/` - domain entities:
  `Movie`, `Session`, `Vote`, `RefreshToken`, `ApplicationUser`.
- `Migrations/` - EF Core migrations.
- `Configuration/` - JWT settings.
- `Enums/`, `Helpers/`, `Extensions/` - shared types
  and helper logic.

## Main API Areas

- `api/auth` - registration, login, and token refresh.
- `api/movies` - movie search, retrieval, saving, and deletion.
- `api/sessions` - session creation, joining, leaving, and ending.
- `api/sessionmovie` - session movies, matches,
  and a random matched movie.
- `api/vote` - like, dislike, and clearing session votes.

## Running the Project

The project is configured as an ASP.NET Core Web API on `.NET 8`.

```bash
dotnet restore
dotnet ef database update
dotnet run --launch-profile https
```

Swagger will be available at:

```bash
https://localhost:7232/swagger
```

or with the HTTP profile:

```bash
http://localhost:5175/swagger
```

For a full launch, the project needs configuration values
that are not stored in `appsettings.json`:

```bash
dotnet user-secrets set "AdminUsername" "admin_name"
dotnet user-secrets set "AdminPassword" "admin_password"
dotnet user-secrets set "JwtSettings:SecretKey" "your_jwt_secret"
dotnet user-secrets set "Tmdb:ApiToken" "your_tmdb_bearer_token"
```

The default database is the SQLite file `CineMatch.db`.

### Current Status

The main backend logic for movie search,
sessions, and voting is implemented.
Some classes for users, profiles, and administration
are present as placeholders and do not yet contain full functionality.
