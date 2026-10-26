The three original scene assets in `web/assets/` were generated with the built-in
image generation tool and inspected before integration. All prompts request a
wide, polished painterly storybook scene with focal objects inside the central
70%, generous margins, no text, UI, characters, border or watermark.

- `treasure-trail-scene.png`: enchanted woodland, winding stepping-stone path,
  golden treasure chest under a flowering tree, turquoise creek and quiet lawn.
- `gem-garden-scene.png`: sunlit glass greenhouse, six colors of crystals growing
  among plants in terracotta pots, quiet central patio, mint and golden light.
- `matching-garden-scene.png`: lilac walled garden, paired ceramic flower pots,
  two butterfly topiaries, rose arbor and central lawn in warm morning light.

Treasure Trail and Matching Garden show dedicated framed scene banners; Gem
Garden uses its own full scene rather than a cell from the shared mode atlas.
UI borders are native CSS so they remain sharp at different window sizes.
Atlas icons use isolated SVG viewports with breathing room, keeping each cell
square when its containing control changes size.

The map refresh uses the built-in image generation tool for two new raster assets:

- `web/assets/map-meadow-v2.png`: a tall, almost top-down storybook meadow village
  with orange-roof cottages, watermill, turquoise stream, flowers and rounded trees
  along the sides. A quiet grassy center accommodates the road overlay. No road,
  sky, horizon, text, UI, characters or watermarks. Matching grassy top and bottom
  edges were requested for vertical tiling. Warm green, honey, turquoise and coral.
- `web/assets/road-stone-border.png`: a seamless square, top-down material tile of
  closely fitted, rounded ivory and honey limestone cobbles, softly beveled edges,
  moss-green joints and gentle overhead lighting. No perspective, central feature,
  frame, text, UI, characters or watermarks. Used as a pattern on the road edging.

The meadow painting replaces the Willow Mill Village map collection, including its
later numbered journeys. Other chapter collections retain their distinct scenes.
Road edging uses the generated stone texture throughout the campaign. Rendering
keeps the painting's aspect ratio and the original route and level coordinates.

The latest `web/assets/map-meadow-720p.png` supersedes the portrait meadow image
for the live village map. It was generated with the built-in image tool as one
complete 16:9 painting for the fixed 1280×720 screen. The prompt specifies a
watermill cottage at x=190–360/y=260–460, creek and waterfall at
x=940–1100/y=240–520, quiet central meadow at x=350–940/y=180–580,
subdued left 160/right 150 pixel margins, a low-detail top 80 pixel header band,
quiet chapter-banner space at x=470–810/y=80–195 and bottom control space at
x=430–850/y=600–710. Polished painterly spring scenery, orange roofs, turquoise
water, green meadow, pink flowers; no road, UI, text, numbers, characters or frame.
The application shows the entire painting without vertical tiling; only the road
and level markers scroll above it.

`web/assets/level-notice-board.png` was generated with the built-in image tool for
a compact level-preview popup. Prompt: one front-facing square wooden village
notice board, rounded honey-brown carved frame, gold/cream beveled trim, one large
blank ivory parchment pinned inside, low-contrast empty center, tiny pink blossoms
and sage leaves at the outside upper corners, brass pins, soft dimensional
storybook game lighting. Entire board centered with transparent surrounding margin
and shadow. No stand, legs, scene, perspective tilt, writing, numbers, buttons,
cards, character or watermark. Intended for a roughly 520×500 popup inside 720p.
The generated alpha is preserved. Exterior game bouquets extend the existing
native vector blossom system in `web/assets/frame-corner-bouquet.svg`.


## Painted interface refresh, 2.4.12

Generated with the built-in imagegen tool, preserving its original transparent
alpha. Project assets:

- `web/assets/painted-ui-controls.png`: 18 painted controls and difficulty icons.
- `web/assets/painted-ui-adventures.png`: 18 painted activity and status icons,
  including Treasure Trail dice/chest and Matching Garden flower tiles.
- `web/assets/painted-garden-jewels.png`: six painted, shape-distinct match pieces.

Prompt set: production sprite sheets on a truly transparent background, chunky
premium storybook village art with dimensional hand-painted polished surfaces,
cream highlights, honey gold details, soft plum shadows, turquoise/coral/lilac
palette. Full objects, generous padding, no labels, text, grids or surrounding
buttons. Controls and adventures: 1536x1024, six columns by three rows. Controls,
row-major: card fan, closed gift, open gift, purple mountains, crown, speaker;
muted speaker, motion comet, settings cog, clock, check, close; undo, restart,
left, right, fullscreen, export. Adventures, row-major: import, unlocked padlock,
star, musical notes, swapping arrows, heart; ascending streak bars, open rule
book, bookmark, code card, dice with blossom-decorated treasure chest, sprout;
paired purple/turquoise flower tiles, palette and brush, folded trail map,
completed card stack, star medal, target. Jewel sheet: three columns by two rows,
pink heart, blue diamond, yellow star; green square, purple round, cyan hexagon.
Each jewel has a polished crystal bevel and clearly distinct silhouette at 48px.

`web/painted-icons.mjs` measures each UI object's individual region rather than
assuming evenly spaced image content. Isolated clipped SVG image viewports keep
neighboring assets from appearing on icon edges, while square viewboxes preserve
proportions. These replace the former code-drawn semantic symbols everywhere.
Existing painted event/helper/chest/Pip art remains in use. Rarity, map and
preview stars now share the painted star; unearned stars are desaturated.


## Shared menu boards, 2.4.14

Small completion, level-start, short confirmation and earned-prize menus reuse
`web/assets/level-notice-board.png`. Larger menus and contextual explanations
reuse the new `web/assets/menu-parchment-frame.png`, generated with the built-in
imagegen tool. `web/menu-boards.css` renders it as a nine-slice image border,
keeping corner blossoms and the wood rails intact at different dialog sizes.

Generation prompt: production UI frame, landscape 1536x1024, front-facing cozy
storybook village notice board; honey-brown polished carved wood, rounded
corners, narrow cream/gold inner bevel, pink blossoms and sage leaves confined
to four 110x110 outer corners. Rails within outer 90px. All central space blank
pale warm ivory parchment with subtle low-contrast paper grain. Straight rails,
orthographic view, frame touching image perimeter. Warm dimensional hand-painted
light from upper left and plum-brown shadows. No raised crest, stand, legs,
writing, text, numbers, icons, characters, buttons, screws, scenery or watermark.


## Home and scrolling map, 2.4.17

Home now reuses the generated nine-slice menu parchment for its welcome,
activities and garden panels, all composed inside the fixed 1280x720 game.
It replaces the tall marketing-style hero and offscreen activity buttons.

At the user's request, the overworld scenery again lives inside the scrolling
road canvas. Meadow maps reuse the previously generated tall, vertically
repeatable `map-meadow-v2.png`; the chapter's landscape painting remains available
for the home/game scene. Other chapter murals also scroll with their route.
The map's decorative frame, activity menu and navigation remain fixed furniture.


## Village menu places, 2.4.18

Built-in imagegen produced four original 1536x1024 backgrounds. Exact production
prompts are recorded in `docs/menu-place-prompts.json`.

- `web/assets/menu-place-shop.png`: village general-store counter and shelves on
  the left, a large empty merchandising parchment on the right.
- `web/assets/menu-place-study.png`: a blank open journal on a lamplit writing desk,
  shared by settings, the progress journal, rules and helper explanations.
- `web/assets/menu-place-collections.png`: an empty scrapbook on a craft table,
  shared by albums, packs, postcards and card customization.
- `web/assets/menu-place-adventures.png`: an empty village bulletin board with
  rolled map and compass, shared by adventure directory, travel and activities.

All artwork was inspected before integration. HTML owns all labels, buttons,
quantities and purchases. `menu-scenes.mjs` assigns each menu its place. Small
confirmation, level and reward notices retain the existing generated notice
board. Existing dedicated garden/trail/memory paintings are retained.

The shop uses rows for the three introductory helpers and scrollable shelves for
larger inventories. Wallet and Gem Garden remain visible in the left counter;
prices, Owned quantities, helper explanations and Buy buttons are explicit.
Purchases retain shelf scroll and update owned quantities. Settings separates
Audio, Playing cards and Saved adventure. Nested menus have one back route.


The same release fixes internal seams in card lettering: the former 0.6px text
stroke traced overlapping glyph contours, and the display weight exceeded the
bundled Baloo font's range. Card faces now use weight 800 without strokes or text
shadows, with dedicated spacing for two-digit ranks. All 52 rank/suit combinations
are visually exercised by `scripts/test_card_lettering.py`.
