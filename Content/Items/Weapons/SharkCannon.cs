using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class SharkCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 108;
            Item.height = 44;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(gold: 2, silver: 75);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item36; 
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10f, -2f);
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            
            return Main.rand.NextFloat() >= 0.25f;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(2));
            
            
            position += velocity.SafeNormalize(Vector2.UnitX) * 30f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            bool isMegaBlast = Main.GameUpdateCount % 9 == 0;
            
            if (isMegaBlast)
            {
                
                SoundEngine.PlaySound(SoundID.Item38, position);
                
                
                for (int i = 0; i < 35; i++)
                {
                    Vector2 speed = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(45)) * Main.rand.NextFloat(8f, 16f);
                    Dust flash = Dust.NewDustPerfect(position, DustID.Smoke, speed, 100, Color.DarkGray, Main.rand.NextFloat(2.0f, 3.5f));
                    flash.noGravity = false;
                    flash.fadeIn = 0.8f;
                }
                
                for (int i = 0; i < 25; i++)
                {
                    Vector2 speed = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(40)) * Main.rand.NextFloat(6f, 14f);
                    Dust fire = Dust.NewDustPerfect(position, DustID.Torch, speed, 0, Color.Orange, Main.rand.NextFloat(2.0f, 3.0f));
                    fire.noGravity = true;
                    fire.fadeIn = 1.2f;
                }
                
                
                for (int i = 0; i < 20; i++)
                {
                    Vector2 speed = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(35)) * Main.rand.NextFloat(4f, 10f);
                    Dust energy = Dust.NewDustPerfect(position, DustID.IceTorch, speed, 100, Color.Cyan, Main.rand.NextFloat(1.5f, 2.5f));
                    energy.noGravity = true;
                    energy.fadeIn = 1.4f;
                }
                
                
                for (int i = 0; i < 7; i++)
                {
                    Vector2 pelletVel = velocity.RotatedByRandom(MathHelper.ToRadians(24)) * Main.rand.NextFloat(0.85f, 1.15f);
                    int bullet = Projectile.NewProjectile(source, position, pelletVel, type, damage, knockback, player.whoAmI);
                    
                    if (bullet >= 0 && bullet < Main.maxProjectiles && Main.projectile[bullet].active)
                    {
                        Main.projectile[bullet].CritChance = player.GetWeaponCrit(Item);
                    }
                }
                
                
                if (Main.LocalPlayer == player)
                {
                    player.GetModPlayer<ScreenShakePlayer>().AddShake(6);
                }
                
                return false;
            }
            else
            {
                
                for (int i = 0; i < 8; i++)
                {
                    Vector2 speed = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(16)) * Main.rand.NextFloat(3f, 8f);
                    Dust smoke = Dust.NewDustPerfect(position, DustID.Smoke, speed, 100, Color.Gray, Main.rand.NextFloat(1.0f, 1.8f));
                    smoke.noGravity = false;
                }
                
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(12)) * Main.rand.NextFloat(2f, 6f);
                    Dust spark = Dust.NewDustPerfect(position, DustID.Torch, speed, 0, Color.OrangeRed, Main.rand.NextFloat(1.2f, 1.8f));
                    spark.noGravity = true;
                }
                
                
                for (int i = 0; i < 3; i++)
                {
                    Dust trail = Dust.NewDustPerfect(position, DustID.IceTorch, 
                        velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(8)) * Main.rand.NextFloat(1f, 3f), 
                        100, Color.Cyan, Main.rand.NextFloat(0.8f, 1.2f));
                    trail.noGravity = true;
                }
                
                
                for (int i = 0; i < 2; i++)
                {
                    Vector2 bulletVel = velocity.RotatedByRandom(MathHelper.ToRadians(12)) * Main.rand.NextFloat(0.9f, 1.1f);
                    int bullet = Projectile.NewProjectile(source, position, bulletVel, type, damage, knockback, player.whoAmI);
                    
                    if (bullet >= 0 && bullet < Main.maxProjectiles && Main.projectile[bullet].active)
                    {
                        Main.projectile[bullet].CritChance = player.GetWeaponCrit(Item);
                    }
                }
                
                return false;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Boomstick, 1)
                .AddIngredient(ItemID.Minishark, 1)
                .AddIngredient(ItemID.SharkFin, 5)
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    
    
    public class ScreenShakePlayer : ModPlayer
    {
        public int shakeTimer = 0;
        public int shakeIntensity = 0;
        
        public void AddShake(int intensity)
        {
            shakeIntensity = intensity;
            shakeTimer = 10;
        }
        
        public override void PostUpdate()
        {
            if (shakeTimer > 0)
            {
                shakeTimer--;
                if (shakeTimer == 0)
                {
                    shakeIntensity = 0;
                }
            }
        }
        
        public override void ModifyScreenPosition()
        {
            if (shakeTimer > 0)
            {
                float intensity = shakeIntensity * (shakeTimer / 10f);
                Main.screenPosition += new Vector2(Main.rand.NextFloat(-intensity, intensity), Main.rand.NextFloat(-intensity, intensity));
            }
        }
    }
}

