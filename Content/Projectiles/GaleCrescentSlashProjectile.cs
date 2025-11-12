using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponMerging.Content.Players;

namespace WeaponMerging.Content.Projectiles
{
    public class GaleCrescentSlashProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 22;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            
            float t = 1f - (Projectile.timeLeft / 22f);
            float ease = MathHelper.SmoothStep(0f, 1f, t);

            
            float swaySign = (Projectile.ai[0] % 2 == 0) ? 1f : -1f;
            Vector2 along = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = along.RotatedBy(MathHelper.PiOver2 * swaySign);
            float curve = (float)System.Math.Sin(ease * System.Math.PI) * 1.4f; 
            Projectile.position += perp * curve;

            
            var stacks = Main.player[Projectile.owner].GetModPlayer<GaleCrescentPlayer>().galeStacks;
            float stackAmp = 1f + stacks * 0.03f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.scale = MathHelper.Lerp(1.25f, 0.85f, ease) * stackAmp;

            Lighting.AddLight(Projectile.Center, 0.2f, 0.35f, 0.6f);

            if (Main.rand.NextBool())
            {
                Vector2 dustVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10)) * 0.18f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + perp * Main.rand.NextFloat(-8f, 8f), DustID.Clentaminator_Blue, dustVel, 120, new Color(170, 220, 255), Main.rand.NextFloat(0.9f, 1.2f));
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
                Color c = new Color(170, 220, 255, 0) * (alphaT * 0.6f);
                float rot = Projectile.oldRot[i];
                float scale = Projectile.scale * (0.9f + 0.1f * alphaT);
                Main.spriteBatch.Draw(texture, pos, null, c, rot, origin, scale, SpriteEffects.None, 0f);
            }

            return true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            
            Rectangle hitbox = Projectile.Hitbox;
            hitbox.Inflate(14, 14);
            return hitbox.Intersects(target.Hitbox);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            var mp = Main.player[Projectile.owner].GetModPlayer<GaleCrescentPlayer>();
            mp.AddStack();

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 0.8f), 140, new Color(180, 230, 255), 1.1f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 spd = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(25)) * Main.rand.NextFloat(0.3f, 1.1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Clentaminator_Blue, spd, 120, new Color(150, 210, 255), Main.rand.NextFloat(0.9f, 1.3f));
                d.noGravity = true;
            }
        }
    }
}
