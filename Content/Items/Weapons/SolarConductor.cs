using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class SolarConductor : ModItem
    {
        public override void SetDefaults()
        {
            
            Item.damage = 55;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
            
            Item.shoot = ProjectileID.BallofFire;
            Item.shootSpeed = 10f;
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Item.type] = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            Vector2 offset = velocity.SafeNormalize(Vector2.UnitX) * 40f;
            position += offset;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.ToRadians(i * 14.4f);
                Vector2 circleOffset = new Vector2(35, 0).RotatedBy(angle);
                Dust circle = Dust.NewDustPerfect(position + circleOffset, DustID.MagicMirror, 
                    circleOffset * 0.08f, 100, Color.Gold, Main.rand.NextFloat(1.4f, 2.0f));
                circle.noGravity = true;
                circle.fadeIn = 1.4f;
            }
            
            
            for (int i = 0; i < 15; i++)
            {
                float spiralAngle = MathHelper.ToRadians(i * 24);
                Vector2 spiralVel = new Vector2(6f, 0).RotatedBy(spiralAngle);
                Dust purple = Dust.NewDustPerfect(position, DustID.PurpleTorch, spiralVel, 120, 
                    Color.Purple, Main.rand.NextFloat(1.2f, 1.8f));
                purple.noGravity = true;
            }
            
            
            for (int i = 0; i < 12; i++)
            {
                Vector2 flameVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust flame = Dust.NewDustPerfect(position, DustID.Torch, flameVel, 0, 
                    Color.OrangeRed, Main.rand.NextFloat(1.5f, 2.2f));
                flame.noGravity = true;
            }
            
            
            int mainOrb = Projectile.NewProjectile(source, position, velocity, 
                ProjectileID.BallofFire, damage, knockback, player.whoAmI);
            
            if (mainOrb >= 0 && mainOrb < Main.maxProjectiles && Main.projectile[mainOrb].active)
            {
                Main.projectile[mainOrb].CritChance = player.GetWeaponCrit(Item);
                Main.projectile[mainOrb].scale = 1.3f;
            }
            
            return false;
        }
    }
}

