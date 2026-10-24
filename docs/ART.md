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
