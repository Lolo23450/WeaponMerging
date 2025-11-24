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

                
                islandCenters.Add((x, y));

                
                CreateNaturalTunnelNetwork(x, y, islandCenters);

                
                CreateCentralCaveHub(x, y);

                
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

            
            for (int cover = 0; cover < WorldGen.genRand.Next(5, 10); cover++)
            {
                
                double angle = (cover * Math.PI * 2) / WorldGen.genRand.Next(5, 10) + WorldGen.genRand.NextDouble() * 0.3;
                double distance = baseRadius + WorldGen.genRand.Next(5, 12);
                int coverCenterX = centerX + (int)(Math.Cos(angle) * distance);
                int coverCenterY = centerY + (int)(Math.Sin(angle) * distance);

                int coverWidth = WorldGen.genRand.Next(2, 5);
                int coverHeight = WorldGen.genRand.Next(3, 8);
                bool isVertical = WorldGen.genRand.NextBool();

                for (int i = coverCenterX - coverWidth; i <= coverCenterX + coverWidth; i++)
                {
                    for (int j = coverCenterY - coverHeight; j <= coverCenterY + coverHeight; j++)
                    {
                        if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                        {
                            double distX = Math.Abs(i - coverCenterX);
                            double distY = Math.Abs(j - coverCenterY);
                            
                            bool inShape = (isVertical && distX <= coverWidth && distY <= coverHeight) || 
                                          (!isVertical && distX <= coverWidth && distY <= coverHeight);

                            if (inShape && WorldGen.genRand.NextFloat() < 0.7f)
                            {
                                
                                float caveCheckNoise = coverNoise.OctaveNoise(i * 0.02f, j * 0.02f, 4, 0.6f);
                                double caveDist = Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                                double caveNormalizedDist = caveDist / (baseRadius + 5);
                                float caveThreshold = 0.3f + (float)caveNormalizedDist * 0.4f;

                                
                                if (caveCheckNoise <= caveThreshold && j + 1 < Main.maxTilesY && Main.tile[i, j + 1].HasTile)
                                {
                                    WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsBrick>());
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
            int chamberType = WorldGen.genRand.Next(4); 

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

                        switch (chamberType)
                        {
                            case 0: 
                                caveThreshold *= 0.8f;
                                break;
                            case 1: 
                                double angle = Math.Atan2(j - centerY, i - centerX);
                                double ovalFactor = 1.0 + 0.3 * Math.Sin(angle * 2);
                                caveThreshold *= (float)ovalFactor;
                                break;
                            case 2: 
                                if (dist < baseRadius * 0.7)
                                {
                                    caveThreshold *= 0.9f;
                                }
                                break;
                            case 3: 
                                double angle3 = Math.Atan2(j - centerY, i - centerX);
                                double lobeFactor = 1.0 + 0.4 * Math.Sin(angle3 * 3);
                                caveThreshold *= (float)lobeFactor;
                                break;
                        }

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

        private void CreateNaturalTunnelNetwork(int centerX, int centerY, List<(int x, int y)> potentialNodes)
        {
            if (potentialNodes == null || potentialNodes.Count == 0)
            {
                return;
            }

            List<(int x, int y)> uniqueNodes = new List<(int x, int y)>();
            foreach (var node in potentialNodes)
            {
                if (!uniqueNodes.Contains(node))
                {
                    uniqueNodes.Add(node);
                }
            }

            (int x, int y) center = (centerX, centerY);
            if (!uniqueNodes.Contains(center))
            {
                uniqueNodes.Add(center);
            }

            List<(int x, int y)> peripheralNodes = new List<(int x, int y)>();
            foreach (var node in uniqueNodes)
            {
                if (node != center)
                {
                    peripheralNodes.Add(node);
                }
            }

            foreach (var node in peripheralNodes)
            {
                int width = WorldGen.genRand.Next(6, 10);
                CreateCaveTunnel(node.x, node.y, centerX, centerY, width);
            }

            if (peripheralNodes.Count < 2)
            {
                return;
            }

            peripheralNodes.Sort((a, b) =>
            {
                double angleA = Math.Atan2(a.y - centerY, a.x - centerX);
                double angleB = Math.Atan2(b.y - centerY, b.x - centerX);
                return angleA.CompareTo(angleB);
            });

            for (int i = 0; i < peripheralNodes.Count - 1; i++)
            {
                var current = peripheralNodes[i];
                var next = peripheralNodes[i + 1];
                int width = WorldGen.genRand.Next(5, 8);
                CreateCaveTunnel(current.x, current.y, next.x, next.y, width);
            }

            if (peripheralNodes.Count > 2)
            {
                var first = peripheralNodes[0];
                var last = peripheralNodes[peripheralNodes.Count - 1];
                int width = WorldGen.genRand.Next(5, 8);
                CreateCaveTunnel(first.x, first.y, last.x, last.y, width);
            }
        }

        private void CreateCentralCaveHub(int centerX, int centerY)
        {
            
            PerlinNoise perlin = new PerlinNoise(WorldGen.genRand.Next());
            int hubRadius = 70; 

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

            
            for (int ring = 0; ring < 3; ring++)
            {
                int lightRadius = 6 + ring * 3;
                int numLights = 8 + ring * 4;
                for (int light = 0; light < numLights; light++)
                {
                    double angle = (light * Math.PI * 2) / numLights + WorldGen.genRand.NextDouble() * 0.3;
                    double distance = 8 + ring * 5 + WorldGen.genRand.Next(-2, 3);
                    int lightX = centerX + (int)(Math.Cos(angle) * distance);
                    int lightY = centerY + (int)(Math.Sin(angle) * distance);

                    for (int i = lightX - lightRadius; i <= lightX + lightRadius; i++)
                    {
                        for (int j = lightY - lightRadius; j <= lightY + lightRadius; j++)
                        {
                            if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                            {
                                double lightDist = Math.Sqrt((i - lightX) * (i - lightX) + (j - lightY) * (j - lightY));
                                if (lightDist <= lightRadius - WorldGen.genRand.Next(1, 3))
                                {
                                    WorldGen.KillTile(i, j, false, false, true);
                                    WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsLight>());
                                }
                            }
                        }
                    }
                }
            }

            
            for (int i = centerX - 4; i <= centerX + 4; i++)
            {
                for (int j = centerY - 4; j <= centerY + 4; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        double dist = Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                        if (dist <= 3)
                        {
                            WorldGen.KillTile(i, j, false, false, true);
                        }
                    }
                }
            }

            WorldGen.KillTile(centerX, centerY, false, false, true);
            WorldGen.PlaceTile(centerX, centerY, ModContent.TileType<Tiles.FusionAltar>());

            
            for (int pillar = 0; pillar < WorldGen.genRand.Next(3, 7); pillar++)
            {
                double angle = (pillar * Math.PI * 2) / WorldGen.genRand.Next(3, 7) + WorldGen.genRand.NextDouble() * 0.5;
                double distance = 15 + WorldGen.genRand.Next(5, 15);
                int pillarX = centerX + (int)(Math.Cos(angle) * distance);
                int pillarY = centerY + (int)(Math.Sin(angle) * distance);

                int pillarHeight = WorldGen.genRand.Next(4, 8);
                int pillarWidth = WorldGen.genRand.Next(1, 3);

                for (int i = pillarX - pillarWidth; i <= pillarX + pillarWidth; i++)
                {
                    for (int j = pillarY - pillarHeight; j <= pillarY + pillarHeight; j++)
                    {
                        if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                        {
                            if (!Main.tile[i, j].HasTile)
                            {
                                WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsBrick>());
                            }
                        }
                    }
                }
            }
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

                        if (wallNoise > wallThreshold && j + 1 < Main.maxTilesY && Main.tile[i, j + 1].HasTile)
                        {
                            
                            bool placeBrick = true;
                            
                            if (i > 0 && Main.tile[i - 1, j].TileType == ModContent.TileType<Tiles.FusionRuinsBrick>()) placeBrick = false;
                            if (i < Main.maxTilesX - 1 && Main.tile[i + 1, j].TileType == ModContent.TileType<Tiles.FusionRuinsBrick>()) placeBrick = false;
                            if (j > 0 && Main.tile[i, j - 1].TileType == ModContent.TileType<Tiles.FusionRuinsBrick>()) placeBrick = false;
                            if (j < Main.maxTilesY - 1 && Main.tile[i, j + 1].TileType == ModContent.TileType<Tiles.FusionRuinsBrick>()) placeBrick = false;

                            if (placeBrick && WorldGen.genRand.NextFloat() < 0.3f)
                            {
                                WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsBrick>());
                            }
                        }
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

            for (int element = 0; element < numElements; element++)
            {
                
                double angle = WorldGen.genRand.NextDouble() * Math.PI * 2;
                double distance = WorldGen.genRand.Next(20, decorationRadius);
                int decoX = centerX + (int)(Math.Cos(angle) * distance);
                int decoY = centerY + (int)(Math.Sin(angle) * distance);

                int decoType = WorldGen.genRand.Next(3); 

                switch (decoType)
                {
                    case 0: 
                        PlaceStalactite(decoX, decoY);
                        break;
                    case 1: 
                        PlaceRockFormation(decoX, decoY);
                        break;
                    case 2: 
                        PlaceGemFormation(decoX, decoY);
                        break;
                }
            }
        }

        private void PlaceStalactite(int x, int y)
        {
            int height = WorldGen.genRand.Next(3, 8);
            for (int j = 0; j < height; j++)
            {
                if (y + j >= 0 && y + j < Main.maxTilesY && x >= 0 && x < Main.maxTilesX)
                {
                    if (!Main.tile[x, y + j].HasTile && Main.tile[x, y + j + 1].HasTile)
                    {
                        WorldGen.PlaceTile(x, y + j, ModContent.TileType<Tiles.FusionRuinsBrick>());
                        break;
                    }
                }
            }
        }

        private void PlaceCrystalCluster(int x, int y)
        {
            int size = WorldGen.genRand.Next(2, 5);
            for (int i = x - size; i <= x + size; i++)
            {
                for (int j = y - size; j <= y + size; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        double dist = Math.Sqrt((i - x) * (i - x) + (j - y) * (j - y));
                        if (dist <= size && WorldGen.genRand.NextFloat() < 0.6f && !Main.tile[i, j].HasTile)
                        {
                            WorldGen.PlaceTile(i, j, TileID.Crystals);
                        }
                    }
                }
            }
        }

        private void PlaceRockFormation(int x, int y)
        {
            int width = WorldGen.genRand.Next(2, 4);
            int height = WorldGen.genRand.Next(2, 4);
            for (int i = x - width; i <= x + width; i++)
            {
                for (int j = y - height; j <= y + height; j++)
                {
                    if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
                    {
                        double distX = Math.Abs(i - x);
                        double distY = Math.Abs(j - y);
                        if (distX <= width && distY <= height && WorldGen.genRand.NextFloat() < 0.4f && !Main.tile[i, j].HasTile)
                        {
                            WorldGen.PlaceTile(i, j, ModContent.TileType<Tiles.FusionRuinsBrick>());
                        }
                    }
                }
            }
        }

        private void PlaceGemFormation(int x, int y)
        {
            int gemType = WorldGen.genRand.Next(6);
            int tileType = TileID.Ruby + gemType;
            
            for (int attempt = 0; attempt < WorldGen.genRand.Next(3, 8); attempt++)
            {
                int gemX = x + WorldGen.genRand.Next(-3, 4);
                int gemY = y + WorldGen.genRand.Next(-3, 4);
                if (gemX >= 0 && gemX < Main.maxTilesX && gemY >= 0 && gemY < Main.maxTilesY && !Main.tile[gemX, gemY].HasTile)
                {
                    WorldGen.PlaceTile(gemX, gemY, tileType);
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

