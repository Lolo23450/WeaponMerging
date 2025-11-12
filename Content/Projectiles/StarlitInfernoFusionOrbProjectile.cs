using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class StarlitInfernoFusionOrbProjectile : ModProjectile
    {
        private int specialTimer = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active)
            {
                Projectile.Kill();
                return;
            }

            specialTimer++;

            if (Projectile.ai[1] == 0f)
            {
                float angle = Main.GlobalTimeWrappedHourly * 0.3f * 2f;
                Vector2 orbitTarget = owner.Center + angle.ToRotationVector2() * 66f;
                Projectile.velocity = (orbitTarget - Projectile.Center) * 0.2f;
                Projectile.rotation += 0.24f;
                Lighting.AddLight(Projectile.Center, new Vector3(1.0f, 0.6f, 0.2f));
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.OrangeTorch, -Projectile.velocity * 0.25f, 100, new Color(255, 200, 100), 1.3f);
                    d.noGravity = true;
                }
            }
            else
            {
                NPC target = FindNearestTarget(Projectile.Center, 850f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 9.5f, 0.12f);

                    
                    if (specialTimer % 18 == 0 && Main.myPlayer == Projectile.owner)
                    {
                        Vector2 baseDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12.5f;
                        float[] offsets = new float[] { -0.28f, -0.14f, 0f, 0.14f, 0.28f };
                        for (int i = 0; i < offsets.Length; i++)
                        {
                            Vector2 v = baseDir.RotatedBy(offsets[i]);
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                Projectile.Center,
                                v,
                                ModContent.ProjectileType<ChaosBoltProjectile>(),
                                (int)(Projectile.damage * 0.5f),
                                Projectile.knockBack * 0.6f,
                                Projectile.owner
                            );
                        }
                    }
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Main.rand.NextBool(2))
                {
                    Dust d1 = Dust.NewDustPerfect(Projectile.Center, DustID.OrangeTorch, Vector2.Zero, 100, new Color(255, 200, 120), 1.2f);
                    d1.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 240);
            target.AddBuff(BuffID.ShadowFlame, 180);
        }

        private NPC FindNearestTarget(Vector2 position, float maxRange)
        {
            NPC best = null;
            float bestDist = maxRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.life > 0 && !npc.dontTakeDamage)
                {
                    float d = Vector2.Distance(position, npc.Center);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = npc;
                    }
                }
            }
            return best;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float t = i / (float)Projectile.oldPos.Length;
                Color trail = new Color(255, 180, 100) * (1f - t) * 0.7f;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, pos, null, trail, Projectile.oldRot[i], origin, Projectile.scale * (1f - t * 0.2f), SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 210, 120), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
