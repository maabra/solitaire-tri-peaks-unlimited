# Architecture

## Shipping application

The application has one shared interface and gameplay implementation in `web/`.
GitHub Pages serves it directly. The Windows EXE embeds the same assets as manifest
resources and displays them in `DesktopGameWindow`, a WinForms WebView2 host.
The older GDI renderer remains available only for legacy diagnostics; it is not
selected by normal launches and is not the UI to extend.

`DesktopGameWindow` intercepts requests to its private HTTPS origin and serves exact
packaged resource bytes. It does not start a web server or fetch the published site.
External navigation, new windows and external document resource requests are blocked.
`DesktopAssets` unpacks the bundled, pinned Fixed Version renderer into a local cache.
The renderer is bundled rather than installed system-wide. Runtime preparation is
serialized across processes and recovers an abandoned extraction mutex. Windows 10
App Container read/execute grants apply only to the runtime directory.

The Windows package includes Microsoft runtime/SDK notices. Fixed Runtime distribution
and Windows 10 permissions follow [Microsoft's distribution documentation](https://learn.microsoft.com/microsoft-edge/webview2/concepts/distribution).

## Replaceable parts

- `Hand`, `GemGame` and the seeded deal modules own rules and deterministic state.
- `Session` coordinates commands, rewards, helper inventory and completion.
- `ProfileStore` owns storage and validation. The browser uses localStorage. In
  Windows it uses the injected `meadowDesktop` bridge and the existing `IProfileStore`.
- `board-view.mjs` reconciles surviving card nodes and owns local deal/clear/draw feedback.
- `gem-view.mjs` owns swap, connector, match, fall/refill and cascade presentation.
- `game-art.mjs` owns icon names, atlas lookup and semantic aliases; gameplay does
  not depend on the artwork's pixel arrangement.
- `semantic-icons.mjs` owns original vector symbols for distinct UI meanings;
  `progress-view.mjs` renders named current/target counters without changing state.
- `LevelPreview` in `level-preview.mjs` owns preview markup and pending selection.
  Its API is `show(level, options)` and one-shot `take()`. It receives a session
  getter and modal callback; it does not start a hand or spend a life.
- `LivesPanel` in `lives-panel.mjs` owns the hearts screen; `Lives` in `lives.mjs`
  owns capped heart counters. Only Session commands perform attempt transitions.
- `ui-markup.mjs` provides escaped text and shared action-button markup.
- `MenuJourney`, `BirdGuide`, tutorial data, music, reward presentation and small
  visual effects live in their own modules. Replacing presentation must not alter
  reward accounting, RNG or save validation.
- `chapter-worlds.mjs` and `chapter-collection.mjs` select the 100 linked chapter
  worlds, tables and backs. `chapter-scene.mjs` supplies baseline environments.
- `adventure-map.mjs` owns logical route geometry and activity artwork.
- `style.css` owns the common colorful theme, spacing, cards, panels and responsive rules.
- `help-ui.css`, `progress-ui.css` and `lives-ui.css` own focused feature overrides;
  `map-ui.css` owns map layout and shared presentation refinements. New CSS files
  are included by the web packager and Windows resource glob automatically.

Windows receives validated save messages only from the packaged origin. The close
handler flushes the current state before disposing the view. A named mutex prevents
two normal Windows hosts from writing the same profile. Browser stores compare the
last loaded snapshot before writing, and storage events stop a stale tab until reload.

## Save compatibility

Profile version 1 remains supported. The Windows host first restores through the C#
factory, then captures a complete board for the shared renderer. Legacy saves without
embedded geometry are migrated in memory. Invalid active hands or garden rounds can
be removed while preserving valid wallet and campaign progress. Startup checks do
not save these in-memory changes. Normal save writes retain the recovery backup.

New shared-engine hands use MP24. Legacy providers retain MP16, MP18, MP19, MP21
and MP22 code compatibility; saved geometry and explicit blockers remain authoritative. Visual
changes never enter the deck RNG. C# remains the compatibility/reference domain,
with Windows/browser fixture parity tests, rather than a second shipping UI.

## Presentation invariants

The map is a 960x10200 logical portrait scene. A single camera scales its background, SVG route
and markers together. HUD panels are outside the transformed scene and remain fixed
while the world pans. Six-digit level labels use smaller type, rather than clipping.
The view preserves image proportions and supports pointer dragging and touch panning.

Solitaire mutations patch existing controls. Resizing updates the board's dimensions
and card coordinates without replacing surviving nodes. Board height follows actual
rotated geometry; compact desktop windows scale the play area to keep actions visible.
Temporary visual state includes the waste rank and obstacle bypass states, so Wild,
Thaw, Keypass, Prune and Phase expiry refresh graphics immediately. Hint animation
and helper cooldowns remain temporary and respect reduced motion.

The wheel credits its prize and consumes its earned spin before animation. Interrupted
or reopened panels cannot duplicate it. Gem controls cannot create a fresh round in
the middle of a cascade. Starting a different hand asks before discarding the active
one; reopening that same level resumes it.

## Verification

The engine fixtures cover 307 Windows/browser deals, 60 gem turns, saves, rewards,
old/new codes and timers. Targeted native tests include all wheel prizes and save
consumption. `test_web_browser.py` verifies full wins, rewards, garden animation and
persistence. `test_presentation_edges.py` checks six aspect ratios, route alignment,
six-digit labels, actual painted asset loading, resize identity, rapid draws, hand
replacement, tab conflicts and temporary effect artwork. Windows `--smoke` exercises
the packaged shared renderer with an isolated test profile. `--startup-check` loads
the real profile read-only. Settings and the project assembly share a tested build stamp.

## Adventure boundaries (2.1)

AdventureHand extends Hand for mutation-aware MP21 rules and winning witnesses.
AdventureSystems owns local event state/reward ledgers; AdventureUI owns their
panels. ChapterWorlds owns seeded paintings, thumbnails, unique names and tables.
The map route contains 100 stops per chapter on a 6,800-unit scrolling world.
DesktopProfileBridge preserves raw shared JSON atomically, avoiding data loss
through the legacy typed model. Browser consent runs before Session loads a save.
See ADVENTURES.md for the complete rules and TODO.txt for deferred undefined scope.


## Journey expansion 2.2

See [JOURNEY.md](JOURNEY.md) for the current portrait worlds, earned cosmetics,
Pip navigation, MP22 formation generation and compatibility details.


The 2.3 menu frame, contextual help layers and presentation contract are documented
in [UI.md](UI.md). Both stylesheets are embedded in Windows and packaged for web.


2.4: new MP24 deals pace card introductions and bounded difficulty through 50,000
levels. Nine original mode paintings provide distinct special-mode scenery.
See docs/PROGRESSION.md for the complete schedule and compatibility rules.


Current 2.4 collection counts supersede the historical counts above: 100 matched
chapter scenes, card backs and tables. See COLLECTIONS.md and SIDE-GAMES.md.
Cards rotate to a player-facing angle without changing cover geometry; scenery
blur is isolated from the sharp cards and UI.


MP26 adds an original structured-layout catalog in `story-layouts-v4.mjs` while
retaining each earlier generation unchanged. `progression.mjs` still controls the
50,000-level mechanic ramp. Only MP26 allows multiple decks for occasional long
gardens, capped at 120 cards; older active-hand limits remain 52. The view fits the
whole deal, not the remaining cards. `RankShuffle` mutates persisted face ranks
without changing deal identities or cover links. Surface and wheel presentation
are separate modules. The desktop host embeds these same modules unchanged.

Menu scenery is an independent presentation decorator (`menu-scenes.mjs`) with a
shared responsive stylesheet (`adventure-restyle.css`). It does not mutate profile
or activity state, replace event handlers, or introduce a second navigation stack.
