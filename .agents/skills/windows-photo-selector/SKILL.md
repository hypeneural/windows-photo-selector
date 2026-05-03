---
name: windows-photo-selector
description: Use when implementing or reviewing Evydencia Escolher Fotos, a native Windows WinUI 3 JPEG photo-selection viewer focused on fullscreen performance, delete/undo, cache, Explorer activation, tests, benchmarks, and future API/second-screen readiness.
---

# Windows Photo Selector Skill

Use this skill for work on **Evydencia Escolher Fotos**.

This app is a native Windows desktop viewer for studio photo selection by deletion. It is not a generic gallery, web viewer, API client, PDV module, RAW processor, or photo editor.

## First Steps

Before implementation work:

1. Read `AGENTS.md`.
2. Read `PLANS.md` for non-trivial tasks.
3. Read `docs/evydencia-escolher-fotos-plano-implementacao.md`.
4. Read the nearest subfolder `AGENTS.md` if one applies.
5. Load only the reference file below that matches the task.

Do not implement API, login, PDV, upload, RAW, AI, Electron, WebView viewer, or remote sync in V1.

## Reference Routing

Read these references only when relevant:

- Layer boundaries, project shape, allowed dependencies: `references/layer-routing.md`
- JPEG scan, decode, EXIF, DPI, cache, prefetch, color: `references/image-pipeline.md`
- Delete, undo, journal, source of truth, recovery: `references/delete-undo.md`
- Explorer context menu, launcher, single-instance, lifecycle: `references/context-menu.md`
- Tests, performance budgets, BenchmarkDotNet, scripts: `references/benchmarks.md`

For architecture decisions, also read `docs/adr/*.md` that match the topic.

## Product Model

The customer removes photos they do not want.

The operator expects:

- open a session folder from Explorer;
- first JPEG appears quickly;
- arrows navigate without lag;
- `Delete` removes the current photo visually immediately;
- `Ctrl+Z` restores safely;
- counters stay correct;
- the app does not hold files open;
- V1 stays offline and JPEG-only.

Prioritize:

1. photo safety and recoverability;
2. UI responsiveness;
3. image quality;
4. keyboard speed;
5. clean architecture;
6. future extensibility.

## Planning Rule

Use `PLANS.md` before coding anything that touches:

- `FolderScanner`;
- `PhotoSession`;
- `NavigationController`;
- `DeleteManager`;
- `UndoManager`;
- `JpegDecodeService`;
- `PreviewCacheService`;
- `PrefetchScheduler`;
- `DisplayContext`;
- activation/single-instance;
- ShellExtension/Launcher;
- benchmarks;
- architectural decisions.

For tiny documentation edits or mechanical fixes, a short note is enough.

## Layer Decision

Route work before editing:

- Domain state or rules -> Core.
- Use case orchestration -> Application.
- Decode, EXIF, cache, prefetch -> Imaging.
- Settings, journal, SQLite, filesystem move/restore -> Storage.
- WinUI windows, overlay, shortcuts, activation composition -> App.
- Explorer menu and broker activation -> ShellExtension/Launcher.
- Future API DTOs only -> Contracts.
- Measurement -> Benchmarks.

If a feature seems to cross layers, define an interface in the inner layer and implement it outward.

## Hard Constraints

- V1 remains local-first and offline.
- Only `.jpg` and `.jpeg` are in scope.
- No permanent delete by default.
- UI thread must not scan, decode, cache, move files, or write heavy journal work.
- `Core` must not depend on UI, storage, imaging, shell, logs, or HTTP.
- `Application` must keep ViewModels thin and must not reference WinUI.
- ShellExtension must stay thin.
- Files displayed by the viewer must remain movable/deletable.

## Required Acceptance Notes

When finishing a task, report:

- files changed;
- layer changed;
- tests added or updated;
- commands run;
- performance impact;
- risks remaining;
- deviations from `AGENTS.md`, if any.

For image-display PRs, also explain:

- where target decode size is calculated;
- how DPI/rasterization scale is considered;
- how EXIF orientation affects dimensions;
- why full-res decode is not used for fit mode;
- how obsolete decodes are cancelled.

## Helper Scripts

Repository scripts:

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Core"
.\tools\benchmarks.ps1
```

Skill wrapper scripts:

```powershell
.\.agents\skills\windows-photo-selector\scripts\validate-solution.ps1
.\.agents\skills\windows-photo-selector\scripts\run-core-tests.ps1
.\.agents\skills\windows-photo-selector\scripts\run-imaging-tests.ps1
.\.agents\skills\windows-photo-selector\scripts\run-benchmarks.ps1
```

The scripts are safe before the solution exists; they report that scaffolding has not started yet.
