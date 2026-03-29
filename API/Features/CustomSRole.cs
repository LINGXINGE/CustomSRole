using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomSRole.API.Features
{
    public abstract class CustomSRole
    {
        /// <summary>
        /// 自定义角色的ID
        /// </summary>
        public abstract uint Id { get; }

        /// <summary>
        /// 自定义角色的名称
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 自定义角色的Info
        /// </summary>
        public abstract string CustomInfo { get; }

        /// <summary>
        /// 自定义角色的基础角色
        /// </summary>
        public abstract RoleTypeId RoleType { get; }

        /// <summary>
        /// 自定义角色的自定义角色类型
        /// </summary>
        public abstract int CustomRoleType { get; set; }

        /// <summary>
        /// 自定义角色所属的自定义阵营
        /// </summary>
        public abstract int CustomFaction { get; set; }

        /// <summary>
        /// 自定义角色的敌对阵营列表
        /// </summary>
        public abstract List<int> EnemyFactions { get; set; }

        /// <summary>
        /// 自定义角色的生命值
        /// </summary>
        public abstract float Health { get; set; } 

        /// <summary>
        /// 自定义角色的休谟护盾值
        /// </summary>
        public abstract float HumeShield { get; set; } 

        /// <summary>
        /// 自定义角色的大小
        /// </summary>
        public abstract Vector3 Scale { get; set; } 

        /// <summary>
        /// 自定义角色生成时是否保持当前位置
        /// </summary>
        public virtual bool KeepPositionOnSpawn { get; set; } = false;

        /// <summary>
        /// 自定义角色生成时是否保持之前的物品栏
        /// </summary>
        public virtual bool KeepInventoryOnSpawn { get; set; } = false;

        /// <summary>
        /// 角色死亡时是否保持当前自定义角色
        /// </summary>
        public virtual bool KeepRoleOnDeath { get; set; } = false;

        /// <summary>
        /// 更改角色时是否保持自定义角色
        /// </summary>
        public virtual bool KeepRoleOnChangingRole { get; set; } = false;

        /// <summary>
        /// 自定义角色的初始物品栏
        /// </summary>
        public abstract List<ItemType> Inventory { get; set; } 

        /// <summary>
        /// 自定义角色的初始弹药
        /// </summary>
        public virtual Dictionary<AmmoType, ushort> Ammo { get; set; } = [];

        /// <summary>
        /// 自定义角色的持久性描述(在屏幕下方)
        /// </summary>
        public virtual string CRoleInfo { get; set; } = "";

        /// <summary>
        /// 自定义角色生成时显示的消息(仿照SL原生角色出场文本，有8秒延迟)
        /// </summary>
        public virtual string CRoleSpawnMessage { get; set; } = "";

        /// <summary>
        /// 自定义角色的生成几率
        /// </summary>
        public abstract float SpawnChance { get; set; }

        /// <summary>
        /// 是否禁止自动生成
        /// </summary>
        public virtual bool IgnoreSpawnSystem { get; set; } = false;

        /// <summary>
        /// 生成此自定义角色所需的服务器内最小玩家数量
        /// </summary>
        public virtual int MinPlayers { get; set; } = 0;

        /// <summary>
        /// 自定义角色可以生成的房间类型列表
        /// </summary>
        public virtual List<RoomType> SpawnRooms { get; set; }

        /// <summary>
        /// 自定义角色生成的V3列表
        /// </summary>
        public abstract List<Vector3> SpawnPositions { get; set; }

        /// <summary>
        /// 受到的伤害倍率
        /// </summary>
        public virtual float IncomingDamageMultiplier { get; set; } = 1f;

        /// <summary>
        /// 造成的伤害倍率
        /// </summary>
        public virtual float DamageMultiplier { get; set; } = 1f;

        /// <summary>
        /// 最大同时存在数量(-1 表示无限制)
        /// </summary>
        public virtual int MaxCount { get; set; } = -1;

        /// <summary>
        /// 每回合最大生成次数(-1 表示无限制)
        /// </summary>
        public virtual int MaxSpawnsPerRound { get; set; } = -1;

        /// <summary>
        /// 死亡后是否可以重生
        /// </summary>
        public virtual bool CanRespawn { get; set; } = false;

        /// <summary>
        /// 最大重生次数(-1 表示无限制)
        /// </summary>
        public virtual int MaxRespawnCount { get; set; } = 0;

        /// <summary>
        /// 重生延迟时间（秒）
        /// </summary>
        public virtual float RespawnDelay { get; set; } = 5f;
        /// <summary>
        /// 重生提示消息
        /// </summary>
        public virtual string RespawnMessage { get; set; } = $"你已重生！";

        // 玩家重生次数
        protected readonly Dictionary<int, int> PlayerRespawnCounts = [];

        public HashSet<int> TrackedPlayers { get; } = [];

        public bool IsRegistered => CustomSRoleManager.RegisteredRoles.ContainsKey(Id);

        public void Register()
        {
            if (IsRegistered)
            {
                Log.Warn($"[CustomSRole] 角色 {Name} (ID: {Id}) 已经注册过了");
                return;
            }

            CustomSRoleManager.RegisteredRoles[Id] = this;
            SubscribeRoleEvents();
            OnRegistered();
            Log.Info($"[CustomSRole] 角色 {Name} (ID: {Id}) 已注册");
        }

        public void Unregister()
        {
            if (!IsRegistered)
            {
                return;
            }

            foreach (var playerId in TrackedPlayers.ToList())
            {
                if (Player.TryGet(playerId, out var player))
                {
                    RemoveRole(player);
                }
            }

            UnsubscribeRoleEvents();
            CustomSRoleManager.RegisteredRoles.Remove(Id);
            OnUnregistered();
        }

        public void AddRole(Player player)
        {
            if (player == null)
            {
                Log.Debug($"[CustomSRole] 无法添加角色 {Name}：玩家无效");
                return;
            }

            if (!CustomSRoleManager.PlayerRolesDict.ContainsKey(player.Id))
                CustomSRoleManager.PlayerRolesDict[player.Id] = [];

            if (CustomSRoleManager.PlayerRolesDict[player.Id].Contains(this))
            {
                Log.Debug($"[CustomSRole] 玩家 {player.Nickname} 已经拥有角色 {Name}");
                return;
            }

            CustomSRoleManager.PlayerRolesDict[player.Id].Add(this);
            TrackedPlayers.Add(player.Id);

            if (!KeepInventoryOnSpawn)
                player.ClearInventory();

            player.Role.Set(RoleType, SpawnReason.Respawn, RoleSpawnFlags.All);

            if (!KeepPositionOnSpawn)
            {
                var spawnPos = GetSpawnPosition();
                if (spawnPos.HasValue)
                    player.Position = spawnPos.Value;
            }

            player.MaxHealth = Health;
            player.Health = Health;

            if (HumeShield > 0)
            {
                player.MaxHumeShield = HumeShield;
                player.HumeShield = HumeShield;
            }

            if (Scale != Vector3.one)
                player.Scale = Scale;

            if (!string.IsNullOrEmpty(CustomInfo))
                player.CustomInfo = CustomInfo;
            if (Inventory != null && Inventory.Count > 0)
            {
                foreach (var item in Inventory)
                    player.AddItem(item);
            }
            if (Ammo != null && Ammo.Count > 0)
            {
                foreach (var ammo in Ammo)
                    player.SetAmmo(ammo.Key, ammo.Value);
            }

            if (!string.IsNullOrEmpty(CRoleInfo))
                RoleInfoHint.AddRoleInfo(player, CRoleInfo);

            if (!string.IsNullOrEmpty(CRoleSpawnMessage))
                Timing.RunCoroutine(RoleInfoHint.ShowRoleSpawnMessage(player, "你是", CRoleSpawnMessage, ""));

            OnRoleAdded(player);
            Log.Debug($"[CustomSRole] 已为玩家 {player.Nickname} 添加角色 {Name}");
        }

        public void RemoveRole(Player player)
        {
            if (player == null) return;

            if (!CustomSRoleManager.PlayerRolesDict.TryGetValue(player.Id, out var roles) || !roles.Contains(this))
                return;

            roles.Remove(this);
            TrackedPlayers.Remove(player.Id);

            RoleInfoHint.RemoveRoleInfo(player);

            OnRoleRemoved(player);
            Log.Debug($"[CustomSRole] 已从玩家 {player.Nickname} 移除角色 {Name}");
        }

        public bool Check(Player player)
        {
            return TrackedPlayers.Contains(player?.Id ?? -1);
        }

        public bool IsEnemy(Player player)
        {
            if (player == null || !player.IsConnected)
                return false;

            if (EnemyFactions == null || EnemyFactions.Count == 0)
                return false;

            foreach (var role in CustomSRoleManager.GetPlayerRoles(player))
            {
                if (EnemyFactions.Contains(role.CustomFaction))
                    return true;
            }
            return false;
        }

        public bool IsAlly(Player player)
        {
            if (player == null || !player.IsConnected)
                return false;

            foreach (var role in CustomSRoleManager.GetPlayerRoles(player))
            {
                if (role.CustomFaction == CustomFaction)
                    return true;
            }
            return false;
        }

        public bool CanPlayerRespawn(Player player)
        {
            if (!CanRespawn) return false;

            if (MaxRespawnCount < 0) return true;

            if (!PlayerRespawnCounts.TryGetValue(player.Id, out var count))
                return true;

            return count < MaxRespawnCount;
        }

        public void ScheduleRespawn(Player player)
        {
            if (player == null || !CanRespawn) return;

            if (!PlayerRespawnCounts.ContainsKey(player.Id))
                PlayerRespawnCounts[player.Id] = 0;

            PlayerRespawnCounts[player.Id]++;

            Timing.CallDelayed(RespawnDelay, () =>
            {
                if (player == null || !player.IsConnected) return;

                if (TrackedPlayers.Contains(player.Id))
                {
                    player.Role.Set(RoleType, SpawnReason.Respawn, RoleSpawnFlags.All);

                    if (!KeepPositionOnSpawn)
                    {
                        var spawnPos = GetSpawnPosition();
                        if (spawnPos.HasValue)
                            player.Position = spawnPos.Value;
                    }

                    player.MaxHealth = Health;
                    player.Health = Health;

                    if (HumeShield > 0)
                    {
                        player.MaxHumeShield = HumeShield;
                        player.HumeShield = HumeShield;
                    }
                    if (!string.IsNullOrEmpty(RespawnMessage))
                    {
                        player.ShowHint(RespawnMessage, 3f);
                    }
                    OnRespawned(player);
                    Log.Debug($"[CustomSRole] 玩家 {player.Nickname} 重生为角色 {Name}");
                }
            });
        }

        protected virtual Vector3? GetSpawnPosition()
        {
            if (SpawnPositions != null && SpawnPositions.Count > 0)
            {
                var random = new System.Random();
                return SpawnPositions[random.Next(SpawnPositions.Count)];
            }

            if (SpawnRooms != null && SpawnRooms.Count > 0)
            {
                var random = new System.Random();
                var roomType = SpawnRooms[random.Next(SpawnRooms.Count)];
                var room = Room.List.FirstOrDefault(r => r.Type == roomType);
                if (room != null)
                    return room.Position + Vector3.up;
            }

            return RoleType.GetRandomSpawnLocation().Position + Vector3.up;
        }

        protected virtual void OnRegistered() { }
        protected virtual void OnUnregistered() { }
        protected virtual void OnRoleAdded(Player player) { }
        protected virtual void OnRoleRemoved(Player player) { }
        protected virtual void OnRespawned(Player player) { }

        protected virtual void SubscribeRoleEvents() { }
        protected virtual void UnsubscribeRoleEvents() { }

        internal virtual void OnPlayerDied(Exiled.Events.EventArgs.Player.DiedEventArgs ev) { }
        internal virtual void OnPlayerLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev) { }
        internal virtual void OnHurting(Exiled.Events.EventArgs.Player.HurtingEventArgs ev) { }
        internal virtual void OnShooting(Exiled.Events.EventArgs.Player.ShootingEventArgs ev) { }
        internal virtual void OnInteractingDoor(Exiled.Events.EventArgs.Player.InteractingDoorEventArgs ev) { }
        internal virtual void OnInteractingLocker(Exiled.Events.EventArgs.Player.InteractingLockerEventArgs ev) { }
        internal virtual void OnUsingItem(Exiled.Events.EventArgs.Player.UsingItemEventArgs ev) { }
    }
}
