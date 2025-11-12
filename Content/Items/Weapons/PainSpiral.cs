using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class PainSpiral : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        private const int COMBO_RESET_TIME = 50;
        public override void SetStaticDefaults()
        {
            
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            
            Item.width = 120;
            Item.height = 120;
            Item.scale = 1.0f;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 2);

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.damage = 35;
            Item.knockBack = 5f;
            Item.DamageType = DamageClass.Magic;

            Item.shoot = ModContent.ProjectileType<Content.Projectiles.PainSpiralProjectile>();
            Item.shootSpeed = 1.2f;
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
                1 => (int)(damage * 1.2f),
                2 => (int)(damage * 1.5f),
                _ => damage
            };

            knockback = comboCounter switch
            {
                2 => knockback * 1.5f,
                1 => knockback * 1.2f,
                _ => knockback
            };
        }

        public override bool CanUseItem(Player player)
        {
            
            if (player.altFunctionUse == 2)
            {
                
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.autoReuse = false;
                return true;
            }
            else
            {
                
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 24;
                Item.useAnimation = 24;
                Item.autoReuse = true;
                return true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            if (player.altFunctionUse == 2)
            {
                
                var pm = player.GetModPlayer<Content.Players.PainModPlayer>();
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
            int requiredCombos = Math.Max(1, 3 - player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().ComboReduction);
            int threshold = requiredCombos - 1; 
            if (comboCounter > threshold)
            {
                
                var pm = player.GetModPlayer<Content.Players.PainModPlayer>();
                pm.GainOrb(player);

                
                SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.7f, Pitch = -0.2f }, player.position);
                for (int i = 0; i < 12; i++)
                {
                    int dustType = Main.rand.Next(new int[] { 60, 174 });
                    Dust.NewDust(player.position, player.width, player.height, dustType,
                        Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.8f);
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
                0 => 6,
                1 => 10,
                2 => 16,
                _ => 6
            };

            int dustType = combo switch
            {
                0 => 60,
                1 => 3,
                2 => 174,
                _ => 60
            };

            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 velocity = angle.ToRotationVector2() * 2.5f;
                Dust dust = Dust.NewDustDirect(player.Center, 0, 0, dustType, velocity.X, velocity.Y, 100, default, 1.3f);
                dust.noGravity = true;
            }

            string comboText = combo switch
            {
                0 => "Spin!",
                1 => "Thorn!",
                2 => "Spiral!",
                _ => ""
            };

            if (combo == 2)
            {
                CombatText.NewText(player.Hitbox, new Color(200, 100, 255), comboText, true, false);
            }
        }

        
    }
}
