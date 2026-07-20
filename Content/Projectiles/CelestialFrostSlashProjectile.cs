using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class CelestialFrostSlashProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 25;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            float t = 1f - (Projectile.timeLeft / 25f);
            float ease = MathHelper.SmoothStep(0f, 1f, t);

            // Sine-wave swaying physics based on combo direction (ai[0])
            float swaySign = (Projectile.ai[0] == 0) ? 1f : -1f;
            Vector2 along = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = along.RotatedBy(MathHelper.PiOver2 * swaySign);
            float curve = (float)System.Math.Sin(ease * System.Math.PI) * 2.0f; 
            Projectile.position += perp * curve;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.scale = MathHelper.Lerp(1.4f, 0.8f, ease);

            Lighting.AddLight(Projectile.Center, 0.1f, 0.4f, 0.6f);

            if (Main.rand.NextBool())
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + perp * Main.rand.NextFloat(-10f, 10f), DustID.IceTorch, Projectile.velocity * 0.2f, 100, Color.LightCyan, Main.rand.NextFloat(1.0f, 1.4f));
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float alphaT = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color c = new Color(150, 230, 255, 0) * (alphaT * 0.7f);
                float rot = Projectile.oldRot[i];
                float scale = Projectile.scale * (0.8f + 0.2f * alphaT);
                Main.spriteBatch.Draw(texture, pos, null, c, rot, origin, scale, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            Rectangle hitbox = Projectile.Hitbox;
            hitbox.Inflate(20, 20);
            return hitbox.Intersects(target.Hitbox);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.Frostburn, 120);
        }
    }
}