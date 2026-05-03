# Delete And Undo Reference

Use this reference for delete, undo, journal, recovery, counters and source-of-truth tasks.

## Default Delete Mode

Default V1 delete destination:

```text
<session-folder>\_deletadas_evydencia
```

Never permanently delete by default.

The customer experience is immediate removal from the active viewer, but the internal state remains recoverable.

## PhotoStatus

Use explicit states:

- `Active`;
- `PendingDelete`;
- `Deleted`;
- `PendingRestore`;
- `Restored`;
- `Missing`;
- `DeleteFailed`.

## Delete Flow

1. Compute next active photo before removal.
2. Mark current item `PendingDelete`.
3. Remove it from active navigation immediately.
4. Display next photo.
5. Move file in background.
6. On success, mark `Deleted`, push undo, append journal.
7. On failure, mark `DeleteFailed`, reconcile counters, show discreet error, append journal.

Filename collision policy:

- preserve original filename when possible;
- if destination exists, generate a unique suffix;
- journal original path and deleted path.

## Undo Flow

`Ctrl+Z` restores the last undoable delete.

1. Mark item `PendingRestore`.
2. Move file back to original path.
3. Handle destination collision safely.
4. Restore original `SortIndex`.
5. Mark `Restored`.
6. Append journal.
7. Navigate to restored photo when practical.

Undo must be deterministic after delete at first, middle or last index.

## Source Of Truth

Filesystem + JSONL journal are the source of truth.

SQLite is derived state for:

- recent sessions;
- cache index;
- settings;
- metrics;
- faster reopening.

If journal and SQLite disagree, rebuild SQLite from filesystem + journal.

## Session Recovery

On reopen:

1. scan original folder;
2. scan `_deletadas_evydencia`;
3. replay journal;
4. reconcile with current filesystem;
5. mark missing files as `Missing`;
6. log conflicts.

Filesystem reality wins over stale derived state.

## Journal Events

Expected JSONL events include:

- `AppStarted`;
- `SessionOpened`;
- `FolderScanStarted`;
- `FolderScanCompleted`;
- `NavigationChanged`;
- `DeleteRequested`;
- `Deleted`;
- `DeleteFailed`;
- `UndoRequested`;
- `Restored`;
- `RestoreFailed`;
- `SessionClosed`.

## Required Tests

- delete first/middle/last;
- delete then undo;
- delete failure becomes `DeleteFailed`;
- restore collision;
- missing file reconciliation;
- journal replay after crash;
- current displayed file can be moved without app-held handle.
