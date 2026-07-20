using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class LuminousMirageBlade : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        // Increased reset time to account for the slower 36-frame swing
        private const int COMBO_RESET_TIME = 70; 
        
        public override void SetDefaults()
        {
            Item.width = 76;
            Item.height = 76;
            Item.scale = 1.2f;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);

            // Slower, heavier swings
            Item.useTime = 36; 
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            
            Item.damage = 65; 
            Item.knockBack = 7.5f;
            Item.DamageType = DamageClass.Melee;
            
            Item.shoot = ModContent.ProjectileType<Content.Projectiles.LuminousMirageBladeProjectile>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            
            Item.UseSound = null; // Handled dynamically in Projectile
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
            // Balanced damage scaling
            damage = comboCounter switch
            {
                0 => damage,
                1 => (int)(damage * 1.15f),
                2 => (int)(damage * 1.25f),
                3 => (int)(damage * 1.50f), // Reasonable finisher damage
                _ => damage
            };
            
            // Balanced knockback
            knockback = comboCounter switch
            {
                3 => knockback * 2.0f, 
                2 => knockback * 1.3f,
                _ => knockback
            };
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
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
            SpawnComboStartEffects(player, comboCounter);
            
            comboCounter++;
            if (comboCounter > 3)
            {
                comboCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item43 with { Volume = 0.8f, Pitch = 0.2f }, player.position);
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, 
                        Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 0, default, 2.5f);
                }
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
                3 => 30,
                _ => 8
            };
            
            int dustType = combo switch
            {
                0 => DustID.Vortex,          // Cyan
                1 => DustID.PinkCrystalShard,// Pink
                2 => DustID.CrystalPulse,    // Magenta
                3 => DustID.GoldFlame,       // Gold
                _ => DustID.MagicMirror
            };
            
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, dustType, velocity.X, velocity.Y, 0, default, 1.8f);
                dust.noGravity = true;
                dust.velocity *= (combo + 1) * 0.8f; 
            }
            
            if (combo == 3)
            {
                CombatText.NewText(player.Hitbox, new Color(100, 255, 255), "MIRAGE BURST!", true, true);
            }
        }
    }
}