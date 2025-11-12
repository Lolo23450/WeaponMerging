using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WeaponMerging.Systems
{
    public class FusionUnlockPlayer : ModPlayer
    {
        
        public bool Unlocked_InfernoOrb;
        public bool Unlocked_ShadowReaper;
        public bool Unlocked_CelestialFrostBlade;
        public bool Unlocked_PainSpiral;
        public bool Unlocked_SolarConductor;
        public bool Unlocked_CrystalCascade;
        public bool Unlocked_FossilDancer;
        public bool Unlocked_GaleCrescent;
        public bool Unlocked_StarlitWhirlwind;
        public bool Unlocked_OrbMasterBand;
        public bool Unlocked_FocusedPersistenceCrystal;
        public bool Unlocked_ComboCatalystCharm;
        public bool Unlocked_ShurikenwoodBow;
        public bool Unlocked_FrostShurikenwoodBow;
        public bool Unlocked_VenomBarrage;
        public bool Unlocked_NightfallHarbinger;
        public bool Unlocked_VerdantConduitTome;
        public bool Unlocked_SharkCannon;
        public bool Unlocked_AbyssalSharkCannon;
        public bool Unlocked_AuroraBow;

        public override void Initialize()
        {
            Unlocked_InfernoOrb = false;
            Unlocked_ShadowReaper = false;
            Unlocked_CelestialFrostBlade = false;
            Unlocked_PainSpiral = false;
            Unlocked_SolarConductor = false;
            Unlocked_CrystalCascade = false;
            Unlocked_FossilDancer = false;
            Unlocked_GaleCrescent = false;
            Unlocked_StarlitWhirlwind = false;
            Unlocked_OrbMasterBand = false;
            Unlocked_FocusedPersistenceCrystal = false;
            Unlocked_ComboCatalystCharm = false;
            Unlocked_ShurikenwoodBow = false;
            Unlocked_FrostShurikenwoodBow = false;
            Unlocked_VenomBarrage = false;
            Unlocked_NightfallHarbinger = false;
            Unlocked_VerdantConduitTome = false;
            Unlocked_SharkCannon = false;
            Unlocked_AbyssalSharkCannon = false;
            Unlocked_AuroraBow = false;
        }

        public override void SaveData(TagCompound tag)
        {
            var flags = new List<string>();
            if (Unlocked_InfernoOrb) flags.Add(nameof(Unlocked_InfernoOrb));
            if (Unlocked_ShadowReaper) flags.Add(nameof(Unlocked_ShadowReaper));
            if (Unlocked_CelestialFrostBlade) flags.Add(nameof(Unlocked_CelestialFrostBlade));
            if (Unlocked_PainSpiral) flags.Add(nameof(Unlocked_PainSpiral));
            if (Unlocked_SolarConductor) flags.Add(nameof(Unlocked_SolarConductor));
            if (Unlocked_CrystalCascade) flags.Add(nameof(Unlocked_CrystalCascade));
            if (Unlocked_FossilDancer) flags.Add(nameof(Unlocked_FossilDancer));
            if (Unlocked_GaleCrescent) flags.Add(nameof(Unlocked_GaleCrescent));
            if (Unlocked_StarlitWhirlwind) flags.Add(nameof(Unlocked_StarlitWhirlwind));
            if (Unlocked_OrbMasterBand) flags.Add(nameof(Unlocked_OrbMasterBand));
            if (Unlocked_FocusedPersistenceCrystal) flags.Add(nameof(Unlocked_FocusedPersistenceCrystal));
            if (Unlocked_ComboCatalystCharm) flags.Add(nameof(Unlocked_ComboCatalystCharm));
            if (Unlocked_ShurikenwoodBow) flags.Add(nameof(Unlocked_ShurikenwoodBow));
            if (Unlocked_FrostShurikenwoodBow) flags.Add(nameof(Unlocked_FrostShurikenwoodBow));
            if (Unlocked_VenomBarrage) flags.Add(nameof(Unlocked_VenomBarrage));
            if (Unlocked_NightfallHarbinger) flags.Add(nameof(Unlocked_NightfallHarbinger));
            if (Unlocked_VerdantConduitTome) flags.Add(nameof(Unlocked_VerdantConduitTome));
            if (Unlocked_SharkCannon) flags.Add(nameof(Unlocked_SharkCannon));
            if (Unlocked_AbyssalSharkCannon) flags.Add(nameof(Unlocked_AbyssalSharkCannon));
            if (Unlocked_AuroraBow) flags.Add(nameof(Unlocked_AuroraBow));
            tag["fusion_unlocks"] = flags;
        }

        public override void LoadData(TagCompound tag)
        {
            var flags = tag.Get<List<string>>("fusion_unlocks") ?? new List<string>();
            Unlocked_InfernoOrb = flags.Contains(nameof(Unlocked_InfernoOrb));
            Unlocked_ShadowReaper = flags.Contains(nameof(Unlocked_ShadowReaper));
            Unlocked_CelestialFrostBlade = flags.Contains(nameof(Unlocked_CelestialFrostBlade));
            Unlocked_PainSpiral = flags.Contains(nameof(Unlocked_PainSpiral));
            Unlocked_SolarConductor = flags.Contains(nameof(Unlocked_SolarConductor));
            Unlocked_CrystalCascade = flags.Contains(nameof(Unlocked_CrystalCascade));
            Unlocked_FossilDancer = flags.Contains(nameof(Unlocked_FossilDancer));
            Unlocked_GaleCrescent = flags.Contains(nameof(Unlocked_GaleCrescent));
            Unlocked_StarlitWhirlwind = flags.Contains(nameof(Unlocked_StarlitWhirlwind));
            Unlocked_OrbMasterBand = flags.Contains(nameof(Unlocked_OrbMasterBand));
            Unlocked_FocusedPersistenceCrystal = flags.Contains(nameof(Unlocked_FocusedPersistenceCrystal));
            Unlocked_ComboCatalystCharm = flags.Contains(nameof(Unlocked_ComboCatalystCharm));
            Unlocked_ShurikenwoodBow = flags.Contains(nameof(Unlocked_ShurikenwoodBow));
            Unlocked_FrostShurikenwoodBow = flags.Contains(nameof(Unlocked_FrostShurikenwoodBow));
            Unlocked_VenomBarrage = flags.Contains(nameof(Unlocked_VenomBarrage));
            Unlocked_NightfallHarbinger = flags.Contains(nameof(Unlocked_NightfallHarbinger));
            Unlocked_VerdantConduitTome = flags.Contains(nameof(Unlocked_VerdantConduitTome));
            Unlocked_SharkCannon = flags.Contains(nameof(Unlocked_SharkCannon));
            Unlocked_AbyssalSharkCannon = flags.Contains(nameof(Unlocked_AbyssalSharkCannon));
            Unlocked_AuroraBow = flags.Contains(nameof(Unlocked_AuroraBow));
        }
    }
}

