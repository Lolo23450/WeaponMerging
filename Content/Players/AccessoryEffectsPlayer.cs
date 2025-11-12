using Terraria;
using Terraria.ModLoader;

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
        public int ComboReduction;

        public int BonusMaxOrbs => (orbBandEquipped ? 1 : 0) + (catalystEquipped ? 1 : 0) + (orbMasterBandEquipped ? 2 : 0);
        public int BonusShotsPerOrb => (focusEquipped ? 2 : 0) + (focusedPersistenceEquipped ? 2 : 0);
        public int IntervalReduction => (focusEquipped ? 2 : 0) + (catalystEquipped ? 1 : 0) + (focusedPersistenceEquipped ? 2 : 0) + (orbMasterBandEquipped ? 1 : 0) + (comboCatalystEquipped ? 1 : 0);

        public override void ResetEffects()
        {
            orbBandEquipped = false;
            focusEquipped = false;
            catalystEquipped = false;
            persistEquipped = false;
            orbMasterBandEquipped = false;
            focusedPersistenceEquipped = false;
            comboCatalystEquipped = false;
            ComboReduction = 0;
        }

        public bool RollPersist()
        {
            if (!persistEquipped && !focusedPersistenceEquipped) return false;
            
            return Main.rand.NextFloat() < 0.10f;
        }
    }
}
