# Compost bin

GameScene is wired to the existing CompostBin, CompostBinUIRoot, ItemsRoot GridView,
three storage-slot instances, and the arrow's Slider UIBlock. It reuses the silo's
slot prefab, gesture handling conventions, indicator, and opening animation.

## Using the bin

- Approach the bin and press E to open or close it. Walking away, opening the
  inventory, or pressing Escape also closes the panel.
- ItemsRoot lists compost items and fertilizer tools in the player's inventory, combining stacks
  of the same asset in a five-column, two-row viewport. Empty slots fill out the
  ten-slot grid. Scroll with the mouse wheel if there are more than ten item types.
- Drag crop waste and natural waste into their matching slots. Each slot holds
  up to 99 items; anything that does not fit stays in the player inventory.
- Before both inputs are ready, drag an ingredient back into ItemsRoot to reclaim it.
- Once 10 crop waste and 1 natural waste are present, the batch starts automatically.
  Ingredients stay visible and cannot be removed during processing.
- After 1,440 game minutes, 10 crop waste and 1 natural waste are consumed and one
  fertilizer appears. Leftover ingredients stay in their slots.
  The arrow fills from 0% to 100%. Sleeping advances this same timer.
- Click the fertilizer, or drag it into ItemsRoot, to collect it. A full inventory
  leaves it in the bin. Collecting the output automatically starts the next batch
  if enough ingredients remain.
- Right-click or Escape cancels a drag without transferring anything.

## Configuration and quick check

The CompostBin component exposes the fertilizer asset, ingredient quantities, and
duration in game days. New compost bins need the same interaction collider and
IndicatorManager arrangement as the silo. Each bin holds its own batch and timer;
closing the UI does not stop processing.

The CompostBin Animator reference is assigned in GameScene, with a child Animator
lookup as a fallback. Its CompostBin controller uses a bool named `IsComposting`:

- `Armature_Open` is the default state; the bin is open while idle or awaiting collection.
- Open -> Close requires `IsComposting == true`.
- Close -> Open requires `IsComposting == false`.
- Both transitions have Has Exit Time off and a 0.1-second transition duration.
- Both clips have Loop Time off, so the lid holds its final pose.

The bin updates this parameter when production starts or completes, independently
of whether the UI is open. For another bin, assign its model's Animator and this
controller; the animation clips must match that model's bone hierarchy.

On CompostBinUI, assign the existing DragPreview UIBlock2D to Drag Icon. Drag Count
is optional if the preview has a TextBlock for the quantity. No preview is created
at runtime. Connect the CloseButton's click event to CompostBinUI.closeUI().

The waste item assets are in `Assets/Scriptable Objects/Inventory/Compost`.
Fertilizer is a ToolItemSO in `Assets/Scriptable Objects/Inventory/Tools`.
Their icons reuse cabbage, potato, and seed art as placeholders; replace the Icon
fields when compost artwork is available.

## Applying fertilizer

Equip the Fertilizer tool and interact with a planted crop. One item is consumed
and that plot gains 2x growth speed for five game days (7,200 game minutes).
The duration, multiplier, and FertilizerGlow prefab are configured on the tool asset.
Empty plots and already-fertilized crops do not consume fertilizer. Fertilizer
interaction takes priority over harvesting; unequip it or select another item to
harvest a mature crop.

The timer advances on every game minute, including sleep, dry soil, and time spent
fully grown. Water is still required to start growth. Harvesting and switching
between watered, sprout, seedling, and mature prefabs preserve the remaining boost.
The same glow instance moves to each replacement plot and is removed when the
timer expires. Destroying the plot also destroys its child glow.

For a Play Mode check, add CropWaste (20) and NaturalWaste (2) to the existing
InitializeInventoryItems component's items list. Open the bin, deposit a batch,
close/reopen it, and advance a day using the normal clock or sleep. Check collection
with available inventory space and with a full inventory. The starting inventory
has not been changed automatically.

Harvest waste drops, marketplace purchases, and save/load persistence are outside
this implementation. Future acquisition code
can use UIManager.TryAddItem with these assets.
