using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace WeaponMerging.Systems
{
    public class TrialArenaSubworld : Subworld
    {
        public override int Width => 500;  // Arena width in tiles - increased for more space
        public override int Height => 300; // Arena height in tiles - increased for more space

        public override bool ShouldSave => false; // Temporary world - don't save to disk

        public override void Load()
        {
            // Set world properties
            Main.worldName = "Fusion Trial Arena";
            Main.maxTilesX = Width;
            Main.maxTilesY = Height;

            // Generate the arena world
            GenerateArena();
        }

        private void GenerateArena()
        {
            // Clear all tiles
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Main.tile[x, y].ClearEverything();
                    Main.tile[x, y].WallType = 0;
                }
            }

            // Create perimeter walls (actual tile walls for containment)
            CreateWalls();

            // Create multiple platforms at different heights
            CreatePlatforms();

            // Add decorative elements and obstacles
            AddDecorations();

            // Add atmospheric background elements
            AddBackgroundElements();
        }

        private void CreateWalls()
        {
            // Left and right walls
            for (int y = 0; y < Height; y++)
            {
                for (int wallThickness = 0; wallThickness < 3; wallThickness++)
                {
                    // Left wall
                    WorldGen.PlaceTile(wallThickness, y, TileID.Stone, true, true);
                    // Right wall
                    WorldGen.PlaceTile(Width - 1 - wallThickness, y, TileID.Stone, true, true);
                }
            }

            // Top wall (ceiling)
            for (int x = 0; x < Width; x++)
            {
                for (int wallThickness = 0; wallThickness < 3; wallThickness++)
                {
                    WorldGen.PlaceTile(x, wallThickness, TileID.Stone, true, true);
                }
            }

            // Bottom wall (ground)
            for (int x = 0; x < Width; x++)
            {
                for (int wallThickness = 0; wallThickness < 5; wallThickness++)
                {
                    WorldGen.PlaceTile(x, Height - 1 - wallThickness, TileID.Stone, true, true);
                }
            }
        }

        private void CreatePlatforms()
        {
            // Main fighting platform (bottom)
            int mainPlatformY = Height - 25;
            for (int x = 20; x < Width - 20; x++)
            {
                for (int thickness = 0; thickness < 3; thickness++)
                {
                    WorldGen.PlaceTile(x, mainPlatformY + thickness, TileID.Stone, true, true);
                }
            }

            // Upper platform (for ranged combat or mobility)
            int upperPlatformY = Height - 60;
            for (int x = 40; x < Width - 40; x++)
            {
                for (int thickness = 0; thickness < 2; thickness++)
                {
                    WorldGen.PlaceTile(x, upperPlatformY + thickness, TileID.Stone, true, true);
                }
            }

            // Side platforms (left and right)
            int sidePlatformY = Height - 45;
            // Left side platform
            for (int x = 15; x < 45; x++)
            {
                for (int thickness = 0; thickness < 2; thickness++)
                {
                    WorldGen.PlaceTile(x, sidePlatformY + thickness, TileID.Stone, true, true);
                }
            }
            // Right side platform
            for (int x = Width - 45; x < Width - 15; x++)
            {
                for (int thickness = 0; thickness < 2; thickness++)
                {
                    WorldGen.PlaceTile(x, sidePlatformY + thickness, TileID.Stone, true, true);
                }
            }

            // Floating platform in center
            int floatingPlatformY = Height - 80;
            for (int x = Width / 2 - 15; x < Width / 2 + 15; x++)
            {
                for (int thickness = 0; thickness < 1; thickness++)
                {
                    WorldGen.PlaceTile(x, floatingPlatformY + thickness, TileID.Stone, true, true);
                }
            }
        }

        private void AddDecorations()
        {
            // Add stone pillars for visual interest and cover
            for (int x = 60; x < Width - 60; x += 35)
            {
                int pillarHeight = Main.rand.Next(20, 35);
                int pillarY = Height - 50;
                for (int y = pillarY - pillarHeight; y < pillarY; y++)
                {
                    WorldGen.PlaceTile(x, y, TileID.Stone, true, true);
                }
                // Add a torch on top of some pillars
                if (Main.rand.NextBool(3)) // 1 in 3 chance
                {
                    WorldGen.PlaceTile(x, pillarY - pillarHeight - 1, TileID.Torches, true, true);
                }
            }

            // Add torches around the arena perimeter for better lighting
            for (int x = 50; x < Width - 50; x += 60)
            {
                // Torches on the ground level
                WorldGen.PlaceTile(x, Height - 35, TileID.Torches, true, true);
                // Torches on platforms
                if (x > 100 && x < Width - 100)
                {
                    WorldGen.PlaceTile(x, Height - 70, TileID.Torches, true, true);
                }
            }

            // Add some crystal formations for atmosphere
            for (int i = 0; i < 8; i++)
            {
                int crystalX = Main.rand.Next(80, Width - 80);
                int crystalY = Height - Main.rand.Next(45, 70);
                WorldGen.PlaceTile(crystalX, crystalY, TileID.Crystals, true, true);
                // Add a small cluster
                if (Main.rand.NextBool())
                {
                    WorldGen.PlaceTile(crystalX + 1, crystalY, TileID.Crystals, true, true);
                    WorldGen.PlaceTile(crystalX, crystalY + 1, TileID.Crystals, true, true);
                }
            }

            // Add decorative banners or flags
            for (int x = 100; x < Width - 100; x += 80)
            {
                // Place banners on the upper platform
                int bannerY = Height - 65;
                WorldGen.PlaceTile(x, bannerY, TileID.Banners, true, true);
            }
        }

        private void AddBackgroundElements()
        {
            // Add some vines or hanging decorations from the ceiling
            for (int i = 0; i < 15; i++)
            {
                int vineX = Main.rand.Next(50, Width - 50);
                int vineLength = Main.rand.Next(8, 15);
                for (int y = 5; y < 5 + vineLength; y++)
                {
                    WorldGen.PlaceTile(vineX, y, TileID.Vines, true, true);
                }
            }

            // Add some floating rocks or debris for atmosphere
            for (int i = 0; i < 6; i++)
            {
                int rockX = Main.rand.Next(100, Width - 100);
                int rockY = Main.rand.Next(50, 120);
                WorldGen.PlaceTile(rockX, rockY, TileID.Stone, true, true);
                // Make it a small cluster
                if (Main.rand.NextBool())
                {
                    WorldGen.PlaceTile(rockX + 1, rockY, TileID.Stone, true, true);
                }
            }
        }

        public override List<GenPass> Tasks => new List<GenPass>();

        // No generation passes needed since we generate manually in Load()
    }
}
