using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WeaponMerging.Content.Projectiles
{
    public class FrostShurikenwoodGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool fromFrostBow = false;
        public bool fromFrostBowSnowball = false;

        public override void OnSpawn(Projectile projectile, Terraria.DataStructures.IEntitySource source)
        {
            if (source is Terraria.DataStructures.EntitySource_ItemUse_WithAmmo ammoSource &&
                ammoSource.Item != null &&
                ammoSource.Item.type == ModContent.ItemType<Content.Items.Weapons.FrostShurikenwoodBow>())
            {
                if (projectile.arrow || projectile.type == ProjectileID.Shuriken)
                    fromFrostBow = true;
            }
        }

        public override void AI(Projectile projectile)
        {
            
            if (fromFrostBow)
            {
                int frost = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.IceTorch);
                Main.dust[frost].velocity = projectile.velocity * -0.25f;
                Main.dust[frost].noGravity = true;
                Main.dust[frost].scale = 0.9f;
            }

            
            if (fromFrostBowSnowball)
            {
                if (Main.rand.NextBool(3))
                {
                    int snow = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Snow);
                    Main.dust[snow].velocity *= 0.4f;
                    Main.dust[snow].scale = 1.2f;
                    Main.dust[snow].noGravity = true;
                }
            }
        }
    }
}
