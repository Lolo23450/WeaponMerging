using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Biomes
{
    public class FusionRuinsBiome : ModBiome
    {
        public override int Music => MusicID.Shimmer; 
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;

        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<FusionRuinsBiomeTileCount>().FusionRuinsTiles >= 5000; 
        }

        public override void OnEnter(Player player)
        {
            
            if (Main.netMode != NetmodeID.Server)
            {
                SkyManager.Instance.Activate("WeaponMerging:FusionRuinsSky");
            }
        }

        public override void OnLeave(Player player)
        {
            
            if (Main.netMode != NetmodeID.Server)
            {
                SkyManager.Instance.Deactivate("WeaponMerging:FusionRuinsSky");
            }
        }
    }

    public class FusionRuinsBiomeTileCount : ModSystem
    {
        public int FusionRuinsTiles = 0;

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            FusionRuinsTiles = tileCounts[ModContent.TileType<Tiles.FusionRuinsBrick>()];
            
            if (FusionRuinsTiles > 0)
            {
                ModContent.GetInstance<WeaponMerging>().Logger.Info($"Fusion Ruins: Found {FusionRuinsTiles} FusionRuinsBrick tiles");
            }
        }
    }
}

