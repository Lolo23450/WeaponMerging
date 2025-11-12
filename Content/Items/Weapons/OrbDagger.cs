using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Content.Items.Weapons
{
    public class OrbDagger : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        private const int COMBO_RESET_TIME = 45; 
        
        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 60;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1, silver: 50);

            
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            
            
            Item.damage = 20;
            Item.knockBack = 6f;
            Item.DamageType = DamageClass.Melee;
            
            
            Item.noMelee = false;
            
            Item.UseSound = SoundID.Item1;
        }

        public override bool AltFunctionUse(Player player)
        {
            return comboCounter >= 3;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2 && comboCounter >= 3)
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.UseSound = null;
                return true;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 12;
                Item.useAnimation = 12;
                Item.UseSound = SoundID.Item1;
                return true;
            }
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

            float scale = 1.1f + comboCounter * 0.2f;
            Item.scale = scale;
            player.HeldItem.scale = scale; // Ensure held sprite scales
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2 || comboCounter < 3)
                return false;

            // Dash
            Vector2 toMouse = Main.MouseWorld - player.Center;
            toMouse.Normalize();
            player.velocity = toMouse * 15f;
            player.immuneTime = 20;
            player.immune = true;

            SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.8f, Pitch = 0.5f }, player.position);
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(player.position, player.width, player.height, DustID.BlueCrystalShard, 
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.5f);
            }

            comboCounter = 0;
            comboResetTimer = COMBO_RESET_TIME;
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnComboHitEffects(target, comboCounter);
            Lighting.AddLight(target.Center, 0.2f, 0.4f, 0.6f); // Blue light

            if (comboCounter == 3)
            {
                // Screen shake
                if (Main.myPlayer == player.whoAmI)
                {
                    Main.screenPosition += Main.rand.NextVector2Circular(5f, 5f);
                }
            }

            comboCounter++;
            if (comboCounter > 3)
            {
                comboCounter = 0;
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = -0.3f }, player.position);
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(player.position, player.width, player.height, DustID.BlueCrystalShard, 
                        Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 2f);
                }
            }

            comboResetTimer = COMBO_RESET_TIME;
        }
                
        private void SpawnComboHitEffects(NPC target, int combo)
        {
            int dustType = combo switch
            {
                0 => DustID.BlueCrystalShard,
                1 => DustID.BlueFairy,
                2 => DustID.PinkFairy,
                3 => DustID.RainbowTorch,
                _ => DustID.BlueCrystalShard
            };

            int dustCount = combo switch
            {
                0 => 5,
                1 => 8,
                2 => 12,
                3 => 20,
                _ => 5
            };
            
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType, velocity.X, velocity.Y, 100, default, combo == 3 ? 2f : 1.5f);
                dust.noGravity = true;
                if (combo == 3)
                {
                    dust.velocity *= 2f;
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ThrowingKnife, 50)
                .AddIngredient(ModContent.ItemType<OrbFragment>(), 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
