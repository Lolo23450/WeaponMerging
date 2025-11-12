using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class InfernoOrb : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        private const int COMBO_RESET_TIME = 55;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.scale = 1.0f;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3);

            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.damage = 55;
            Item.knockBack = 4.5f;
            Item.DamageType = DamageClass.Magic;

            Item.shoot = ModContent.ProjectileType<Content.Projectiles.InfernoOrbProjectile>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = false;

            Item.UseSound = SoundID.Item20;
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
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.autoReuse = false;
                Item.UseSound = null;
                return true;
            }
            else
            {
                
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.autoReuse = true;
                Item.UseSound = SoundID.Item20;
                return true;
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            damage = comboCounter switch
            {
                0 => damage,
                1 => (int)(damage * 1.2f),
                2 => (int)(damage * 1.45f),
                _ => damage
            };
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                
                var pm = player.GetModPlayer<Content.Players.InfernoPlayer>();
                pm.UseOrbAttack(player, source, damage, knockback);
                return false;
            }

            SpawnComboEffects(player, comboCounter);

            comboCounter++;
            int requiredCombos = Math.Max(1, 3 - player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().ComboReduction);
            int threshold = requiredCombos - 1;
            if (comboCounter > threshold)
            {
                
                var pm = player.GetModPlayer<Content.Players.InfernoPlayer>();
                pm.GainOrb(player);

                SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.8f, Pitch = 0.2f }, player.position);
                for (int i = 0; i < 25; i++)
                {
                    int dustType = Main.rand.Next(new int[] { 6, 158, 174 });
                    Dust.NewDust(player.position, player.width, player.height, dustType,
                        Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f), 100, default, 2.2f);
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
                0 => 10,
                1 => 16,
                2 => 24,
                _ => 10
            };

            int dustType = combo switch
            {
                0 => 6,
                1 => 158,
                2 => 174,
                _ => 6
            };

            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 velocity = angle.ToRotationVector2() * 3.5f;
                Dust dust = Dust.NewDustDirect(player.Center, 0, 0, dustType, velocity.X, velocity.Y, 100, default, 1.6f);
                dust.noGravity = true;
            }

            string comboText = combo switch
            {
                0 => "Ignite!",
                1 => "Blaze!",
                2 => "Inferno!",
                _ => ""
            };

            if (combo == 2)
            {
                CombatText.NewText(player.Hitbox, new Color(255, 140, 0), comboText, true, false);
            }
        }

        
    }
}
