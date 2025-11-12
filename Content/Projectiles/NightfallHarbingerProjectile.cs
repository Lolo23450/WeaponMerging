using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class NightfallHarbingerProjectile : ModProjectile
    {
        private const int MAX_LIFETIME = 120; 
        private const float RETURN_SPEED = 18f;
        private const float OUTWARD_SPEED = 16f;
        private const float CURVE_STRENGTH = 0.2f;
        
        private bool returning = false;
        private float rotationSpeed = 0.4f;
        
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = MAX_LIFETIME;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            
            if (Main.rand.NextBool(1)) 
            {
                int dustType = Main.rand.Next(new int[] { 27, 173, 62, 64 }); 
                Dust dust = Dust.NewDustDirect(Projectile.Center - Vector2.One * 10, 20, 20, dustType, 0f, 0f, 100, default, 1.8f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.2f;
                dust.fadeIn = 1.3f;
            }
            
            
            if (Main.rand.NextBool(3))
            {
                Dust glow = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 
                    269, 
                    Projectile.velocity * 0.1f, 
                    100, 
                    new Color(150, 50, 200), 
                    1.5f);
                glow.noGravity = true;
            }

            Projectile.ai[0]++;
            
            
            if (Projectile.ai[0] > MAX_LIFETIME * 0.6f && !returning)
            {
                returning = true;
                Projectile.netUpdate = true;
                
                
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.4f, Pitch = -0.5f }, Projectile.position);
            }

            Player owner = Main.player[Projectile.owner];
            
            if (returning)
            {
                
                Vector2 directionToOwner = owner.Center - Projectile.Center;
                float distance = directionToOwner.Length();
                
                if (distance < 50f)
                {
                    Projectile.Kill();
                    return;
                }
                
                directionToOwner.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToOwner * RETURN_SPEED, 0.12f);
                
                
                rotationSpeed = 0.8f;
                
                
                if (Main.rand.NextBool(2))
                {
                    Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 27, 0f, 0f, 100, default, 2.0f);
                    trail.noGravity = true;
                    trail.velocity *= 0.1f;
                }
            }
            else
            {
                
                float curveDirection = Projectile.ai[1]; 
                
                
                Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
                perpendicular.Normalize();
                Projectile.velocity += perpendicular * CURVE_STRENGTH * curveDirection;
                
                
                float detectionRange = 300f;
                NPC closest = null;
                float closestDist = detectionRange;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closest = npc;
                            closestDist = dist;
                        }
                    }
                }
                
                if (closest != null)
                {
                    Vector2 toEnemy = closest.Center - Projectile.Center;
                    toEnemy.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toEnemy * OUTWARD_SPEED, 0.03f);
                }
                
                
                if (Projectile.velocity.Length() > OUTWARD_SPEED)
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= OUTWARD_SPEED;
                }
            }
            
            
            Projectile.rotation += rotationSpeed * (Projectile.velocity.X > 0 ? 1 : -1);
            
            
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.2f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, 0.7f * pulse, 0.3f * pulse, 1.0f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            SoundEngine.PlaySound(SoundID.NPCHit54 with { Volume = 0.6f, Pitch = -0.2f }, Projectile.position);
            
            for (int i = 0; i < 20; i++)
            {
                int dustType = Main.rand.Next(new int[] { 27, 173, 62, 264 });
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 2.0f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ProjectileID.ShadowBeamHostile, 
                    (int)(Projectile.damage * 0.3f),
                    0f,
                    Projectile.owner
                );
            }
            
            
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.DemonScythe].Value;
            Rectangle sourceRect = texture.Frame(1, 1, 0, 0);
            Vector2 origin = sourceRect.Size() / 2f;
            
            
            for (int i = 0; i < 5; i++)
            {
                float progress = i / 5f;
                Vector2 afterimagePos = Projectile.Center - Projectile.velocity * (i * 2f) - Main.screenPosition;
                float afterimageAlpha = (1f - progress) * 0.4f;
                Color afterimageColor = new Color(120, 60, 180) * afterimageAlpha;
                
                Main.EntitySpriteDraw(
                    texture,
                    afterimagePos,
                    sourceRect,
                    afterimageColor,
                    Projectile.rotation - (i * 0.1f),
                    origin,
                    Projectile.scale * (1f - progress * 0.2f),
                    SpriteEffects.None,
                    0
                );
            }
            
            
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            
            Color mainColor = returning ? 
                new Color(200, 100, 255, 255) : 
                new Color(220, 140, 255, 255);
            
            
            mainColor = Color.Lerp(mainColor, lightColor, 0.5f);
            
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                sourceRect,
                mainColor,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.2f, 
                SpriteEffects.None,
                0
            );
            
            
            Color glowColor = new Color(150, 80, 255, 0) * 0.8f;
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                sourceRect,
                glowColor,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.4f,
                SpriteEffects.None,
                0
            );
            
            return false;
        }
        
        public override void Kill(int timeLeft)
        {
            
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.position);
            
            for (int i = 0; i < 30; i++)
            {
                int dustType = Main.rand.Next(new int[] { 27, 173, 62 });
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 2.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
            
            
            for (int i = 0; i < 3; i++)
            {
                Dust glow = Dust.NewDustPerfect(Projectile.Center, 269, Main.rand.NextVector2Circular(4, 4), 100, new Color(180, 100, 255), 2f);
                glow.noGravity = true;
            }
        }
    }
}
