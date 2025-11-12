using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WeaponMerging.Content.Projectiles
{
    public class ShurikenwoodGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        private bool fromShurikenwoodBow = false;

        public override void OnSpawn(Projectile projectile, Terraria.DataStructures.IEntitySource source)
        {
            
            if (source is Terraria.DataStructures.EntitySource_ItemUse_WithAmmo ammoSource &&
                ammoSource.Item != null &&
                ammoSource.Item.type == ModContent.ItemType<Content.Items.Weapons.ShurikenwoodBow>())
            {
                fromShurikenwoodBow = true;
            }
        }

        public override void AI(Projectile projectile)
        {
            
            if (fromShurikenwoodBow && projectile.arrow)
            {
                
                int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.SilverCoin);
                Main.dust[dust].velocity = projectile.velocity * -0.2f;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = 0.9f;
            }
        }
    }
}

