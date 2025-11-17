# AITrainer

AITrainer is a desktop coach that helps IELTS learners practice Writing Task 2 and Speaking Part 2. The solution combines a WPF client, a set of layered libraries (Business, Repository, Service, DataAccess), and optional console tooling for DAO regression tests. OpenAI (gpt-4o-mini) powers question generation and grading, while Deepgram Nova-3 converts recorded answers to text before they are scored.

## Solution layout
| Project | Description |
| --- | --- |
| `AITrainer/` | WPF front end targeting .NET 8 (Windows only). Handles login, registration, profile editing, writing and speaking practice flows, and API key management UI. |
| `Business/` | Business logic wrappers (for example `AccountBusiness`) that coordinate validation and hashing before reaching the data layer. |
| `Repository/` | Entity models, session helpers, and DAO classes (e.g., `AccountDAO`, `APIKeyDAO`, `WritingQuestionDAO`, `SpeakingQuestionDAO`). |
| `Service/` | Integrations with OpenAI and Deepgram. `AIWritingService` and `AISpeakingService` expose async factories that fetch user-specific keys from the database before calling external APIs. |
| `DataAccess/` | EF Core DbContext (`AiIeltsDbContext`) and configuration file (`appsettings.json`) that store the SQL Server connection string. |
| `TestRunner/` | Console harness that runs DAO CRUD checks against the EF Core InMemory provider for deterministic local testing. |

## Features
- **Account onboarding and profile management**: register, log in, edit profile details, and manage API keys through dedicated WPF screens. Passwords are hashed before being stored, and the current user is shared via `AppSession`.
- **Writing practice automation**: generate IELTS Writing Task 2 prompts, capture essays with live word counts, and send submissions to OpenAI for JSON-formatted scores plus six-sentence feedback. Essays and grades are persisted through `WritingQuestionDAO`/`WritingAnswerDAO`.
- **Speaking practice automation**: produce IELTS Speaking Part 2 cards, record audio through NAudio, transcribe with Deepgram Nova-3, and grade transcripts with OpenAI. Results are stored in the Speaking tables for later review.
- **API key storage**: the Settings screen reads and writes `APIKey` rows so every user can supply their own OpenAI and Deepgram credentials without rebuilding the app.
- **Regression-friendly data layer**: the TestRunner project demonstrates how to exercise DAO operations in isolation, using `TestAiIeltsDbContext` to swap SQL Server for the EF Core InMemory provider.

## Prerequisites
1. **.NET 8 SDK** with WPF workload. The UI is Windows-only (`UseWPF=true`).
2. **SQL Server** instance that hosts the `AI_IELTS_DB` schema described in `DataAccess/AiIeltsDbContext.cs`. Update the connection string inside `DataAccess/appsettings.json` if you run a different instance or credential.
3. **OpenAI API key** with access to the `gpt-4o-mini` model.
4. **Deepgram API key** with access to the `nova-3` transcription model.

## Setup
1. Clone the repository and restore dependencies:
   ```bash
   git clone <your-fork-url>
   cd PRN_Assignment
   dotnet restore AITrainer.sln
   ```
2. Edit `DataAccess/appsettings.json` so that `DBDefault` points to your SQL Server. The DbContext was scaffolded database-first, so the target database must already contain the tables used by the Repository models (`Account`, `UserDetail`, `APIKey`, `WritingQuestion`, `WritingAnswer`, `SpeakingQuestion`, `SpeakingAnswer`).
3. Seed initial accounts and their API keys directly in SQL Server, or use the Sign Up and Settings screens after the app is running.
4. Install the Deepgram native dependency if you plan to use the Speaking workflow (the `Deepgram` NuGet package loads native binaries via `Library.Initialize`).

## Running the desktop app
1. Open `AITrainer.sln` in Visual Studio 2022 (17.8 or later) on Windows.
2. Set `AITrainer` as the startup project.
3. Press **F5**. The main page lets you navigate between Login, Sign Up, Home, Writing, Speaking, Profile, and Settings.
4. Before using Writing or Speaking features, visit Settings to enter valid OpenAI and Deepgram keys. These keys are saved per user in the `APIKey` table and reused through the async factory pattern inside `AIWritingService` and `AISpeakingService`.

## Running automated DAO checks
The repository currently lacks CI/CD automation, so developers share a lightweight console harness instead:
```bash
dotnet run --project TestRunner/TestRunner.csproj
```
This spins up `TestAiIeltsDbContext` (EF Core InMemory), performs create/read/update/delete calls for accounts and API keys, and prints verification logs. Use it whenever you touch DAO logic to avoid regressions.

## Troubleshooting
- **Unable to run WPF on non-Windows platforms**: `AITrainer` targets `net8.0-windows` and references Windows-specific assemblies. Build and run it on Windows only.
- **OpenAI/Deepgram errors inside Writing or Speaking screens**: confirm that the current user has both keys stored in the `APIKey` table. The async factory throws descriptive messages if a key is missing, and the UI surfaces those messages.
- **SQL connection issues**: ensure the connection string in `DataAccess/appsettings.json` matches an accessible SQL Server instance. Because the DbContext is database-first, migrations are not tracked in this repository.

## Next steps
Future improvements include adding CI/CD (GitHub Actions or Azure DevOps), publishing repeatable database migration scripts, and capturing UI automation or integration tests around the AI workflows.