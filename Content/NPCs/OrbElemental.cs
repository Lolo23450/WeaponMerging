using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.NPCs
{
    public class OrbElemental : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;

            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 1f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            NPC.damage = 25;
            NPC.defense = 10;
            NPC.lifeMax = 150;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 500f;
            NPC.aiStyle = -1; 
            NPC.noGravity = true;
            NPC.noTileCollide = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("A floating orb of fused energies, drawn to ancient ruins where weapon fusions once occurred.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneRockLayerHeight || spawnInfo.Player.ZoneDirtLayerHeight)
            {
                
                return ModContent.GetInstance<Biomes.FusionRuinsBiomeTileCount>().FusionRuinsTiles >= 40 ? 0.1f : 0f;
            }
            return 0f;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);

            Player player = Main.player[NPC.target];
            Vector2 toPlayer = player.Center - NPC.Center;
            float distance = toPlayer.Length();

            if (distance > 300f)
            {
                
                NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer.SafeNormalize(Vector2.Zero) * 2f, 0.05f);
            }
            else if (distance < 100f)
            {
                
                NPC.velocity = Vector2.Lerp(NPC.velocity, -toPlayer.SafeNormalize(Vector2.Zero) * 1.5f, 0.05f);
            }
            else
            {
                
                NPC.velocity *= 0.95f;

                if (NPC.ai[0]++ > 120) 
                {
                    NPC.ai[0] = 0;
                    Vector2 direction = toPlayer.SafeNormalize(Vector2.Zero);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 5f,
                        ModContent.ProjectileType<Projectiles.OrbBlast>(), NPC.damage / 2, 0f, Main.myPlayer);
                    SoundEngine.PlaySound(SoundID.Item15, NPC.position);
                }
            }

            
            NPC.frameCounter++;
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += NPC.frame.Height;
                if (NPC.frame.Y >= NPC.frame.Height * Main.npcFrameCount[Type])
                    NPC.frame.Y = 0;
            }

            
            Lighting.AddLight(NPC.Center, 0.3f, 0.4f, 0.5f);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.OrbFragment>(), 1, 1, 3));
            npcLoot.Add(ItemDropRule.Common(3, 2, 1, 5));
        }
    }
}

