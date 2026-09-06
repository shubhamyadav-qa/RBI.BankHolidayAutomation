# RBI Bank Holidays – Test Automation

Production-quality Playwright automation for extracting and analyzing RBI Bank Holidays data for the year 2001 across Mumbai and Srinagar regional offices.

## Project Overview

This project automates the following workflow:

1. Opens the RBI website (`https://www.rbi.org.in/`)
2. Scrolls to the bottom of the page
3. Clicks **"Bank Holidays"**
4. Selects **Regional Office = Mumbai**, **Month = All Months**, **Year = 2001**
5. Clicks **GO**
6. Extracts all holiday information from the results table
7. Repeats the process for **Regional Office = Srinagar**
8. Calculates holiday statistics dynamically
9. Generates a text report at `Reports/rbi_2001_holidays.txt`

## Technology Stack

```
C#
.NET 8
Playwright
NUnit
Page Object Model
Git
Chromium
```

## Architecture

```
RBIHolidayAutomation/
│
├── RBIHolidayAutomation.sln
│
├── src/
│   └── RBIHolidayAutomation/
│       │
│       ├── Models/
│       │   ├── HolidayRecord.cs        # Strongly-typed holiday entry
│       │   └── HolidaySummary.cs       # Aggregated statistics
│       │
│       ├── Pages/
│       │   ├── RbiHomePage.cs          # Home page navigation
│       │   └── BankHolidaysPage.cs     # Holiday search + extraction
│       │
│       ├── Services/
│       │   ├── HolidayAnalyzer.cs      # Statistics calculation
│       │   └── HolidayReportWriter.cs  # Report generation
│       │
│       ├── Tests/
│       │   ├── RbiHolidayTests.cs      # UI automation test
│       │   └── HolidayAnalyzerTests.cs # Unit tests for analyzer
│       │
│       └── Utilities/
│           └── BrowserFactory.cs       # Browser configuration
│
├── TestResults/                        # Failure diagnostics
├── Reports/                            # Generated reports
├── README.md
├── .gitignore
└── appsettings.json                    # Browser configuration
```

### Folder Responsibilities

| Folder | Responsibility |
|--------|---------------|
| `Models` | Strongly-typed data structures for holiday records and computed summaries |
| `Pages` | Page Object Model classes encapsulating page-specific locators and actions |
| `Services` | Business logic for analysis and report generation, independent of UI |
| `Tests` | NUnit test classes (UI automation + unit tests) |
| `Utilities` | Reusable infrastructure such as browser factory |
| `TestResults` | Screenshots and diagnostics captured on test failure |
| `Reports` | Generated holiday report output |

## Prerequisites

- **.NET 8 SDK** – [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PowerShell** (Windows) or bash (macOS/Linux)
- **Playwright Chromium browser** – installed via the Playwright CLI

## Installation

```bash
dotnet restore
dotnet build
```

### Install Playwright Browser

```bash
dotnet tool install --global Microsoft.Playwright.CLI
playwright install chromium
```

## Execution

```bash
dotnet test
```

This runs both the unit tests and the UI automation test.

### Running Only Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~HolidayAnalyzerTests"
```

### Running Only the UI Test

```bash
dotnet test --filter "FullyQualifiedName~RbiHolidayTests"
```

## Headed Execution

To run the browser in headed (visible) mode, edit `appsettings.json`:

```json
{
  "Browser": {
    "Headless": false
  }
}
```

## Output

The generated report is stored at:

```
Reports/rbi_2001_holidays.txt
```

The report contains:

- Holiday data for **Mumbai** and **Srinagar** in `Month,Day,Occasion` format
- Total holidays per regional office
- Month with the highest number of holidays
- Number of long weekends (3+ continuous non-working days)
- Longest continuous holiday stretch

## Design Decisions

### Why Page Object Model (POM)?

POM separates page-specific locators and interactions from test logic. This makes tests more readable, reduces duplication, and isolates UI changes to a single class.

### Why Semantic Selectors?

The RBI website uses ASP.NET WebForms with generated control IDs (e.g., `ctl00_ContentPlaceHolder1_xxx`). These IDs are unstable and can change with any site update. Instead, this project uses:

- `GetByRole(AriaRole.Link, new() { Name = "Bank Holidays" })` for the footer link
- `#drRegionalOffice`, `#drMonth`, `#drYear` for the dropdowns (stable semantic IDs)
- `#btnGo` for the GO button
- `table.tablebg` for the results table

### Why Services?

Business logic (statistics calculation, report generation) is isolated in service classes. This makes the logic unit-testable without a browser and keeps the test classes focused on orchestration.

### Why Strongly Typed Models?

`HolidayRecord` and `HolidaySummary` provide compile-time safety, prevent stringly-typed bugs, and make the data flow explicit.

### How Long Weekends Are Calculated

A **long weekend** is defined as a continuous stretch of **3 or more consecutive holiday dates** (based on the actual holiday dates from the RBI data). The `HolidayAnalyzer`:

1. Extracts all valid `DateOnly` values from the holiday records
2. Sorts them chronologically
3. Groups consecutive dates into stretches
4. Counts stretches of length ≥ 3 as long weekends
5. Reports the maximum stretch length

**Important:** This interpretation is based on the assignment requirement ("a continuous stretch of 3 or more non-working days") and the available RBI holiday data. The RBI data only provides holiday dates, not weekend (Saturday/Sunday) information, so the calculation is based purely on consecutive holiday dates.

## Known Limitations

- The RBI website is a live ASP.NET WebForms application. Its HTML structure may change without notice, which could require updating selectors.
- The dropdown options (regional offices, months, years) are populated server-side. If the RBI site removes the year 2001 from the dropdown, the test will fail.
- The holiday table format (month header rows followed by day/occasion rows) is specific to the current RBI page structure.
- The RBI website may be slow or unavailable at times; the test uses generous timeouts (60 seconds) but may still fail if the site is down.
- The long weekend calculation is based on consecutive holiday dates only, not on actual calendar weekends (Saturday/Sunday), because the RBI data does not include weekend information.

## Git Readiness

The project includes a `.gitignore` that excludes:

- `bin/`, `obj/` – build output
- `TestResults/` – test diagnostics
- `Reports/` – generated reports
- `.vs/` – Visual Studio user files
- `_inspect/` – temporary inspection scripts