using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace WeaponMerging.Content.Players
{
    public class AccessoryEffectsPlayer : ModPlayer
    {
        public bool orbBandEquipped;
        public bool focusEquipped;
        public bool catalystEquipped;
        public bool persistEquipped;
        public bool orbMasterBandEquipped;
        public bool focusedPersistenceEquipped;
        public bool comboCatalystEquipped;
        public bool amplifierEquipped;
        public bool infusionCoreEquipped;
        public Dictionary<string, float> orbSpeedMultipliers = new Dictionary<string, float>();
        public int ComboReduction;

        public int BonusMaxOrbs => (orbBandEquipped ? 1 : 0) + (catalystEquipped ? 1 : 0) + (orbMasterBandEquipped ? 2 : 0) + (infusionCoreEquipped ? 1 : 0);
        public int BonusShotsPerOrb => (focusEquipped ? 2 : 0) + (focusedPersistenceEquipped ? 2 : 0);
        public int IntervalReduction => (focusEquipped ? 2 : 0) + (catalystEquipped ? 1 : 0) + (focusedPersistenceEquipped ? 2 : 0) + (orbMasterBandEquipped ? 1 : 0) + (comboCatalystEquipped ? 1 : 0);
        public float GetOrbRotationSpeedMultiplier(int orbitingOrbCount) => amplifierEquipped ? 1f + orbitingOrbCount * 0.10f : 1f;

        public override void ResetEffects()
        {
            orbBandEquipped = false;
            focusEquipped = false;
            catalystEquipped = false;
            persistEquipped = false;
            orbMasterBandEquipped = false;
            focusedPersistenceEquipped = false;
            comboCatalystEquipped = false;
            amplifierEquipped = false;
            infusionCoreEquipped = false;
            ComboReduction = 0;
            if (orbSpeedMultipliers.Count == 0)
            {
                orbSpeedMultipliers["Inferno"] = 1f;
                orbSpeedMultipliers["Shadow"] = 1f;
                orbSpeedMultipliers["Crystal"] = 1f;
                orbSpeedMultipliers["Starlit"] = 1f;
            }
        }

        public override void PostUpdateEquips()
        {
            var orbMana = Player.GetModPlayer<OrbManaPlayer>();

            if (orbBandEquipped)
                orbMana.AddBonusMaxUnits(1);

            if (catalystEquipped)
                orbMana.AddBonusMaxUnits(1);

            if (orbMasterBandEquipped)
                orbMana.AddBonusMaxUnits(2);

            if (infusionCoreEquipped)
                orbMana.AddBonusMaxUnits(1);
        }

        public bool RollPersist()
        {
            if (!persistEquipped && !focusedPersistenceEquipped) return false;
            
            return Main.rand.NextFloat() < 0.10f;
        }
    }
}

