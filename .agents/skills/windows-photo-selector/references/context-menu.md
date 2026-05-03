# Context Menu And Activation Reference

Use this reference for Windows Explorer integration, launcher, single-instance and lifecycle tasks.

## Required Behavior

The Explorer item is:

```text
Abrir Escolher Fotos
```

The app should receive:

```text
--folder "C:\Path\To\Session" --source explorer
```

Handle:

- right-click on a folder;
- right-click background inside a folder;
- invalid paths;
- multiple selected paths;
- uninstall/removal.

## Modes

Development fallback:

- HKCU registry command;
- acceptable if it appears under `Mostrar mais opcoes`;
- only for testing `--folder`.

Internal product:

- signed MSIX;
- package identity;
- `IExplorerCommand`;
- app opens by argument or URI.

Future professional product:

- MSIX or sparse package;
- trusted certificate;
- auto-update;
- shell activation logs.

## Launcher

`Evydencia.PhotoSelector.Launcher` should:

- receive Explorer path;
- normalize path;
- validate it is a folder;
- reject invalid/session-deleted roots;
- activate or redirect to main app;
- exit.

It must not:

- scan folders;
- decode images;
- move files;
- write session journal;
- load heavy Imaging/Core dependencies.

## Single Instance

Single-instance must be implemented before Explorer context menu is complete.

Rules:

- if app is closed, open normally;
- if app is open without session, load received folder;
- if app is open with active session, ask before replacing;
- repeated Explorer activation must not create duplicate app instances;
- activation redirect happens before creating a main window in a secondary process.

## Acceptance

- double-clicking context menu does not create two live sessions;
- clicking folder passes that folder;
- clicking background passes current folder;
- invalid path shows controlled error;
- ShellExtension remains thin in dependency graph.
