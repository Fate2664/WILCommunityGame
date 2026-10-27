# Sprinklers

Equip the Sprinkler inventory item to place it using the existing building preview.
Only an unoccupied `SocketCenter` on a crop plot accepts it. Ground placement,
edge sockets, and centers already holding a sprinkler are invalid. Placement
consumes one sprinkler; cancelling consumes nothing.

A placed sprinkler waters its own plot and the eight surrounding cells, including
diagonals. Coverage follows the plot's rotation and uses the existing 2.7-unit plot
spacing. Plots on a different floor or beyond this 3×3 area are excluded.

Planting inside the area waters the seed immediately. Subsequent watering uses the
existing crop watering rules and the plot's clock updates, including during sleep.
Empty plots and dead crops are not watered. Sprinklers do not consume watering-can
water, fertilizer, or other inventory items while operating.

Sprinklers remain attached and keep their center occupied through watering, growth,
harvesting, and clearing a dead crop. Removing the supporting plot removes its
sprinkler. Removing a sprinkler restores normal watering deadlines after the crop's
remaining water runs out. Overlapping sprinklers can cover the same plot.

## Setup

- The Sprinkler inventory asset has `Requires Plot Socket` enabled.
- Its prefab has a `BuildPart` set to `BuildableObject`, a root collider, and the
  `Sprinkler` script. `Placement Point` references the existing bottom socket so
  the base aligns with the plot center.
- Every current crop-stage prefab, including empty and dead plots, has a
  `PlotSocket` on its `SocketCenter`, with a trigger collider on the `Socket` layer.
- For future crop prefabs, preserve this center socket so mounted sprinklers can
  transfer when the plot is replaced.
- If plot spacing changes, update the sprinkler prefab's `Plot Spacing` to match.

## Play Mode checks

1. Equip a sprinkler. Confirm only plot centers allow placement and cancellation
   leaves the inventory count unchanged.
2. Place it in the middle of a 3×3 group. Confirm all nine seeded plots are watered,
   while a plot in the next row remains thirsty.
3. Try placing another sprinkler on the same center; placement should be invalid.
4. Harvest, replant, and sleep through multiple days. Confirm covered crops continue
   growing and the sprinkler survives each plot replacement.
5. Move a placement preview over dry crops; it must not water them.
6. Remove a placed sprinkler using destroy mode. Confirm watering requests return
   after the remaining two-day watering period, unless another sprinkler covers them.
