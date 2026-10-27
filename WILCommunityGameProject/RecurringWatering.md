# Recurring crop watering

Each watering lasts two in-game days (2,880 clock minutes). Harvesting keeps the
existing sprout → seedling → harvestable cycle and does not reset the watering timer.

When water runs out, the plot shows its water icon. Sprouts can reach the seedling
stage, but seedlings stop growing until watered. Watering preserves their current
growth progress and starts another two-day watering period.

Already mature crops remain harvestable while thirsty. Interacting with a watering
can waters them first; other normal harvest interactions still harvest them.

After one full day (1,440 clock minutes) needing water, a crop dies and uses its
seed's dead prefab. This deadline also applies to newly planted seeds waiting for
their first watering. Sleeping advances these timers; fertilizer only accelerates
growth, not the watering schedule. Dead crops lose their fertilizer glow.

Interacting with a dead crop, with any item or empty hands, replaces it with
`Assets/Prefabs/Building Pieces/EmptyPlot.prefab`. The replacement has no seed,
growth, watering, or fertilizer state and can be planted again.

## Prefab setup

All eight existing seed assets reference their matching dead crop prefabs. Each
dead prefab has its Crop Behaviour stage set to `Dead` and its Land Manager's
`Empty Plot Prefab` assigned. When adding another crop, configure these same fields.

## Play Mode checks

1. Plant and water a crop. Harvest and allow it to regrow several times.
2. At two days since watering, confirm the water icon appears. Leave a regrowing
   crop thirsty and confirm it remains a seedling.
3. Water it before the next day expires. Confirm growth resumes and the next water
   request occurs two days after that watering.
4. Leave another crop thirsty for a full day, including by sleeping. Confirm its
   matching dead prefab appears and no harvest or water icon remains.
5. Interact with the dead plot while empty-handed and while holding fertilizer.
   Confirm it becomes an empty plot without consuming an item, then plant it again.
