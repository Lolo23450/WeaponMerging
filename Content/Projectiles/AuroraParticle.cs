using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace WeaponMerging.Content.Projectiles
{
    public class AuroraParticle : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 30; 
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.alpha += 8; 
            if (Projectile.alpha > 255) Projectile.Kill();

            
            Projectile.velocity *= 0.95f;

            
            Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.4f, 0.6f) * (1f - Projectile.alpha / 255f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Color color = Color.White * (1f - Projectile.alpha / 255f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, color, 0f, texture.Size() / 2, 0.5f, SpriteEffects.None, 0);
            return false;
        }
    }
}

