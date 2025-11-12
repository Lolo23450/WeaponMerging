using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Content.Items.Weapons
{
    public class AbyssalSharkCannon : ModItem
    {
        private int shotCounter = 0;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 120;
            Item.height = 48;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.2f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item38; 
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AbyssalSharkTorpedo>();
            Item.shootSpeed = 15.5f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-12f, -3f);
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            
            return Main.rand.NextFloat() >= 0.33f;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
            position += dir * 36f;
            velocity = dir.RotatedByRandom(MathHelper.ToRadians(3)) * velocity.Length();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            bool frenzy = shotCounter % 6 == 0; 

            if (frenzy)
            {
                SoundEngine.PlaySound(SoundID.Item38, position);
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 v = velocity.RotatedBy(MathHelper.ToRadians(i * 6)) * Main.rand.NextFloat(0.95f, 1.05f);
                    int p = Projectile.NewProjectile(source, position, v, ModContent.ProjectileType<AbyssalSharkTorpedo>(), (int)(damage * 1.1f), knockback + 1.5f, player.whoAmI, 1f);
                    if (p >= 0 && p < Main.maxProjectiles)
                        Main.projectile[p].CritChance = player.GetWeaponCrit(Item) + 6;
                }

                
                for (int i = 0; i < 30; i++)
                {
                    Vector2 spd = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(40)) * Main.rand.NextFloat(6f, 14f);
                    var d = Dust.NewDustPerfect(position, DustID.IceTorch, spd, 120, new Color(120, 200, 255), Main.rand.NextFloat(1.2f, 1.8f));
                    d.noGravity = true;
                }
                for (int i = 0; i < 18; i++)
                {
                    float ang = MathHelper.TwoPi * i / 18f;
                    Vector2 spd = new Vector2(1f, 0f).RotatedBy(ang) * Main.rand.NextFloat(4f, 7f);
                    var d = Dust.NewDustPerfect(position, DustID.DungeonWater, spd, 120, new Color(100, 180, 255), Main.rand.NextFloat(1.0f, 1.4f));
                    d.noGravity = true;
                }
                for (int i = 0; i < 16; i++)
                {
                    Vector2 spd = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(22)) * Main.rand.NextFloat(5f, 11f);
                    var d = Dust.NewDustPerfect(position, DustID.GemSapphire, spd, 0, new Color(160, 220, 255), Main.rand.NextFloat(1.1f, 1.6f));
                    d.noGravity = true;
                }
                Lighting.AddLight(position, 0.15f, 0.35f, 0.65f);
                if (Main.LocalPlayer == player)
                {
                    player.GetModPlayer<ScreenShakePlayer>().AddShake(7);
                }
                return false;
            }
            else
            {
                int p = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<AbyssalSharkTorpedo>(), damage, knockback, player.whoAmI, 0f);
                if (p >= 0 && p < Main.maxProjectiles)
                    Main.projectile[p].CritChance = player.GetWeaponCrit(Item);

                
                for (int i = 0; i < 10; i++)
                {
                    Vector2 spd = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(20)) * Main.rand.NextFloat(3f, 8f);
                    var d = Dust.NewDustPerfect(position, DustID.Smoke, spd, 80, Color.Gray, Main.rand.NextFloat(1.0f, 1.5f));
                    d.noGravity = false;
                }
                for (int i = 0; i < 12; i++)
                {
                    float ang = MathHelper.TwoPi * i / 12f;
                    Vector2 spd = new Vector2(1f, 0f).RotatedBy(ang) * Main.rand.NextFloat(2.5f, 4.5f);
                    var d = Dust.NewDustPerfect(position, DustID.DungeonWater, spd, 120, new Color(100, 180, 255), Main.rand.NextFloat(0.9f, 1.2f));
                    d.noGravity = true;
                }
                Lighting.AddLight(position, 0.08f, 0.25f, 0.45f);
                if (Main.LocalPlayer == player)
                {
                    player.GetModPlayer<ScreenShakePlayer>().AddShake(3);
                }
                return false;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SharkCannon>())
                .AddIngredient(ItemID.LunarBar, 12)
                .AddIngredient(ItemID.FragmentVortex, 18)
                .AddIngredient(ItemID.SharkFin, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}

