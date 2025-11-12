using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Localization;

namespace WeaponMerging.Content.Items.Weapons
{
    public class CelestialFrostBlade : ModItem
    {
        private int swingCounter = 0;

        public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 10f;
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs();

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            
            float progress = 1f - player.itemAnimation / (float)player.itemAnimationMax;
            float rotationOffset = MathHelper.Lerp(-MathHelper.PiOver4 * 1.5f, MathHelper.PiOver4 * 1.5f, progress);
            
            
            player.itemRotation = rotationOffset * player.direction;
            
            
            if (player.itemAnimation > player.itemAnimationMax / 2)
            {
                float swingProgress = (player.itemAnimation - player.itemAnimationMax / 2) / (float)(player.itemAnimationMax / 2);
                
                
                if (Main.rand.NextBool(2))
                {
                    Vector2 position = player.Center;
                    float rotation = player.itemRotation + (player.direction == 1 ? 0 : MathHelper.Pi);
                    Vector2 offset = new Vector2(40, 0).RotatedBy(rotation);
                    
                    
                    Dust ice = Dust.NewDustPerfect(position + offset, DustID.IceTorch, 
                        offset.SafeNormalize(Vector2.UnitX) * 3f, 100, Color.LightCyan, Main.rand.NextFloat(1.2f, 1.8f));
                    ice.noGravity = true;
                    ice.fadeIn = 1.1f;
                    ice.alpha = 70;
                }
                
                
                if (Main.rand.NextBool(4))
                {
                    Vector2 position = player.Center;
                    float rotation = player.itemRotation + (player.direction == 1 ? 0 : MathHelper.Pi);
                    Vector2 offset = new Vector2(Main.rand.Next(20, 50), 0).RotatedBy(rotation);
                    
                    Dust star = Dust.NewDustPerfect(position + offset, DustID.BlueFairy, 
                        Vector2.Zero, 180, Color.White, Main.rand.NextFloat(0.8f, 1.3f));
                    star.noGravity = true;
                    star.fadeIn = 1.0f;
                }
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 target = Main.MouseWorld;
            position = new Vector2(target.X + Main.rand.Next(-150, 151), target.Y - 500);
            velocity = Vector2.Normalize(target - position) * 12f; 
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            
            
            for (int i = 0; i < 15; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                Dust starDust = Dust.NewDustPerfect(position, DustID.BlueFairy, 
                    speed, 100, Color.LightCyan, Main.rand.NextFloat(1.0f, 1.5f));
                starDust.noGravity = true;
                starDust.fadeIn = 1.2f;
            }
            
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.ToRadians(i * 30);
                Vector2 offset = new Vector2(30, 0).RotatedBy(angle);
                Dust glow = Dust.NewDustPerfect(position + offset, DustID.IceTorch, 
                    -offset * 0.1f, 100, Color.Cyan, 1.2f);
                glow.noGravity = true;
                glow.alpha = 80;
            }
            
            
            int mainProj = Projectile.NewProjectile(source, position, velocity, 
                ProjectileID.StarCannonStar, damage, knockback, player.whoAmI);
            
            if (mainProj >= 0 && mainProj < Main.maxProjectiles && Main.projectile[mainProj].active)
            {
                Main.projectile[mainProj].CritChance = player.GetWeaponCrit(Item);
                Main.projectile[mainProj].scale = 1.2f;
            }
            
            
            int particleCount = 8;
            float rotationOffset = (swingCounter % 2 == 0) ? 0 : MathHelper.ToRadians(22.5f);
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.ToRadians(i * 45) + rotationOffset;
                Vector2 dustVel = new Vector2(3f, 0).RotatedBy(angle);
                Dust dust = Dust.NewDustPerfect(player.Center, DustID.IceTorch, dustVel, 100, Color.LightCyan, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1.0f;
                dust.alpha = 60;
            }
            
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.IceTorch, 0f, 0f, 100, Color.LightCyan, Main.rand.NextFloat(1.3f, 1.8f));
                dust.noGravity = true;
                dust.velocity = player.velocity * 0.3f;
                dust.fadeIn = 1.0f;
                dust.alpha = 70;
            }
            
            
            if (Main.rand.NextBool(5))
            {
                Dust star = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.BlueFairy, 0f, 0f, 180, Color.White, 1.1f);
                star.noGravity = true;
                star.velocity = Vector2.Zero;
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);
            
            
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.ToRadians(i * 22.5f);
                Vector2 speed = new Vector2(4f, 0).RotatedBy(angle);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.IceTorch, speed, 100, Color.LightCyan, 1.6f);
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
                dust.alpha = 60;
            }
            
            
            for (int i = 0; i < 5; i++)
            {
                Dust star = Dust.NewDustPerfect(target.Center, DustID.BlueFairy, 
                    Main.rand.NextVector2Circular(2f, 2f), 200, Color.White, 1.3f);
                star.noGravity = true;
            }
        }
    }
}

