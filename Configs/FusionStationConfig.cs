using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace WeaponMerging.Configs
{
    public class FusionStationConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Disable Animation")]
        [Tooltip("Disables fusion animations when unlocking new weapons.")]
        [DefaultValue(false)]
        public bool DisableAnimation { get; set; }
    }
}
