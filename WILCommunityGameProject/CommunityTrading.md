# Community house trading

Interact with a community house to open `CommunityHouseRoot`. The panel shows that house's hunger status and delivered/requested crop counts. **Feed House** delivers whatever requested crops the player currently holds; partial deliveries are supported.

A fully fed house unlocks its daily offer. **Give** is the player's payment and **Receive** is the reward. A trade requires all payment items and enough inventory space after payment. Failed trades leave inventory unchanged. Each house can complete one trade per game day; closing the panel does not change its offer or reset this limit. A new day resets both feeding requests and the trade, including when sleeping advances the clock.

The close button, Escape, interacting with the same house again, opening inventory, or leaving the house closes the panel. The existing tutorial feeding unlocks still apply.

## Editing offers

Three starter offers are in `Assets/Scriptable Objects/Community`: five potatoes for three carrot seeds, five carrots for three potato seeds, and five corn for three tomato seeds.

Create more through **Assets > Create > Community > Trade**. Set the `Give` and `Receive` item arrays and positive amounts. Items can be any existing inventory item ScriptableObject. Set `Quality` to `Normal` or `Improved`, and add the offer to the house's `Available Trades`. Existing offers default to `Normal`. Each house chooses one valid offer of the unlocked quality when its daily requests are generated. The shared `CommunityHouse` prefab supplies the starter offers; override this list on individual houses for different rewards.

## Community food streak

Fully feeding every feeding-enabled house increases `StreakNumber` immediately, once per game day. Tutorial-locked houses join the requirement when feeding is enabled. A missed day resets the count at the following midnight; sleeping uses the same day rollover. Earned stages stay unlocked for the current game session, even when the count resets. Like the existing currency and upgrades, streak progress is not saved between sessions.

- **5 days:** awards R1000 once.
- **10 days:** unlocks `Improved` trades. Untraded houses refresh their offers immediately; already traded houses keep their daily limit. Future daily offers use the unlocked quality. A house falls back to `Normal` if it has no valid improved offers, so add improved assets to `Available Trades` to supply better rewards.
- **15 days:** updates the third stage and notification. Map unlocking is reserved for a later implementation.

`CommunityUIRoot` holds the stage backgrounds, outgoing connectors, icon textures, descriptions, and locked/unlocked colors. `StreakUpgradeNotificationVisuals` uses the same DOTween slide pattern as `InformationTipVisuals`; its duration and on-screen Y position are configurable on `StreakUpgradeNotification`.

All nine houses in `GameScene` are connected to the panel. For new house instances, assign `Community House UI` to `CommunityHouseRoot`, along with the house's existing inventory and community references. On the panel, `Crop Item Prefab` uses `CommunityCropItem` and `Trade Item Prefab` uses `TradeItemRoot`. The crop, Give, and Receive containers may be empty; the controller creates entries from these prefabs and reuses them. Existing scene entries retain their layout overrides.

## Play Mode checks

1. Open two different houses and confirm each displays its own requests and status.
2. Feed some requested crops, then complete the remaining requests. Confirm counts, status, community happiness, and the feeding objective update.
3. Check that trading is locked before full feeding, with missing payment items, and when rewards cannot fit.
4. Complete an offer and check both inventory changes. Reopen the same house: it must still say `Traded Today`. Another fully fed house must remain independently tradable.
5. Fill inventory and trade away a whole stack to verify its freed slot can hold the reward.
6. Advance to the next game day, including while the panel is open. Confirm new requests and an available daily offer that requires feeding again.
7. Fully feed every unlocked house. Confirm the streak increases exactly once, including after reopening or feeding a completed house. Partially fed communities must not count.
8. Reach five consecutive days. Confirm R1000 is awarded once, the first box and connector change color, and the coin notification slides down and back up.
9. Miss a day, then sleep. Confirm the count resets but the earned stage stays unlocked; reaching five again must not award another R1000.
10. Add a valid `Improved` offer to the shared house prefab, then reach ten consecutive days. Confirm the second stage and notification update, untraded houses show improved offers, and traded houses cannot trade again until tomorrow. A house without improved offers must retain normal offers.
11. Reach fifteen days. Confirm the third box and icon notification update without unlocking a map area yet.
