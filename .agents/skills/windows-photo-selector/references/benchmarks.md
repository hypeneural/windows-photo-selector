# Benchmarks And Validation Reference

Use this reference for tests, performance budgets, BenchmarkDotNet and validation scripts.

## Performance Budgets V1

Initial targets with real studio JPEGs:

- 500 JPEGs: folder open keeps UI responsive.
- 2,000 JPEGs: app does not wait for thumbnails.
- Time to first image: target <= 1.5s on a typical SSD machine.
- Cached navigation: target <= 50ms perceived.
- Delete visual response: immediate, before file move completes.
- No full-res decode for normal fit mode.
- No `FileStream` retained after decode.

## Benchmark Areas

Use BenchmarkDotNet when the project exists.

Measure:

- scan 50, 500 and 2,000 JPEGs;
- large JPEG preview decode;
- cached navigation;
- uncached navigation;
- delete/undo volume;
- journal append;
- cache hit/miss;
- memory under sustained navigation.

Benchmark output should go under `artifacts/performance` or a documented equivalent.

## Test Coverage

Core tests:

- session creation;
- JPEG filtering;
- navigation;
- delete first/middle/last;
- undo;
- counters;
- pending/failure states;
- replay/reconciliation.

Imaging tests:

- EXIF orientation;
- decode target sizing;
- cache key;
- invalidation;
- LRU;
- cancellation;
- stream/file-handle safety.

Integration tests:

- temporary folder session;
- move to `_deletadas_evydencia`;
- restore;
- collision;
- read-only file;
- locked file;
- long path;
- crash/replay.

UI smoke tests:

- open app;
- open folder;
- display first JPEG;
- fullscreen toggle;
- shortcuts do not trigger in text input.

## Commands

Repository scripts:

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Core"
.\tools\benchmarks.ps1
```

Skill wrappers:

```powershell
.\.agents\skills\windows-photo-selector\scripts\validate-solution.ps1
.\.agents\skills\windows-photo-selector\scripts\run-core-tests.ps1
.\.agents\skills\windows-photo-selector\scripts\run-imaging-tests.ps1
.\.agents\skills\windows-photo-selector\scripts\run-benchmarks.ps1
```

## Review Rule

Do not claim performance improved unless:

- a benchmark proves it; or
- the change directly removes a known expensive operation from the critical path and the reasoning is documented.
