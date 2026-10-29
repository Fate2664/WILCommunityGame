# Plot placement areas

The hoe can only create empty plots inside the boxes assigned to `BuildPlacer > Plot Placement Areas`. An empty list blocks all new hoe placement.

## Editor setup

1. Create an empty GameObject for each planting area and add a **Box Collider**.
2. Enable **Is Trigger** so the box does not block the player or the ground placement raycast. Keep the GameObject and collider enabled.
3. Use **Edit Collider** to set the area bounds. Make the box tall enough to extend above and below the ground surface; only the plot footprint at its placement height needs to fit inside it.
4. Select the scene GameObject with **Build Placer** and drag each box into its **Plot Placement Areas** list. No special layer or Rigidbody is required.

Placement checks all four corners of a square footprint defined by **Grid Size**, after grid or socket snapping. The entire footprint must fit inside a single assigned box, including for overlapping areas. Rotated and scaled area objects are supported. Disabling an area's collider or GameObject prevents new plots there.

Invalid locations use the existing invalid preview material and cannot be placed. Fences, doors, inventory buildables, existing crops, and plot removal retain their existing behavior.

## Play Mode checks

- With no areas assigned, confirm the hoe cannot place plots.
- Inside a sufficiently large area, confirm an unoccupied position shows the valid preview and places a plot.
- At an edge or outside all areas, confirm the invalid preview appears and clicking creates nothing.
- Snap to a plot socket that would extend beyond an area and confirm placement is rejected.
- Rotate or scale an area and confirm its bounds still apply; disable it and confirm placement is rejected there.
- Confirm occupied plots still reject placement and fences and inventory buildables still work as before.
