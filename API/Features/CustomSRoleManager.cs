using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CustomSRole.API.Features
{
    public static class CustomSRoleManager
    {
        internal static readonly Dictionary<uint, CustomSRole> RegisteredRoles = [];
        internal static readonly Dictionary<int, HashSet<CustomSRole>> PlayerRolesDict = [];

        private static bool _eventsSubscribed = false;
        private static readonly System.Random Random = new();

        public static IReadOnlyDictionary<uint, CustomSRole> AllRegisteredRoles => RegisteredRoles;
        public static IReadOnlyDictionary<int, HashSet<CustomSRole>> AllPlayerRoles => PlayerRolesDict;

        public static int SpawnedPlayersCount { get; private set; } = 0;

        public static void ResetSpawnedCount() => SpawnedPlayersCount = 0;

        public static void RegisterAllRoles()
        {
            SubscribeGlobalEvents();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsAbstract)
                    continue;

                if (type == typeof(CustomSRole))
                    continue;

                if (type.IsNested)
                    continue;

                if (!typeof(CustomSRole).IsAssignableFrom(type))
                    continue;

                if (type.BaseType != typeof(CustomSRole))
                    continue;

                if (type.ContainsGenericParameters)
                    continue;

                if (type.GetCustomAttributes<CompilerGeneratedAttribute>().Any())
                    continue;

                var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (constructor == null)
                    continue;

                try
                {
                    if (Activator.CreateInstance(type) is CustomSRole role)
                        role.Register();
                    else
                        Log.Warn($"[CustomSRole] 无法将 {type.Name} 转换为 CustomSRole");
                }
                catch (Exception ex)
                {
                    Log.Error($"[CustomSRole] 无法创建角色实例 {type.Name}: {ex}");
                }
            }

            Log.Info($"[CustomSRole] 已自动注册 {RegisteredRoles.Count} 个角色");
        }

        public static void UnregisterAllRoles()
        {
            foreach (var role in RegisteredRoles.Values.ToList())
            {
                role.Unregister();
            }

            UnsubscribeGlobalEvents();
            RegisteredRoles.Clear();
            Log.Info($"[CustomSRole] 已注销所有角色");
        }

        public static CustomSRole Get(uint id) => RegisteredRoles.TryGetValue(id, out var role) ? role : null;
        public static CustomSRole Get(string name) => RegisteredRoles.Values.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        public static CustomSRole Get<T>() where T : CustomSRole => RegisteredRoles.Values.FirstOrDefault(r => r is T);
        public static CustomSRole Get(int customRoleType) => RegisteredRoles.Values.FirstOrDefault(r => r.CustomRoleType == customRoleType);

        public static bool TryGet(uint id, out CustomSRole role) => RegisteredRoles.TryGetValue(id, out role);
        public static bool TryGet(string name, out CustomSRole role)
        {
            role = Get(name);
            return role != null;
        }
        public static bool TryGet(int customRoleType, out CustomSRole role)
        {
            role = Get(customRoleType);
            return role != null;
        }

        public static IEnumerable<CustomSRole> GetPlayerRoles(Player player)
        {
            if (player == null || !PlayerRolesDict.TryGetValue(player.Id, out var roles))
                return [];
            return roles;
        }

        public static bool HasRole<T>(Player player) where T : CustomSRole
        {
            return GetPlayerRoles(player).Any(r => r is T);
        }

        public static bool HasRole(Player player, uint roleId)
        {
            return GetPlayerRoles(player).Any(r => r.Id == roleId);
        }

        public static bool HasRole(Player player, int customRoleType)
        {
            return GetPlayerRoles(player).Any(r => r.CustomRoleType == customRoleType);
        }

        public static bool HasAnyRole(Player player)
        {
            return PlayerRolesDict.ContainsKey(player?.Id ?? -1);
        }

        public static bool IsCustomEnemyButSameTeam(Player attacker, Player target)
        {
            if (attacker == null || target == null) return false;
            if (!attacker.IsConnected || !target.IsConnected) return false;

            if (attacker.Role.Team != target.Role.Team) return false;

            var attackerRoles = GetPlayerRoles(attacker).ToList();
            if (attackerRoles.Count == 0) return false;

            foreach (var attackerRole in attackerRoles)
            {
                if (attackerRole.IsEnemy(target))
                    return true;
            }

            return false;
        }

        public static bool ShouldSpawn(CustomSRole role)
        {
            if (role.IgnoreSpawnSystem)
                return false;

            if (role.SpawnChance <= 0f)
                return false;

            if (Player.Count < role.MinPlayers)
                return false;

            if (role.MaxCount > 0 && role.TrackedPlayers.Count >= role.MaxCount)
                return false;

            if (role.MaxSpawnsPerRound > 0)
            {
                var spawnedCount = SpawnedPlayersCount;
                if (spawnedCount >= role.MaxSpawnsPerRound)
                    return false;
            }

            return Random.NextDouble() * 100 < role.SpawnChance;
        }

        public static CustomSRole GetRandomSpawnableRole()
        {
            var spawnableRoles = RegisteredRoles.Values
                .Where(r => !r.IgnoreSpawnSystem && r.SpawnChance > 0f && Player.Count >= r.MinPlayers)
                .ToList();

            if (spawnableRoles.Count == 0)
                return null;

            var totalChance = spawnableRoles.Sum(r => r.SpawnChance);
            var randomValue = Random.NextDouble() * totalChance;

            float cumulative = 0f;
            foreach (var role in spawnableRoles)
            {
                cumulative += role.SpawnChance;
                if (randomValue < cumulative)
                    return role;
            }

            return spawnableRoles[Random.Next(spawnableRoles.Count)];
        }

        public static void TrySpawnRole(Player player)
        {
            if (player == null || !player.IsAlive)
                return;

            if (HasAnyRole(player))
                return;

            var role = GetRandomSpawnableRole();
            if (role != null && ShouldSpawn(role))
            {
                role.AddRole(player);
                SpawnedPlayersCount++;
            }
        }

        public static void SubscribeGlobalEvents()
        {
            if (_eventsSubscribed) return;
            _eventsSubscribed = true;

            Exiled.Events.Handlers.Server.RoundEnded += CustomSRoleEvents.OnRoundEnded;
            Exiled.Events.Handlers.Player.Died += CustomSRoleEvents.OnPlayerDiedGlobal;
            Exiled.Events.Handlers.Player.Left += CustomSRoleEvents.OnPlayerLeftGlobal;
            Exiled.Events.Handlers.Player.Hurting += CustomSRoleEvents.OnHurtingGlobal;
            Exiled.Events.Handlers.Player.Shooting += CustomSRoleEvents.OnShootingGlobal;
            Exiled.Events.Handlers.Player.InteractingDoor += CustomSRoleEvents.OnInteractingDoorGlobal;
            Exiled.Events.Handlers.Player.InteractingLocker += CustomSRoleEvents.OnInteractingLockerGlobal;
            Exiled.Events.Handlers.Player.UsingItem += CustomSRoleEvents.OnUsingItemGlobal;
            Exiled.Events.Handlers.Player.Spawned += CustomSRoleEvents.OnSpawnedGlobal;

            Log.Info("[CustomSRole] 全局事件已订阅");
        }

        public static void UnsubscribeGlobalEvents()
        {
            if (!_eventsSubscribed) return;
            _eventsSubscribed = false;

            Exiled.Events.Handlers.Server.RoundEnded -= CustomSRoleEvents.OnRoundEnded;
            Exiled.Events.Handlers.Player.Died -= CustomSRoleEvents.OnPlayerDiedGlobal;
            Exiled.Events.Handlers.Player.Left -= CustomSRoleEvents.OnPlayerLeftGlobal;
            Exiled.Events.Handlers.Player.Hurting -= CustomSRoleEvents.OnHurtingGlobal;
            Exiled.Events.Handlers.Player.Shooting -= CustomSRoleEvents.OnShootingGlobal;
            Exiled.Events.Handlers.Player.InteractingDoor -= CustomSRoleEvents.OnInteractingDoorGlobal;
            Exiled.Events.Handlers.Player.InteractingLocker -= CustomSRoleEvents.OnInteractingLockerGlobal;
            Exiled.Events.Handlers.Player.UsingItem -= CustomSRoleEvents.OnUsingItemGlobal;
            Exiled.Events.Handlers.Player.Spawned -= CustomSRoleEvents.OnSpawnedGlobal;

            Log.Info("[CustomSRole] 全局事件已取消订阅");
        }
    }
}
