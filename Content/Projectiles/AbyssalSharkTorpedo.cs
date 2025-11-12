using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class AbyssalSharkTorpedo : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            
            Projectile.velocity *= 1.01f;
            NPC target = FindTarget(550f);
            if (target != null)
            {
                Vector2 to = target.Center - Projectile.Center;
                float speed = Projectile.velocity.Length();
                Vector2 desired = to.SafeNormalize(Vector2.UnitX) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.06f);
            }

            
            Projectile.rotation = Projectile.velocity.ToRotation();

            
            Lighting.AddLight(Projectile.Center, 0.08f, 0.25f, 0.5f);
            if (Main.rand.NextBool())
            {
                Vector2 dv = Projectile.velocity.RotatedByRandom(0.2f) * 0.18f;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, dv, 120, new Color(90, 170, 255), Main.rand.NextFloat(1.0f, 1.3f));
                d.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Vector2 dv2 = Projectile.velocity.SafeNormalize(Vector2.UnitX) * -0.2f;
                Dust d2 = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.2f, DustID.IceTorch, dv2, 100, new Color(160, 220, 255), 0.9f);
                d2.noGravity = true;
            }
        }

        private NPC FindTarget(float maxDist)
        {
            NPC best = null;
            float bestSq = maxDist * maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy(this))
                {
                    float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d < bestSq)
                    {
                        bestSq = d;
                        best = n;
                    }
                }
            }
            return best;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode();
        }

        private void Explode()
        {
            
            for (int i = 0; i < 28; i++)
            {
                Vector2 spd = Projectile.velocity.RotatedByRandom(0.7f) * Main.rand.NextFloat(0.5f, 1.4f);
                var d = Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, spd, 120, new Color(110, 205, 255), Main.rand.NextFloat(1.0f, 1.5f));
                d.noGravity = true;
            }
            
            for (int i = 0; i < 20; i++)
            {
                float ang = MathHelper.TwoPi * i / 20f;
                Vector2 spd = new Vector2(1f, 0f).RotatedBy(ang) * Main.rand.NextFloat(3.5f, 6f);
                var d = Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, spd, 120, new Color(120, 210, 255), Main.rand.NextFloat(1.0f, 1.3f));
                d.noGravity = true;
            }

            
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(8f, 12f);
                    int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, v, ModContent.ProjectileType<AbyssalMiniShark>(), (int)(Projectile.damage * 0.6f), Projectile.knockBack + 1f, Projectile.owner);
                    if (p >= 0 && p < Main.maxProjectiles)
                    {
                        Main.projectile[p].CritChance = Main.player[Projectile.owner].GetWeaponCrit(Main.player[Projectile.owner].HeldItem) + 6;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            
            Color head = new Color(180, 230, 255, 0) * 0.55f;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, head, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0f);
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float t = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color c = new Color(100, 170, 255, 0) * (t * 0.6f);
                float rot = Projectile.oldRot[i];
                float scale = Projectile.scale * (0.9f + t * 0.1f);
                Main.spriteBatch.Draw(tex, pos, null, c, rot, origin, scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }

    public class AbyssalMiniShark : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false; 
            Projectile.ignoreWater = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            
            if (Projectile.timeLeft < 100)
                Projectile.tileCollide = true;

            NPC target = FindTarget(500f);
            if (target != null)
            {
                Vector2 to = target.Center - Projectile.Center;
                float speed = 14f;
                Vector2 desired = to.SafeNormalize(Vector2.UnitX) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.15f);
            }
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                var d = Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, Projectile.velocity * 0.05f, 120, new Color(110, 200, 255), 1.0f);
                d.noGravity = true;
            }
            if (Projectile.timeLeft == 100)
            {
                
                for (int i = 0; i < 10; i++)
                {
                    float ang = MathHelper.TwoPi * i / 10f;
                    Vector2 spd = new Vector2(1f, 0f).RotatedBy(ang) * Main.rand.NextFloat(1.8f, 3f);
                    var d2 = Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, spd, 120, new Color(100, 180, 255), 1.0f);
                    d2.noGravity = true;
                }
            }
        }

        private NPC FindTarget(float maxDist)
        {
            NPC best = null;
            float bestSq = maxDist * maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy(this))
                {
                    float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d < bestSq)
                    {
                        bestSq = d;
                        best = n;
                    }
                }
            }
            return best;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float t = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color c = new Color(100, 180, 255, 0) * (t * 0.55f);
                float rot = Projectile.oldRot[i];
                float scale = Projectile.scale * (0.9f + t * 0.12f);
                Main.spriteBatch.Draw(tex, pos, null, c, rot, origin, scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}
