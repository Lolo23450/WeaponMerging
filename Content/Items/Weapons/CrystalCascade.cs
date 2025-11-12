using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class CrystalCascade : ModItem
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
            Item.width = 50;
            Item.height = 50;
            Item.scale = 1.0f;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 80);

            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;

            Item.damage = 22;
            Item.knockBack = 4.5f;
            Item.DamageType = DamageClass.Magic;

            Item.shoot = ModContent.ProjectileType<Content.Projectiles.CrystalCascadeProjectile>();
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
                Item.useTime = 12;
                Item.useAnimation = 12;
                Item.autoReuse = false;
                return true;
            }
            else
            {
                
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.autoReuse = true;
                return true;
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            damage = comboCounter switch
            {
                0 => damage,
                1 => (int)(damage * 1.1f),
                2 => (int)(damage * 1.25f),
                _ => damage
            };
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                
                var pm = player.GetModPlayer<Content.Players.CrystalCascadePlayer>();
                pm.UseCrystalAttack(player, source, damage, knockback);
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
            if (comboCounter > 2)
            {
                
                var pm = player.GetModPlayer<Content.Players.CrystalCascadePlayer>();
                pm.GainCrystal(player);

                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f, Pitch = 0.3f }, player.position);
                
                
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.ToRadians(i * 22.5f);
                    Vector2 vel = new Vector2(4f, 0).RotatedBy(angle);
                    Dust crystal = Dust.NewDustPerfect(player.Center, 68, vel, 100, Color.Cyan, 1.8f);
                    crystal.noGravity = true;
                    crystal.fadeIn = 1.2f;
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
                0 => 6,
                1 => 9,
                2 => 14,
                _ => 6
            };

            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 velocity = angle.ToRotationVector2() * 2.5f;
                Dust dust = Dust.NewDustDirect(player.Center, 0, 0, 68, velocity.X, velocity.Y, 100, Color.LightCyan, 1.3f);
                dust.noGravity = true;
            }

            if (combo == 2)
            {
                CombatText.NewText(player.Hitbox, Color.Cyan, "Cascade!", true, false);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            target.AddBuff(BuffID.Slow, 60);
            target.AddBuff(BuffID.Frostburn, 120);
        }
    }
}
