# OPC Live Status Dashboard

A full-stack local dashboard for monitoring and toggling OPC DA tags.

This project has:

- A **React + Vite frontend** for live status display and operator buttons.
- An **ASP.NET Core Minimal API backend** for OPC DA tag subscription, caching, reading, and writing.
- **QuickOPC / EasyDAClient** for communication with the OPC DA server.
- A **JSON-based button configuration** in the frontend.
- A **JSON-based tag subscription configuration** in the backend.

---

# Features

- Live OPC tag monitoring
- OPC DA subscription-based backend caching
- Fast frontend refresh using polling
- Toggle buttons for selected tags
- Button metadata stored in JSON
- Responsive split-screen dashboard
- Independent scroll areas for cards and buttons

---

# Tech Stack

## Frontend
- React
- Vite
- JavaScript
- CSS

## Backend
- ASP.NET Core Minimal API
- C#
- QuickOPC / OpcLabs EasyOPC DataAccess

## Communication
- HTTP REST API
- CORS enabled for local frontend development

---

# Architecture

## Frontend
The frontend:
- Calls the backend every 500 ms to get current tag values
- Displays all tags as status cards
- Displays selected control buttons from a JSON file
- Sends POST requests to toggle a tag value

## Backend
The backend:
- Reads `tags.json` on startup
- Subscribes to all configured OPC DA tags using `EasyDAClient`
- Stores current values in memory using `ConcurrentDictionary`
- Exposes REST endpoints for reading tags and toggling selected tags

## OPC Flow
1. Backend starts
2. Backend loads `tags.json`
3. Backend subscribes to OPC DA items
4. OPC server pushes value changes through `ItemChanged`
5. Backend updates in-memory cache
6. Frontend reads `/api/tags`
7. Frontend sends `/api/tags/write` for button toggles

---

# Project Structure

```text
project-root/
│
├── backend/
│   ├── Program.cs
│   ├── tags.json
│   ├── backend.csproj
│   ├── appsettings.json
│   ├── Properties/
│   └── bin/
│
├── frontend/
│   ├── src/
│   │   ├── App.jsx
│   │   ├── App.css
│   │   ├── main.jsx
│   │   └── buttonTags.json
│   ├── public/
│   ├── package.json
│   ├── vite.config.js
│   └── .env
│
└── README.md
```

If your actual folder names differ, update the commands below accordingly.

---

# Prerequisites

Before running this project, make sure the following are installed.

## Required Software

### Frontend
- Node.js
- npm

### Backend
- .NET SDK
- Visual Studio 2022 or VS Code (optional but recommended)

### OPC
- OPC DA server accessible from the backend machine
- QuickOPC package installed via NuGet
- DCOM / OPC permissions configured correctly if the OPC server is remote

---

# Recommended Versions

These are recommended stable versions for this project.

## Frontend
- Node.js 18.x or 20.x
- npm 9.x or 10.x
- Vite 5.x or later
- React 18.x

Vite documents the standard local dev server and production build flow used in this project. [web:469][web:472]

## Backend
- .NET 8 SDK recommended
- ASP.NET Core Minimal API on .NET 8

Microsoft documents Minimal APIs and configuration/environment behavior for ASP.NET Core, which matches the backend pattern used here. [web:470][web:473]

## OPC Library
- QuickOPC / OpcLabs EasyOPC DataAccess package version used in your project

If you want exact package versions recorded, run:
```bash
dotnet list package
```

and copy the results into this README.

---

# Environment Details

## Frontend Environment Variable

Create a `.env` file inside the frontend folder:

```env
VITE_API_BASE_URL=http://localhost:5000
```

### Meaning
- `VITE_API_BASE_URL` = backend API base URL
- Vite exposes only variables prefixed with `VITE_` to frontend code. [web:469]

---

# Backend Configuration

## `tags.json`

This file contains the OPC server settings and the list of tags the backend subscribes to.

Example:

```json
{
  "MachineName": "",
  "ServerName": "Your.OPC.Server.Name",
  "UpdateRate": 500,
  "Tags": [
    "Tag1",
    "Tag2",
    "Tag3"
  ]
}
```

### Fields
- `MachineName`: machine name hosting the OPC DA server, use empty string for local machine
- `ServerName`: OPC DA server ProgID
- `UpdateRate`: OPC subscription update rate in milliseconds
- `Tags`: list of OPC items to monitor

---

# Frontend Button Configuration

## `src/buttonTags.json`

This file contains the list of button labels and their mapped OPC tag names.

Example:

```json
[
  {
    "label": "CALL 1 LEFT",
    "tagName": "ML_MTB_Andon.Layout.CALL_LH.ABS-1_CALL_1L"
  },
  {
    "label": "CALL 2 LEFT",
    "tagName": "ML_MTB_Andon.Layout.CALL_LH.ABS-2_CALL_2L"
  }
]
```

---

# Installation

## 1. Clone or copy the project

```bash
git clone <your-repository-url>
cd <your-project-folder>
```

If you are not using Git, simply place the frontend and backend folders in one workspace.

---

## 2. Frontend setup

Go to the frontend folder:

```bash
cd frontend
```

Install dependencies:

```bash
npm install
```

Start development server:

```bash
npm run dev
```

Default Vite dev server usually runs at:

```text
http://localhost:5173
```

---

## 3. Backend setup

Open a new terminal and go to the backend folder:

```bash
cd backend
```

Restore NuGet packages:

```bash
dotnet restore
```

Run the backend:

```bash
dotnet run
```

If configured normally, backend may run on:

```text
http://localhost:5000
```

or sometimes a random ASP.NET Core development port depending on launch settings.

To force a port:

```bash
dotnet run --urls=http://localhost:5000
```

Microsoft documents Minimal API development and local launch behavior for ASP.NET Core apps. [web:470][web:473]

---

# Commands

## Frontend Commands

Inside `frontend/`:

### Install dependencies
```bash
npm install
```

### Start dev server
```bash
npm run dev
```

### Build production bundle
```bash
npm run build
```

### Preview production build
```bash
npm run preview
```

### Check installed versions
```bash
node -v
npm -v
npm list
```

---

## Backend Commands

Inside `backend/`:

### Restore packages
```bash
dotnet restore
```

### Run app
```bash
dotnet run
```

### Run on fixed URL
```bash
dotnet run --urls=http://localhost:5000
```

### Build project
```bash
dotnet build
```

### Publish project
```bash
dotnet publish -c Release
```

### Check .NET SDK version
```bash
dotnet --version
```

### List NuGet packages
```bash
dotnet list package
```

---

# API Endpoints

## GET `/api/tags`
Returns all subscribed tag values from in-memory cache.

### Response example
```json
[
  {
    "tagName": "Tag1",
    "value": "1",
    "quality": "Good",
    "timestamp": "2026-04-15 17:20:00",
    "error": null
  }
]
```

---

## GET `/api/tags/{tagName}`
Returns one tag by name.

### Example
```http
GET /api/tags/ML_MTB_Andon.Layout.CALL_LH.ABS-1_CALL_1L
```

---

## POST `/api/tags/write`
Toggles a tag value.

### Request body
```json
{
  "tagName": "ML_MTB_Andon.Layout.CALL_LH.ABS-1_CALL_1L"
}
```

### Success response
```json
{
  "success": true,
  "tagName": "ML_MTB_Andon.Layout.CALL_LH.ABS-1_CALL_1L",
  "value": "1"
}
```

---

## OPTIONS `/api/tags/write`
Used for CORS preflight handling for cross-origin frontend requests. ASP.NET Core CORS configuration and preflight behavior are part of the standard cross-origin request model documented by Microsoft. [web:473]

---

# How the Backend Works

## Startup
On startup:
- `tags.json` is loaded
- OPC client is created
- all configured tags are subscribed using `SubscribeItem`

## Live Updates
Whenever the OPC server pushes a value change:
- `client.ItemChanged` is triggered
- backend normalizes the value
- in-memory cache is updated

## Writes
When the frontend sends a toggle request:
- backend reads the current cached value
- determines next value (`0` or `1`)
- writes boolean `true` or `false` to OPC
- updates cache immediately

---

# Value Normalization

The backend converts incoming OPC values into string `"0"` or `"1"` when possible.

Handled types include:
- `bool`
- `byte`
- `short`
- `int`
- `long`
- `float`
- `double`
- `decimal`
- text values like `"true"`, `"false"`, `"on"`, `"off"`

This helps keep the frontend button state logic simple and consistent.

---

# UI Layout

The UI uses:
- left half for OPC cards
- right half for action buttons
- separate scroll behavior for both sides

This allows:
- large tag lists without affecting button visibility
- compact operator control layout
- better screen usage on wide displays

---

# Development Notes

## CORS
The backend enables CORS for:

```text
http://localhost:5173
```

This allows the Vite frontend to call the ASP.NET Core API during development. Microsoft’s docs describe origin, methods, and headers as the key CORS controls used here. [web:473]

## Polling
Frontend polls `/api/tags` every 500 ms.

You can change this in `App.jsx`:

```js
const interval = setInterval(fetchTags, 500);
```

For example:
- `500` = 0.5 second
- `1000` = 1 second
- `200` = faster refresh but more HTTP traffic

## OPC Performance
For large numbers of tags:
- use subscriptions instead of repeated reads
- avoid too-small update rates unless necessary
- verify OPC server scan capacity
- prefer grouping critical tags separately if needed

---

# Troubleshooting

## 1. Frontend shows CORS error
Check:
- backend is running
- backend URL matches `.env`
- CORS policy includes `http://localhost:5173`
- `/api/tags/write` OPTIONS route exists if needed

---

## 2. POST write gives 405
Possible reasons:
- wrong backend URL
- backend not running on expected port
- CORS preflight issue
- route mismatch

Check browser DevTools Network tab and verify:
- request URL
- method
- response code
- preflight OPTIONS request

---

## 3. OPC values not updating
Check:
- `tags.json` server name is correct
- OPC DA server is accessible
- DCOM permissions are configured
- tag names are valid
- OPC server quality/status is good

---

## 4. Button color does not match tag state
Check:
- `buttonTags.json` tagName exactly matches subscribed tag name
- backend `NormalizeValue` returns `"0"` or `"1"`
- frontend `isOn()` logic matches backend values

---

## 5. Backend starts but OPC write fails
Possible reasons:
- tag is read-only
- wrong OPC item type
- server expects another datatype
- permissions issue on OPC server

---

# Example `.env`

```env
VITE_API_BASE_URL=http://localhost:5000
```

---

# Example `package.json` scripts

Typical frontend scripts:

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview"
  }
}
```

---

# Version Tracking

To document exact versions used in your environment, record the outputs of the following commands.

## Frontend
```bash
node -v
npm -v
npm list react react-dom vite
```

## Backend
```bash
dotnet --version
dotnet list package
```

Add the output here after installation.

### Example format
```text
Node.js: v20.11.1
npm: 10.2.4
React: 18.x
Vite: 5.x
.NET SDK: 8.0.x
QuickOPC: <your-installed-version>
```

---

# Recommended Local Development Workflow

## Terminal 1 - Backend
```bash
cd backend
dotnet run --urls=http://localhost:5000
```

## Terminal 2 - Frontend
```bash
cd frontend
npm install
npm run dev
```

Then open:

```text
http://localhost:5173
```

---

# Future Improvements

- Replace polling with SignalR for live push updates
- Add grouped button sections
- Add search/filter for tags
- Add alarm highlighting
- Add authentication/authorization
- Add write audit logging
- Add export/import of button configuration
- Move button configuration to backend API

---

# Author

Developed by Abhishek Adhalkar.