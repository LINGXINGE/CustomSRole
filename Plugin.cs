using CustomSRole.API.Features;
using Exiled.API.Features;
using System;

namespace CustomSRole
{
    public class Plugin : Plugin<Config>
    {
        public override string Prefix => "CustomSRole";
        public override string Name => "CustomSRole";
        public override string Author => "小鲨鱼";
        public override Version Version => new(1, 0, 0);
        public static Plugin Instance { get; set; }

        public override void OnEnabled()
        {
            Instance = this;
            CustomSRoleManager.RegisterAllRoles();
            Log.Info("[CustomSRole]成功加载");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            CustomSRoleManager.UnregisterAllRoles();
            Instance = null;
            base.OnDisabled();
        }
    }
}
