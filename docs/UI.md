# Shared interface

Windows and browser load the same modules, styles and artwork. The current
presentation uses bundled Nunito interface text, Baloo 2 headings, cream panels,
coral ribbons, plum navigation controls and green primary actions. Fonts load
locally; their licenses are recorded in [THIRD_PARTY.md](THIRD_PARTY.md).

## Navigation and readable state

Map controls are Travel Hub, Play and Locate. Travel Hub owns chapter browsing
and level search. Play opens a preview first, including locked-level previews.
Menus have a compact shell with contextual Back, Resume when a hand exists, a
map return and close. Related activity tabs connect their rewards and resources.
Returning to a hand preserves its cards and score.

Numeric resources use icon badges, tooltips and accessible labels. Garden shows
moves, coins and swaps toward a heart. Activity counters and progress bars show
actual earned progress. Information buttons appear only where explanation helps.
The illustrated top-layer help dialog supports Escape and focus return.

Activity tabs reserve space for icons, shadows, hover and focus. Scroll tracks
hide scrollbar chrome while remaining scrollable. Laptop chest layouts keep the
primary action visible at standard screen heights; narrow layouts retain every
tier and use the menu's single scrolling region.

## Pip, chest states and level identities

Pip uses an original 16-expression illustrated atlas. Guides select a contextual
expression. Unrelated tutorials stay out of menus such as Settings; in-panel
Pip occupies document flow and cannot cover their headings.

Chapter chests show Bronze, Silver, Gold, Platinum, Diamond and Obsidian at
10/60/120/200/250/300 stars. Their rules and tier track are always shown, with a
centered chest, next milestone, current prizes and one opening per chapter.
Closed/open art reflects state. See [Pip and treasure](PIP-AND-TREASURE.md).

Map stops and previews distinguish hard, extra-hard, bonus, gold and suit levels.
Gift badges identify bonus stops. Quiet tabletop margins frame the portrait map.

## Stable play and faster results

`PlayViewport` fits once per hand and actual window dimensions. Moves, timers,
helper disappearance and menu returns reuse the scale. Timer chips reserve a row;
the table decoration class applies before measurement. Font readiness is included
in initial fitting. Hover lifts and card flights do not alter tableau geometry.

Base coins and stars are automatic. The result offers Next level directly.
Additional prizes use an optional reveal tray. Animation callbacks never award
currency; interrupted or repeated claims cannot duplicate rewards. Completion and
deal animations are shorter and skippable. Reduced motion applies throughout.

Wild remains armed for one match. Five rescue cards stay in stock until used.
Their shared helper recharge remains visible. Other temporary effects retain
their individual duration rules. Normal stock draws are immediately available, with no cooldown.

## Component boundaries

- `ui-shell.mjs`, `menu-journey.mjs`: modal frame and fresh-state route history.
- `typography.css`, `menu-polish.css`, `mobile-polish.css`: shared visual contracts.
- `level-preview.mjs`, `lives-panel.mjs`, `chest-view.mjs`: focused menu views.
- `pip-view.mjs`, `state-art.mjs`, `semantic-icons.mjs`: character and state imagery.
- `chest-tiers.mjs`, `win-rewards.mjs`: tier configuration and extra-prize selection.
- `resource-view.mjs`, `progress-view.mjs`: accessible graphic resource displays.
- `audio-controls.mjs`, `settings-ui.css`: full-width live volume sliders.
- `tableau-viewport.mjs`: uniformly fits the entire formation, including rotated bounds.
- `play-viewport.mjs`, `board-view.mjs`: stable geometry and local card animations.
- `play-actions.mjs`, `story-play.css`: compact card-first stage and icon controls.
- `motion.mjs`, `win-summary.mjs`: shared fast timing and saved reward presentation.
- `journey-guide.mjs`, `feature-tutorials.mjs`: contextual introductions and acknowledgement.

## Verification and screenshots

`audit_ui.py` reviews 26 screens at 1280, 390 and 320 pixels, detecting clipped
labels, horizontal overflow, font-loading failures and script errors. Screenshots
are saved under ignored `artifacts/ui-audit/`.

`test_mobile_refresh.py` checks five sizes, visible chest actions on laptops,
hovered tabs, six thresholds, one-time claims and automatic base rewards.
`test_stable_table.py` measures surviving-card and tableau bounds through moves,
draws, timers, undo, menu resume and resizing. Additional scripts verify tutorial,
volume, saved-hand navigation and interrupted-animation behavior.

`capture_readme_screenshots.py` replaces all existing documentation images with
captures of the current running game. Reference games and videos are excluded.

`test_story_play.py` checks five gameplay sizes, rotated-card containment, visible
Save/Code controls, hint icons after timer updates, waste suit changes and tutorial
dismissal without reflow. `tableau-viewport.test.mjs` fits 256 complete formations
at four sizes; the fit always uses all slots, including removed cards.

Home-screen chatter lives in `pip-home.mjs` and `pip-home.css`. Twelve thoughts
rotate every nine seconds with matching expressions; clicking Pip advances one.
Rotation pauses while reading with hover/focus, while hidden or in a dialog.
Leaving home clears the timer; the contextual map guide resumes on the map.


The reference expansion adds `story-layouts-v4.mjs` (128 families and long gardens),
`rank-shuffle.mjs` (rank refresh), `hand-progress.mjs` (actual objective counts and
play prompts), `play-surface.mjs` (scenery/table choice), `wheel-view.mjs` and
`reference-polish.css`. These run unchanged in the desktop renderer. Long gardens
reuse the complete-deal viewport so extra depth never resizes surviving cards.
`test_reference_expansion.py` exercises Shuffle's first-use introduction,
consumption, recharge, save recovery, surface changes and short laptop bounds.

The rest of the adventure now follows the scenic play/wheel presentation. Shared
teal-and-gold headers, illustrated badges, compact activity tiles and consistent
reward cards replace the older oversized coral panels. Activity tabs swipe on
narrow screens with enough inset for hover/focus effects. Helper descriptions can
be expanded individually; prices and inventory remain visible. Home uses original
scenery, illustrated activity icons and Pip's rotating thoughts.

`menu-scenes.mjs` decorates the existing menu DOM without owning game state or
routes. `adventure-restyle.css` is the final shared presentation layer; map road
geometry and solitaire card geometry remain in their own modules. Modal scenes
use only bundled original landscape artwork, available offline in both hosts.

Touch layout now uses a shared 44px minimum for menu navigation and primary
controls. The sticky top bar keeps Close and Back available while long panels
scroll; phone dialogs sit near the bottom edge with safe-area padding. Titles
precede swipeable activity tabs. Home becomes a compact launch panel with Pip
and stacked activity cards, while laptop layouts retain wider grids.
`phone-ui.css` owns these sizing/alignment rules; existing animation timings and
instant stock draws are unchanged.
