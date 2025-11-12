using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class GaleCrescentFinisherProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1; 
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 2;
        }

        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            
            if (Projectile.ai[0] == 0f)
            {
                Projectile.velocity *= 1.02f;
                if (Projectile.timeLeft <= 40)
                {
                    Projectile.ai[0] = 1f;
                }
            }
            else
            {
                Vector2 toOwner = owner.MountedCenter - Projectile.Center;
                float speed = 18f;
                float turn = 0.2f;
                Vector2 desired = toOwner.SafeNormalize(Vector2.UnitX) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, turn);

                
                if (toOwner.LengthSquared() < 40f * 40f)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            
            Lighting.AddLight(Projectile.Center, 0.25f, 0.45f, 0.8f);
            if (Main.rand.NextBool(2))
            {
                Vector2 dv = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10)) * 0.2f;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Clentaminator_Blue, dv, 120, new Color(180, 235, 255), Main.rand.NextFloat(1.0f, 1.35f));
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
                float t = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color c = new Color(170, 220, 255, 0) * (t * 0.7f);
                float rot = Projectile.oldRot[i];
                float scale = Projectile.scale * (0.9f + t * 0.15f);
                Main.spriteBatch.Draw(texture, pos, null, c, rot, origin, scale, SpriteEffects.None, 0f);
            }

            return true;
        }

        public override void OnKill(int timeLeft)
        {
            
            for (int i = 0; i < 24; i++)
            {
                Vector2 spd = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45)) * Main.rand.NextFloat(0.4f, 1.2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Clentaminator_Blue, spd, 110, new Color(185, 240, 255), Main.rand.NextFloat(1.0f, 1.4f));
                d.noGravity = true;
            }
        }
    }
}

