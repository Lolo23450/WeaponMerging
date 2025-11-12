using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    
    public class VerdantRune : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360; 
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            
            float t = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
            Projectile.rotation += 0.04f;
            Projectile.Center += new Vector2(0f, (float)System.Math.Sin(Projectile.localAI[0] * 0.15f) * 0.1f);
            Projectile.localAI[0]++;

            
            Lighting.AddLight(Projectile.Center, 0.05f, 0.22f, 0.06f);
            if (Main.rand.NextBool(3))
            {
                Vector2 ring = Main.rand.NextVector2CircularEdge(10f, 10f);
                var d = Dust.NewDustPerfect(Projectile.Center + ring, DustID.Grass, ring.SafeNormalize(Vector2.UnitY) * 0.35f, 120, new Color(80, 200, 120), Main.rand.NextFloat(0.9f, 1.2f));
                d.noGravity = true;
            }

            
            int cooldown = 12; 
            if (Projectile.ai[0] % cooldown == 0)
            {
                int fired = 0;
                const int maxTargets = 2; 
                const float range = 520f;
                int bestIndex;
                for (int k = 0; k < maxTargets; k++)
                {
                    NPC target = FindNextTarget(range, out bestIndex);
                    if (target == null)
                        break;

                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    float speed = 14f;
                    int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, dir * speed, ModContent.ProjectileType<VerdantZap>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI);
                    if (p >= 0 && p < Main.maxProjectiles)
                    {
                        Main.projectile[p].CritChance = owner.GetWeaponCrit(owner.HeldItem);
                    }
                    fired++;
                    
                    if (bestIndex >= 0 && bestIndex < Main.maxNPCs)
                        Main.npc[bestIndex].immune[Projectile.owner] = 1; 
                }

                
                if (fired > 0)
                {
                    for (int i = 0; i < 14; i++)
                    {
                        float ang = MathHelper.TwoPi * i / 14f;
                        Vector2 spd = new Vector2(1f, 0f).RotatedBy(ang) * Main.rand.NextFloat(1.5f, 2.8f);
                        var d2 = Dust.NewDustPerfect(Projectile.Center, DustID.Grass, spd, 120, new Color(90, 220, 120), Main.rand.NextFloat(0.8f, 1.1f));
                        d2.noGravity = true;
                    }
                }
            }
            Projectile.ai[0]++;
        }

        private NPC FindNextTarget(float maxDist, out int index)
        {
            index = -1;
            float bestSq = maxDist * maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy(this))
                {
                    float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d < bestSq && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1))
                    {
                        bestSq = d;
                        index = i;
                    }
                }
            }
            return index >= 0 ? Main.npc[index] : null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            
            var runeTex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 runeOrigin = new Vector2(runeTex.Width * 0.5f, runeTex.Height * 0.5f);
            Main.spriteBatch.Draw(runeTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, runeOrigin, 1f, SpriteEffects.None, 0f);

            
            var px = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            float scale = 1.0f + 0.05f * (float)System.Math.Sin(Projectile.localAI[0] * 0.2f);
            Color c1 = new Color(120, 255, 160) * 0.35f;
            Vector2 origin = new Vector2(px.Width * 0.5f, px.Height * 0.5f);
            for (int i = 0; i < 3; i++)
            {
                float rot = Projectile.rotation + i * 0.7f;
                float w = (24f + i * 6f) * scale;
                float h = 2f * scale;
                Vector2 drawScale = new Vector2(w / px.Width, h / px.Height);
                Main.spriteBatch.Draw(px, Projectile.Center - Main.screenPosition, null, c1, rot, origin, drawScale, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }

    
    public class VerdantZap : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            int targetIndex = (int)Projectile.ai[0];
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIndex];
                if (target.active && target.CanBeChasedBy(this))
                {
                    Vector2 to = target.Center - Projectile.Center;
                    float speed = 16f;
                    Vector2 desired = to.SafeNormalize(Vector2.UnitX) * speed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.18f);
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.05f, 0.24f, 0.08f);
            if (Main.rand.NextBool(2))
            {
                var d = Dust.NewDustPerfect(Projectile.Center, DustID.Grass, Projectile.velocity * 0.05f, 120, new Color(100, 240, 140), 1.0f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            target.AddBuff(BuffID.Poisoned, 120);
            for (int i = 0; i < 6; i++)
            {
                var d = Dust.NewDustPerfect(Projectile.Center, DustID.Grass, Main.rand.NextVector2Circular(1.2f, 1.2f), 120, new Color(110, 255, 170), 1.1f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float t = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color c = new Color(120, 255, 170, 0) * (t * 0.6f);
                float rot = Projectile.oldRot[i];
                float scale = Projectile.scale * (0.9f + 0.08f * t);
                Main.spriteBatch.Draw(tex, pos, null, c, rot, origin, scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}

