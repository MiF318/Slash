using Slash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slash
{
    public static class Blocks
    {
        public static BlockType DirtTypesTree
        {
            get
            {
                var mainType = AllBlockTypes.Dirt;

                var secondTypes = new List<AllBlockTypes>()
                {
                    AllBlockTypes.FlintDirt
                };

                var thirdTypes = new List<List<AllBlockTypes>>()
                {
                    new List<AllBlockTypes>()
                    {
                        AllBlockTypes.FlintDirt0,
                        AllBlockTypes.FlintDirt1,
                        AllBlockTypes.FlintDirt2
                    }
                };

                return CreateBlockTypeTree(mainType, secondTypes, thirdTypes);
            }

            private set { }
        }

        public static BlockType TwigTypesTree
        {
            get
            {
                var mainType = AllBlockTypes.Twig;
                var blockTypeTree = new BlockType(mainType);

                var secondTypes = new List<AllBlockTypes>()
                {
                    AllBlockTypes.LeftTwig,
                    AllBlockTypes.RightTwig
                };

                return AddBlockSubTypes(blockTypeTree, secondTypes);
            }

            private set { }
        }

        public static BlockType BlackStoneTypesTree
        {
            get
            {
                var mainType = AllBlockTypes.BlackStone;

                var secondTypes = new List<AllBlockTypes>()
                {
                    AllBlockTypes.BrokenBlackStone,
                    AllBlockTypes.GoldBlackStone,
                    AllBlockTypes.MagmaBlackStone,

                };

                var thirdTypes = new List<List<AllBlockTypes>>()
                {
                    new List<AllBlockTypes>()
                    {
                        AllBlockTypes.BrokenBlackStone0,
                        AllBlockTypes.BrokenBlackStone1,
                        AllBlockTypes.BrokenBlackStone2
                    },
                    new List<AllBlockTypes>()
                    {
                        AllBlockTypes.GoldBlackStone0,
                        AllBlockTypes.GoldBlackStone1,
                        AllBlockTypes.GoldBlackStone2
                    },
                    new List<AllBlockTypes>()
                    {
                        AllBlockTypes.MagmaBlackStone0,
                        AllBlockTypes.MagmaBlackStone1,
                        AllBlockTypes.MagmaBlackStone2
                    }
                };

                return CreateBlockTypeTree(mainType, secondTypes, thirdTypes);
            }

            private set { }
        }

        public static Block UpdatePlatform(Block[,] map, Point currentBlockCords, BlockType platformTypeTree)
        {
            var leftBlock = currentBlockCords.X != 0 ? map[currentBlockCords.X - 1, currentBlockCords.Y] : map[currentBlockCords.X, currentBlockCords.Y];
            var rightBlock = currentBlockCords.X != map.GetLength(0) - 1 ? map[currentBlockCords.X + 1, currentBlockCords.Y] : map[currentBlockCords.X, currentBlockCords.Y];

            if (leftBlock.Type.MainType == AllBlockTypes.Air)
                return new Block(platformTypeTree.SubTypes[0], BlockRoles.Platform, currentBlockCords, true, false);

            else if (rightBlock.Type.MainType == AllBlockTypes.Air)
                return new Block(platformTypeTree.SubTypes[1], BlockRoles.Platform, currentBlockCords, true, false);

            else
                return new Block(platformTypeTree, BlockRoles.Platform, currentBlockCords, true, false);
        }

        // Сделать для дерева любого размера
        private static BlockType CreateBlockTypeTree (AllBlockTypes mainType, List<AllBlockTypes> secondTypes, List<List<AllBlockTypes>> thirdTypes)
        {
            if (secondTypes.Count != thirdTypes.Count)
                throw new Exception("Количество вторых типов не совпадает с количеством третьих");

            var blockTypeTree = new BlockType(mainType);

            AddBlockSubTypes(blockTypeTree, secondTypes);

            for (int subTypeNumber = 0;  subTypeNumber < secondTypes.Count; subTypeNumber++)
                AddBlockSubTypes(blockTypeTree.SubTypes[subTypeNumber], thirdTypes[subTypeNumber]);

            return blockTypeTree;
        }
        private static BlockType AddBlockSubTypes (BlockType root, List<AllBlockTypes> subTypes)
        {
            root.SubTypes = new List<BlockType>();

            foreach (var subType in subTypes)
            {
                root.SubTypes.Add(new BlockType(subType));
            }
            foreach (var subType in root.SubTypes)
            {
                subType.ParentType = root;
            }

            return root;
        }

        public static Block CreateBlock(Point cords, AllBlockTypes type, Block[,] map)
        {
            if (type == AllBlockTypes.Air)
                return new Block(new BlockType(AllBlockTypes.Air), BlockRoles.Atmosphere, cords, false, false);

            else if (type == AllBlockTypes.Grass)
                return new Block(new BlockType(AllBlockTypes.Grass), BlockRoles.Ground, cords, true, false);

            else if (type == AllBlockTypes.Dirt)
                return new Block(GenerateBlockTypeByTypeTree(DirtTypesTree, cords, map), BlockRoles.Ground, cords, true, false);

            else if (type == AllBlockTypes.Twig)
                return new Block(new BlockType(AllBlockTypes.Twig), BlockRoles.Platform, cords, true, false);

            else if (type == AllBlockTypes.BlackStone)
                return new Block(GenerateBlockTypeByTypeTree(BlackStoneTypesTree, cords, map), BlockRoles.Ground, cords, true, false);

            else if (type == AllBlockTypes.Lava)
                return new Block(new BlockType(AllBlockTypes.Lava), BlockRoles.Ground, cords, true, true);

            else
                return new Block(new BlockType(AllBlockTypes.Error), BlockRoles.Else, cords, true, false);
        }

        // Отрефакторить
        private static BlockType GenerateBlockTypeByTypeTree(BlockType typeTree, Point cords, Block[,] map)
        {
            BlockType blockType;

            var random = new Random();

            var similarBlockTypesInNeighboring = new List<BlockType>();

            //Поиск похожих блоков рядом
            foreach (var subType in typeTree.SubTypes)
            {
                if (cords.X > 0 && map[cords.X - 1, cords.Y].Type.ParentType != null && map[cords.X - 1, cords.Y].Type.ParentType.MainType == subType.MainType)
                {
                    if (!similarBlockTypesInNeighboring.Contains(subType))
                        similarBlockTypesInNeighboring.Add(subType);
                }

                if (cords.Y > 0 && map[cords.X, cords.Y - 1].Type.ParentType != null && map[cords.X, cords.Y - 1].Type.ParentType.MainType == subType.MainType)
                {
                    if (!similarBlockTypesInNeighboring.Contains(subType))
                        similarBlockTypesInNeighboring.Add(subType);
                }
            }

            if (similarBlockTypesInNeighboring.Count == 0)
            {
                if (random.NextDouble() < 0.1)
                {
                    //unc
                    var subTypeChance = random.Next(typeTree.SubTypes.Count);
                    var subSubTypeChance = random.Next(typeTree.SubTypes[subTypeChance].SubTypes.Count);
                    blockType = typeTree.SubTypes[subTypeChance].SubTypes[subSubTypeChance];
                }
                else
                    blockType = typeTree;
            }
            else
            {
                if (random.NextDouble() < 0.5)
                {
                    if (random.NextDouble() < 0.1)
                    {
                        //unc
                        var subTypeChance = random.Next(typeTree.SubTypes.Count);
                        var subSubTypeChance = random.Next(typeTree.SubTypes[subTypeChance].SubTypes.Count);
                        blockType = typeTree.SubTypes[subTypeChance].SubTypes[subSubTypeChance];
                    }
                    else
                        blockType = typeTree;
                }

                else
                {
                    var subType = similarBlockTypesInNeighboring[random.Next(similarBlockTypesInNeighboring.Count)];

                    blockType = subType.SubTypes[random.Next(subType.SubTypes.Count)];
                }
            }

            return blockType;
        }
    }

    public class BlockType
    {
        public AllBlockTypes MainType { get; private set; }

        public BlockType? ParentType { get; set; }

        public List<BlockType>? SubTypes { get; set; }

        public BlockType(AllBlockTypes type)
        {
            MainType = type;
        }
    }

    public class Block
    {
        public readonly BlockType Type;
        public readonly BlockRoles Role;

        public readonly Point Cords;

        public Image Texture { get; private set; }

        public bool Solid { get; private set; }
        public bool Dangerous { get; private set; }

        public Block(BlockType type, BlockRoles role, Point cords, bool solid, bool dangerous)
        {
            Type = type;
            Role = role;
            Cords = cords;

            Texture = Textures.Blocks.GetTextureByType(type.MainType);
            
            Solid = solid;
            Dangerous = dangerous;
        }
    }

    public enum BlockRoles
    {
        Ground,
        Platform,
        Atmosphere,
        KillBlock,
        Else
    }

    public enum AllBlockTypes
    {
        Error,
        Air, // a

        Hole, // h
        Portal, // p

        //Forest
        Dirt, // d

        FlintDirt,
        FlintDirt0,
        FlintDirt1,
        FlintDirt2,

        Grass, // g

        Twig, // t
        LeftTwig,
        RightTwig,
        

        //Clifs
        Stone, // s

        MossyStone,
        MossyStone0,
        MossyStone1,
        MossyStone2,

        BrokenStone,
        BrokenStone0,
        BrokenStone1,
        BrokenStone2,

        Cloud, // c
        LeftCloud,
        RightCloud,

        //Lava
        BlackStone, // b

        GoldBlackStone,
        GoldBlackStone0,
        GoldBlackStone1,
        GoldBlackStone2,

        BrokenBlackStone,
        BrokenBlackStone0,
        BrokenBlackStone1,
        BrokenBlackStone2,

        MagmaBlackStone,
        MagmaBlackStone0,
        MagmaBlackStone1,
        MagmaBlackStone2,

        Lava // l
    }

    public enum Locations
    {
        MagicForest,
        HighClifs,
        LavaWastelands
    }
}
