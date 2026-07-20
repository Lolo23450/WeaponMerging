using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Systems
{
    public class OrbFusionSystem : ModSystem
    {
        private const float FusionRange = 42f;

        public override void PostUpdateProjectiles()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile first = Main.projectile[i];
                if (!CanFuse(first))
                    continue;

                OrbElement firstElement = GetElement(first.type);
                if (firstElement == OrbElement.None)
                    continue;

                for (int j = i + 1; j < Main.maxProjectiles; j++)
                {
                    Projectile second = Main.projectile[j];
                    if (!CanFuse(second) || second.owner != first.owner)
                        continue;

                    OrbElement secondElement = GetElement(second.type);
                    if (secondElement == OrbElement.None || secondElement == firstElement)
                        continue;

                    int fusionType = GetFusionType(firstElement, secondElement);
                    if (fusionType == 0)
                        continue;

                    if (Vector2.DistanceSquared(first.Center, second.Center) > FusionRange * FusionRange)
                        continue;

                    CreateFusion(first, second, fusionType);
                    return;
                }
            }
        }

        private static bool CanFuse(Projectile projectile)
        {
            return projectile.active
                && projectile.owner >= 0
                && projectile.owner < Main.maxPlayers
                && projectile.ai[1] == 0f
                && GetElement(projectile.type) != OrbElement.None;
        }

        private static OrbElement GetElement(int projectileType)
        {
            if (projectileType == ModContent.ProjectileType<InfernoOrbProjectile>())
                return OrbElement.Inferno;

            if (projectileType == ModContent.ProjectileType<ShadowOrbProjectile>())
                return OrbElement.Shadow;

            if (projectileType == ModContent.ProjectileType<StarlitOrbProjectile>())
                return OrbElement.Starlit;

            return OrbElement.None;
        }

        private static int GetFusionType(OrbElement first, OrbElement second)
        {
            bool Has(OrbElement element) => first == element || second == element;

            if (Has(OrbElement.Inferno) && Has(OrbElement.Shadow))
                return ModContent.ProjectileType<ChaosOrbProjectile>();

            if (Has(OrbElement.Starlit) && Has(OrbElement.Shadow))
                return ModContent.ProjectileType<StarlitShadowFusionOrbProjectile>();

            if (Has(OrbElement.Starlit) && Has(OrbElement.Inferno))
                return ModContent.ProjectileType<StarlitInfernoFusionOrbProjectile>();

            return 0;
        }

        private static void CreateFusion(Projectile first, Projectile second, int fusionType)
        {
            Vector2 fusionPosition = (first.Center + second.Center) * 0.5f;
            int baseDamage = System.Math.Max(first.damage, second.damage);
            if (baseDamage <= 0 && first.owner >= 0 && first.owner < Main.maxPlayers)
            {
                baseDamage = (int)(Main.player[first.owner].HeldItem.damage * 0.75f);
            }

            int fusion = Projectile.NewProjectile(
                first.GetSource_FromThis(),
                fusionPosition,
                Vector2.Zero,
                fusionType,
                System.Math.Max(20, baseDamage * 2),
                System.Math.Max(first.knockBack, second.knockBack),
                first.owner
            );

            if (fusion >= 0 && fusion < Main.maxProjectiles)
            {
                Main.projectile[fusion].ai[0] = 0f;
                Main.projectile[fusion].ai[1] = 0f;
                Main.projectile[fusion].localAI[0] = 0f;
                Main.projectile[fusion].netUpdate = true;
            }

            for (int i = 0; i < 28; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.6f, 1.2f);
                Dust dust = Dust.NewDustPerfect(fusionPosition, DustID.RainbowMk2, velocity, 100, Color.White, Main.rand.NextFloat(1.2f, 1.9f));
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.85f, Pitch = 0.25f }, fusionPosition);
            first.Kill();
            second.Kill();
        }

        private enum OrbElement
        {
            None,
            Inferno,
            Shadow,
            Starlit
        }
    }
}
