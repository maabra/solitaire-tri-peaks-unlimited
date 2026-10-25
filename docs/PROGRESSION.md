# Campaign pacing and special worlds — 2.4

New MP24 hands stretch card-mechanic introductions and difficulty across the first
50,000 levels. Existing activities stay available early, and every existing card
mechanic remains in the game. Bonus levels still occur every 25 levels; chapter
completion rewards, card backs and maps still use 100-level chapters.

| Level | Card mechanic |
| --- | --- |
| 50 | Stock gifts |
| 300 | Ice |
| 1,200 | Keys and locks |
| 2,500 | Bombs |
| 5,000 | Moving up/down ranks and directional stock bonuses |
| 8,000 | Stone armor |
| 11,000 | Lanterns |
| 14,000 | Lucky stock cards |
| 18,000 | Vines |
| 22,000 | Shield cards |
| 28,000 | Tornadoes |
| 35,000 | Fog |
| 43,000 | Sun/moon gates |
| 48,000 | Color seals |
| 50,000 | Full difficulty caps |

Rules for each mechanic remain unchanged. Hazard count, key requirements, fuse
slack, armor hits, vine streaks and stock pressure grow on their own bounded curves.
Chapter focus rotates through the unlocked mechanics every 100 levels. After
50,000, difficulty stays capped while seeds, formations and combinations vary.
This is the full current ruleset, not a claim that every following level adds a
new mechanic. The campaign remains 480,000 levels.

MP16/18/19/21/22 codes retain their old generation and pacing. Existing active
hands resume their stored board; restarting that board preserves its generation.
New hands use MP24. Existing discoveries, inventory, collections and rewards remain.

Nine independent original paintings in mode-worlds.png give bonus gardens, balloon
flights, ticket trails, regatta, diamond caverns, spade gardens, gold courtyards,
endless meadows and Gem Garden distinct scenery. Mode badges show where you are.
Automatic tables match the mode; manually selected earned tables take precedence.
When activities overlap, flight or ticket identity takes priority, then special
objectives, then ordinary bonus scenery. The bonus rewards still apply.

progression.mjs owns pacing; mode-scenes.mjs owns scenery and precedence. Both
Windows and web use these same modules. Tests cover all introduction boundaries,
retained mechanics, maximum difficulty, mode precedence and old deal-code support.
