# RAXY Loot System

RAXY Loot System provides loot tables, drop processing, pooled world drops, and direct inventory grants for Unity projects.

## Features

- **LootTable / LootRandomizer** — weighted item rolls into `ItemAmountContainer`
- **ILootDropper** — contract for enemies, chests, and interactables to drop loot
- **LootDropManager** — spawns pooled `DroppedLoot`, or adds loot directly to player inventory
- **DroppedLoot** — physics throw, magnet chase, auto-collect, and inventory grant
- **LevelLootTableSO** — level-range loot table lookup

## Setup

1. Add `LootDropManager` to a scene bootstrap object.
2. Register `DroppedLoot` prefabs in `droppedLootPrefabs` (include id `default`).
3. Implement `ILootDropper` on drop sources, or use `LootDropper` with serialized input data.
4. Call `LootDropManager.SetAttractTarget(playerTransform)` so drops chase the player.

## Dependencies

- **RAXY Inventory** (`com.raxy.inventory`) — `ItemAmountContainer`, `InventoryManagerBase`
- **RAXY Utility** (`com.raxy.utility`) — `Singleton`, `CustomDebug`
- **Odin Inspector** (project plugin) — editor attributes; runtime works without Odin if attributes are stripped

## Notes

Game-specific obtainables, chests, and enemy loot wiring should live in your project, not in this package.
