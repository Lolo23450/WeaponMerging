using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items
{
    public class FusionOrbReleaser : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 2);
            Item.UseSound = SoundID.Item4;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.consumable = false;
        }

        public override bool? UseItem(Player player)
        {
            bool releasedAny = false;
            int releasedCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && proj.ai[1] == 0f)
                {
                    
                    int t = proj.type;
                    if (t == ModContent.ProjectileType<Content.Projectiles.FusionOrbProjectile>()
                        || t == ModContent.ProjectileType<Content.Projectiles.ChaosOrbProjectile>()
                        || t == ModContent.ProjectileType<Content.Projectiles.StarlitShadowFusionOrbProjectile>()
                        || t == ModContent.ProjectileType<Content.Projectiles.StarlitInfernoFusionOrbProjectile>())
                    {
                        Vector2 direction = (Main.MouseWorld - proj.Center).SafeNormalize(Vector2.UnitX);
                        float spread = MathHelper.ToRadians((releasedCount - 1) * 8f);
                        proj.ai[1] = 1f;
                        proj.velocity = direction.RotatedBy(spread) * 12f;
                        proj.timeLeft = System.Math.Max(proj.timeLeft, 180);
                        proj.netUpdate = true;
                        releasedAny = true;
                        releasedCount++;
                    }
                }
            }

            if (releasedAny)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.7f, Pitch = 0.1f }, player.Center);
            }

            return releasedAny;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.CrystalShard, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}

