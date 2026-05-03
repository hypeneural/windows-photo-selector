# Layer Routing Reference

Use this reference for architecture, dependency direction, project placement and review of layer boundaries.

## Expected Projects

```text
/src
  /Evydencia.PhotoSelector.App
  /Evydencia.PhotoSelector.Application
  /Evydencia.PhotoSelector.Core
  /Evydencia.PhotoSelector.Imaging
  /Evydencia.PhotoSelector.Storage
  /Evydencia.PhotoSelector.Infrastructure
  /Evydencia.PhotoSelector.Contracts
  /Evydencia.PhotoSelector.ShellExtension
  /Evydencia.PhotoSelector.Launcher
```

## Application

Application owns use cases and orchestration:

- `OpenSessionUseCase`;
- `NavigateNextPhotoUseCase`;
- `NavigatePreviousPhotoUseCase`;
- `DeleteCurrentPhotoUseCase`;
- `UndoLastDeleteUseCase`;
- `RecoverSessionUseCase`;
- `BuildLocalSelectionSummaryUseCase`;
- `PrepareViewerImageUseCase`;
- abstractions for folder scan, file move, journal, preview, settings and performance.

Application must not reference WinUI, XAML, `Window`, `Page`, controls, or HTTP implementation in V1.

## Core

Core owns local domain rules:

- `PhotoSession`;
- `PhotoItem`;
- `PhotoStatus`;
- `PhotoSessionFactory`;
- `FolderScanPolicy`;
- `PhotoFileCandidate`;
- scan result rules;
- `NavigationController`;
- `DeleteManager` domain state machine;
- `UndoManager`;
- counters;
- journal replay contracts;
- reconciliation rules.

Core must not reference:

- WinUI;
- Windows App SDK;
- WIC;
- Win2D;
- SQLite;
- Serilog;
- Shell;
- HTTP;
- concrete filesystem when an interface is enough.

## Imaging

Imaging owns:

- JPEG decode;
- EXIF orientation;
- preview target sizing;
- memory cache;
- disk preview/thumbnail cache;
- prefetch scheduler;
- cancellation of obsolete decode;
- future Win2D renderer abstraction.

Imaging does not decide whether a photo is deleted, restored or missing.

## Storage

Storage owns:

- settings;
- recent sessions;
- JSONL journal;
- SQLite derived state;
- file move/restore;
- cache paths;
- disk cache cleanup.

Storage must not own navigation policy.

## App

App owns:

- WinUI pages/windows;
- view models;
- keyboard input;
- fullscreen;
- overlay;
- activation composition;
- DI composition.

Keep code-behind focused on UI behavior. Domain behavior belongs in Core.

## ShellExtension And Launcher

ShellExtension and Launcher only:

- receive Explorer path;
- normalize/validate;
- launch or redirect activation;
- exit.

They do not scan folders, decode images, move files, write session journal or hold session state.

Do not create `Evydencia.PhotoSelector.Shell` in V1 unless there is real shared code that cannot live in Launcher or packaging scripts.

## Contracts

Contracts should stay minimal in V1:

- `LocalSelectionSummary`;
- `DeletedPhotoEventDto`;

Remote interfaces and order/customer context require a future ADR and no HTTP implementation in V1.

## Review Checklist

- Does Core remain pure?
- Does Application keep use cases out of ViewModels?
- Is UI logic separated from domain state?
- Does Imaging avoid business status decisions?
- Does Storage avoid navigation policy?
- Is ShellExtension still thin?
- Are future API contracts inert in V1?
