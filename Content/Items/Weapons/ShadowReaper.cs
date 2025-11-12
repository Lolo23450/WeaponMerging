using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class ShadowReaper : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        private const int COMBO_RESET_TIME = 60;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 1.0f;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;

            Item.damage = 70;
            Item.knockBack = 6f;
            Item.DamageType = DamageClass.Magic;

            Item.shoot = ModContent.ProjectileType<Content.Projectiles.ShadowReaperProjectile>();
            Item.shootSpeed = 1.0f;
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

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.autoReuse = false;
                return true;
            }
            else
            {
                
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.autoReuse = true;
                return true;
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            damage = comboCounter switch
            {
                0 => damage,
                1 => (int)(damage * 1.15f),
                2 => (int)(damage * 1.35f),
                _ => damage
            };
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                
                var pm = player.GetModPlayer<Content.Players.ShadowReaperPlayer>();
                pm.UseOrbAttack(player, source, damage, knockback);
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

            SpawnComboEffects(player, comboCounter);

            comboCounter++;
            int requiredCombos = Math.Max(1, 3 - player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().ComboReduction);
            int threshold = requiredCombos - 1;
            if (comboCounter > threshold)
            {
                
                var pm = player.GetModPlayer<Content.Players.ShadowReaperPlayer>();
                pm.GainOrb(player);

                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.8f, Pitch = -0.3f }, player.position);
                for (int i = 0; i < 20; i++)
                {
                    int dustType = Main.rand.Next(new int[] { 27, 173 });
                    Dust.NewDust(player.position, player.width, player.height, dustType,
                        Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 100, default, 2.0f);
                }

                comboCounter = 0;
            }

            comboResetTimer = COMBO_RESET_TIME;

            return false;
        }

        private void SpawnComboEffects(Player player, int combo)
        {
            int dustCount = combo switch
            {
                0 => 8,
                1 => 12,
                2 => 18,
                _ => 8
            };

            int dustType = combo switch
            {
                0 => 27,
                1 => 173,
                2 => 54,
                _ => 27
            };

            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 velocity = angle.ToRotationVector2() * 3f;
                Dust dust = Dust.NewDustDirect(player.Center, 0, 0, dustType, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
            }

            string comboText = combo switch
            {
                0 => "Reap!",
                1 => "Harvest!",
                2 => "Death!",
                _ => ""
            };

            if (combo == 2)
            {
                CombatText.NewText(player.Hitbox, new Color(120, 50, 180), comboText, true, false);
            }
        }
    }
}
