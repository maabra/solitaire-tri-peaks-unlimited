# Architecture

## Shipping application

Version 2.0 has one shipping interface and gameplay implementation in `web/`.
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
- `chapter-scene.mjs` selects one of ten painted environments and seeded decoration.
- `adventure-map.mjs` owns logical route geometry and activity artwork.
- `style.css` owns the common colorful theme, spacing, cards, panels and responsive rules.

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

New hands use MP19. `StoryDealProvider` and the browser retain MP16 and MP18 providers
for old codes; saved geometry and explicit blockers remain authoritative. Visual
changes never enter the deck RNG. C# remains the compatibility/reference domain,
with Windows/browser fixture parity tests, rather than a second shipping UI.

## Presentation invariants

The map is a 1280x800 logical scene. A single camera scales its background, SVG route
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
