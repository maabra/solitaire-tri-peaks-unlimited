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
