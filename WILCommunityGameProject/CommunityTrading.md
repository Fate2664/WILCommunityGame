# Community house trading

Interact with a community house to open `CommunityHouseRoot`. The panel shows that house's hunger status and delivered/requested crop counts. **Feed House** delivers whatever requested crops the player currently holds; partial deliveries are supported.

A fully fed house unlocks its daily offer. **Give** is the player's payment and **Receive** is the reward. A trade requires all payment items and enough inventory space after payment. Failed trades leave inventory unchanged. Each house can complete one trade per game day; closing the panel does not change its offer or reset this limit. A new day resets both feeding requests and the trade, including when sleeping advances the clock.

The close button, Escape, interacting with the same house again, opening inventory, or leaving the house closes the panel. The existing tutorial feeding unlocks still apply.

## Editing offers

Three starter offers are in `Assets/Scriptable Objects/Community`: five potatoes for three carrot seeds, five carrots for three potato seeds, and five corn for three tomato seeds.

Create more through **Assets > Create > Community > Trade**. Set the `Give` and `Receive` item arrays and positive amounts. Items can be any existing inventory item ScriptableObject. Each house chooses one valid entry from `Available Trades` when its daily requests are generated. The shared `CommunityHouse` prefab supplies the starter offers; override this list on individual houses for different rewards.

All nine houses in `GameScene` are connected to the panel. For new house instances, assign `Community House UI` to `CommunityHouseRoot`, along with the house's existing inventory and community references. On the panel, `Crop Item Prefab` uses `CommunityCropItem` and `Trade Item Prefab` uses `TradeItemRoot`. The crop, Give, and Receive containers may be empty; the controller creates entries from these prefabs and reuses them. Existing scene entries retain their layout overrides.

## Play Mode checks

1. Open two different houses and confirm each displays its own requests and status.
2. Feed some requested crops, then complete the remaining requests. Confirm counts, status, community happiness, and the feeding objective update.
3. Check that trading is locked before full feeding, with missing payment items, and when rewards cannot fit.
4. Complete an offer and check both inventory changes. Reopen the same house: it must still say `Traded Today`. Another fully fed house must remain independently tradable.
5. Fill inventory and trade away a whole stack to verify its freed slot can hold the reward.
6. Advance to the next game day, including while the panel is open. Confirm new requests and an available daily offer that requires feeding again.
