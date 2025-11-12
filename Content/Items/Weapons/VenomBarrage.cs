using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class VenomBarrage : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ProjectileID.ThrowingKnife;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Dart; 
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 0f);
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            
            return Main.rand.NextFloat() >= 0.33f;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ProjectileID.ThrowingKnife;
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(6));
            position += velocity.SafeNormalize(Vector2.UnitX) * 20f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 speed = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(30)) * Main.rand.NextFloat(2f, 6f);
                Dust toxic = Dust.NewDustPerfect(position, DustID.JungleSpore, speed, 100, Color.LimeGreen, Main.rand.NextFloat(1.2f, 2.0f));
                toxic.noGravity = true;
                toxic.fadeIn = 1.3f;
            }
            
            for (int i = 0; i < 8; i++)
            {
                Dust venom = Dust.NewDustPerfect(position, DustID.Poisoned, 
                    velocity.RotatedByRandom(MathHelper.ToRadians(20)) * Main.rand.NextFloat(0.3f, 0.8f), 
                    150, Color.Green, Main.rand.NextFloat(1.5f, 2.2f));
                venom.noGravity = true;
            }
            
            int mainKnife = Projectile.NewProjectile(source, position, velocity, ProjectileID.ThrowingKnife, damage, knockback, player.whoAmI);
            
            if (mainKnife >= 0 && mainKnife < Main.maxProjectiles && Main.projectile[mainKnife].active)
            {
                Main.projectile[mainKnife].CritChance = player.GetWeaponCrit(Item);
            }
            
            int burstCount = 2;
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 burstVel = velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(0.85f, 1.15f);
                int knife = Projectile.NewProjectile(source, position, burstVel, ProjectileID.PoisonedKnife, 
                    (int)(damage * 0.7f), knockback * 0.6f, player.whoAmI);
                
                if (knife >= 0 && knife < Main.maxProjectiles && Main.projectile[knife].active)
                {
                    Main.projectile[knife].CritChance = player.GetWeaponCrit(Item);
                }
            }
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.ToRadians(i * 30);
                Vector2 ringVel = new Vector2(4f, 0).RotatedBy(angle + velocity.ToRotation());
                Dust ring = Dust.NewDustPerfect(position, DustID.JungleTorch, ringVel, 120, Color.YellowGreen, 1.5f);
                ring.noGravity = true;
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ThrowingKnife, 100)
                .AddIngredient(ItemID.Blowpipe, 1)
                .AddIngredient(ItemID.Stinger, 15)
                .AddIngredient(ItemID.JungleSpores, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}

