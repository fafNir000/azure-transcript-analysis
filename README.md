# Transcript Analysis — Advanced Multi-Language Segmenter & PII Extraction Engine
### Complete Enterprise Technical Reference Manual for Defense & Presentation

Welcome to the comprehensive technical and architectural documentation for the **Transcript Analysis** platform. This system is a high-availability, full-stack, secure platform designed for the intelligent processing, speaker role segmentation, and Personally Identifiable Information (PII) extraction of long call center transcripts in English (`en`) and Armenian (`hy`).

This document serves as an exhaustive guide across all phases of the Software Development Life Cycle (SDLC), providing a deep technical review of the platform's backend services (built on ASP.NET Core and .NET 8.0) and its frontend single-page application (built on React 19, TypeScript, and Ant Design v6). It is structured to be "slide-ready" and fully comprehensive, preparing developers and stakeholders for a successful project defense.

---

## 🎯 1. PROJECT OVERVIEW & DEEP BUSINESS VALUE ANALYSIS

### The Raw Transcript Challenge
In high-volume call centers, customer helpdesks, and support environments, raw text representations of audio (voice-to-text transcriptions) are generated at an exponential rate. However, these documents are highly unstructured, presenting three massive bottlenecks:
1. **Critical Compliance & Privacy Vulnerabilities**: Raw customer calls are highly saturated with Personally Identifiable Information (PII) such as full names, home addresses, phone numbers, email addresses, and United States Social Security Numbers (SSN). Storing or exposing these in raw logs directly violates international regulations, including the General Data Protection Regulation (**GDPR**), the Health Insurance Portability and Accountability Act (**HIPAA**), and cybersecurity frameworks like **ISO 27001/27701**.
2. **Structural Incoherence**: Standard automated speech-to-text systems produce a single continuous block of text without delineating who said what. It is extremely exhausting for operators, supervisors, and audits to read conversations lacking speaker separation (e.g., distinguishing the customer support agent from the client caller).
3. **Complex Multi-Language & Dialect Mixing**: Real-world operations in global support centers involve language switching. Armenian call transcripts represent a unique technical challenge, frequently featuring Western and Eastern dialects, transliterated characters (using Latin letters to spell out Armenian phonetically), Russian-Armenian code-switching, and technical English vocabulary integrated into local conversation.

### The Business Solution
The **Transcript Analysis** platform addresses these challenges by offering a scalable, robust, fully automated software pipeline:
* **Context-Aware Speaker Separation (Diarization)**: Automatically parses and formats continuous texts into discrete conversation bubbles labeled `Agent` and `Caller` using a multi-mode architecture (combining low-latency local parsing rules with Azure OpenAI GPT-5-mini contextual analysis).
* **Deep Neural PII Detection**: Extracts and groups sensitive information into five core variables (Name, Address, Phone Number, Email, and SSN) utilizing deep learning Named Entity Recognition (NER) models from Azure AI Language.
* **Streamlined Operator Dashboard**: Provides customer service operators and compliance officers with a modern, reactive panel to submit texts up to 50,000 characters, instantly copy extracted attributes with a single click, and view a locally persisted audit history.

---

## 🗺️ 2. REPOSITORY ARCHITECTURE MAP

The directory structure of the project strictly separates concerns using clean architecture principles, ensuring high modularity and mockable interfaces.

```
/ (Workspace Root)
├── Controllers/                  # HTTP API Endpoint Routing Layer
│   └── TranscriptController.cs   # Orchestrator handling incoming requests, validation, and responses
├── Models/                       # Data Transfer Objects (DTOs) & Strongly-Typed Domain Models
│   ├── ConversationTurn.cs       # Object representation of a single chat bubble (Role + Content)
│   ├── ExtractedAttributes.cs    # Strongly-typed schema containing the 5 extracted PII variables
│   ├── TranscriptRequest.cs      # Schema validating incoming text payloads and target language
│   └── TranscriptResponse.cs     # Combined payload returned to frontend (Chat turns + PII)
├── Services/                     # Business Logic & Core Integrations Layer
│   ├── AzureLanguageService.cs   # Client wrapper for Azure AI Language (PII NER & Smart Chunking)
│   ├── AzureOpenAIService.cs     # Client wrapper for Azure OpenAI (GPT-5-mini for role assignments)
│   ├── SpeakerRoleService.cs     # Three-tiered coordinator for Speaker Role Identification
│   └── TranscriptAnalysisService.cs # Post-processing pipeline (SSN regex, de-duplication, confidence filtering)
├── Resources/                    # Static Server-Side Assets
│   └── SpeakerRolePrompt.txt     # Highly optimized system instruction prompt for Azure OpenAI
├── Tests/                        # Automated High-Coverage Integration Suite
│   └── TranscriptAnalysisTests.cs# xUnit suite checking validators, SSN overrides, and chunking boundaries
├── Properties/
│   └── launchSettings.json       # Local Kestrel profiles, port bindings, and environment configs
├── data/                         # Local server-side audit folder (JSON & raw text records, git-ignored)
├── docs/                         # Extended technical documentation and cloud research logs
│   ├── ApiDocumentation.md       # API endpoint specification and parameter descriptions
│   ├── Azure_PII_NER_Endpoint_Research.md # Azure AI Language model capabilities logs
│   ├── speaker-roles.md          # Comprehensive breakdown of speech segmentation theories
│   └── TestResults.md            # Automated test run execution outputs
├── frontend/                     # Client-Side Single Page Application (SPA)
│   ├── public/                   # Public static files (Vector icons, browser manifest, favicons)
│   ├── src/
│   │   ├── api/                  # Network client layer
│   │   │   ├── client.ts         # Axios client instance with central interceptors & timeout configs
│   │   │   └── transcript.ts     # Network mapping endpoints
│   │   ├── components/           # Reusable Presentation & Form Components
│   │   │   ├── AppLayout.tsx     # Application outer boundary, navigation, and sidebar layout
│   │   │   ├── AttributesCard.tsx# Interactive PII visualization grid with copy-to-clipboard buttons
│   │   │   ├── ConversationView.tsx # Messenger-style dialogue bubble renderer with alignment
│   │   │   └── TranscriptForm.tsx # Input form bounded by Yup-validation schemas
│   │   ├── hooks/                # React custom hooks
│   │   │   └── useTranscripts.ts # TanStack React Query connection for mutation and caching state
│   │   ├── pages/                # High-level screens
│   │   │   ├── NewTranscriptionPage.tsx # Analysis dashboard (input forms & live responsive panels)
│   │   │   ├── HistoryPage.tsx   # Detailed archive of previous analysis sessions with search
│   │   │   └── TranscriptionDetailsPage.tsx # Detailed view of cached items
│   │   ├── storage/              # Client-side persistence engine
│   │   │   └── history.ts        # LocalStorage wrapper (FIFO, capacity restricted to 100 entries)
│   │   ├── App.tsx               # Routing and React Router DOM configurations
│   │   ├── index.css             # Tailwind CSS entries and global custom scrollbars
│   │   ├── main.tsx              # Front-end bootstrapping, React Query, and Ant Design token setup
│   │   └── types.ts              # TypeScript interface declarations mirroring C# backend contracts
│   ├── vite.config.ts            # Vite compiler configuration, including dev-proxy rules
│   └── tsconfig.json             # Compiler directives guaranteeing strict TypeScript safety
├── Task_2_TranscriptAnalysis.csproj # C# Project Manifest with MSBuild dependencies
├── Program.cs                    # Dependency Injection (DI) bootstrapper & request pipeline configuration
├── appsettings.json              # Service parameters, credentials, and model descriptors
└── package.json                  # Root configurations, Husky triggers, and pre-commit hooks
```

---

## ⚙️ 3. ARCHITECTURAL DATA FLOW & CORE PIPELINE

The diagram below represents the exact flow of data through the platform, showcasing how user input is transformed into structured, redacted speaker conversations.

```
+------------------------------------------------------------+
|                     1. OPERATOR BROWSER                    |
|  - Operator inputs transcript (Up to 50,000 characters)     |
|  - Submits via NewTranscriptionPage Form                  |
+-----------------------------+------------------------------+
                              |
                     HTTPS POST /api/transcript/analyze
                              |
                              v
+------------------------------------------------------------+
|                  2. TRANSCRIPT CONTROLLER                  |
|  - Performs validation (non-empty, <= 50k chars)           |
|  - Resolves services from Dependency Injection             |
+-----------------------------+------------------------------+
                              |
                              +-----+
                              |     |
                              |  [Step 3: Define Speaker Roles]
                              |     v
+-----------------------------v------------------------------+
|                     3. SPEAKER ROLE SERVICE                |
|  Matches speaker patterns using a 3-tiered fallback rule:  |
|  - Mode 1: Local Regex parser (looks for "Agent:", etc.)   |
|  - Mode  mode 2: Azure OpenAI GPT-5-mini Contextual Parse   |
|  - Mode 3: Alternate Line deterministic sequence fallback  |
+-----------------------------+------------------------------+
                              |
                              +-----+
                              |     |
                              |  [Step 4: Extract Sensitive Entities]
                              |     v
+-----------------------------v------------------------------+
|                   4. AZURE LANGUAGE SERVICE                |
|  - Detects characters limit (max 5,120)                    |
|  - Applies smart chunking at '\n' limits (< 5,000 chars)   |
|  - Ships batch of tasks in parallel to Azure AI Language  |
|  - Reassembles entity array and recalculates offsets      |
+-----------------------------+------------------------------+
                              |
                              +-----+
                              |     |
                              |  [Step 5: Post-Process & Validate]
                              |     v
+-----------------------------v------------------------------+
|                 5. TRANSCRIPT ANALYSIS SERVICE             |
|  - Filters out entities with low confidence scores (< 0.5) |
|  - Resolves SSN regex mismatch (Reclassifies SSN errors)   |
|  - Normalizes and de-duplicates variables list             |
+-----------------------------+------------------------------+
                              |
                              +--------------------+
                              |                    |
                              v                    v
+----------------------------------+ +----------------------------------+
|      6A. ASYNC AUDIT LOGGER      | |       6B. CLIENT RESPONSE        |
|  - Dispatches file-write task    | |  - Serializes response to JSON  |
|  - Writes structured report to   | |  - Returns HTTP 200 OK          |
|    /data/ folder (unblocked)     | |  - Updates LocalStorage History |
+----------------------------------+ +----------------------------------+
```

---

## 🛠️ 4. THE TECHNOLOGY STACK & ARCHITECTURAL ADVANTAGES

Every tool in our workspace was carefully selected for performance, type-safety, resilience, and fast iteration.

### Backend Stack: C# .NET 8 & ASP.NET Core
* **Microsoft .NET 8.0 SDK (LTS)**: Chosen for its leading cross-platform performance, optimized memory model, fast garbage collection, and native support for asynchronous tasks (`Task`/`await`).
* **ASP.NET Core Web API**: Offers lightweight, controller-based routing. Its native Dependency Injection (DI) container promotes loosely coupled services and allows easy hot-swapping of production APIs with test mocks.
* **Azure AI Cognitive Services Suite**:
  * **`Azure.AI.TextAnalytics` (v5.3.0)**: The official SDK for Entity Recognition. Handles TLS, authentication keys, and error recovery out-of-the-box.
  * **`Azure.AI.OpenAI` (v2.0.0)** & **`OpenAI` (v2.12.0)**: Leverages OpenAI's official SDK supporting **Structured JSON Outputs**. This guarantees that our GPT-5-mini engine yields deterministic, structured JSON schemas without unwanted chat preamble or markdown formatting blocks.
* **Swashbuckle Swagger UI (v6.6.2)**: Automatically translates C# controller decorators into a live OpenAPI playground, hosted locally at `/swagger`, which improves development and debugging speed.
* **xUnit & Microsoft.AspNetCore.Mvc.Testing**: Provides an in-memory HTTP test-server context via `WebApplicationFactory`, allowing high-speed, cost-free integration testing without spinning up live database clusters.

### Frontend Stack: React 19, TypeScript 6 & Vite 8
* **React 19**: Leverages advanced state rendering and a highly optimized Virtual DOM, ensuring smooth transitions between chat bubbles and instant loading states during heavy operations.
* **TypeScript 6**: Implements compile-time types checking. It guarantees that our client data models are always aligned with our backend C# contracts, eliminating runtime errors during client-server communication.
* **Vite 8**: Offers incredibly fast Hot Module Replacement (HMR) and relies on Rollup to package assets into optimized, compressed chunks.
* **Ant Design 6 (antd)**: An enterprise-grade UI library that provides polished, responsive components (e.g. textareas, select inputs, sidebars, copyable elements, and spinners) matching modern web accessibility standards.
* **TanStack React Query 5**: Replaces boilerplate state-management code. It coordinates network status, prevents multiple identical requests, and manages client caching.
* **Emotion (`@emotion/react`, `@emotion/styled`)**: A robust CSS-in-JS utility enabling modular, scoped component styling that can dynamically adapt based on React state variables (such as speaker alignments).

---

## 🚀 5. THE 4 DEVELOPMENT STAGES (SDLC PHASES)

Here is the chronology of how our system evolved from initial research to a fully functional, production-ready, containerized application.

---

### STAGE 1: RESEARCHING (FEASIBILITY, LIMITATIONS & PROMPT ENGINEERING)

Before writing any code, we conducted rigorous research to map cloud capabilities against our business requirements.

#### 1. Sifting Through Azure AI Cognitive Options
We evaluated two separate cognitive endpoints under the Azure Cognitive Services ecosystem:
1. *Azure Conversation PII (Transcript-Optimized)*:
   * *The Problem*: Although explicitly designed for dialogues, deep-dive testing revealed that its public release **does not support the Armenian language (`hy`)**.
2. *Azure Text PII Named Entity Recognition*:
   * *The Finding*: Our research confirmed that Text PII features deep multilingual dictionary mappings, with outstanding support for both English and Armenian.
   * *The Decision*: We chose the universal Text PII endpoint and handled conversation structure reconstruction directly within our custom backend C# service layer.

#### 2. Architecting the Chunking Pipeline
Our testing uncovered a critical limitation: Azure's Text PII synchronous REST endpoint strictly blocks documents exceeding **5,120 characters**. However, our call transcripts can be up to **50,000 characters** (approx. 45 minutes of spoken dialogue).
* *The Solution*: We engineered a smart chunking pipeline on the server. The text is divided into segments of under 5,000 characters. Instead of a hard slice that could cut sensitive entities (like phone numbers or addresses) in half, the algorithm scans backward and cuts at the last carriage return (`\n`). This ensures high classification accuracy.

#### 3. Optimizing the GPT-5-mini Speaker Identification Prompt
To segment raw files where speaker indicators are missing, we selected the fast and cost-effective `gpt-5-mini` model on Azure OpenAI. The challenge was Armenian-mixed dialogue, often featuring Russian-Armenian code-switching (e.g., mixing Russian technical terms or slang into Armenian speech).
* *The Prompt Design*: We crafted a system prompt (`Resources/SpeakerRolePrompt.txt`) that enforces strict rules:
  1. Carefully analyze dialogue context to assign speaker roles (`Agent` vs `Caller`).
  2. Parse mixed, conversational Armenian dialect structures.
  3. Respond strictly in structured JSON, mapping turns to `role` and `text`, without any extra conversational text.

---

### STAGE 2: BACKEND (IMPLEMENTING ROBUST C# SERVICES)

We implemented the backend services using clean architecture patterns in ASP.NET Core, focusing on resilience, speed, and safety.

#### 1. Creating the Three-Tiered SpeakerRoleService
To minimize cloud costs and guarantee application uptime, we implemented a **Multi-Mode Fallback Strategy**:
* **Mode 1 (Explicit Regex Parser - Cost: FREE, Speed: <1ms)**: Scans the incoming raw text for explicit labels (such as `Agent:`, `Caller:`, `Օպերատոր:`, `Հաճախորդ:`). If found, it parses the lines locally on the fly without making cloud calls.
* **Mode 2 (Azure OpenAI GPT - Cost: Minimal, Speed: 1.5s)**: If no tags exist, it sends the raw file to `AzureOpenAIService` for semantic speaker identification.
* **Mode 3 (Deterministic Fallback - Cost: FREE, Speed: <1ms)**: If Azure OpenAI experiences a network timeout or quota limit, the backend catches the exception and falls back to a deterministic, alternate-line layout (Line 1 = Agent, Line 2 = Caller). This ensures the API remains robust under any conditions.

#### 2. Resolving the US SSN Misclassification Mismatch
During integration testing, we noticed that Azure AI Language frequently misclassified US Social Security Numbers (`XXX-XX-XXXX`) as standard `PhoneNumber` types due to their hyphens.
* *The Fix*: In `TranscriptAnalysisService.cs`, we introduced a dedicated regular expression validator:
  ```csharp
  private static readonly Regex SsnRegex = new(@"^\d{3}-\d{2}-\d{4}$", RegexOptions.Compiled);
  ```
  If any extracted entity is classified as a `PhoneNumber` but matches this pattern, our service automatically corrects its category to `SocialSecurityNumber` and routes it to the correct DTO property, fixing the cloud provider's error.

#### 3. Smart Offset Tracking & Chunk Reassembly
When rebuilding the PII results from multiple chunked text requests, we implemented custom offset calculations. For each chunk $k$, we add the cumulative sum of character lengths of all previous chunks ($0$ to $k-1$) to the entity offsets returned by Azure:
$$\text{CorrectedOffset} = \text{AzureOffset} + \sum_{i=0}^{k-1} \text{Length}(\text{Chunk}_i)$$
This ensures that the front-end can highlight and link sensitive words back to their exact positions in the original full-text document.

#### 4. Safe Asynchronous Audit Logging
To keep our API fast, we offload the audit report writing task. When an analysis completes, a structured file is generated in `/data/`. The writing process is wrapped in a protective `try-catch` block; if a disk error occurs (due to permissions or storage limits), the error is logged locally, and the API proceeds with returning the JSON response to the user. This ensures disk issues never block the client experience.

---

### STAGE 3: FRONTEND (POLISHING THE OPERATOR EXPERIENCE)

We built the frontend to be highly reactive, visually intuitive, and efficient, ensuring optimal productivity for support agents.

#### 1. Managing Network State with TanStack React Query
We wrapped our API calls in a custom React Hook (`hooks/useTranscripts.ts`) linked with TanStack React Query. This hook monitors the mutation state:
* While waiting for the API response, it disables the text area, spins loading wheels, and displays a responsive progress bar.
* Once successful, it automatically marks our history caches as stale, prompting the history log to refresh.

#### 2. Visualizing Chat Dialogues Natively
We designed `ConversationView.tsx` with customized Emotion styles to display transcripts like a modern messaging app.
* Support agent lines are left-aligned in a clean white bubble with light grey borders.
* Customer/Caller lines are right-aligned in a vivid blue bubble (`#2f54eb`) with crisp white text. This makes it easy for supervisors to review call flows in seconds.

#### 3. Enhancing Operator Workflows (PII Cards)
The `AttributesCard.tsx` component groups and displays the 5 core PII fields.
* Each detected attribute includes an icon and a single-click copy utility built on the browser's clipboard API:
  ```typescript
  <Typography.Text copyable={{ text: attributeValue }}>
  ```
* This feature allows operators to quickly transfer verified customer details directly into CRM or ticketing databases, saving time and reducing manual errors.

#### 4. Offline-First History Tracking
To avoid server database overhead while preserving user history across browser restarts, we built a client-side database wrapper (`storage/history.ts`) utilizing the browser's `localStorage`:
* Every analysis is assigned a client-side UUID and ISO timestamp, then pushed into `localStorage`.
* We implemented a FIFO cache capped at 100 entries. If the log exceeds 100 entries, the oldest entries are purged automatically. This keeps the browser's storage consumption well below standard limits.

---

### STAGE 4: TESTING & DEPLOYMENT (QUALITY ASSURANCE & DOCKERIZATION)

To prepare the application for production, we implemented rigorous testing and deployment processes.

#### 1. High-Fidelity API Mocking (Fake Service Pattern)
To run automated tests without relying on active Azure endpoints or spending API credits, we created a mock-based test suite:
* We built a nested class called `FakeAzureLanguageService` inside `TranscriptAnalysisTests.cs`.
* Using Microsoft’s official `TextAnalyticsModelFactory` class, our mock generates real, typed `CategorizedEntity` response schemas. This allows us to test complex backend logic (like chunking offsets, SSN regex correction, and Armenian translation) locally and instantly.

#### 2. Expanding Integration Test Coverage
We wrote **11 robust automated integration tests** using xUnit. These tests cover:
* Input validation limits (verifying that texts exceeding 50,000 characters return `400 Bad Request`).
* Empty-string handling.
* The chunking pipeline (ensuring long documents are sliced, analyzed, and successfully reassembled).
* Correct classification of SSNs.
* Robust error mapping (verifying that the API returns appropriate HTTP codes when mock services simulate network timeouts or invalid credentials).
* These tests run in-memory via `WebApplicationFactory` and execute on local systems or CI/CD pipelines via `dotnet test`.

#### 3. Enforcing Quality Control (Husky & Pre-commit Hooks)
To keep the code clean and uniform across team members, we configured Husky git hooks:
* Every time a developer attempts to commit code (`git commit`), Husky runs `lint-staged` on the modified files.
* ESLint checks the codebase for syntactic errors, while Prettier automatically formats files, ensuring a consistent code style across the project. If any check fails, the commit is blocked.

#### 4. Containerization and Production Hosting (Docker & Render)
* **Dockerization**: We created a multi-stage `Dockerfile`. The build environment uses the full .NET SDK to compile the C# backend, while the runtime environment uses a minimal ASP.NET Core runtime image to keep the final container size small.
* **Hosting**: The backend is containerized and hosted on the **Render PaaS** platform, with API secrets managed securely through environment variables. The React frontend is compiled into optimized static assets and served via the **Netlify CDN** to ensure low-latency deliveries. CORS policies are restricted to allow requests only from our Netlify domain, securing the API against unauthorized cross-site access.

---

## 💡 6. PRESENTATION SLIDES & PROJECT DEFENSE REFERENCE

This section serves as a slide-by-slide reference guide, complete with speaker notes and common defense questions, designed to help developers deliver an outstanding presentation.

---

### SLIDE 1: TITLE & TEAM
* **Header**: Transcript Analysis: Secure Multi-Language Dialogue Segmentation and PII Redaction
* **Visual**: Clean corporate slide, logo, and list of presenters.
* **Content**:
  * Modern contact-center automation.
  * Integration of ASP.NET Core (.NET 8) with React 19, Azure AI Language, and Azure OpenAI.
* **Speaker Notes**:
  > "Good morning, members of the committee. Today, we present Transcript Analysis, an enterprise-grade full-stack platform designed to structure conversational transcripts, protect customer privacy, and streamline operator workflows using Azure AI Services."

---

### SLIDE 2: THE PROBLEM & COMPLIANCE GAP
* **Header**: The Unstructured Transcript Problem & Security Vulnerabilities
* **Visual**: Before-and-after contrast showing a raw, unformatted text block filled with clear-text SSNs, and a structured, safe visual alternative.
* **Content**:
  * **GDPR & HIPAA violations**: Personal details stored in clear-text logs.
  * **Operator Overload**: Manually parsing endless text walls to extract customer info.
  * **Armenian Language Challenges**: Low support for dialects and transliterations in off-the-shelf tools.
* **Speaker Notes**:
  > "Many customer support departments generate gigabytes of raw voice-to-text transcriptions daily. However, these files are highly unstructured and contain sensitive data like names, phone numbers, and Social Security Numbers. Storing this data in clear-text directly violates GDPR and HIPAA compliance. Our platform aims to solve these issues automatically."

---

### SLIDE 3: PLATFORM ARCHITECTURE & PIPELINE
* **Header**: Architectural Design: Robust, Asynchronous & Loosely Coupled
* **Visual**: Clean data flow diagram (React Client -> Kestrel API Controller -> Speaker Identification -> Text Chunking -> Azure AI -> Normalized JSON).
* **Content**:
  * Clean separation of concerns (ASP.NET Web API and Vite/React SPA).
  * Safe multi-tier fallback architecture.
  * Scalable, stateless backend ready for Docker orchestration.
* **Speaker Notes**:
  > "Here is our system's architecture. The React frontend interacts with our C# backend via a single REST endpoint. The backend coordinates two critical operations: speaker segmentation and PII entity extraction. It is designed to be completely stateless, allowing easy horizontal scaling in Docker-based container registries."

---

### SLIDE 4: THE MULTI-MODE SPEAKER SEGMENTATION
* **Header**: Speaker Delineation: Cost-Efficient & Resilient Fallback Mechanics
* **Visual**: Decision tree showing Mode 1 (Regex), Mode 2 (Azure OpenAI), and Mode 3 (Alternate Fallback).
* **Content**:
  * **Mode 1**: Direct Regex scanning for existing tags (`Agent:`, `Caller:`, etc.) — 0ms, free.
  * **Mode 2**: OpenAI `gpt-5-mini` contextual analysis for unlabelled transcripts — handles local Armenian dialects.
  * **Mode 3**: Automatic fallback to alternate-line distribution if APIs are unreachable.
* **Speaker Notes**:
  > "To ensure high resilience while keeping cloud costs low, we implemented a three-tiered Speaker Role Service. If the transcript is already tagged, our local regex parses it instantly and for free. If it is unlabelled, we invoke GPT-5-mini for context-aware speaker identification. If a cloud outage occurs, we automatically fall back to a deterministic alternate-line sequence, keeping the system functional."

---

### SLIDE 5: SOLVING CLOUD LIMITATIONS (THE CHUNKING ALGORITHM)
* **Header**: Overcoming Cloud Constraints: Intelligent Offset-Tracking Chunking
* **Visual**: Diagram showing a 50,000-character transcript being split into 5,000-character blocks at `\n` boundaries, and the mathematical formula for offset reconstruction.
* **Content**:
  * **The Challenge**: Azure's PII endpoint has a hard limit of 5,120 characters.
  * **The Solution**: Splitting text into chunks of under 5,000 characters by scanning backward for the last `\n` carriage return.
  * **Offset Reconstruction**: Recalculating character offsets relative to the original document:
    $$\text{CorrectedOffset} = \text{AzureOffset} + \sum \text{PreviousChunkLengths}$$
* **Speaker Notes**:
  > "A major challenge we resolved was Azure's synchronous text length limit of 5,120 characters. To process transcripts up to 50,000 characters, we designed an intelligent chunking algorithm. It splits the document at safe newline boundaries to avoid cutting names or numbers. It then queries Azure in parallel and mathematically reconstructs the original offsets, allowing us to highlight sensitive information in the full text."

---

### SLIDE 6: MITIGATING EDGE CASES: REGEX SSN INTERCEPTOR
* **Header**: Refining Machine Learning Outputs: The Regex Correction Pattern
* **Visual**: Highlighting how a US SSN (`123-45-6789`) is misclassified as a `PhoneNumber` by Azure, and how our C# regex interceptor corrects it.
* **Content**:
  * ML models are highly capable but can still produce false positives or misclassifications.
  * Azure often flags the US SSN hyphenated pattern as a standard `PhoneNumber`.
  * We introduced a fast regex validation interceptor to correct this class mapping in our post-processing service.
* **Speaker Notes**:
  > "No cloud ML model is perfect. During integration testing, we found that Azure's NER model often misclassified US Social Security Numbers as standard phone numbers due to the hyphen spacing. To address this, we added a regex-based validation interceptor that corrects this mapping before returning the data to the client."

---

### SLIDE 7: FRONT-END REACT ENGINEERING
* **Header**: React 19 Operator Experience: Polished, Reactive & Offline-First
* **Visual**: Clean mockups of the Messenger Chat View and the copyable Attributes Panel.
* **Content**:
  * **TanStack React Query**: Smooth server-state and cache management.
  * **Messenger Chat View**: Left/right aligned message bubbles using Emotion styled components.
  * **Offline History Logs**: Browser-persisted FIFO storage capped at 100 entries.
* **Speaker Notes**:
  > "The frontend is built using React 19, TypeScript, and Ant Design v6. We focused heavily on operator usability. Transcripts are visualized as a chat flow, with different alignments and colors for each speaker. We also implemented an offline history engine that stores up to 100 past analyses in localStorage, allowing operators to reload sessions instantly without repeating cloud API calls."

---

### SLIDE 8: INTEGRATION TESTING & MOCK DESIGNS
* **Header**: Comprehensive Quality Assurance: xUnit & WebApplicationFactory
* **Visual**: Code snippets showing our mock service setup and a list of our 11 integration tests.
* **Content**:
  * High-fidelity testing isolated from active cloud endpoints.
  * Integration testing using the `FakeAzureLanguageService` pattern and `TextAnalyticsModelFactory`.
  * Automated pre-commit checks enforced via Husky git hooks.
* **Speaker Notes**:
  > "To ensure reliability, we wrote 11 automated integration tests using xUnit and WebApplicationFactory. These tests run in-memory, allowing us to simulate network timeouts, test length limits, verify regex corrections, and test chunking logic without incurring cloud API costs."

---

### SLIDE 9: CONTAINERIZATION & PRODUCTION DEPLOYMENT
* **Header**: Deployment Architecture: Enterprise Containerization & Edge Delivery
* **Visual**: Deployment pipeline diagram (Local git push -> Husky lint check -> Docker Multistage Build -> Netlify CDN frontend / Render backend -> restricted CORS policies).
* **Content**:
  * Multi-stage Docker configuration (keeps runtime images under 120MB).
  * API services containerized and hosted on the Render PaaS platform.
  * Frontend built as highly compressed static assets and served via Netlify.
  * Security hardened via strict CORS policies.
* **Speaker Notes**:
  > "For deployment, we designed a multi-stage Dockerfile that keeps our runtime image small. The backend container runs on Render, while the React frontend is deployed as static assets on Netlify. Security is enforced through strict CORS rules, allowing API requests only from our registered frontend domain."

---

### SLIDE 10: PROJECT DEFENSE — EXPERT Q&A PREPARATION

Here are the top technical questions frequently asked by academic committees and engineering leads, along with their precise, expert answers:

#### Q1: "Why did you use standard Text PII instead of Conversation PII, which is specifically designed for conversational transcripts?"
* **Answer**:
  > "We thoroughly researched both options. While Azure's Conversation PII is designed for transcripts, it lacks support for the Armenian language (`hy`) in its public release. On the other hand, the universal Text PII endpoint features deep multilingual dictionary support and excels at processing Armenian. We decided to use Text PII and handled conversation formatting and speaker parsing ourselves in our backend C# service layer, achieving excellent multlingual support without sacrificing accuracy."

#### Q2: "Since your C# API chunking service runs multiple tasks to Azure AI in parallel, how do you handle partial failures? What if 3 out of 4 chunks succeed, but 1 fails?"
* **Answer**:
  > "We wrapped our parallel chunk processing in an `all-or-nothing` execution model using `Task.WhenAll`. If any chunk fails (e.g. due to credentials issues or network drops), the entire request fails, and we return a structured `503 Service Unavailable` status to the frontend. This prevents returning incomplete, partially redacted data, which could lead to critical compliance issues. For future iterations, we plan to implement a partial-retry mechanism using libraries like Polly."

#### Q3: "Storing the operator’s analysis history in `localStorage` is convenient, but is it secure? What happens if multiple operators share the same workstation?"
* **Answer**:
  > "This is an important security point. Our client-side history uses the browser's `localStorage`, which is sandboxed per domain and protocol. In a multi-user enterprise environment, operators should use individual Windows/Active Directory profiles or secure browser sessions. This ensures their local storage states remain isolated. For a production-ready enterprise release, we would migrate this history to a secure relational database (such as PostgreSQL) with OAuth2 user authentication and row-level security."

#### Q4: "Vite and modern frameworks support Hot Module Replacement (HMR). Why was HMR explicitly disabled (`DISABLE_HMR=true`) during development?"
* **Answer**:
  > "During active development with AI-driven coding agents, code is written incrementally in batches. If HMR were enabled, the compilation process would trigger on every minor keystroke or file write, causing the browser frame to constantly reload, lose state, or display temporary syntax errors. Disabling HMR ensures the workspace remains stable, and compiles the changes reliably on demand."

#### Q5: "How does the backend distinguish between implicit Armenian tags like `Օպերատոր:` and English tags like `Agent:` without hardcoding languages?"
* **Answer**:
  > "In `SpeakerRoleService.cs`, we used a compiled regular expression dictionary that maps known structural patterns in both English and Armenian. Since the API request payload contains a `TargetLanguage` parameter (`en` or `hy`), our service uses this parameter to select the appropriate parsing rule, ensuring high accuracy for both languages."

---

## 🏆 7. CONCLUSION

The **Transcript Analysis** platform is a complete, production-ready, highly secure full-stack application. It successfully combines modern cloud AI capabilities with clean architecture, robust fallback mechanics, and a polished user interface. This technical reference manual contains all the details necessary to understand, extend, and successfully defend this project.
