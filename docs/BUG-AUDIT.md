# Bug audit ? 2.4.25 (26 October 2026)

## Remedied

- Narrow dialogs used the fixed 1280-pixel game width for responsive rules. Activity labels, album categories, memory tiles and customization choices now adapt to the dialog itself.
- Volume sliders inherited a two-column layout inside an already narrow Settings column. Each slider now has a full row, with pointer, keyboard and endpoint checks.
- Back navigation restored only the outer dialog scroll. It now also restores book-page and shop-shelf positions after rendering the destination.
- Maximum valid imported balances could overflow during rewards and make subsequent saves invalid. Wallets, inventories, scores, streaks and activity counters now respect their validated limits; native C# economy and hand scoring follow the same bounds.
- Buying a helper with a full inventory could deduct coins before failing. The transaction now refuses before charging and the shop displays Bag full. Full pack storage also refuses token purchases without charging. Album advancement preserves the existing collection at its storage limit.

## Verification

- Native full suite: 25,229,361 assertions, including formations and witness solutions across all 480,000 campaign levels.
- Release desktop build: zero warnings and errors.
- All 15 browser engine test files passed, including new maximum-value save and transaction regressions, malformed-state recovery, stale-tab conflicts and reward persistence.
- Browser audit: 26 screens at 1280?800, 390?844 and 320?640; no detected clipped labels, dialog horizontal overflow or script errors.
- Real browser checks: Settings and journal Back scroll restoration; volume pointer/keyboard/0?100 endpoints and persistence; audio; unlocked tutorials; shop buying; book-page scrolling; compact notice transparency; repeated-mistake Pip guidance; full-size Pip and independently timed attention effects; header bounds and feedback; native Ace click coordinates; hearts, restart and leave costs; activity navigation and interrupted wheel; board stability through moves, stock draws, timers, undo, menu resume and resizing.

The game intentionally keeps a fixed 1280?720 composition with outer browser scrolling on smaller displays. This audit exercised Edge/Chromium and the native engine; it does not establish that every browser, device or possible imported save is defect-free. Screenshots and detailed local logs are generated under artifacts/.
