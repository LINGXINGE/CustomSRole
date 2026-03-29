# CustomSRole
一个仿照 EXILED.CustomRole 的插件，比 EXCustomRole 拥有更多功能，适用于 SCP:SL

## 功能特性

- **自定义角色系统** - 创建具有自定义属性的独特角色
- **自动生成系统** - 基于几率的玩家生成时随机角色分配
- **阵营系统** - 自定义阵营和敌友关系，友军伤害保护
- **角色信息显示** - 持久性角色信息提示和生成消息
- **更多功能....**

## 安装

1. 确保已安装 [EXILED](https://github.com/Exiled-Team/EXILED)
2. 将 `CustomSRole.dll` 和 `HintServiceMeow-Exiled.dll` 放入 `EXILED/Plugins` 文件夹
3. 重启服务器

## 快速开始

### 创建自定义角色

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
        public override string Name => "坦克";
        public override string CustomInfo => "高生命值防御角色";
        public override RoleTypeId RoleType => RoleTypeId.NtfPrivate;
        
        // 基础属性
        public override float Health { get; set; } = 200f;
        public override float HumeShield { get; set; } = 100f;
        
        // 伤害修改
        public override float IncomingDamageMultiplier { get; set; } = 0.5f;
        
        // 生成配置
        public override float SpawnChance { get; set; } = 15f;
        public override int MinPlayers { get; set; } = 5;
        public override int MaxCount { get; set; } = 2;
        
        // 重生配置
        public override bool CanRespawn { get; set; } = true;
        public override int MaxRespawnCount { get; set; } = 1;
        
        protected override void OnRoleAdded(Player player)
        {
            player.ShowHint("你已成为坦克！", 5f);
        }
    }
}
```

## API 文档

### 必需属性

| 属性 | 类型 | 描述 |
|----------|------|-------------|
| `Id` | `uint` | 角色唯一标识符 |
| `Name` | `string` | 角色显示名称 |
| `CustomInfo` | `string` | 玩家信息中显示的自定义信息 |
| `RoleType` | `RoleTypeId` | 使用的基础角色类型 |
| `CustomRoleType` | `int` | 自定义角色类型标识符 |
| `CustomFaction` | `int` | 自定义阵营标识符 |
| `EnemyFactions` | `List<int>` | 敌对阵营ID列表 |
| `Health` | `float` | 最大生命值 |
| `HumeShield` | `float` | 休谟护盾值 |
| `Scale` | `Vector3` | 玩家大小 |
| `SpawnChance` | `float` | 生成几率（百分比） |
| `SpawnPositions` | `List<Vector3>` | 生成位置列表 |
| `Inventory` | `List<ItemType>` | 初始物品栏 |

### 可选属性

#### 生成配置

| 属性 | 类型 | 默认值 | 描述 |
|----------|------|---------|-------------|
| `MinPlayers` | `int` | 0 | 生成所需的最小玩家数 |
| `MaxCount` | `int` | -1 | 最大同时存在数量（-1 = 无限制） |
| `MaxSpawnsPerRound` | `int` | -1 | 每回合最大生成次数（-1 = 无限制） |
| `IgnoreSpawnSystem` | `bool` | false | 禁用自动生成（仅手动分配） |
| `SpawnRooms` | `List<RoomType>` | null | 有效生成房间类型列表 |
| `KeepPositionOnSpawn` | `bool` | false | 生成时保持当前位置 |
| `KeepInventoryOnSpawn` | `bool` | false | 生成时保持当前物品栏 |

#### 伤害修改

| 属性 | 类型 | 默认值 | 描述 |
|----------|------|---------|-------------|
| `IncomingDamageMultiplier` | `float` | 1f | 受到伤害倍率 |
| `DamageMultiplier` | `float` | 1f | 造成伤害倍率 |

#### 重生系统

| 属性 | 类型 | 默认值 | 描述 |
|----------|------|---------|-------------|
| `CanRespawn` | `bool` | false | 启用死亡后重生 |
| `MaxRespawnCount` | `int` | 0 | 最大重生次数（-1 = 无限） |
| `RespawnDelay` | `float` | 5f | 重生延迟（秒） |
| `RespawnMessage` | `string` | "你已重生！" | 重生时显示的消息 |
| `KeepRoleOnDeath` | `bool` | false | 死亡时保持角色（如果不重生） |
| `KeepRoleOnChangingRole` | `bool` | false | 更改角色时保持自定义角色 |

#### 物品栏和弹药

| 属性 | 类型 | 默认值 | 描述 |
|----------|------|---------|-------------|
| `Ammo` | `Dictionary<AmmoType, ushort>` | 空 | 初始弹药 |

#### 显示和消息

| 属性 | 类型 | 默认值 | 描述 |
|----------|------|---------|-------------|
| `CRoleInfo` | `string` | "" | 持久性角色信息（显示在屏幕下方） |
| `CRoleSpawnMessage` | `string` | "" | 生成消息（8秒延迟，类似原生角色） |

### 方法

#### 公共方法

| 方法 | 描述 |
|--------|-------------|
| `Register()` | 注册角色 |
| `Unregister()` | 注销角色 |
| `AddRole(Player)` | 为玩家分配角色 |
| `RemoveRole(Player)` | 移除玩家的角色 |
| `Check(Player)` | 检查玩家是否拥有此角色 |
| `IsEnemy(Player)` | 检查玩家是否为敌人 |
| `IsAlly(Player)` | 检查玩家是否为友军 |
| `CanPlayerRespawn(Player)` | 检查玩家是否可以重生 |


#### 游戏事件

| 事件 | 描述 |
|-------|-------------|
| `OnPlayerDied(DiedEventArgs)` | 玩家死亡时调用 |
| `OnHurting(HurtingEventArgs)` | 玩家受到伤害时调用 |
| `OnShooting(ShootingEventArgs)` | 玩家射击时调用 |
| `OnInteractingDoor(InteractingDoorEventArgs)` | 玩家交互门时调用 |
| `OnInteractingLocker(InteractingLockerEventArgs)` | 玩家交互储物柜时调用 |
| `OnUsingItem(UsingItemEventArgs)` | 玩家使用物品时调用 |

## 依赖

- [EXILED](https://github.com/Exiled-Team/EXILED) = 9.13.3
- [HSM](https://github.com/MeowServer/HintServiceMeow/releases/tag/V5.5.1) = 5.5.1

## 许可证

[MIT](LICENSE)

## 支持和错误报告

如果您遇到任何问题，请在 GitHub Issues 中提交。
或发送邮件至 1801665309@qq.com
