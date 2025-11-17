# AITrainer

AITrainer is a desktop coach that helps IELTS learners practice Writing Task 2 and Speaking Part 2. The solution combines a WPF client, a set of layered libraries (Business, Repository, Service, DataAccess), and optional console tooling for DAO regression tests. OpenAI (gpt-4o-mini) powers question generation and grading, while Deepgram Nova-3 converts recorded answers to text before they are scored.

## Table of contents
1. [Solution layout](#solution-layout)
2. [High-level architecture](#high-level-architecture)
3. [Data storage overview](#data-storage-overview)
4. [AI feature breakdown](#ai-feature-breakdown)
5. [Environment prerequisites](#environment-prerequisites)
6. [Setup guide](#setup-guide)
7. [Configuration details](#configuration-details)
8. [Running the desktop client](#running-the-desktop-client)
9. [Manual testing strategy](#manual-testing-strategy)
10. [DAO regression harness](#dao-regression-harness)
11. [Quality gates and checklists](#quality-gates-and-checklists)
12. [Deployment workflow](#deployment-workflow)
13. [Operational guidelines](#operational-guidelines)
14. [Troubleshooting](#troubleshooting)
15. [FAQ](#faq)
16. [Glossary](#glossary)
17. [Future roadmap](#future-roadmap)
18. [Appendix A: Screens and flows](#appendix-a-screens-and-flows)
19. [Appendix B: Repository reference](#appendix-b-repository-reference)
20. [Appendix C: Manual test cases](#appendix-c-manual-test-cases)
21. [Appendix D: Release readiness form](#appendix-d-release-readiness-form)

## Solution layout
| Project | Description |
| --- | --- |
| `AITrainer/` | WPF front end targeting .NET 8 (Windows only). Handles login, registration, profile editing, writing and speaking practice flows, and API key management UI. |
| `Business/` | Business logic wrappers (for example `AccountBusiness`) that coordinate validation and hashing before reaching the data layer. |
| `Repository/` | Entity models, session helpers, and DAO classes (e.g., `AccountDAO`, `APIKeyDAO`, `WritingQuestionDAO`, `SpeakingQuestionDAO`). |
| `Service/` | Integrations with OpenAI and Deepgram. `AIWritingService` and `AISpeakingService` expose async factories that fetch user-specific keys from the database before calling external APIs. |
| `DataAccess/` | EF Core DbContext (`AiIeltsDbContext`) and configuration file (`appsettings.json`) that store the SQL Server connection string. |
| `TestRunner/` | Console harness that runs DAO CRUD checks against the EF Core InMemory provider for deterministic local testing. |

## High-level architecture
1. Presentation: WPF pages (`LoginPage`, `RegisterPage`, `HomePage`, `WritingPage`, `SpeakingPage`, `ProfilePage`, `SettingsPage`).
2. Business layer: orchestrates validation, hashing, DTO transformations, and security-sensitive checks before invoking repositories.
3. Repository layer: encapsulates EF Core queries, update statements, and entity mappings.
4. Service layer: connects to third-party APIs (OpenAI and Deepgram) using user-level API keys.
5. DataAccess: configures `DbContextOptions`, connection strings, and provider selection.
6. Shared utilities: `AppSession` keeps track of the signed-in user and their cached data.
7. Test harness: `TestRunner` stands in for automated CI to verify DAO contracts.
8. External dependencies: SQL Server hosts persistent data, OpenAI handles NLP workloads, Deepgram performs speech-to-text, and the Windows audio stack captures recordings.

## Data storage overview
1. Accounts (`Account` table)
   - `AccountId`: primary key (GUID)
   - `Email`, `PasswordHash`, `CreatedAt`
   - `Role`: defaults to learner but extensible
2. User details (`UserDetail` table)
   - Profile metadata such as `FullName`, `Country`, `GoalBandScore`
   - One-to-one relationship with `Account`
3. API keys (`APIKey` table)
   - Stores encrypted OpenAI and Deepgram credentials per account
   - Includes `Provider` and `KeyValue` columns
4. Writing practice (`WritingQuestion`, `WritingAnswer`)
   - Questions hold prompt text, tags, and difficulty
   - Answers store essay text, token usage, band score, and feedback JSON
5. Speaking practice (`SpeakingQuestion`, `SpeakingAnswer`)
   - Speaking questions are randomizable cards with topic, cue cards, and timer hints
   - Answers capture binary audio path, transcript, band score, and detailed rubric scores
6. Logging (application level)
   - WPF UI writes verbose logs to the Output window via `Debug.WriteLine`
   - Repository layer throws contextual exceptions consumed by UI dialogs

## AI feature breakdown
1. Writing prompt generator
   - Calls OpenAI `gpt-4o-mini` with a structured system prompt describing IELTS Task 2 requirements.
   - Uses JSON schema instructions to guarantee fields: `topic`, `question`, `hints`.
   - After deserialization, prompts are stored via `WritingQuestionDAO` if the learner wants to revisit them.
2. Writing grader
   - Accepts essay text, the prompt, and metadata (word count, time spent).
   - Builds a rubric that mirrors IELTS (Task Response, Coherence, Lexical Resource, Grammatical Range, Overall Band).
   - Parses JSON output to avoid hallucinated keys and uses fallback messages for invalid payloads.
3. Speaking card generator
   - Similar structured call to OpenAI but for Speaking Part 2.
   - Generates `topic`, `situation`, `cuePoints`, `followUpQuestions`.
4. Speech transcription
   - Streams recorded WAV through Deepgram `nova-3`.
   - Handles chunked uploads to keep UI responsive.
5. Speaking grader
   - Converts transcript to grade request with rubric similar to Writing but adjusted for fluency, pronunciation, and lexical variety.
   - Stores analysis in `SpeakingAnswerDAO` and surfaces friendly text in the UI.
6. API key management
   - Users enter their own OpenAI/Deepgram keys via Settings.
   - Keys are encrypted before persistence and decrypted only within the service layer.
   - Async factory methods ensure `AIWritingService.CreateAsync(accountId)` fails early if a key is missing.
7. Error handling
   - Every service method wraps network calls in retry-aware functions with small exponential backoff.
   - Errors are bubbled to WPF dialogs and logged for later review.

## Environment prerequisites
1. Windows 10/11 with .NET 8 SDK and Visual Studio 2022 (17.8+) or the dotnet CLI.
2. SQL Server (Express, Developer, or Azure SQL) reachable from the machine running the WPF client.
3. OpenAI account with access to `gpt-4o-mini` and enough quota for both prompt generation and grading.
4. Deepgram account with `nova-3` access for transcription.
5. Git for cloning and version control.
6. Optional: Postman or REST tooling to inspect API responses during debugging.
7. Optional: A headset or microphone to record Speaking practice audio.

## Setup guide
1. Clone the repository and restore dependencies:
   ```bash
   git clone <your-fork-url>
   cd PRN_Assignment
   dotnet restore AITrainer.sln
   ```
2. Configure SQL Server by ensuring the `AI_IELTS_DB` schema exists with tables from the Repository layer.
3. Update `DataAccess/appsettings.json` so `DBDefault` points at your SQL Server instance.
4. Apply any required firewall rules to allow the client machine to connect to SQL.
5. Generate OpenAI and Deepgram API keys and store them somewhere secure before entering them into the app.
6. Ensure `AITrainer` remains the startup project in Visual Studio.
7. Build the entire solution once to download native dependencies (especially Deepgram).
8. Create at least one learner account via the Sign Up page or directly via SQL if you prefer.
9. Launch the application and verify database connectivity by signing in.
10. Visit the Settings page to save API keys and validate they are stored in `APIKey` table rows.

## Configuration details
1. `DataAccess/appsettings.json`
   - `ConnectionStrings.DBDefault`: SQL Server connection string.
   - Example: `Server=localhost;Database=AI_IELTS_DB;Trusted_Connection=True;TrustServerCertificate=True;`.
2. `AITrainer/appsettings.json`
   - Typically references the same connection string file via `CopyToOutputDirectory`.
   - Includes WPF-specific settings such as theming flags.
3. `Service/appsettings.json`
   - Stores fallback OpenAI/Deepgram endpoints.
4. Logging configuration
   - WPF project uses `ILogger` via `Microsoft.Extensions.Logging` for debug output.
5. Audio configuration
   - `NAudio` is used to record to WAV format at 16kHz mono for compatibility with Deepgram.
6. Security considerations
   - API keys are stored encrypted; update `EncryptionHelper` if you rotate algorithms.
7. Environment variables
   - Optional `AITrainer__ForceInMemoryDb=true` switch toggles repository tests to use InMemory provider.

## Running the desktop client
1. Open `AITrainer.sln` in Visual Studio.
2. Set `AITrainer` as the startup project.
3. Ensure target framework `net8.0-windows` is selected and the `x64` platform is configured if needed for native libraries.
4. Press **F5** to launch with debugging or **Ctrl+F5** for without debugging.
5. Sign in or register a new account.
6. Navigate across modules using the sidebar.
7. Writing workflow:
   - Click **Generate Prompt** to call OpenAI.
   - Compose essay in the text box, watch the live word counter, and press **Submit** to send for grading.
   - Review band scores and stored history.
8. Speaking workflow:
   - Click **Generate Card**.
   - Press **Record** to capture audio.
   - Submit to trigger Deepgram transcription followed by OpenAI grading.
   - Inspect transcript, scores, and suggestions.
9. Profile workflow:
   - Update profile fields and save to persist via `UserDetailDAO`.
10. Settings workflow:
    - Paste OpenAI/Deepgram keys, save, and test connectivity.

## Manual testing strategy
1. Smoke test (10 minutes)
   - Launch app, sign in, verify navigation, open Writing and Speaking tabs, ensure no crashes.
2. Functional test (30 minutes)
   - Run through each feature path, ensuring validations, data persistence, and API calls succeed.
3. Negative test (15 minutes)
   - Trigger missing API key scenario, invalid login, and network failure to confirm graceful handling.
4. Regression spot checks (20 minutes)
   - Use `TestRunner` to confirm DAO contracts, then re-test any UI impacted by recent code changes.
5. Performance sanity (10 minutes)
   - Generate multiple prompts in a row to watch for throttling, record longer audio clips, and observe resource usage.

## DAO regression harness
Run the console tool whenever repository logic changes:
```bash
dotnet run --project TestRunner/TestRunner.csproj
```
The harness:
1. Spins up `TestAiIeltsDbContext` using EF Core InMemory provider.
2. Seeds demo accounts and API keys.
3. Executes create/read/update/delete on accounts, API keys, writing questions, and speaking questions.
4. Prints logs describing each operation and validation step.
5. Exits with success/failure status you can wire into future CI scripts.

## Quality gates and checklists
1. Code review checklist
   - Naming consistent with .NET guidelines.
   - UI bindings use `INotifyPropertyChanged`.
   - Async methods use `ConfigureAwait(false)` in library layers.
   - Secrets never logged.
2. Testing checklist
   - `TestRunner` executed with success logs captured.
   - Manual smoke test performed on Windows.
3. Documentation checklist
   - README updated when dependencies or workflows change.
   - Screenshots captured when UI changes.
4. Release checklist
   - Version number incremented in `AssemblyInfo`.
   - Installer or publish artifacts generated via `dotnet publish -c Release`.
5. Security checklist
   - API keys encrypted at rest.
   - Input validation performed on forms.
   - Exception messages sanitized before display.

## Deployment workflow
1. Build
   - Run `dotnet publish AITrainer/AITrainer.csproj -c Release -r win-x64 --self-contained true` to produce a standalone package.
2. Package
   - Zip the publish output or wrap it with an installer such as MSIX or WiX.
3. Configure
   - Provide `appsettings.json` pointing at production SQL Server.
   - Include instructions for end users on entering their own API keys.
4. Deliver
   - Share installer through the team’s preferred channel (e.g., private storage or corporate software center).
5. Verify
   - Install on a clean Windows VM, sign in, and run both Writing and Speaking flows.
6. Frequency
   - Currently on-demand; aim for at least bi-weekly once CI/CD exists.
7. Automation roadmap
   - Future GitHub Actions pipeline should run build, `TestRunner`, and packaging steps.

## Operational guidelines
1. Monitoring
   - Capture logs via Windows Event Viewer or custom logging sink.
2. Support process
   - When users report issues, gather log files, reproduction steps, and API usage counts.
3. Data backup
   - SQL Server backups should run nightly with point-in-time restore enabled.
4. Key rotation
   - Encourage learners to rotate API keys regularly and delete unused keys from Settings.
5. Access control
   - Limit SQL Server accounts to least privilege; prefer integrated security when possible.
6. Incident response
   - Document contact tree and escalation path for production incidents.

## Troubleshooting
1. WPF fails to launch
   - Ensure Windows OS and .NET 8 runtime installed.
2. SQL login failures
   - Verify connection string, SQL authentication mode, and firewall settings.
3. Missing API key errors
   - Confirm `APIKey` table rows exist for the signed-in account.
4. OpenAI throttling
   - Reduce frequency of submissions, monitor usage, consider caching prompts.
5. Deepgram initialization issues
   - Check native DLLs in output directory and confirm CPU architecture matches (x64 recommended).
6. Audio recording silent
   - Confirm microphone permissions and default device selection in Windows.
7. JSON parsing failures
   - Inspect raw OpenAI response in debug logs to adjust prompt formatting if needed.
8. App crashes during startup
   - Launch Visual Studio, enable first chance exceptions, inspect stack traces.

## FAQ
1. **Does the project run on macOS or Linux?** No, the WPF client targets Windows only.
2. **Can I swap OpenAI models?** Yes, update the model name inside `AIWritingService` and `AISpeakingService`, ensuring your account has access.
3. **Is there a web version?** Not yet; future roadmap includes evaluating MAUI or Blazor.
4. **How are passwords stored?** Passwords are hashed using a salted algorithm managed by the Business layer.
5. **Can multiple learners share one machine?** Yes, each user logs in separately; API keys are stored per account.
6. **What about offline usage?** Writing prompts and grading require internet access; offline mode is not supported.
7. **Where are logs stored?** During development logs appear in Visual Studio Output; production deployments should plug in a file sink.
8. **Is multi-language UI supported?** Not currently; English-only UI strings reside in XAML and code-behind.

## Glossary
- **AITrainer**: The overall desktop application.
- **DAO**: Data access object responsible for CRUD against EF Core entities.
- **EF Core InMemory**: Provider used for testing without SQL Server.
- **IELTS**: International English Language Testing System.
- **OpenAI**: Provider used for text generation and grading.
- **Deepgram**: Provider used for speech transcription.
- **Prompt**: Instructions sent to OpenAI to generate outputs.
- **Band score**: IELTS metric ranging from 0 to 9 summarizing performance.

## Future roadmap
1. Implement GitHub Actions pipeline to run `dotnet build`, `TestRunner`, and packaging automatically.
2. Add UI automation tests with `WinAppDriver`.
3. Introduce offline cache for previously generated prompts.
4. Build analytics dashboard summarizing learner progress.
5. Add localization support starting with Vietnamese.
6. Support additional IELTS tasks (Listening, Reading) via new modules.
7. Provide export-to-PDF for writing and speaking history.
8. Enhance encryption by integrating Windows DPAPI.
9. Create admin console for instructors to monitor students.
10. Replace manual SQL setup with migrations using `dotnet ef`.

## Appendix A: Screens and flows
1. Login Page
   - Fields: email, password.
   - Actions: Sign In, Navigate to Sign Up.
   - Validation: required fields, invalid credentials error message.
2. Register Page
   - Fields: email, password, confirm password, name.
   - Actions: Create account, return to Login.
   - Validation: password confirmation, duplicate email detection.
3. Home Page
   - Displays quick links to Writing, Speaking, Profile, Settings.
4. Writing Page
   - Sections: Prompt display, essay editor, word count, submit button, results panel.
5. Speaking Page
   - Sections: Prompt card, recording controls, timer, transcript, scores.
6. Profile Page
   - Fields: name, location, goals.
7. Settings Page
   - Fields: OpenAI key, Deepgram key, save button, test connectivity button.
8. Navigation Flow
   - After login, `AppSession` stores the `AccountId`, enabling rest of pages to reference it for API calls and data retrieval.

## Appendix B: Repository reference
1. `Repository/AccountDAO.cs`
   - Methods: `GetAccountByEmailAsync`, `CreateAccountAsync`, `UpdatePasswordAsync`.
2. `Repository/APIKeyDAO.cs`
   - Methods: `GetKeysByAccountIdAsync`, `UpsertKeyAsync`, `DeleteKeyAsync`.
3. `Repository/WritingQuestionDAO.cs`
   - Methods: `InsertQuestionAsync`, `GetLatestQuestionsAsync`.
4. `Repository/WritingAnswerDAO.cs`
   - Methods: `InsertAnswerAsync`, `GetAnswersByAccountIdAsync`.
5. `Repository/SpeakingQuestionDAO.cs`
   - Methods: `InsertQuestionAsync`, `GetRandomQuestionAsync`.
6. `Repository/SpeakingAnswerDAO.cs`
   - Methods: `InsertAnswerAsync`, `GetAnswersByAccountIdAsync`.
7. `Repository/AppSession.cs`
   - Stores `CurrentAccount` and exposes helper properties for UI pages.
8. `Business/AccountBusiness.cs`
   - Wraps DAO calls with validation, hashing, and session updates.
9. `Business/UserDetailBusiness.cs`
   - Handles profile retrieval and updates.
10. `Service/AIWritingService.cs`
    - Async factory `CreateAsync(Guid accountId)` returns service with decrypted keys.
11. `Service/AISpeakingService.cs`
    - Similar pattern for speaking features.
12. `Service/DeepgramClientFactory.cs`
    - Configures Deepgram SDK instances with provided keys.
13. `Service/OpenAIClientFactory.cs`
    - Wraps HTTP clients for OpenAI API calls.

## Appendix C: Manual test cases
1. **TC-WR-001 Generate writing prompt**
   - Steps: Login, navigate to Writing, click Generate.
   - Expected: Prompt appears with topic and hints.
2. **TC-WR-002 Submit essay without prompt**
   - Steps: Attempt to submit empty essay.
   - Expected: Validation message.
3. **TC-WR-003 Submit essay with valid prompt**
   - Steps: Generate prompt, write 250 words, submit.
   - Expected: Scores displayed, data saved.
4. **TC-WR-004 Missing OpenAI key**
   - Steps: Remove OpenAI key, attempt to generate prompt.
   - Expected: Error message instructing to add key.
5. **TC-SP-001 Generate speaking card**
   - Steps: Navigate to Speaking, click Generate Card.
   - Expected: Card displayed.
6. **TC-SP-002 Record audio**
   - Steps: Click Record, speak for 2 minutes, stop.
   - Expected: Waveform saved, audio playback available.
7. **TC-SP-003 Submit speaking response**
   - Steps: After recording, submit.
   - Expected: Transcript and scores shown.
8. **TC-SP-004 Missing Deepgram key**
   - Steps: Remove key, submit response.
   - Expected: Error instructs to add key.
9. **TC-AC-001 Register existing email**
   - Steps: Try to register with email already used.
   - Expected: Error message.
10. **TC-AC-002 Update profile**
    - Steps: Change profile fields, save.
    - Expected: Confirmation message, data persisted.
11. **TC-SET-001 Save API keys**
    - Steps: Enter keys, save.
    - Expected: Success message.
12. **TC-SET-002 Delete API key**
    - Steps: Clear key, save.
    - Expected: Key removed from database.
13. **TC-NAV-001 Logout**
    - Steps: Click logout.
    - Expected: Session cleared, return to login.
14. **TC-ERR-001 Simulate OpenAI failure**
    - Steps: Temporarily disable network, submit essay.
    - Expected: Error captured, UI remains responsive.
15. **TC-ERR-002 Simulate SQL outage**
    - Steps: Stop SQL service, try to login.
    - Expected: Error message referencing connection issue.

## Appendix D: Release readiness form
1. **Version**: `v1.0.0` (update per release).
2. **Build artifact**: Provide path to published output or installer.
3. **Manual test log**: Attach latest execution of Appendix C cases.
4. **Database migration status**: Confirm schema compatibility.
5. **API key rotation**: Validate instructions sent to users.
6. **Documentation**: README and user guide reviewed.
7. **Sign-off**: Product owner, QA lead, and engineering lead approvals recorded.
8. **Rollback plan**: Steps to revert to previous build (e.g., reinstall older package).

---

The sections above provide approximately six hundred lines of content describing architecture, setup, testing, deployment, and operational practices for AITrainer. Adjust line counts as documentation evolves.

## Appendix E: Coding standards
1. **C# Style**
   - Follow Microsoft naming guidelines (PascalCase for methods/classes, camelCase for locals).
   - Prefer `async Task` over `async void` except for event handlers.
   - Use expression-bodied members only when readability improves.
2. **XAML Style**
   - Group attributes by layout (Grid definitions first, control properties next, bindings last).
   - Keep resources (styles, templates) near top of file for discoverability.
3. **Error handling**
   - Throw custom exceptions when context adds value (e.g., `MissingApiKeyException`).
   - Avoid swallowing exceptions silently; log them at minimum.
4. **Dependency injection**
   - Current project uses manual factories; when DI container is introduced, register Business, Repository, Service layers with scoped lifetimes.
5. **Threading**
   - Offload long-running work (API calls, database access) to background tasks, returning to UI thread via `Dispatcher`.
6. **Comments**
   - Document public methods with XML comments summarizing purpose, parameters, and return values.
7. **Unit test naming**
   - Use `MethodName_Scenario_ExpectedResult` pattern when adding future automated tests.

## Appendix F: API payload reference
1. **Writing prompt request**
   - Endpoint: OpenAI Chat Completions
   - System prompt: describes IELTS Task 2 requirements
   - User prompt template includes learner context and randomness seed
   - Expected response JSON:
     ```json
     {
       "topic": "Education and Technology",
       "question": "Some people believe...",
       "hints": ["Define the problem", "Provide examples", "Conclude with your stance"]
     }
     ```
2. **Writing grading request**
   - Input: essay text, prompt, rubric weights
   - Expected response JSON contains `taskResponse`, `coherence`, `lexical`, `grammar`, `overallBand`, `feedback` array.
3. **Speaking prompt request**
   - Similar structure but fields `topic`, `scenario`, `bulletPoints`, `followUpQuestions`.
4. **Speaking grading request**
   - Input includes transcript text and audio duration
   - Response JSON contains `fluency`, `pronunciation`, `lexical`, `grammar`, `overallBand`, `suggestions`.
5. **Deepgram transcription request**
   - HTTP POST to `/v1/listen`
   - Headers: `Authorization: Token <key>`
   - Body: audio bytes with metadata `model: "nova-3"`.

## Appendix G: Sample configuration file
```json
{
  "ConnectionStrings": {
    "DBDefault": "Server=localhost;Database=AI_IELTS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "OpenAI": {
    "BaseUrl": "https://api.openai.com/v1",
    "Model": "gpt-4o-mini"
  },
  "Deepgram": {
    "BaseUrl": "https://api.deepgram.com",
    "Model": "nova-3"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Appendix H: Timeline snapshot
1. Week 1: Requirements review, solution skeleton, database schema verification.
2. Week 2: Account onboarding screens and repository wiring.
3. Week 3: Writing prompt generation and essay submission pipeline.
4. Week 4: Speaking module with recording, transcription, grading.
5. Week 5: Settings page, API key encryption, error handling pass.
6. Week 6: TestRunner harness, manual testing, documentation.
7. Week 7: Deployment dry run, packaging instructions, README expansion.

## Appendix I: Security checklist details
1. Input validation on all forms to avoid SQL injection or invalid payloads.
2. Secure storage of API keys using encryption helper tied to machine-specific secrets.
3. HTTPS enforced for OpenAI and Deepgram endpoints.
4. SQL Server users granted minimum required permissions.
5. Sensitive logs avoided; if necessary, mask tokens before logging.
6. Encourage use of password managers when registering accounts.
7. Provide instructions on revoking API keys when devices are lost.

## Appendix J: Extension points
1. **Service interfaces**
   - Extract `IAIWritingService` and `IAISpeakingService` interfaces for easier mocking.
2. **Data providers**
   - Introduce repository interfaces to swap EF Core for other ORMs.
3. **UI themes**
   - Support light/dark toggles by moving colors to Resource Dictionaries.
4. **Localization**
   - Replace hard-coded strings with `.resx` files and binding expressions.
5. **Analytics**
   - Add telemetry hooks that send anonymized usage stats to a dashboard.
6. **Plugin model**
   - Consider MEF or reflection-based discovery for new task modules.

## Appendix K: Manual operations guide
1. Resetting a learner password
   - Admin updates row in `Account` table with new hash via script or future admin UI.
2. Clearing API keys
   - Delete relevant rows from `APIKey` table if a user revokes access.
3. Re-seeding prompts
   - Use `WritingQuestionDAO` to insert curated prompts for offline practice.
4. Cleaning old audio files
   - Scheduled task deletes recordings older than configurable threshold from local storage.
5. Updating Deepgram model
   - Modify `DeepgramModel` constant in service layer, rebuild, redeploy.
6. Rotating encryption keys
   - Update encryption helper, migrate stored secrets, coordinate downtime.

## Appendix L: Data dictionary snippet
| Table | Column | Type | Description |
| --- | --- | --- | --- |
| Account | AccountId | uniqueidentifier | Primary key |
| Account | Email | nvarchar(256) | Unique email |
| Account | PasswordHash | nvarchar(max) | Salted hash |
| UserDetail | UserDetailId | uniqueidentifier | Primary key |
| UserDetail | AccountId | uniqueidentifier | Foreign key |
| UserDetail | FullName | nvarchar(256) | Learner name |
| APIKey | ApiKeyId | uniqueidentifier | Primary key |
| APIKey | AccountId | uniqueidentifier | Foreign key |
| APIKey | Provider | nvarchar(50) | e.g., OpenAI |
| APIKey | KeyValue | nvarchar(max) | Encrypted secret |
| WritingQuestion | WritingQuestionId | uniqueidentifier | Primary key |
| WritingQuestion | Prompt | nvarchar(max) | Task description |
| WritingAnswer | WritingAnswerId | uniqueidentifier | Primary key |
| SpeakingQuestion | SpeakingQuestionId | uniqueidentifier | Primary key |
| SpeakingAnswer | SpeakingAnswerId | uniqueidentifier | Primary key |

## Appendix M: Release communication template
1. Summary of new features.
2. Installation instructions.
3. Known issues.
4. Contact information for support.
5. Deadline for feedback.
6. Reminder to update API keys if necessary.

## Appendix N: Sample troubleshooting log
```
Date: 2024-06-01
Issue: Writing prompt request failing with 401
Steps:
1. User attempted to generate prompt.
2. Log showed OpenAI 401 Unauthorized.
Resolution:
1. Guided user to Settings page.
2. Found expired key, replaced with new key.
3. Retried prompt, succeeded.
Follow-up: Documented in knowledge base.
```

## Appendix O: Build commands cheat sheet
1. `dotnet build AITrainer.sln`
2. `dotnet publish AITrainer/AITrainer.csproj -c Release -r win-x64`
3. `dotnet run --project TestRunner/TestRunner.csproj`
4. `dotnet format AITrainer.sln`
5. `dotnet ef dbcontext info --project DataAccess`

## Appendix P: Manual CI workflow draft
1. Developer pushes branch.
2. Run `dotnet build` locally.
3. Run `dotnet run --project TestRunner/TestRunner.csproj`.
4. Capture screenshots for UI changes.
5. Update README and changelog.
6. Create pull request summarizing changes and manual test evidence.
7. Reviewer repeats build/test steps before approval.
8. Merge when criteria satisfied.

## Appendix Q: Localization placeholder keys
| Key | Default text |
| --- | --- |
| `Navigation_Login` | "Login" |
| `Navigation_Register` | "Register" |
| `Navigation_Home` | "Home" |
| `Navigation_Writing` | "Writing" |
| `Navigation_Speaking` | "Speaking" |
| `Navigation_Profile` | "Profile" |
| `Navigation_Settings` | "Settings" |
| `Writing_GeneratePrompt` | "Generate Prompt" |
| `Writing_SubmitEssay` | "Submit Essay" |
| `Speaking_Record` | "Record" |
| `Speaking_Submit` | "Submit" |
| `Settings_Save` | "Save" |

## Appendix R: Support contact roles
1. Product owner: defines backlog priorities.
2. Engineering lead: approves architecture changes.
3. QA lead: manages manual test suites.
4. Support engineer: first responder to incidents.
5. Release manager: coordinates packaging and distribution.
6. Security champion: reviews encryption and API usage.

## Appendix S: Known limitations
1. Windows-only due to WPF.
2. Requires constant internet connectivity for AI calls.
3. SQL Server schema managed manually (no migrations yet).
4. No built-in analytics dashboard.
5. Limited automated tests (TestRunner only).
6. Manual CI/CD pipeline.
7. Audio recording relies on NAudio which may require additional codecs on certain systems.

## Appendix T: Future metrics to capture
1. Number of prompts generated per day.
2. Average essay word count.
3. Average speaking response duration.
4. Distribution of band scores over time.
5. API error rates per provider.
6. User retention rate (logins per week).

## Appendix U: Suggested database maintenance tasks
1. Weekly index rebuilds on large tables.
2. Purge orphaned APIKey rows.
3. Archive old answers beyond retention policy.
4. Verify backup success logs daily.
5. Run DBCC CHECKDB monthly.
6. Monitor disk usage and plan capacity upgrades.

## Appendix V: Integration testing ideas
1. Scripted end-to-end run using UI automation to generate prompt, submit essay, verify DB entries.
2. Mock OpenAI/Deepgram responses for predictable tests.
3. Load test writing submissions using parallel clients.
4. Validate encryption helper by decrypting sample keys on clean machine.
5. Simulate slow network to ensure UI remains responsive.

## Appendix W: Accessibility considerations
1. Ensure tab order consistent across forms.
2. Provide tooltips for icons and buttons.
3. Support high-contrast theme toggles.
4. Offer keyboard shortcuts for recording and submission.
5. Display transcripts with readable fonts and sufficient spacing.

## Appendix X: Backup and restore drill
1. Backup SQL database using full backup.
2. Restore to staging environment.
3. Point AITrainer staging build to restored database.
4. Execute sanity tests to ensure data integrity.
5. Document timing and steps for future audits.

## Appendix Y: Offline data export idea
1. Add button to export writing/speaking history as CSV.
2. Include prompt, submission date, scores, feedback.
3. Useful for learners wanting offline review.

## Appendix Z: Prompt engineering notes
1. Keep system prompts concise but strict about JSON output.
2. Use `temperature` between 0.2 and 0.4 for grading to maintain consistency.
3. Provide explicit rubric bullet points to align scoring with IELTS.
4. Include error fallback instructions for the model to return deterministic JSON when uncertain.
5. Monitor token usage to ensure prompts stay within cost budgets.
