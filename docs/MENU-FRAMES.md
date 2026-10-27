# Pip panels and Daily Picnic (2.5.2)

Pip's destination picker, tutorials, repeated-error tips and contextual explanations use `web/assets/pip-help-frame.png`. The frame has an opaque parchment center and transparent surroundings. `guide-frames.css` uses nine-slice rendering so corners remain intact while straight rails adapt to the panel. Only the destination list scrolls; its heading and dismissal button stay visible. A picker opened inside another menu mounts beside its content, so it cannot push the book page off its background. Short lessons retain the existing tutorial bay above helpers; jewel tips retain their horizontal lane.

Daily Picnic uses its own generated tray, `web/assets/menu-place-picnic.png`. Its three task cards separate the objective, completion state, capped progress, reward and claim action. The saved counters remain unchanged even when the visible progress reaches its target. Claims still go through `AdventureSystems.claimMission`, with the existing one-time claim and next-task behavior. The tutorial targets live in `InterfaceCatalog.cs` and are exported to the browser catalog. A smaller menu uses stacked cards; its content scrolls within the fixed tray.

The browser checks cover early/full destination lists, scrolling without moving the frame, opening and closing a parent menu, stable tableau/helper geometry, contextual-help focus and dismissal, picnic ready/pending states, duplicate claims and all six completed tasks. Desktop-sized picnic menus also check that the footer clears the painted wooden rim with the tutorial visible.

Both assets were produced with the built-in imagegen tool. The help-frame prompt specified a compact portrait wooden rail, delicate gold beading, small corner blossoms, a blank opaque ivory center, transparent outer margins and straight stretchable rails. The picnic prompt specified a landscape wooden serving tray with at least 85% blank ivory linen, a small basket and teacup confined to the outer corners, a gingham fold and meadow flowers. A follow-up edit reduced the basket and cup and placed all artwork fully inside the canvas with a grass margin; no UI labels or buttons are baked into either asset.

## Fixed painted surfaces (2.5.3)

Adventure and collection menus now use the same fixed-surface model as Settings: the complete painting belongs to the dialog, the modal is inset onto its paper, and only the content page scrolls. The translucent full-page wash and second border are removed. Picnic stays 960 by 600 logical pixels rather than stretching its painting when Pip appears; overflow stays inside the linen area. Navigation and close controls avoid the basket, teacup and decorative rails. Narrow Travel and Milestones rows stack their actions so labels retain usable space.

`test_menu_surfaces.py` verifies paper bounds, complete background coverage and stationary navigation/frame geometry during wheel scrolling. The existing Settings book checks and 26-menu, three-size audit also pass.
