# Current adventure expansion

Version 2.1 adds 100-stop scrolling chapters, uniquely named/composed destinations,
matching Travel Hub thumbnails, playing tables, local-save consent and all offline
adventure systems. See [ADVENTURES.md](ADVENTURES.md) for the current complete rules.
The following section describes the shared-renderer foundation introduced in 2.0.

# Shared Windows and web presentation

Version 2.0 uses the same `web/` interface, rules, CSS, animations and artwork on both
platforms. GitHub Pages hosts these files. The Windows EXE embeds and serves them to
a bundled WebView2 control from a private local origin; it does not depend on Pages.

The map camera scales artwork, route and markers together, while the HUD stays fixed.
Drag or swipe to explore on narrow/tall windows. All panels follow the colorful ribbon,
purple control and green primary-action theme, with shared spacing and close targets.
Forty-eight illustrated icons cover menus, helpers and obstacles. Transparent ice,
lock-and-chain and vine overlays keep the central card rank visible.

Ten actual original paintings supply chapter environments for BOTH the menu and
solitaire: spring village, snow village, autumn orchard, moonlit garden, lavender
valley, tropical cove, desert oasis, cherry garden, enchanted forest and festival
garden. Chapters cycle through these paintings, with additional seeded decoration.
They are shared environments, not 24,000 separately painted backgrounds.

Profiles stay local. Windows keeps `LocalAppData/MeadowPeaks/progress.json`; browser
storage is separate. Settings export/import transfers the compatible save. There is
no online synchronization, account or purchase system. A browser needs the page's
initial asset load; the packaged Windows game runs offline.

Build 2.0 is visible in Settings and checked against the Windows assembly version.
Versioned module imports avoid mixing old cached scripts with the updated interface.
The source and browser packages include all original artwork, and exclude references.

## Regression checks

Run a local static server on port 8765, then:

```text
node --test web/tests/engine.test.mjs
python scripts/test_web_browser.py
python scripts/test_presentation_edges.py
```

The browser checks cover a full legal win, connected gem animation, garden persistence,
shops/settings, six viewport shapes, aligned markers, maximum level labels, stable
resize, repeated draws, saved-hand resumption, confirmation before replacing a hand,
stale tabs and immediate Wild/Thaw expiry graphics. Invalid hands and garden swaps
preserve valid wallet/map data. Invalid saves cannot replace a valid stored profile.

Future UI and gameplay features belong in the shared modules, rather than a separate
Windows renderer. Update the build stamp, cache-busting imports, documentation and
relevant fixtures together when shipping a new version.


## Journey expansion 2.2

See [JOURNEY.md](JOURNEY.md) for the current portrait worlds, earned cosmetics,
Pip navigation, MP22 formation generation and compatibility details.
