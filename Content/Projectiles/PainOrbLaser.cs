using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace WeaponMerging.Content.Projectiles
{
    public class PainOrbLaser : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, 113, Vector2.Zero, 100, default, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.8f, 0.3f));
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            
            if (Projectile.ai[1] > 0)
            {
                modifiers.SetMaxDamage((int)Projectile.ai[1]);
                modifiers.SourceDamage.Flat = (int)Projectile.ai[1];
                modifiers.SourceDamage.Base = 0;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 113,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                d.noGravity = true;
                d.scale = 1.1f;
            }

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 113,
                    Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f));
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            
            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float trailProgress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.5f;
                Color trailColor = new Color(80, 255, 140) * trailAlpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(
                    tex,
                    trailPos,
                    null,
                    trailColor,
                    Projectile.oldRot[i],
                    origin,
                    Projectile.scale * (1f - trailProgress * 0.3f),
                    SpriteEffects.None,
                    0
                );
            }

            
            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                new Color(100, 255, 150),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}

