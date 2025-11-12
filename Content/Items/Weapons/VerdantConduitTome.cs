using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Content.Items.Weapons
{
    public class VerdantConduitTome : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 42; 
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 32;
            Item.height = 34;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.0f;
            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ItemRarityID.LightRed; 
            Item.UseSound = SoundID.Item8;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<VerdantRune>();
            Item.shootSpeed = 0f; 
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<VerdantRune>())
                {
                    p.Kill();
                }
            }

            Vector2 spawn = Main.MouseWorld;
            int proj = Projectile.NewProjectile(source, spawn, Vector2.Zero, ModContent.ProjectileType<VerdantRune>(), damage, knockback, player.whoAmI);
            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].CritChance = player.GetWeaponCrit(Item);
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpellTome)
                .AddIngredient(ItemID.CrystalShard, 20)
                .AddIngredient(ItemID.JungleSpores, 12)
                .AddIngredient(ItemID.Vine, 5)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}

