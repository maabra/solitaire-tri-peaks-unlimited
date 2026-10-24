# Browser edition

The original browser game is in `web/`. It uses standard ES modules, has no build
dependencies, and runs entirely in the browser.

For a local preview, run `python -m http.server 8765 --directory web` from the
repository and open http://localhost:8765. Opening index.html directly with a
file URL does not support ES modules reliably.

For GitHub Pages, select **Settings > Pages > Build and deployment > GitHub
Actions** once. The `Play on GitHub Pages` workflow tests and publishes only the
original browser files after pushes to main. It excludes reference examples,
Windows binaries, tests, and developer files from the published site.

## Windows and browser parity

Both editions include the 480,000-level campaign, 24,000 chapters, endless mode,
layered formations, 5–8 tower layouts, fifteen card kinds, eleven consumables, the shared coin economy,
Gem Garden, hint timers, temporary effects, two-second stock draw cooldowns,
ten-second rescue cards, thirty-second fresh deals, daily and garden gifts,
chapter gifts, bonus levels, picnic goals, favorites, star hunt, and versioned MP16/MP18 codes.
Windows has a native animated effects layer and synthesized sound; the browser
uses CSS/Web Animations and Web Audio. Their visual presentation differs.

Browser progress stays in local storage on the current browser/device. It does
not automatically sync with Windows. Settings provides JSON save import/export;
the current schema and active boards work in both editions. Back up progress
before clearing browser data. Windows saves to the path in PLAYING.txt.

## Keeping future features aligned

Changes to rules, seeds, rewards, helpers, timers, or saves must ship in both
`src/MeadowPeaks.Core` and `web/core.mjs` / `web/session.mjs` in the same change.
The JavaScript core uses classes for the same replaceable responsibilities as
Windows: Hand, GemGame, Session, and ProfileStore. Views do not award coins or
implement card rules. A common seed format and saved-board schema preserve
cross-platform deals and active hands.

Run native tests, regenerate fixtures with
`dotnet run --project tests -c Release -- --export-web shared/engine-fixtures.json`,
then run `node --test web/tests/engine.test.mjs`. The fixture comparison checks
exact card order, formations, metadata, winning paths, and Gem Garden turns.
Add representative fixtures for each new component or rule before shipping it.
Optional UI checks: serve web on port 8765, then run
`python scripts/test_web_browser.py` with Playwright and Edge installed.

The campaign count supports a planning estimate of 8,000 campaign hours at one
minute per level, plus up to 2,000 hours budgeted for endless. This is an estimate,
not a measured playtime guarantee or an artificial play limit.

## Release 1.8 presentation

New deals use 16 story formation families with deterministic geometry variations:
rotated fans, rings, twin arches, butterflies, ribbons, diamond loops, starbursts,
5-8 towers, crowns, hourglasses and serpents. Explicit cover graphs match rotated
card intersections. Old saved boards and MP16 codes keep their original rules;
new layouts use MP18 codes.

Both editions share the original generated story-meadow illustration. Saved
reference pages, videos and examples remain excluded from builds and source ZIPs.
Unchanged cards retain their controls/DOM nodes during stock draws and clears.
Gem Garden displays swap, connected-match highlights, coin bursts, falls and
cascades; reduced motion uses a brief static highlight. Helpers are hidden until
discovered, and obstacle helpers stay off boards where they have no target.

Fresh Garden has a minimum one-hour real-time recharge. Its deadline, gem board
and successful swaps are saved, including across browser reloads and Windows
restarts. Menus do not pause this deadline.


### Recording-based card presentation (1.8.1)

The supplied October 24 gameplay recording informed the new rank-first card faces:
a large central rank, small corner rank and suit, cream paper, blue patterned backs,
and strong red/dark suit contrast. Both Windows and browser render these faces using
original drawing code. Existing formations, obstacle rules, timers, saves and MP16/MP18
shared codes retain their behavior. The browser stock now has a layered fan silhouette
and a 300 ms stock-to-waste draw animation. Reduced motion skips that flight.
Untouched tableau nodes remain in place throughout a draw.

Reference recordings and extracted frames are development-only material under examples
and artifacts; neither directory is included in distributable packages. These changes
adapt presentation cues; they do not copy the reference game's assets or claim feature
parity with it.
