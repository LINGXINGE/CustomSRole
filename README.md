# CustomSRole
A plugin modeled after EXILED.CustomRole, with more functions than EXCustomRole For SCP：SL

## Features

-  **Custom Role System** - Create unique custom roles with custom attributes
-  **Auto Spawn System** - Chance-based random role assignment on player spawn
-  **Faction System** - Custom factions with ally/enemy relationships and friendly fire protection
-  **Respawn System** - Automatic respawn after death with configurable limits
-  **Spawn Location** - Flexible spawn position configuration (rooms or coordinates)
-  **Damage Modifiers** - Custom incoming and outgoing damage multipliers
-  **Inventory System** - Custom starting items and ammo
-  **Role Info Display** - Persistent role info hint and spawn message

## Installation

1. Ensure [EXILED](https://github.com/Exiled-Team/EXILED) is installed
2. Place `CustomSRole.dll` into `EXILED/Plugins` folder
3. Restart the server

## Quick Start

### Creating a Custom Role

```csharp
using CustomSRole.API.Features;
using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;

namespace MyPlugin.Roles
{
    public class TankRole : CustomSRole
    {
        public override uint Id => 1;
        public override string Name => "Tank";
        public override string CustomInfo => "High HP defensive role";
        public override RoleTypeId RoleType => RoleTypeId.NtfPrivate;
        
        // Basic attributes
        public override float Health { get; set; } = 200f;
        public override float HumeShield { get; set; } = 100f;
        
        // Damage modifiers
        public override float IncomingDamageMultiplier { get; set; } = 0.5f;
        
        // Spawn configuration
        public override float SpawnChance { get; set; } = 15f;
        public override int MinPlayers { get; set; } = 5;
        public override int MaxCount { get; set; } = 2;
        
        // Respawn configuration
        public override bool CanRespawn { get; set; } = true;
        public override int MaxRespawnCount { get; set; } = 1;
        
        protected override void OnRoleAdded(Player player)
        {
            player.ShowHint("You have become a Tank!", 5f);
        }
    }
}
```

## API Documentation

### Required Properties (Abstract)

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `uint` | Unique identifier for the role |
| `Name` | `string` | Display name of the role |
| `CustomInfo` | `string` | Custom info shown in player info |
| `RoleType` | `RoleTypeId` | Base role type to use |
| `CustomRoleType` | `int` | Custom role type identifier |
| `CustomFaction` | `int` | Custom faction identifier |
| `EnemyFactions` | `List<int>` | List of enemy faction IDs |
| `SpawnChance` | `float` | Spawn chance percentage |
| `SpawnPositions` | `List<Vector3>` | List of spawn positions |
| `Inventory` | `List<ItemType>` | Starting inventory items |

### Optional Properties (Virtual)

#### Basic Attributes

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Health` | `float` | 100f | Maximum health |
| `HumeShield` | `float` | 0f | Hume shield value |
| `Scale` | `Vector3` | Vector3.one | Player scale |

#### Spawn Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MinPlayers` | `int` | 0 | Minimum players required for spawn |
| `MaxCount` | `int` | -1 | Maximum simultaneous instances (-1 = unlimited) |
| `MaxSpawnsPerRound` | `int` | -1 | Maximum spawns per round (-1 = unlimited) |
| `IgnoreSpawnSystem` | `bool` | false | Disable auto-spawn (manual assignment only) |
| `SpawnRooms` | `List<RoomType>` | null | List of valid spawn room types |
| `KeepPositionOnSpawn` | `bool` | false | Keep current position on spawn |
| `KeepInventoryOnSpawn` | `bool` | false | Keep current inventory on spawn |

#### Damage Modifiers

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IncomingDamageMultiplier` | `float` | 1f | Damage received multiplier |
| `DamageMultiplier` | `float` | 1f | Damage dealt multiplier |

#### Respawn System

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `CanRespawn` | `bool` | false | Enable respawn on death |
| `MaxRespawnCount` | `int` | 0 | Maximum respawn count (-1 = unlimited) |
| `RespawnDelay` | `float` | 5f | Respawn delay in seconds |
| `RespawnMessage` | `string` | "你已重生！" | Message shown on respawn |
| `KeepRoleOnDeath` | `bool` | false | Keep role on death (if not respawning) |
| `KeepRoleOnChangingRole` | `bool` | false | Keep role when changing to another role |

#### Inventory & Ammo

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Ammo` | `Dictionary<AmmoType, ushort>` | empty | Starting ammo |

#### Display & Messages

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `CRoleInfo` | `string` | "" | Persistent role info (shown at bottom of screen) |
| `CRoleSpawnMessage` | `string` | "" | Spawn message (8 second delay, like vanilla roles) |

### Methods

#### Public Methods

| Method | Description |
|--------|-------------|
| `Register()` | Register the role |
| `Unregister()` | Unregister the role |
| `AddRole(Player)` | Assign role to a player |
| `RemoveRole(Player)` | Remove role from a player |
| `Check(Player)` | Check if player has this role |
| `IsEnemy(Player)` | Check if player is an enemy |
| `IsAlly(Player)` | Check if player is an ally |
| `CanPlayerRespawn(Player)` | Check if player can respawn |

#### Virtual Events

| Event | Description |
|-------|-------------|
| `OnRegistered()` | Called when role is registered |
| `OnUnregistered()` | Called when role is unregistered |
| `OnRoleAdded(Player)` | Called when player receives the role |
| `OnRoleRemoved(Player)` | Called when player loses the role |
| `OnRespawned(Player)` | Called when player respawns |

#### Game Events

| Event | Description |
|-------|-------------|
| `OnPlayerDied(DiedEventArgs)` | Called when player dies |
| `OnHurting(HurtingEventArgs)` | Called when player takes damage |
| `OnShooting(ShootingEventArgs)` | Called when player shoots |
| `OnInteractingDoor(InteractingDoorEventArgs)` | Called when player interacts with a door |
| `OnInteractingLocker(InteractingLockerEventArgs)` | Called when player interacts with a locker |
| `OnUsingItem(UsingItemEventArgs)` | Called when player uses an item |

## Complete Example

```csharp
using CustomSRole.API.Features;
using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugin.Roles
{
    public class AssaultRole : CustomSRole
    {
        // Required properties
        public override uint Id => 2;
        public override string Name => "Assault";
        public override string CustomInfo => "Aggressive assault unit";
        public override RoleTypeId RoleType => RoleTypeId.ChaosRifleman;
        public override int CustomRoleType { get; set; } = 2;
        public override int CustomFaction { get; set; } = 1;
        public override List<int> EnemyFactions { get; set; } = [2, 3];
        public override float SpawnChance { get; set; } = 10f;
        public override List<Vector3> SpawnPositions { get; set; } = [];
        public override List<ItemType> Inventory { get; set; } = [
            ItemType.KeycardChaosInsurgency,
            ItemType.GunAK,
            ItemType.Medkit,
            ItemType.Adrenaline,
            ItemType.Radio
        ];

        // Basic attributes
        public override float Health { get; set; } = 150f;
        public override float HumeShield { get; set; } = 50f;
        public override Vector3 Scale { get; set; } = new Vector3(1.1f, 1f, 1.1f);

        // Damage modifiers
        public override float IncomingDamageMultiplier { get; set; } = 0.8f;
        public override float DamageMultiplier { get; set; } = 1.25f;

        // Spawn configuration
        public override int MinPlayers { get; set; } = 5;
        public override int MaxCount { get; set; } = 3;
        public override int MaxSpawnsPerRound { get; set; } = 5;
        public override List<RoomType> SpawnRooms { get; set; } = [
            RoomType.HczEzCheckpoint,
            RoomType.LczCrossing
        ];

        // Respawn configuration
        public override bool CanRespawn { get; set; } = true;
        public override int MaxRespawnCount { get; set; } = 1;
        public override float RespawnDelay { get; set; } = 5f;
        public override string RespawnMessage { get; set; } = "You have respawned as Assault!";

        // Display
        public override string CRoleInfo { get; set; } = "Assault Unit\n+25% Damage | -20% Damage Taken";
        public override string CRoleSpawnMessage { get; set; } = "Assault\nYou are an elite assault unit!";

        // Ammo
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new()
        {
            { AmmoType.Nato762x39, 120 },
            { AmmoType.Nato9, 50 }
        };

        protected override void OnRoleAdded(Player player)
        {
            Log.Info($"[Assault] Player {player.Nickname} became an Assault");
            player.ShowHint("You are now an Assault!\n+25% Damage | -20% Damage Taken", 5f);
        }

        protected override void OnRoleRemoved(Player player)
        {
            Log.Info($"[Assault] Player {player.Nickname} is no longer an Assault");
        }

        protected override void OnRespawned(Player player)
        {
            player.ShowHint("You have respawned!", 3f);
        }

        protected override void OnHurting(Exiled.Events.EventArgs.Player.HurtingEventArgs ev)
        {
            if (!Check(ev.Player)) return;

            if (ev.Attacker != null && IsAlly(ev.Attacker))
            {
                ev.IsAllowed = false;
                ev.Attacker.ShowHint("You cannot hurt allies!", 2f);
            }
        }
    }
}
```

## Dependencies

- [EXILED](https://github.com/Exiled-Team/EXILED) >= 9.0.0

## License

[MIT](LICENSE)

## Contributing

Issues and Pull Requests are welcome!

## Support

If you encounter any issues, please submit them on GitHub Issues.
