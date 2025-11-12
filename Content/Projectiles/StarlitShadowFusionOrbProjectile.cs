using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class StarlitShadowFusionOrbProjectile : ModProjectile
    {
        private int specialTimer = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 220;
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
                float angle = Main.GlobalTimeWrappedHourly * 0.25f * 2f;
                Vector2 orbitTarget = owner.Center + angle.ToRotationVector2() * 62f;
                Projectile.velocity = (orbitTarget - Projectile.Center) * 0.18f;
                Projectile.rotation += 0.22f;
                Lighting.AddLight(Projectile.Center, new Vector3(0.7f, 0.6f, 0.9f));
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, -Projectile.velocity * 0.2f, 100, new Color(180, 120, 255), 1.2f);
                    d.noGravity = true;
                }
            }
            else
            {
                NPC target = FindNearestTarget(Projectile.Center, 750f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 9f, 0.1f);

                    
                    if (specialTimer % 20 == 0 && Vector2.Distance(Projectile.Center, target.Center) > 120f && Vector2.Distance(Projectile.Center, target.Center) < 700f)
                    {
                        Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Vector2 from = Projectile.Center;
                        Projectile.Center += dir * 160f;
                        for (int i = 0; i < 8; i++)
                        {
                            float t = i / 8f;
                            Vector2 pos = Vector2.Lerp(from, Projectile.Center, t);
                            Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Vector2.Zero, 100, new Color(200, 160, 255), 1.1f);
                            d.noGravity = true;
                        }
                    }

                    
                    if (specialTimer % 26 == 0 && Main.myPlayer == Projectile.owner)
                    {
                        Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 14f;
                        int proj = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            dir,
                            ProjectileID.ShadowBeamHostile,
                            (int)(Projectile.damage * 0.65f),
                            Projectile.knockBack * 0.7f,
                            Projectile.owner
                        );
                        if (proj >= 0 && proj < Main.maxProjectiles)
                        {
                            Main.projectile[proj].friendly = true;
                            Main.projectile[proj].hostile = false;
                            Main.projectile[proj].DamageType = DamageClass.Magic;
                            Main.projectile[proj].timeLeft = 120;
                        }
                    }
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Main.rand.NextBool(2))
                {
                    Dust d1 = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Vector2.Zero, 100, new Color(200, 160, 255), 1.2f);
                    d1.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 240);
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
                Color trail = new Color(180, 120, 255) * (1f - t) * 0.7f;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, pos, null, trail, Projectile.oldRot[i], origin, Projectile.scale * (1f - t * 0.2f), SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, new Color(220, 180, 255), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}

