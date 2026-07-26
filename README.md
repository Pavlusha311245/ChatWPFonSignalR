# SignalR Chat

A real-time intracollege chat application with an ASP.NET Core backend and a WPF desktop client.

## Features

- JWT-based registration, login, email confirmation, and password reset
- One-to-one and group chat over SignalR
- Tasks and document attachments in messages
- User administration and profile data

## Tech stack

- ASP.NET Core 5, SignalR, Entity Framework Core, and ASP.NET Identity
- SQL Server / LocalDB
- WPF with Material Design

## Run locally

Prerequisites: the .NET 5 SDK, SQL Server LocalDB (or another SQL Server instance), and Windows for the WPF client.

1. Set `ConnectionStrings:ServerContext` in `Backend/appsettings.json` for your database. The included value uses LocalDB.
2. Start the backend with its **IIS Express** profile. It listens on `https://localhost:44316`, which is the address configured in the client.
3. Run the `Client` project from Visual Studio.

The backend applies its Entity Framework migrations automatically when it cannot connect to the configured database.

## Structure

- `Backend/` — REST API, authentication, SignalR hub, and database migrations
- `Client/` — WPF desktop client
- `Library/` — shared project placeholder
