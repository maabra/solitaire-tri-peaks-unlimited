# Offline adventures (2.1)

Both Windows and web use these systems from the same modules. No event needs a
server. Rivals explicitly display CPU labels. All rewards are earned in the game.

## Chapters and presentation

The 480,000 campaign levels form **4,800 chapters of 100 levels**. Every chapter
has all 100 stops on one continuous scrolling trail, with automatic positioning
at the current stop. HUD controls remain attached to the viewport. Travel Hub
lists earlier and future chapters; future destinations can be previewed, while
their levels remain locked.

Every chapter has a distinct two-part name and seeded image composition. Fourteen
original full-size paintings provide the scenery; crop, framing, light and seeded
foreground details produce the individual compositions. These are generated on
demand, rather than shipping 4,800 separate full-size paintings. Travel Hub and
postcards use the exact same generated chapter image. The cache is bounded.
Sixteen original playing-table surfaces and subtle chapter colors replace busy
landscapes behind the cards. Original extra artwork includes 100 sticker tiles,
32 event icons and a 16-scene travel overview atlas.

## New hand rules

New hands use MP21; older MP16/MP18/MP19 hands and codes retain their rules.
Each MP21 deal constructs and replays a winning witness with the actual mutations,
including extra ice hits, live rank movement and tornado replacements.

- Ice needs a rank match to thaw, then another to collect. Temporary Thaw bypasses
  the first hit. Unrelated card clears no longer thaw new-hand ice.
- TNT removes the cards exposed when it is used. It does not recursively remove
  newly exposed layers. Joker moves one chosen exposed card to the waste,
  bypassing its obstacle. Both are consumable shop helpers.
- Up/down cards change rank by one on each successful action. Ranks wrap 1–13.
  Draws and helper use count as actions. A rejected action changes nothing.
- A tornado card replaces the exposed ranks and suits when collected. Live faces,
  planned tornado data, thaw states and exposure counters survive saving.
- Fog alternates visible and obscured action windows. The planned parity varies
  per card. Its face is hidden while obscured; another action changes the window.
- Every five consecutive collected cards adds two regular stock cards. Drawing
  resets the streak. Regular stock draw cooldowns remain in force.
- Pack cards appear from level 15, trophies from 25, TNT from 75, rank motion from
  400, tornadoes from 1,100 and fog from 1,400. Existing mechanics keep their
  gradual introduction. Discovery still hides unavailable helpers.
- Rare suit goals appear every 40/60 levels and gold goals every 75. Marked goal
  cards stay identifiable even if a tornado changes their current suit.

Pack, trophy and arrow rewards are pending until the hand is won. Undo restores
pending rewards; a paid victory cannot pay again. Restarting an unfinished hand
discards its pending haul.

## Collections and activities

- **Album:** ten categories of ten stickers, five rarity tiers. A pack opens three
  stickers, one guaranteed at the pack's tier, with the others at or below it.
  A new sticker gives 5 × rarity coins. Duplicates give 5 × rarity tokens. Token
  packs cost 30/60/100/160/240. Category and album rewards are claimable once.
  Completing the album unlocks the next album collection cycle.
- **Daily missions:** a rolling 24-hour period, three visible tasks and six total.
  Claiming a task reveals the next. Claims are stored before their result appears.
- **Lifetime milestones:** scalable tiers for clears, draws, spent coins, wins,
  stars, arrows and packs. Known historical win/star totals seed existing saves;
  previously unrecorded actions start counting with this update.
- **Monthly voyage:** an offline calendar-month reward track. The first port takes
  5 wins, then 6 additional wins, then 7, etc. Rewards never require purchases.
- **Tournament:** exposed trophy cards offer 20/30/40/50 points and lose their
  trophy after five actions. Winning banks the haul. Weekly threshold rewards
  and explicitly simulated rivals provide progression.
- **Regatta:** successive new campaign wins sail 1/5/10/20/50 miles, capped at 50
  per win. Replaying completed levels grants no distance. Abandoning/restarting
  an unfinished hand resets the streak. Four CPU crews show local competition;
  own-distance thresholds of 25/75/150/300/600 award prizes once per weekly race.
- **Balloon Trail:** one entry per week, three consecutive campaign wins. Active
  trail hands contain arrow cards. Winning banks their arrows for Souvenir Hunt.
  Three stages award 150 coins, a three-star pack and three more arrows.
- **Souvenir Hunt:** one arrow reveals a tile in a 9×9 island. Tiles hold coins,
  helpers, packs or bingo draws. Every ten shots gives two arrows and 50 coins;
  all 81 tiles completes the island and awards 300 coins and a five-star pack.
- **Bingo:** one token per win. Each draw marks a random unmarked number on a 5×5
  board. Rows, columns and diagonals pay once. Filling the board begins a new
  round with increased line rewards.
- **Weekly ticket trail:** ten consecutive wins, then 400 coins, TNT and a
  four-star pack. Restarting an unfinished hand resets its stage.
- **Chapter chest:** opens once per chapter after ten stars. Waiting improves its
  tier, up to the 300-star chapter maximum. Previous chapter chests remain reachable
  through Travel Hub. Old 20-level chapter gifts remain recorded independently;
  new chapter gifts require all 100 levels.
- **Postcards:** six stars unlock each chapter's postcard and 30 coins.
- **Lucky wheel:** keeps earned spins and adds one independent 24-hour spin.
  The shown segment determines the prize, which is saved before animation.

## Storage and edge cases

The browser asks for local-saving consent before loading the adventure. A small
cookie and storage flag remember permission; the full save uses localStorage
because it exceeds cookie limits. The consent dialog cannot be dismissed to bypass
permission. No tracking or network save is added. Windows continues using its JSON
profile and does not display a browser consent prompt.

Native saves preserve the complete shared schema instead of passing new fields
through the legacy C# hand model. Atomic replacement and a backup protect saves.
Validation rejects malformed live faces and collection counters. Corrupt hand/event
data can be reset while preserving a valid wallet and map. Stale-tab protection,
one-hour Fresh Garden cooldown, reduced motion, stable tableau nodes and older deal
codes remain supported.

Tests cover repeated reward clicks, all rarity tiers, duplicate conversion,
24-hour/month boundaries, all 81 hunt tiles, all bingo lines, regatta streaks,
balloon banking, live mutation persistence, undo, consent, all activity panels at
320/390/1280 pixels, and map alignment at six viewport sizes.

Full campaign validation: all 480,000 MP21 campaign seeds passed mutation-aware
winning-witness replay. Another 3,000 alternate-seed/balloon deals passed.
Completed album volumes remain available in the album gallery.

2.1.1: TNT, Joker and Remove collect key and gift effects, preventing cleared keys
from leaving lower locks impossible. Clear counters include helper removals.
