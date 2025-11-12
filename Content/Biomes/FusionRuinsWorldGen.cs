using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace WeaponMerging.Content.Biomes
{
    public class PerlinNoise
    {
        private int[] permutation;
        private Vector2[] gradients;

        public PerlinNoise(int seed = 0)
        {
            
            permutation = new int[512];
            gradients = new Vector2[512];

            
            int[] p = new int[256];
            for (int i = 0; i < 256; i++)
                p[i] = i;

            
            Random rand = new Random(seed);
            for (int i = 0; i < 256; i++)
            {
                int j = rand.Next(256);
                int temp = p[i];
                p[i] = p[j];
                p[j] = temp;
            }

            
            for (int i = 0; i < 512; i++)
            {
                permutation[i] = p[i % 256];
                
                float angle = (float)(rand.NextDouble() * Math.PI * 2);
                gradients[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }
        }

        private float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        private float DotGridGradient(int ix, int iy, float x, float y)
        {
            int index = permutation[(ix + permutation[iy % 256]) % 256];
            Vector2 gradient = gradients[index];
            float dx = x - ix;
            float dy = y - iy;
            return dx * gradient.X + dy * gradient.Y;
        }

        public float Noise(float x, float y)
        {
            
            int x0 = (int)Math.Floor(x);
            int x1 = x0 + 1;
            int y0 = (int)Math.Floor(y);
            int y1 = y0 + 1;

            
            float sx = Fade(x - x0);
            float sy = Fade(y - y0);

            
            float n0 = DotGridGradient(x0, y0, x, y);
            float n1 = DotGridGradient(x1, y0, x, y);
            float ix0 = Lerp(n0, n1, sx);

            n0 = DotGridGradient(x0, y1, x, y);
            n1 = DotGridGradient(x1, y1, x, y);
            float ix1 = Lerp(n0, n1, sx);

            return Lerp(ix0, ix1, sy);
        }

        public float OctaveNoise(float x, float y, int octaves, float persistence = 0.5f, float scale = 1f)
        {
            float value = 0;
            float amplitude = 1;
            float frequency = scale;

            for (int i = 0; i < octaves; i++)
            {
                value += Noise(x * frequency, y * frequency) * amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }

            return value;
        }
    }

    public class FusionRuinsWorldGen : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            
            tasks.Add(new FusionRuinsGenPass("Fusion Ruins", 100f));
        }
    }

    public class FusionRuinsGenPass : GenPass
    {
        public FusionRuinsGenPass(string name, float loadWeight) : base(name, loadWeight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            try
            {
                
                int radius = 250; 
                int x = Main.maxTilesX / 2;
                
                int y = (int)(Main.worldSurface + Main.worldSurface * 1.0f); 

                ModContent.GetInstance<WeaponMerging>().Logger.Info($"Fusion Ruins: Starting massive cave generation at ({x}, {y}) with radius {radius}");

                List<(int x, int y)> islandCenters = new List<(int x, int y)>();

                
                int[] chamberSizes = { 105, 84, 96, 75, 90, 81, 99, 78 }; 
                for (int chamber = 0; chamber < chamberSizes.Length; chamber++)
                {
                    int chamberRadius = chamberSizes[chamber];
                    double angle = (chamber * 2 * Math.PI) / chamberSizes.Length;
                    double angleOffset = WorldGen.genRand.NextDouble() * 0.5 - 0.25; 
                    
                    int chamberX = x + (int)(Math.Cos(angle + angleOffset) * WorldGen.genRand.Next(105, 165)); 
                    int chamberY = y + (int)(Math.Sin(angle + angleOffset) * WorldGen.genRand.Next(105, 165)); 

                    
                    CreateCaveChamber(chamberX, chamberY, chamberRadius, chamber < 4); 

                    islandCenters.Add((chamberX, chamberY));
                }

                
                for (int smallCave = 0; smallCave < 25; smallCave++) 
                {
                    int caveX = x + WorldGen.genRand.Next(-210, 210); 
                    int caveY = y + WorldGen.genRand.Next(-210, 210); 
                    int caveSize = WorldGen.genRand.Next(35, 50); 

                    
                    double distFromCenter = Math.Sqrt((caveX - x) * (caveX - x) + (caveY - y) * (caveY - y));
                    if (distFromCenter > 75 && distFromCenter < 240) 
                    {
                        CreateCaveChamber(caveX, caveY, caveSize, smallCave < 6); 

                        islandCenters.Add((caveX, caveY));
                    }
                }

                
                CreateNaturalTunnelNetwork(x, y, 25); 

                
                CreateCentralCaveHub(x, y);

                islandCenters.Add((x, y));

                
                CreateCaveBrickCovers(x, y, radius);

                
                AddCaveDecorations(x, y, 40, radius - 10);

                
                CreateVineConnections(islandCenters);

                
                for (int smoothX = x - radius - 25; smoothX <= x + radius + 25; smoothX++)
                {
                    for (int smoothY = y - radius - 25; smoothY <= y + radius + 25; smoothY++)
                    {
                        if (smoothX >= 0 && smoothX < Main.maxTilesX && smoothY >= 0 && smoothY < Main.maxTilesY)
                        {
                            WorldGen.SquareTileFrame(smoothX, smoothY);
                            WorldGen.SquareWallFrame(smoothX, smoothY);
                        }
                    }
                }

                ModContent.GetInstance<WeaponMerging>().Logger.Info("Fusion Ruins: Natural cave complex generation successful");
                return;
            }
            catch (Exception e)
            {
                ModContent.GetInstance<WeaponMerging>().Logger.Error($"Fusion Ruins: Error during cave generation: {e.Message}");
                return;
            }
        }

        private void CreateChamberBrickCovers(int centerX, int centerY, int baseRadius)
        {
            PerlinNoise coverNoise = new PerlinNoise(WorldGen.genRand.Next() + 2000);

            
            for (int cover = 0; cover < WorldGen.genRand.Next(3, 6); cover++)
            {
                
                double angle = (cover * Math.PI * 2) / WorldGen.genRand.Next(3, 6) + WorldGen.genRand.NextDouble() * 0.5;
                double distance = baseRadius + WorldGen.genRand.Next(3, 8);
                int coverCenterX = centerX + (int)(Math.Cos(angle) * distance);
                int coverCenterY = centerY + (int)(Math.Sin(angle) * distance);

                int coverSize = WorldGen.genRand.Next(4, 8); 

                for (int i = coverCenterX - coverSize; i <= coverCenterX + coverSize; i++)
                {
                    for (int j = coverCenterY - coverSize; j <= coverCenterY + coverSize; j++)
                    {
                        if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                        {
                            double dist = Math.Sqrt((i - coverCenterX) * (i - coverCenterX) + (j - coverCenterY) * (j - coverCenterY));

                            
                            if (dist <= coverSize - WorldGen.genRand.Next(1, 3))
                            {
                                
                                float caveCheckNoise = coverNoise.OctaveNoise(i * 0.02f, j * 0.02f, 4, 0.6f);
                                double caveDist = Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                                double caveNormalizedDist = caveDist / (baseRadius + 5);
                                float caveThreshold = 0.3f + (float)caveNormalizedDist * 0.4f;

                                
                                if (caveCheckNoise <= caveThreshold)
                                {
                                    
                                    if (j + 1 < Main.maxTilesY && Main.tile[i, j + 1].HasTile)
                                    {
                                        WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsBrick>());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateCaveChamber(int centerX, int centerY, int baseRadius, bool addAltar)
        {
            
            PerlinNoise perlin = new PerlinNoise(WorldGen.genRand.Next());

            for (int i = centerX - baseRadius - 10; i < centerX + baseRadius + 10; i++)
            {
                for (int j = centerY - baseRadius - 10; j < centerY + baseRadius + 10; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        
                        float noiseScale = 0.02f; 
                        float caveNoise = perlin.OctaveNoise(i * noiseScale, j * noiseScale, 4, 0.6f);

                        
                        double dist = Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                        double normalizedDist = dist / (baseRadius + 5);

                        
                        float caveThreshold = 0.3f + (float)normalizedDist * 0.4f; 

                        if (caveNoise > caveThreshold)
                        {
                            
                            WorldGen.KillTile(i, j, false, false, true);
                            WorldGen.KillWall(i, j);
                        }

                    }
                }
            }

            
            CreateChamberBrickCovers(centerX, centerY, baseRadius);

            
            if (addAltar)
            {
                
                for (int i = centerX - 2; i <= centerX + 2; i++)
                {
                    for (int j = centerY - 2; j <= centerY + 2; j++)
                    {
                        if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                        {
                            WorldGen.KillTile(i, j, false, false, true);
                            WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsBrick>());
                        }
                    }
                }
                WorldGen.KillTile(centerX, centerY, false, false, true);
                WorldGen.PlaceTile(centerX, centerY, ModContent.TileType<Tiles.FusionAltar>());
            }
        }

        private void CreateNaturalTunnelNetwork(int centerX, int centerY, int numTunnels)
        {
            List<(int x, int y)> connectionPoints = new List<(int x, int y)>();

            
            for (int i = 0; i < numTunnels; i++)
            {
                double angle = (i * 2 * Math.PI) / numTunnels;
                double distance = WorldGen.genRand.Next(60, 180); 
                int pointX = centerX + (int)(Math.Cos(angle) * distance);
                int pointY = centerY + (int)(Math.Sin(angle) * distance);
                connectionPoints.Add((pointX, pointY));
            }

            
            for (int i = 0; i < connectionPoints.Count; i++)
            {
                for (int j = i + 1; j < Math.Min(i + 4, connectionPoints.Count); j++) 
                {
                    var start = connectionPoints[i];
                    var end = connectionPoints[j];
                    CreateCaveTunnel(start.x, start.y, end.x, end.y, WorldGen.genRand.Next(6, 10)); 
                }
            }

            
            for (int i = 0; i < 12; i++) 
            {
                double angle = (i * Math.PI * 2) / 12; 
                int endX = centerX + (int)(Math.Cos(angle) * 320); 
                int endY = centerY + (int)(Math.Sin(angle) * 320);
                CreateCaveTunnel(centerX, centerY, endX, endY, WorldGen.genRand.Next(5, 9)); 
            }
        }

        private void CreateCentralCaveHub(int centerX, int centerY)
        {
            
            PerlinNoise perlin = new PerlinNoise(WorldGen.genRand.Next());
            int hubRadius = 100; 

            for (int i = centerX - hubRadius - 30; i < centerX + hubRadius + 30; i++) 
            {
                for (int j = centerY - hubRadius - 30; j < centerY + hubRadius + 30; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        
                        float cavernNoise = perlin.OctaveNoise(i * 0.01f, j * 0.01f, 6, 0.6f, 1.3f); 

                        double dist = Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                        double normalizedDist = dist / (hubRadius + 12); 

                        
                        float cavernThreshold = 0.15f + (float)normalizedDist * 0.6f + cavernNoise * 0.25f;

                        if (cavernNoise > cavernThreshold)
                        {
                            
                            WorldGen.KillTile(i, j, false, false, true);
                            WorldGen.KillWall(i, j);
                        }

                    }
                }
            }

            
            for (int i = centerX - 12; i <= centerX + 12; i++) 
            {
                for (int j = centerY - 12; j <= centerY + 12; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        double dist = Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                        if (dist <= 9) 
                        {
                            WorldGen.KillTile(i, j, false, false, true);
                            WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsLight>());
                        }
                    }
                }
            }
            WorldGen.KillTile(centerX, centerY, false, false, true);
            WorldGen.PlaceTile(centerX, centerY, ModContent.TileType<Tiles.FusionAltar>());
        }

        private void CreateCaveBrickCovers(int centerX, int centerY, int radius)
        {
            PerlinNoise perlin = new PerlinNoise(WorldGen.genRand.Next());

            
            
            int xOffset = WorldGen.genRand.Next(50, 100); 
            int effectiveCenterX = centerX + xOffset;

            
            int processingRadius = radius + 50;

            for (int i = effectiveCenterX - processingRadius; i <= effectiveCenterX + processingRadius; i++)
            {
                for (int j = centerY - processingRadius; j <= centerY + processingRadius; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        
                        if (j < Main.worldSurface + 50) continue; 

                        
                        float noiseScale = 0.02f; 
                        float wallNoise = perlin.OctaveNoise(i * noiseScale, j * noiseScale, 4, 0.6f);

                        
                        double dist = Math.Sqrt((i - effectiveCenterX) * (i - effectiveCenterX) + (j - centerY) * (j - centerY));
                        double normalizedDist = dist / (radius + 30);

                        
                        float wallThreshold = 0.3f + (float)normalizedDist * 0.4f;

                    }
                }
            }
        }

        private void CreateCaveTunnel(int startX, int startY, int endX, int endY, int width)
        {
            PerlinNoise perlin = new PerlinNoise(WorldGen.genRand.Next());
            int dx = Math.Abs(endX - startX);
            int dy = Math.Abs(endY - startY);
            int sx = startX < endX ? 1 : -1;
            int sy = startY < endY ? 1 : -1;
            int err = dx - dy;

            int currentX = startX;
            int currentY = startY;

            while (true)
            {
                
                int tunnelWidth = width + WorldGen.genRand.Next(-1, 2); 

                for (int i = currentX - tunnelWidth - 3; i <= currentX + tunnelWidth + 3; i++)
                {
                    for (int j = currentY - tunnelWidth - 3; j <= currentY + tunnelWidth + 3; j++)
                    {
                        if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                        {
                            
                            float tunnelNoise = perlin.OctaveNoise(i * 0.03f, j * 0.03f, 3, 0.7f);

                            double dist = Math.Sqrt((i - currentX) * (i - currentX) + (j - currentY) * (j - currentY));
                            double effectiveDist = dist + tunnelNoise * tunnelWidth * 0.6f;

                            if (effectiveDist <= tunnelWidth + 1)
                            {
                                WorldGen.KillTile(i, j, false, false, true);
                                WorldGen.KillWall(i, j);

                            }
                        }
                    }
                }

                if (currentX == endX && currentY == endY) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    currentX += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    currentY += sy;
                }
            }
        }

        private void AddCaveDecorations(int centerX, int centerY, int numElements, int maxDistance)
        {
            PerlinNoise perlin = new PerlinNoise(WorldGen.genRand.Next());

            
            int decorationRadius = maxDistance;

            for (int i = centerX - decorationRadius; i <= centerX + decorationRadius; i++)
            {
                for (int j = centerY - decorationRadius; j <= centerY + decorationRadius; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        
                        double distFromCenter = Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));

                        
                        if (distFromCenter <= decorationRadius)
                        {
                            
                            float noiseScale = 0.02f; 
                            float formationNoise = perlin.OctaveNoise(i * noiseScale, j * noiseScale, 4, 0.6f);

                            
                            double normalizedDist = distFromCenter / (decorationRadius + 5);

                            
                            float caveThreshold = 0.3f + (float)normalizedDist * 0.4f;

                        }
                    }
                }
            }
        }

        private void CreateVineConnections(List<(int x, int y)> centers)
        {
            
            for (int i = 0; i < centers.Count; i++)
            {
                for (int j = i + 1; j < centers.Count; j++)
                {
                    var start = centers[i];
                    var end = centers[j];
                    double dist = Math.Sqrt((start.x - end.x) * (start.x - end.x) + (start.y - end.y) * (start.y - end.y));
                    if (dist < 180) 
                    {
                        DrawCurvedVineLine(start.x, start.y, end.x, end.y);
                    }
                }
            }
        }

        private void DrawCurvedVineLine(int x1, int y1, int x2, int y2)
        {
            
            int midX = (x1 + x2) / 2;
            int midY = (y1 + y2) / 2;
            
            int dx = x2 - x1;
            int dy = y2 - y1;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0) return; 
            int offsetX = (int)(-dy / length * 30); 
            int offsetY = (int)(dx / length * 30);
            midX += offsetX;
            midY += offsetY;

            
            int steps = 50;
            for (int t = 0; t <= steps; t++)
            {
                double tt = (double)t / steps;
                int px = (int)((1 - tt) * (1 - tt) * x1 + 2 * (1 - tt) * tt * midX + tt * tt * x2);
                int py = (int)((1 - tt) * (1 - tt) * y1 + 2 * (1 - tt) * tt * midY + tt * tt * y2);

                
                for (int ix = px - 2; ix <= px + 2; ix++)
                {
                    for (int iy = py - 2; iy <= py + 2; iy++)
                    {
                        if (ix >= 0 && ix < Main.maxTilesX && iy >= 0 && iy < Main.maxTilesY)
                        {
                            double d = Math.Sqrt((ix - px) * (ix - px) + (iy - py) * (iy - py));
                            if (d <= 2.5)
                            {
                                
                                if (!Main.tile[ix, iy].HasTile)
                                {
                                    WorldGen.PlaceWall(ix, iy, 60);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

