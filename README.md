# Quizzy

Quizzy is a small full-stack quiz application with a .NET 9 Web API backend, a Vue 3 frontend, and a SQLite database.

It supports:
- user registration and JWT authentication
- quiz gameplay across three difficulty levels: easy, medium, hard
- multiple choice and open-text questions
- similarity-based scoring for open-text answers
- quiz attempt history
- leaderboards per difficulty level
- admin management for quizzes and questions
- media attachments for questions and answers (image/audio stored in SQLite)

## Tech Stack
- Backend: ASP.NET Core 9 Web API, EF Core 9, SQLite, JWT Bearer Auth
- Frontend: Vue 3, Vite, Vuetify 3, Pinia, Vue Router
- Database: SQLite

## Seeded Data
On first run, the backend seeds:
- 1 admin account
- 3 quizzes: Easy, Medium, Hard
- 10 questions in each quiz

Admin credentials:
- Email: `admin@quizzy.com`
- Password: `Admin123!`

## Requirements
- .NET SDK 9
- Node.js 18+ and npm

## How to Run
Open two terminals.

### 1. Start the backend
```powershell
cd backend/Quizzy.API
dotnet restore
dotnet run
```

Backend API runs at:
- `http://localhost:5292`

On first startup, the SQLite database is created automatically and seed data is inserted.

### 2. Start the frontend
```powershell
cd frontend
npm install
npx vite
```

Frontend runs at:
- `http://localhost:5173`

The Vite dev server proxies `/api` requests to the backend.

## Build
Backend:
```powershell
cd backend/Quizzy.API
dotnet build
```

Frontend:
```powershell
cd frontend
npx vite build
```

## Main Features
- Landing page with leaderboards for all difficulty levels
- Authentication modal for sign in / sign up
- Quiz play flow with timer-based bonus scoring
- Open-text answer evaluation using similarity scoring
- Attempt results and history pages
- Admin dashboard for managing quizzes and questions

## Screenshots

See [images/README.md](images/README.md) for a visual tour of the application with annotated screenshots.

## Project Structure
```text
backend/Quizzy.API   ASP.NET Core API, EF Core models, services, controllers
frontend/            Vue 3 application
images/              Application screenshots and visual documentation
README.md            Project overview and run instructions
```
