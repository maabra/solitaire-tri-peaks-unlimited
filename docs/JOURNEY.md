# Connected journeys 2.2

The first 51 chapters have independent original portrait paintings and thematic
names. Seventeen `portrait-worlds-NN.png` sheets each contain three full-height
paintings. These are new scenes, without tint, mirror or filter variations.
Chapters after 51 reuse that collection with numbered journey names. Travel Hub
and postcards use the matching chapter picture.

Each map contains 100 levels. The painting scrolls with a 960 by 6200 world;
the thick winding road, short spurs and alternating left/right stops share its
coordinates. Fixed controls stay accessible while scrolling. Activity destinations,
the journey rail and related-panel links connect rewards to their next useful action.

Pip introduces unlocked activities, shows their actual controls and explains newly
encountered special cards. Ask Pip opens the guide again. Seen lessons are saved.

Completing a chapter earns its card back and table in Card Studio. Older completed
chapters receive these unlocks retrospectively. Selection persists and changes no
gameplay stats. There are 56 original back motifs in `collectible-backs.png`;
later chapter backs use decorative borders and unique gold seals. Four atlases
contain 64 distinct table designs, including the original sixteen. Tables can follow
the chapter automatically or stay on an earned selection. The fitted table uses
nine-slice drawing around the actual board, retaining corners on narrow screens.

`art-catalog.mjs` owns names and atlas bounds. `chapter-worlds.mjs` owns rendering
and bounded image caches. `customization.mjs` owns cosmetic eligibility and selection.
`overworld-view.mjs` owns map presentation; `journey-guide.mjs` owns onboarding.
Both Windows and browser load these same modules and assets.

New MP22 hands use 64 deterministic formation families in `story-layouts-v3.mjs`:
interwoven braids, boxes, circles, rings, arches, spirals, islands and linked shapes.
Explicit acyclic cover graphs drive exposure and the mutation-aware deal solver.
Saved older hands and MP16/18/19/21 codes retain their original layouts and rules.
All 480,000 campaign seeds passed actual mutation-aware winning-witness replay.

Browser checks cover all 51 chapter pictures, natural feature tours, retrospective
cosmetics, persistence and table fitting at six viewport sizes, including 320 pixels.
The expansion preserves local-only saves and contains no reference game assets.
