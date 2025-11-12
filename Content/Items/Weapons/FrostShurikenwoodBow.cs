using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WeaponMerging.Content.Items.Weapons
{
    public class FrostShurikenwoodBow : ModItem
    {
        private int shotCounter = 0;
        private int snowCounter = 0;

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 26;
            Item.height = 48;
            Item.useTime = 23;
            Item.useAnimation = 23;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2.2f;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            snowCounter++;

            
            if (shotCounter >= 3)
            {
                shotCounter = 0;
                Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(10)) * 0.85f;
                Projectile.NewProjectile(source, position, vel, ProjectileID.Shuriken, (int)(damage * 0.7f), knockback, player.whoAmI);
            }

            
            if (snowCounter >= 5)
            {
                snowCounter = 0;
                Vector2 snowVel = velocity * 0.75f;
                Projectile snowball = Main.projectile[
                    Projectile.NewProjectile(source, position, snowVel, ProjectileID.SnowBallFriendly, (int)(damage * 0.85f), knockback + 1f, player.whoAmI)
                ];

                
                snowball.GetGlobalProjectile<Projectiles.FrostShurikenwoodGlobalProjectile>().fromFrostBowSnowball = true;
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ShurikenwoodBow>())
                .AddIngredient(ItemID.SnowballCannon)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}

