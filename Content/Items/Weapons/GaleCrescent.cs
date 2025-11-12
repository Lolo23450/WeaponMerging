using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Localization;
using WeaponMerging.Content.Players;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Content.Items.Weapons
{
    public class GaleCrescent : ModItem
    {
        private int swingState = 0; 

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 17;
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5.0f;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.noMelee = true; 

            Item.shoot = ModContent.ProjectileType<GaleCrescentSlashProjectile>();
            Item.shootSpeed = 13f;
        }

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs();

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            
            float raw = 1f - (player.itemAnimation - 1) / (float)player.itemAnimationMax;
            float eased = MathHelper.SmoothStep(0f, 1f, raw);

            float rotation;
            if (swingState == 0)
            {
                
                rotation = MathHelper.Lerp(-MathHelper.PiOver4 * 1.35f, MathHelper.PiOver4 * 1.35f, eased);
            }
            else
            {
                
                rotation = MathHelper.Lerp(-MathHelper.PiOver2 * 1.0f, MathHelper.PiOver2 * 1.0f, eased) + MathHelper.PiOver2;
            }

            player.itemRotation = rotation * player.direction;

            
            float armRot = player.itemRotation + (player.direction == 1 ? 0f : MathHelper.Pi);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRot);

            
            if (Main.rand.NextBool(2))
            {
                Vector2 tip = player.MountedCenter + new Vector2(38 * player.direction, -6f).RotatedBy(player.itemRotation * player.direction);
                Vector2 trailVel = Vector2.UnitX.RotatedBy(player.itemRotation) * 2.6f;
                Dust d1 = Dust.NewDustPerfect(tip, DustID.Clentaminator_Blue, trailVel, 120, new Color(170, 225, 255), Main.rand.NextFloat(0.9f, 1.25f));
                d1.noGravity = true;
                if (Main.rand.NextBool(3))
                {
                    Dust d2 = Dust.NewDustPerfect(tip, DustID.Smoke, trailVel * 0.6f, 140, new Color(150, 200, 240), Main.rand.NextFloat(0.8f, 1.1f));
                    d2.noGravity = true;
                }
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4))
            {
                Dust wind = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Clentaminator_Blue, 0f, 0f, 120, default, 1.1f);
                wind.noGravity = true;
                wind.velocity = Vector2.Normalize(player.velocity + new Vector2(player.direction, 0f)) * 2.5f;
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            float rot = player.itemRotation + (player.direction == 1 ? 0 : MathHelper.Pi);
            Vector2 offset = new Vector2(44f, 0f).RotatedBy(rot);
            position = player.MountedCenter + offset;

            
            Vector2 toCursor = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitX);
            velocity = toCursor * Item.shootSpeed;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<GaleCrescentPlayer>();

            
            if (mp.galeStacks >= 3)
            {
                Vector2 toCursor = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitX) * (Item.shootSpeed + 3f);
                int fin = Projectile.NewProjectile(source, position, toCursor, ModContent.ProjectileType<GaleCrescentFinisherProjectile>(), (int)(damage * 1.15f), knockback + 0.5f, player.whoAmI);
                if (fin >= 0 && fin < Main.maxProjectiles)
                {
                    Main.projectile[fin].CritChance = player.GetWeaponCrit(Item) + 4;
                }

                
                mp.ResetStacksVisual();
                mp.galeStacks = 0;

                
                for (int i = 0; i < 18; i++)
                {
                    Vector2 spd = toCursor.RotatedByRandom(MathHelper.ToRadians(35)) * Main.rand.NextFloat(0.2f, 0.9f);
                    var d = Dust.NewDustPerfect(position, DustID.Clentaminator_Blue, spd, 100, new Color(185, 235, 255), Main.rand.NextFloat(1.0f, 1.4f));
                    d.noGravity = true;
                }
            }
            else
            {
                
                swingState = 1 - swingState;

                int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, swingState);
                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].CritChance = player.GetWeaponCrit(Item);
                }

                for (int i = 0; i < 10; i++)
                {
                    Vector2 spd = velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(0.3f, 0.8f);
                    Dust d = Dust.NewDustPerfect(position, DustID.Clentaminator_Blue, spd, 120, new Color(180, 230, 255), Main.rand.NextFloat(1.0f, 1.4f));
                    d.noGravity = true;
                }
            }

            return false; 
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            target.AddBuff(BuffID.Slow, 90);
        }
    }
}
