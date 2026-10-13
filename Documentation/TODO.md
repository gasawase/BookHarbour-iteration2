# BookHarbour TODO

Running list of upcoming work, ideas, and future components. Newest items go at the bottom of each section.
File paths are relative to `Unity/Assets/Scripts/` unless noted.

## Urgent

## In progress

## Up next
- [ ] Orientation: tap a placed item to show a rotate button; upright ↔ lying flat, saved to the DB (snapping step 5a). **Step recipe ready:** [Recipes/Orientation.md](Recipes/Orientation.md)
- [ ] Stacking items on shelves (snapping step 5b, after 5a). Decisions so far:
  - Change orientation by tapping (or tap-and-holding) a placed item, which shows a button above it; touch-friendly, and reusable for leaning later
  - A book auto-lies-flat when dropped on a lying-flat book, but the user can still change its orientation (e.g. an upright book on top of a stack)
  - Anything can go on top of a stack, as long as it fits under the shelf above
  - Stacks sit flush next to other items, like any other item on the shelf ruler
  - Pulling an item out of the middle of a stack drops the items above it down (a plain `List` is fine for this)
- [ ] Push neighbouring items to make room (snapping step 6)

## Ideas / future features
- [ ] Object pooling for 3D books instead of `Destroy` on drop (`BookDragManager.cs:188`, `:197`)
- [ ] Hardcover API as a fallback data source when Google Books returns nothing (`APIDataFetcherService.cs:56`, stub at `:245`)
- [ ] Parse the EPUB manifest for chapters (`EpubMetadataParserService.cs:189`)
- [ ] App modes, with a way to switch between them:
  - Editing the room
  - Editing the bookshelf
  - Reading books
- [ ] Room editing / decorating
  - Swap furniture
  - Change wallpaper and flooring
  - Time of day: rotate the directional light, which changes the room's color and mood
- [ ] Camera controls so the user can move around the room
- [ ] Tablet support (iPad / Android): touch input for dragging, tap-to-select, and on-screen buttons instead of keyboard shortcuts
- [ ] Leaning orientation for books (uses the same orientation button as stacking)
- [ ] Settings button, plus a settings panel (decide which settings go in it)
- [ ] Animated UI
  - Slide-out book panel
  - Animations when placing objects on shelves
- [ ] Let the user pick their EPUB folder instead of the hardcoded path (`PersistanceManager.cs:19`); a good first setting
- [ ] Loading indicator for slow operations: one reusable UI element that can show a spinner when progress is unknown and a progress bar when it is ("12 / 40 books")
  - First use: parsing / loading books. `ProgressReporter` events are already in place (`PersistanceManager.cs:80`); the parser would need to report the total file count up front for a real progress bar.
  - Later uses: restoring shelves on startup, cover caching, library rescans, API lookups
  - Show it only if the operation takes longer than ~0.3s, so quick loads don't flash
  - Pairs with moving parsing off the main thread (see Efficiency); a spinner can't animate while the main thread is busy
- [ ] Two-phase import: show books from EPUB data first, enrich from Google in the background (`EpubMetadataParserService.ParseFolderToJSON`). Today each book waits on Google before it's saved, so if the API call fails the book is skipped entirely and never reaches the DB or panel.
  - **Phase 1 (local, fast):** parse every EPUB → insert into DB → cache the embedded cover → report to the panel. No network calls.
  - **Phase 2 (background):** a queue enriches books one at a time (page count, description, ISBN, rating, published date) → updates the DB row → raises a `BookEnriched` event so the UI refreshes that book
  - Add an enrichment status per book (e.g. `Pending` / `Done` / `NotFound` / `Failed`) so a later launch only re-queues unfinished ones, and already-enriched books never hit the API again
  - Rework `UpdateBookInDatabase` for this: it currently only updates `PageCount` and `ISBN`, and matches by re-deriving GUIDs instead of using `BookID`
  - Keep the `BookID` fixed once assigned, even if enrichment finds an ISBN the EPUB didn't have; otherwise saved shelf positions and the cover cache lose track of the book
  - Page count drives the 3D book's width, so enrichment can make a book on a shelf wider. Decide what happens then: re-snap it, push neighbours (snapping step 6), or only apply the new width the next time it's placed.
  - Combines with the retry / rate-limit rework (under Fixes): the background queue is where the request spacing and the queue-wide pause live
  - Loading indicator: a progress bar for phase 1, then something subtler for phase 2 (e.g. "Fetching details… 12 / 40") that doesn't block using the app
- [ ] Show parse errors to the user (`ErrorOccurred` is raised but nothing listens to it)
- [ ] Library rescan: pick up new files and handle EPUBs that were moved or deleted
- [ ] Book details panel showing the metadata that's already stored (description, rating, series, publisher)
- [ ] Features for DB tables that already exist but aren't used yet: reading status, tags, highlights/annotations, series grouping
- [ ] Auto-arranged shelves (sort by genre or page count); `eShelfType.Genre` and `eShelfType.PageCount` already exist
- [ ] Drop onto any bookshelf in the room, not just the one in focus (`BookDragManager.cs:168`)
- [ ] Search / filter in the book panel
- [ ] Variable book heights (from the old TODO.txt)

## Code quality
- [ ] General code cleanup pass
- [ ] Make the `TryGet...` helpers (`TryGetShelfPlaceable`, `TryGetShelfLayoutInfo`, `TryGetShelfPlacedItem`, `TryGetShelfPlaceableAndLayoutInfo`) static or shared so they can be used outside `ShelfLayoutService`, e.g. as extension methods in `HelperExtensions.cs`
- [ ] Unit testing: set up the Unity Test Framework, starting with pure logic like `ShelfLayoutInfo.ToU` and `FindFreeSpans`

### Documentation
Do these after the dead-code cleanup below, so we don't document code that's about to be deleted.
- [ ] `///` XML `<summary>` comments above every class and method saying what it does. Visual Studio / Rider show these as hover tooltips wherever the method is called, not just in its own file.
  - Include `<param>` / `<returns>` where they aren't obvious, and note side effects (writes to DB, destroys objects, fires events)
  - Replace the empty auto-generated `<param name="..."></param>` stubs that are already there (e.g. `BookObject3D.GetDominantColor`)
- [ ] One-place script reference: `Documentation/ScriptReference.md`
  - Every script grouped by folder (Models / Services / UI / Extensions), with a one-line purpose, its public methods, and what each does
  - A short "how it fits together" section: startup order (`PersistanceManager` → shelves register → books spawn), the drag-and-drop flow, and the parse → DB → panel flow
  - Write it from the `<summary>` comments so the two stay in sync, and update it whenever a script is added or removed

### Dead code / leftovers
- [ ] Delete commented-out code: `ShelfLayoutService.cs:167-325` (old insertion-index approach), `ShelfModel.cs:42-52`
- [ ] Remove unused code
  - `CalculateLayoutFor`, which duplicates `ShelfLayoutInfo.ToWorldPosition` (`ShelfLayoutService.cs:55`)
  - `UpdateBookInDatabase` (`SQLiteService.cs:201`)
  - `BookModel` (still has the old `slotIndex`)
  - `LibraryExport`
  - The empty `HelperExtensions` class
  - The empty `OnSceneLoaded`
  - `FOLDERNOTFOUNDEXCEPTION` in `PersistanceManager`
  - `oldVertUVDict` / `newVertUVDict` in `RemapWrapUVs`
  - The unused `halfRealWorldHeightForPlacement` in `Initialize`
- [ ] Remove `ShelfOverlapDetector`. The shelf ruler replaced it, but it's still on `NewBookPrefab` and `UpdateDrag` still reads it (`BookDragManager.cs:100-104`).
- [ ] `DebuggingScript` is attached to `NewBookPrefab`. Remove it or move it to a debug-only setup; it also duplicates `SetBookSize`.
- [ ] Remove `InitializeCube` and its branch in `SpawningService` if the cube book is retired; `Initialize` is a near-copy of it.
- [ ] Remove the leftover `Assets/ReDo/` folder and `Assets/UIHandler.cs` (old scene and scripts). First move `UIBookData` into `Scripts/`, because current code depends on it.
- [ ] Build settings point to a scene that no longer exists (`MainBookshelfViewScene`), and `testingScene` isn't in the build

### Structure / consistency
- [ ] Split UI-panel code out of `PersistanceManager` into a new `BookPanelController` (instantiating UI books, the book-ID dictionary, cover sprites, restore/remove from panel). **Step recipe ready:** [Recipes/BookPanelController.md](Recipes/BookPanelController.md)
- [ ] Rename `PersistanceManager` → `PersistenceManager`; the code currently uses both spellings
- [ ] One singleton pattern (there are 3 slightly different ones), maybe as a shared base class
- [ ] Put everything in namespaces. `BookObject3D`, `Object3DDragSource`, `PersistanceManager` and `UIBookData` are in the global namespace.
- [ ] Move `<summary>` comments that sit above `namespace` onto their classes (`BookDragManager`, `DragPlaneProjector`). Also refresh `ShelfLayoutService`'s summary, which still describes insertion indexes.
- [ ] Inject dependencies consistently. `SQLiteService` mixes `_filePath` with `PersistanceManager.Instance._dbPath`, and `ShelfLayoutService.ClearShelf` uses the singleton despite having injected services.
- [ ] Drop the `Unity.VisualScripting` dependency. It's only used for `AddComponent` in `BookDragManager` (use `gameObject.AddComponent`) and is unused in `APIDataFetcherService`.
- [ ] Use types instead of strings: `shelfType` → `eShelfType`, `PageCount` → `int`
- [ ] Consistent naming in models (`bookID` vs `Title` vs `book_button`)
- [ ] Collapse duplicated logic: the two cover-finding loops in the parser
- [ ] Replace the custom `StringExtensions.IsNullOrWhiteSpace` with the built-in `string.IsNullOrWhiteSpace`
- [ ] One constant for the default page count (both 200 and 300 are used)
- [ ] Rename `ParseFolderToJSON`, since it doesn't produce JSON anymore
- [ ] Fix mojibake `�` in log strings (`BookObject3D.cs:196`, `DebuggingScript.cs:131`)
- [ ] `async void ParseBooks` swallows exceptions; wrap it in a try/catch or make it return a `Task`
- [ ] Remove the empty `catch {}` in `RemoveBookEntryFromUIBookDataDict`
- [ ] Logging: replace scattered `Debug.Log` calls (per book, per drag, raw SQL strings) with a small logger that can be turned off in builds
- [ ] DB schema versioning / migrations, so table changes don't require deleting `database.db`

## Efficiency
- [ ] SQLite: every query opens a new connection. Keep one connection open, and wrap multi-row inserts in a transaction (book + user data + series).
- [ ] Spawning a book rebuilds everything each time: reads the cover from disk, decodes it, renders an atlas, then `ReadPixels` (GPU stall). Cache the atlas per book ID; this pairs well with pooling.
- [ ] Memory leaks on every spawn and destroy: the atlas `Texture2D`, the instanced mesh from `.mesh`, and the UI sprite textures are never destroyed
- [ ] `LoadImageAsSprite` calls `LoadImage` three times: three file reads, three decodes, and two leaked textures (`PersistanceManager.cs:168`)
- [ ] `GetDominantColor` calls `GetPixel` per pixel; use `GetPixels32()` once
- [ ] EPUB parsing (zip + XML) runs on the main thread, so the app hitches during import. Parse on a background thread and hop back to the main thread for Unity calls.
- [ ] `FindFreeSpans` allocates (LINQ + lists) on every call, and the ghost preview will call it every frame
- [ ] `RemoveItemFromShelves` scans every shelf; keep an item → shelf lookup
- [ ] `UUIDHelper` creates a new `SHA256` per call and never disposes it

## Fixes / things to account for
- [ ] Spine width is hardcoded to `0.10f`; make the dynamic `ComputeSpineFraction(pageCount)` work with the new FBX (`BookObject3D.cs:274`)
- [ ] Spine color sampling: add a separate sample step for the cover so the average favours the area near the spine (`BookObject3D.cs:387`)
- [ ] Confirm a page count can actually be read from EPUBs; if not, change the `ePageCountSource.Epub` enum (`Enums.cs:17`, `APIDataFetcherService.cs:30`)
- [ ] Rethink Google API retries / rate limiting (`APIDataFetcherService.cs:105`, `EpubMetadataParserService.cs:74`). The retry counter is already per book, but once Google rate-limits, every later book also fails all 3 tries, so it acts like a shared limit.
  - Space out requests across the whole parse (e.g. at most ~1 per second) instead of firing them back to back
  - When a 429/503 arrives, pause the whole queue rather than just that book; honor the `Retry-After` header if present, and use longer waits (the current 2s/4s/8s can't outlast a per-minute limit)
  - Treat 403 quota errors (`rateLimitExceeded`, `dailyLimitExceeded`) as rate limits; if the daily quota is gone, stop calling the API for this run
  - Don't retry errors that won't succeed (400, other 403s); every non-success response is currently retried
  - Books that couldn't be enriched still get saved; mark them so a later launch retries just those
  - Cut wasted requests: the ISBN → title fallback doubles calls
- [ ] Pass the actual EPUB path to `ParseOpfStream` instead of `null` (`EpubMetadataParserService.cs:92`)
- [ ] Roll back if removing a book from the shelf fails partway, since the book may end up off the shelf but not back in the panel (`BookDragManager.cs:212`)
- [ ] Books with no cover image crash. `GetDominantColor` is called with a `null` texture (`BookObject3D.cs:361`), and `LoadImageAsSprite` does `.width` on `null`.
- [ ] `FormatDates` throws on year-only dates like `"2005"` (common from Google Books) and on empty dates, which makes the whole book insert fail. It also parses `DateTime.ToString()`, which depends on the system locale, and has a `"yyy-MM-dd"` typo. Pass `DateTime`s directly.
- [ ] Foreign keys aren't enforced; SQLite needs `PRAGMA foreign_keys = ON` per connection. `shelf_positions` also has a redundant `UNIQUE` + composite key.
- [ ] Shelf IDs come from the GameObject name (`ShelfModel.cs:21`), so renaming a shelf in the editor orphans its saved book positions. Use a serialized, stable ID.
- [ ] `FindShelfUnder` reads `shelf.layoutInfo` before checking `shelf` for null (`BookshelfModel.cs:27`)
- [ ] Cover detection only accepts JPEG, so PNG covers are ignored (`EpubMetadataParserService.cs:204`, `:235`)
- [ ] `CoverCacheService` uses `File.Exists` on a directory (should be `Directory.Exists`)
- [ ] `PersistenceManagerReady` fires inside `Awake`, before anything can subscribe to it
- [ ] `spawningService` is silently `null` if `SpawningService` isn't on the same GameObject as `PersistanceManager`
- [ ] `BeginDrag3DObject` doesn't set `currentlyDraggedObjectUID`
- [ ] Verify `DragPlaneProjector.BuildPlane`: the normal is `forward` but the offset uses `right`, while the comment says both are along the same axis
- [ ] Clean up titles before searching Google Books (`APIDataFetcherService.FindMatch` / `CreateGoogleURL`). Titles with series or edition info in parentheses never match, e.g. "Wicked Crown (Shattered Kingdom Book 2)" and "Far From the Madding Crowd (Barnes & Noble Classics Series)"
  - The `intitle:` query includes the parenthetical, and the title-match check asks if Google's (shorter) title contains the full EPUB title
  - Build a search-only title: strip `(...)` / `[...]`, trim whitespace; keep `book.Title` unchanged for display
  - Use it for both the URL and the title-match check
  - Books still not found after this (e.g. "Two Princes of Summer") are candidates for the Hardcover fallback
- [ ] Placeholder publish dates from the EPUB block Google's real date. Villette is stored as `0101-01-01`, which is Calibre's "unknown date" placeholder; since `ApplyMatch` only fills `PublishedDate` when it's empty, Google never replaces it
  - When parsing the EPUB, treat `0101-01-01` (or any year before ~1000) as empty
  - Related to the `FormatDates` item above

## Before release
Do these near the end of development, before shipping.
- [ ] Move the Google Books API key out of the app: a small server (or serverless function) holds the key and makes the Books API calls for the app. Any key built into the app can be extracted, even from `Resources/`.

## Done
- [x] Ghost preview while dragging onto a shelf (snapping step 4)
- [x] Google Books API key: regenerated in Cloud Console and loaded from git-ignored `Resources/Secrets/GoogleBooksApiKey`
- [x] Google ISBN fallback no longer wipes the ISBN or overwrites fields (replaced by the search-mode loop: `FindMatch` / `ApplyMatch`)
- [x] `GetWithRetryAsync`: logs the real status code and Google's error body, one merged retry branch, no wait after the final attempt
- [x] Removed the unused `HttpClient` from `APIDataFetcherService`
- [x] `SelectLiteBookInfo` returns `null` on a miss or a failed query, so new books get imported again and `RestoreBookToPanel`'s null check works
- [x] Parser no longer re-fetches known books that have no cover (`results.Add(known); continue;` moved out of the cover check)
