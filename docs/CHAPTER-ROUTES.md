# Chapter roads and gates (2.5.1)

`ChapterRoutes.cs` owns 100 art-directed terrain corridors, one per original chapter mural. Each row records horizontal percentages at sixteen equal height intervals. The content exporter validates their count and bounds and exports them with the rest of the C# catalog. These are authored visual routes, not an automatic building detector or a pixel collision mask.

`adventure-map.mjs` samples those corridors with bounded smoothstep interpolation. Road drawing uses dense samples rather than smoothing between level buttons, which could swing past a bend. Level buttons sit directly on the route and retain their original size and vertical spacing. The stone border and sand are narrower. Chapter one's route repeats in the same bottom-anchored coordinates as its meadow painting; the other routes stretch with their portrait mural. Changes to the crop, tile dimensions or artwork require reviewing its corridor too.

An illustrated gateway sits beyond each chapter's last stop, clear of the fixed chapter ribbon. Completing level 100, 200 and subsequent chapter endings offers **Open chapter gate**. The gate enters the next map focused on its first level. The final gate opens an endless-hand preview. Progression and chapter prizes still use the existing saved win transaction; entering a gate grants no duplicate rewards and charges no coins or hearts. Both the displayed state and action handler check completion. Previously completed chapters remain open.

Verification includes all 100 corridor bounds and aligned stops, tile-repeat alignment, locked/open/final gate states, a real level-100 win followed by entry to level 101, a forged locked-gate action, final-level victory followed by Endless, map/menu scrolling, and header/frame clearance. Geometry tests do not establish pixel-perfect obstacle avoidance across every part of every painting.

## Gateway art

Asset: `web/assets/chapter-gate.png`. Generated with the built-in imagegen tool, with transparent surroundings and an open transparent arch. Existing map and road artwork remain visible through it.

Prompt: Create one production-ready 2D game sprite for Meadow Peaks, a cozy richly painted casual solitaire village game. A welcoming chapter gateway: an open arch made of warm carved honey wood and creamy pale stone, delicate gold trim, climbing green leaves and small pink spring blossoms around its outside, two tiny warm lanterns. Front-facing, nearly symmetrical, slightly elevated game-map perspective. Beneath the arch a small stone threshold and three shallow stepping stones; no surrounding scenery. The center is open and transparent so the map road shows through. No doors, lock, writing, letters, numbers, UI card, badge or large opaque glow. Compact silhouette readable at 144 pixels tall, polished outlines and soft painterly shading matching a cheerful mobile game. Entire gateway and leaves visible with transparent margins, nothing cropped. Portrait 4:5 composition.
