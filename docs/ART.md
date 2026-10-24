# Original game artwork

The original image-generation artwork in web/assets is shared by Windows and web.

Ten painted environments: spring-village, snow-village, autumn-orchard,
moonlit-garden, lavender-valley, tropical-cove, desert-oasis, cherry-garden,
enchanted-forest and festival-garden. Each is a complete painting, rather than a
color filter over the same image. Chapter selection cycles through them.

`game-icons.png` is a transparent 8x4 atlas of 32 game icons. `interface-icons.png`
is a transparent 4x4 atlas of 16 interface icons. Name-to-cell mappings live in
`game-art.mjs`, so artwork and semantic actions can be replaced independently.
`ice-card-overlay.png`, `lock-card-overlay.png` and `vine-card-overlay.png` are
transparent card overlays. Existing original gem SVG artwork remains in gem-art.mjs.

The earlier original story-meadow and adventure-village paintings remain available
for legacy diagnostics. No images or audio extracted from the supplied reference
games or recordings are included in the released source or game packages.

## Adventure expansion 2.1

Four additional full-size paintings: lighthouse-harbor, crystal-cavern, sky-islands
and pumpkin-village. chapter-worlds.mjs generates bounded-cache, deterministic
chapter compositions and 4,800 distinct names. The same image is used on the map,
in Travel Hub and on its postcard. No 4,800 separate bitmap files are claimed.

event-icons.png: transparent 8x4 atlas, 32 new event/helper icons.
sticker-atlas.png: original 10x10 collectible tile painting.
card-tables.png: 4x4 atlas of sixteen uncluttered playing surfaces.
chapter-worlds.png: original 4x4 destination overview artwork.


## Journey expansion 2.2

See [JOURNEY.md](JOURNEY.md) for the current portrait worlds, earned cosmetics,
Pip navigation, MP22 formation generation and compatibility details.
