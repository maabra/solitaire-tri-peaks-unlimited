# Shared interface 2.3

Windows and web load the same ui-theme.css and ui-shell.mjs. The theme owns the
menu color, spacing, button, panel and motion rules; game layout and original
art remain separate. No additional icon atlas is necessary: the existing 80
illustrations now appear consistently beside destinations, titles and explanations.

Every game menu includes Map, My cards, Activities and Shop navigation. Activity
groups show useful state such as available packs, ready tasks, arrows, bingo draws,
flight stages and earned designs. Related-place tabs and reward links connect
collections and the activities that supply them. Returning to My cards resumes
the existing hand. Buying a helper keeps the shop scroll position and confirms
the purchase.

Information buttons appear beside menu headings, shop helpers and gameplay
helpers. Explanations use a separate modal top layer so the underlying page,
selection and scroll position remain intact. Escape dismisses help first and
focus returns to its source. Gameplay timers and keyboard shortcuts pause while
help is open. The close control stays in the sticky navigation header.

Menus fade in; activity tiles reveal in sequence; buttons respond to hover and
press. Reduced motion disables these effects. Dialog transitions avoid moving
interactive controls. Scenery and tableau animations remain independent.

Browser verification covers six general menus at four window sizes, all thirteen
activity panels at three sizes, individual label containment, layered help,
saved-hand continuity, keyboard dismissal and motion preferences. Existing game,
garden, map, timers, persistence and chapter-art regressions are also run.


## Visual-first pass 2.3.1

visual-cues.mjs supplies compact, accessible destination controls and a pictorial
Play → Earn → Collect strip. Illustrated navigation, red title ribbons, gold
selection states and larger activity art establish the hierarchy. Short labels
and numeric badges replace long activity names and explanations.

Shop items show helper artwork, quantity and coin price. Their i buttons retain
the complete helper rules. Long activity instructions move into an accessible
Details disclosure, preserving rules on the same screen. Pip gives one brief
next step; its information button opens the full explanation, including special
card mechanics. Accessible names preserve the intent of compact buttons.


Current 2.4 collection counts supersede the historical counts above: 100 matched
chapter scenes, card backs and tables. See COLLECTIONS.md and SIDE-GAMES.md.
Cards rotate to a player-facing angle without changing cover geometry; scenery
blur is isolated from the sharp cards and UI.


## Map and Pip fixes 2.4.1

map-ui.css owns the bounded map overlay layout. The chapter header and counters
use the cream/plum palette, ordinary rounded corners and explicit readable text
colors. The scrolling mural is softly blurred separately from the road, markers,
buttons and guide. A blurred tinted ambient layer fills wide-window side margins.

The visible bird has a real 90-pixel click target. Pip always provides Play and
Cards, even before activities unlock. Topic pictures directly open unlocked
destinations. Locked topics show their required level without a dead Show me
button. Welcome guidance changes to Press Play after opening a level preview.

The green map Play button opens the current level preview; a separate compass
locates its stop. Extra map-camera scroll space and resize focusing keep that
stop between the header and the guide. The pointer-based map/Pip regression test
checks five window sizes, clipped text, blur isolation and actual early-level
navigation. Gameplay, reward rules and saved hands are unchanged.
