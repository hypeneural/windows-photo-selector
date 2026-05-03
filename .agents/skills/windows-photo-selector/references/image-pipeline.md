# Image Pipeline Reference

Use this reference for folder scan, JPEG decode, EXIF, DPI, cache, prefetch, file handles, fullscreen image quality and color strategy.

## Folder Scan

Use progressive enumeration:

```csharp
Directory.EnumerateFiles(...)
```

Do not use `Directory.GetFiles(...)` for large folders unless a benchmark proves it is acceptable.

Phase A, fast scan:

- path;
- filename;
- extension;
- size;
- last write time UTC;
- sort index.

Phase B, background metadata:

- dimensions;
- EXIF orientation;
- capture date;
- cache key.

Rules:

- accept `.jpg` and `.jpeg`, case-insensitive;
- ignore `_deletadas_evydencia`;
- no recursion in V1;
- no thumbnail generation on the critical startup path.

Required scan acceptance:

- a folder with 2,000 JPEGs shows the first image without waiting for complete metadata or thumbnails.

## DisplayContext

Create `DisplayContext` early.

It should represent:

- window id;
- monitor id when available;
- effective size;
- rasterization scale;
- physical pixel size;
- usable viewer area;
- fullscreen state;
- future role: customer or operator display.

Use it for decode target sizing and future second-screen support.

## Decode Target

For normal fullscreen fit:

1. get `DisplayContext`;
2. determine monitor/window physical pixel area;
3. apply DPI/rasterization scale;
4. apply EXIF orientation to dimensions;
5. compute fit-contain target;
6. add 1.15x to 1.35x quality margin;
7. clamp to original dimensions;
8. decode to target size;
9. dispose stream;
10. update UI on the UI thread only after decode result is ready.

Never decode original full-res JPEG for normal fit mode.

Full-res is reserved for future zoom 100% or explicit inspection.

## File Handle Rule

Image decoding must not block delete/move.

Open JPEGs with short-lived streams compatible with deletion:

```csharp
FileAccess.Read
FileShare.ReadWrite | FileShare.Delete
FileOptions.Asynchronous | FileOptions.SequentialScan
```

Dispose streams immediately after decode. Do not bind UI directly to a file stream that remains open.

Required test:

- display current photo and move/delete it immediately without IOException caused by the viewer.

## Cache

Memory cache:

- LRU;
- current photo highest priority;
- next 3 medium priority;
- previous 2 lower priority;
- configurable limit;
- default should be dynamic when implemented.

Disk cache:

- `%LOCALAPPDATA%\Evydencia\PhotoSelector\Cache`;
- thumbnails can use JPEG 85-90;
- fullscreen previews should avoid visible recompression loss;
- if persisted, prefer quality 95+ or a documented alternative;
- never use recompressed preview for zoom 100%;
- cleanup by age and size outside critical startup.

Cache key must include:

- normalized full path;
- file size;
- last write UTC;
- EXIF orientation;
- target width/height;
- algorithm version;
- quality/color mode.

## Prefetch

On navigation:

1. cancel obsolete queued work;
2. queue current photo highest priority;
3. queue next 3;
4. queue previous 2;
5. queue thumbnails only idle/low priority in V1.

Holding the right arrow must not build a long stale decode queue.

## Color Strategy

V1:

- fast mode;
- assume sRGB;
- read ICC presence when cheap;
- log non-sRGB or embedded ICC cases;
- do not transform ICC by default.

V1.5:

- optional color-accurate mode;
- evaluate WIC color context/transform;
- document performance impact.

## Decode Acceptance Notes

Any image-display PR must explain:

- where target decode size is calculated;
- how DPI/rasterization scale is considered;
- how EXIF orientation affects dimensions;
- why full-res decode is not used for fit mode;
- how obsolete decodes are cancelled.
