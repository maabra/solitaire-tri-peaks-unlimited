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
