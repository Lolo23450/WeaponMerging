using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WeaponMerging.Content.Items.Weapons
{
    public class ShurikenwoodBow : ModItem
    {
        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 26;
            Item.height = 46;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(copper: 75);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly; 
            Item.shootSpeed = 7.5f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;

            
            
            if (shotCounter >= 3)
            {
                shotCounter = 0;

                
                Vector2 shurikenVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(8)) * 0.85f;

                Projectile.NewProjectile(
                    source,
                    position,
                    shurikenVelocity,
                    ProjectileID.Shuriken,
                    (int)(damage * 0.7f),
                    knockback,
                    player.whoAmI
                );
            }

            return true; 
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.WoodenBow)
                .AddIngredient(ItemID.Shuriken, 50)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
