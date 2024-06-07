using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slash
{
    public static class Textures
    {
        private static string TexturesPath = Directory.GetCurrentDirectory() + "\\Textures";

        public static class BG
        {
            private static Image MagicForest { get; set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\BG\\BG.png");
            private static Image LavaWastelands { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BG\\BG.png");

            public static Image GetBG(Locations location)
            {
                if (location == Locations.MagicForest)
                    return MagicForest;

                else if (location == Locations.LavaWastelands)
                    return LavaWastelands;

                return MagicForest;
            }
        }

        public static class Portals
        {
            private static Image Hole { get; set; } = new Bitmap(TexturesPath + "\\Portals\\Hole.png");
            private static Image GodPortal { get; set; } = new Bitmap(TexturesPath + "\\Portals\\Portal.png");

            private static Image Error { get; set; } = new Bitmap(TexturesPath + "\\Error.png");

            public static Image GetTexture(Portal.Types type)
            {
                if (type == Portal.Types.Hole)
                    return Hole;
                else if (type == Portal.Types.GodPortal)
                    return GodPortal;

                else
                    return Error;
            }
        }

        public static class Blocks
        {
            private static Image Error { get;  set; } = new Bitmap(TexturesPath + "\\Error.png");

            private static Image Air { get;  set; } = new Bitmap(TexturesPath + "\\Air.png");


            private static Image Grass { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Grass\\Grass.png");

            private static Image Dirt { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Dirt\\Dirt.png");
            private static Image FlintDirt0 { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Dirt\\FlintDirt0.png");
            private static Image FlintDirt1 { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Dirt\\FlintDirt1.png");
            private static Image FlintDirt2 { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Dirt\\FlintDirt2.png");

            private static Image Twig { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Twig\\Twig.png");
            private static Image LeftTwig { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Twig\\LeftTwig.png");
            private static Image RightTwig { get;  set; } = new Bitmap(TexturesPath + "\\Locations\\MagicForest\\Twig\\RightTwig.png");

            private static Image BlackStone { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\BlackStone.png");
            private static Image BrokenBlackStone0 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\BrokenBlackStone0.png");
            private static Image BrokenBlackStone1 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\BrokenBlackStone1.png");
            private static Image BrokenBlackStone2 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\BrokenBlackStone2.png");
            private static Image GoldBlackStone0 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\GoldBlackStone0.png");
            private static Image GoldBlackStone1 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\GoldBlackStone1.png");
            private static Image GoldBlackStone2 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\GoldBlackStone2.png");
            private static Image MagmaBlackStone0 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\MagmaBlackStone0.png");
            private static Image MagmaBlackStone1 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\MagmaBlackStone1.png");
            private static Image MagmaBlackStone2 { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\BlackStone\\MagmaBlackStone2.png");

            private static Image Lava { get; set; } = new Bitmap(TexturesPath + "\\Locations\\LavaWastelands\\Lava\\Lava.png");

            // Switch / Case
            public static Image GetTextureByType(AllBlockTypes type)
            {
                if (type == AllBlockTypes.Air)
                    return Air;

                else if (type == AllBlockTypes.Grass)
                    return Grass;

                else if (type == AllBlockTypes.Dirt)
                    return Dirt;
                else if (type == AllBlockTypes.FlintDirt0)
                    return FlintDirt0;
                else if (type == AllBlockTypes.FlintDirt1)
                    return FlintDirt1;
                else if (type == AllBlockTypes.FlintDirt2)
                    return FlintDirt2;

                else if (type == AllBlockTypes.Twig)
                    return Twig;
                else if (type == AllBlockTypes.LeftTwig)
                    return LeftTwig;
                else if (type == AllBlockTypes.RightTwig)
                    return RightTwig;

                else if (type == AllBlockTypes.BlackStone)
                    return BlackStone;
                else if (type == AllBlockTypes.BrokenBlackStone0)
                    return BrokenBlackStone0;
                else if (type == AllBlockTypes.BrokenBlackStone1)
                    return BrokenBlackStone1;
                else if (type == AllBlockTypes.BrokenBlackStone2)
                    return BrokenBlackStone2;
                else if (type == AllBlockTypes.GoldBlackStone0)
                    return GoldBlackStone0;
                else if (type == AllBlockTypes.GoldBlackStone1)
                    return GoldBlackStone1;
                else if (type == AllBlockTypes.GoldBlackStone2)
                    return GoldBlackStone2;
                else if (type == AllBlockTypes.MagmaBlackStone0)
                    return MagmaBlackStone0;
                else if (type == AllBlockTypes.MagmaBlackStone1)
                    return MagmaBlackStone1;
                else if (type == AllBlockTypes.MagmaBlackStone2)
                    return MagmaBlackStone2;

                else if (type == AllBlockTypes.Lava)
                    return Lava;

                else
                    return Error;
            }
        }

        public static class Entities
        {
            public static Image Slasher { get; private set; } = new Bitmap(TexturesPath + "\\Slasher\\0.png");
            private static Image Square { get; set; } = new Bitmap(TexturesPath + "\\Enemies\\Square.png"); 
            private static Image Totem { get; set; } = new Bitmap(TexturesPath + "\\Enemies\\Totem.png");

            private static Image Error { get; set; } = new Bitmap(TexturesPath + "\\Error.png");

            public static Image GetTexture(Entity.Types type)
            {
                if (type == Entity.Types.Slasher)
                    return Slasher;

                else if (type == Entity.Types.Square)
                    return Square;

                else if (type == Entity.Types.Totem)
                    return Totem;
                else
                    return Error;
            }
            public static Image GetTexture(Entity.Types type, int spriteNumber)
            {
                if (type == Entity.Types.Slasher)
                {
                    return new Bitmap(TexturesPath + "\\Slasher\\" + spriteNumber.ToString() + ".png");
                }

                else
                    return Error;
            }
        }

        public static class Other
        {
            public static Image PlashkaForGribs { get; private set; } = new Bitmap(TexturesPath + "\\PlashkaForGribs.png");

            public static Image TotemProgressBar { get; private set; } = new Bitmap(TexturesPath + "\\Other\\TotemProgressBar.png");
        }
    }
}
