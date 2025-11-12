using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class StarlightShieldVisual : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            
            Projectile.Center = player.Center;
            
            
            var modPlayer = player.GetModPlayer<Content.Players.StarlitModPlayer>();
            if (!modPlayer.hasShield)
            {
                Projectile.Kill();
                return;
            }
            
            Projectile.timeLeft = 60; 
            
            
            Projectile.alpha = (int)(128 + Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 80);
            
            
            Projectile.rotation += 0.02f;
            
            
            if (Main.rand.NextBool(20))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * 60f;
                Dust d = Dust.NewDustPerfect(pos, DustID.YellowStarDust, angle.ToRotationVector2() * 2f);
                d.noGravity = true;
                d.scale = 1.3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D ringTex = Terraria.GameContent.TextureAssets.Extra[50].Value; 
            Vector2 origin = ringTex.Size() / 2f;
            
            float scale = 1.4f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.15f;
            Color color = new Color(255, 240, 150, Projectile.alpha);
            
            Main.EntitySpriteDraw(
                ringTex,
                Projectile.Center - Main.screenPosition,
                null,
                color,
                Projectile.rotation,
                origin,
                scale,
                SpriteEffects.None,
                0
            );
            
            return false;
        }
    }
}
