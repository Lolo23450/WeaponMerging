using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class NightfallHarbinger : ModItem
    {
        public override void SetDefaults()
        {
            
            Item.width = 24;
            Item.height = 48;
            Item.scale = 1.1f;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2);

            
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            
            
            Item.damage = 18;
            Item.knockBack = 3f;
            Item.DamageType = DamageClass.Ranged;
            
            
            Item.shoot = ModContent.ProjectileType<Content.Projectiles.NightfallHarbingerProjectile>();
            Item.shootSpeed = 13f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            
            
            Item.useAmmo = AmmoID.Arrow;
            Item.UseSound = new SoundStyle("Terraria/Sounds/Item_8") with 
            { 
                Volume = 0.7f,
                Pitch = -0.3f,
                PitchVariance = 0.1f
            };
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            type = ModContent.ProjectileType<Content.Projectiles.NightfallHarbingerProjectile>();
            
            
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            int projectileCount = Main.rand.Next(2, 4);
            float spreadAngle = MathHelper.ToRadians(7);
            
            for (int i = 0; i < projectileCount; i++)
            {
                
                float angleOffset = MathHelper.Lerp(-spreadAngle, spreadAngle, i / (float)(projectileCount - 1));
                Vector2 perturbedSpeed = velocity.RotatedBy(angleOffset);
                
                
                float curveDirection = (i % 2 == 0) ? 1f : -1f;
                
                Projectile proj = Projectile.NewProjectileDirect(
                    source,
                    position,
                    perturbedSpeed,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );
                
                
                proj.ai[1] = curveDirection;
            }
            
            
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(position, 20, 20, 27, velocity.X * 0.4f, velocity.Y * 0.4f, 100, default, 1.5f);
                dust.noGravity = true;
            }
            
            return false; 
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DemonBow)
                .AddIngredient(ItemID.DemonScythe)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(ModContent.TileType<Content.Tiles.FusionStationTile>())
                .Register();
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4, 0);
        }
    }
}
