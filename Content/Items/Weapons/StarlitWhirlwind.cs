using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class StarlitWhirlwind : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        private const int COMBO_RESET_TIME = 45;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 100;
            Item.height = 100;
            Item.scale = 1.0f;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);

            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.damage = 24;
            Item.knockBack = 3f;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 2;

            Item.shoot = ModContent.ProjectileType<Content.Projectiles.StarlitWhirlwindProjectile>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.UseSound = null;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer <= 0)
                {
                    comboCounter = 0;
                }
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            damage = comboCounter switch
            {
                0 => damage,
                1 => (int)(damage * 1.15f),
                2 => (int)(damage * 1.4f),
                _ => damage
            };

            knockback = comboCounter switch
            {
                2 => knockback * 1.4f,
                1 => knockback * 1.15f,
                _ => knockback
            };
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.autoReuse = false;
                return true;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 26;
                Item.useAnimation = 26;
                Item.autoReuse = true;
                return true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var pm = player.GetModPlayer<Content.Players.StarlitModPlayer>();
                pm.ReleaseAllOrbs(player, source);
                return false;
            }

            Vector2 toMouse = Main.MouseWorld - player.Center;
            toMouse.Normalize();

            Projectile proj = Projectile.NewProjectileDirect(
                source,
                player.Center,
                toMouse,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            proj.ai[0] = comboCounter;
            proj.ai[1] = 0;

            SpawnComboStartEffects(player, comboCounter);

            comboCounter++;
            if (comboCounter > 2)
            {
                var pm = player.GetModPlayer<Content.Players.StarlitModPlayer>();
                pm.GainOrb(player);

                
                if (pm.hasShield && Main.myPlayer == player.whoAmI)
                {
                    
                    bool hasShieldProj = false;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && 
                            Main.projectile[i].owner == player.whoAmI && 
                            Main.projectile[i].type == ModContent.ProjectileType<Content.Projectiles.StarlightShieldVisual>())
                        {
                            hasShieldProj = true;
                            break;
                        }
                    }
                    
                    if (!hasShieldProj)
                    {
                        Projectile.NewProjectile(
                            source,
                            player.Center,
                            Vector2.Zero,
                            ModContent.ProjectileType<Content.Projectiles.StarlightShieldVisual>(),
                            0,
                            0f,
                            player.whoAmI
                        );
                    }
                }

                SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.7f, Pitch = 0.1f }, player.position);
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(player.position, player.width, player.height, DustID.YellowStarDust,
                        Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 100, default, 1.8f);
                }

                comboCounter = 0;
            }

            

            comboResetTimer = COMBO_RESET_TIME;

            return false;
        }

        private void SpawnComboStartEffects(Player player, int combo)
        {
            int dustCount = combo switch
            {
                0 => 8,
                1 => 12,
                2 => 18,
                _ => 8
            };

            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 velocity = angle.ToRotationVector2() * 3f;
                Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.YellowStarDust, velocity.X, velocity.Y, 100, default, 1.4f);
                dust.noGravity = true;
            }

            if (combo == 2)
            {
                CombatText.NewText(player.Hitbox, new Color(255, 240, 150), "Starburst!", true, false);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 120);
        }
    }
}

