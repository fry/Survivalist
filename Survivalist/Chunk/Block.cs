using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class BlockUpdateEventArgs {
		public World World;
		public int X;
		public int Y;
		public int Z;
	}

	public class Block {
		public static Block[] Blocks = new Block[256];
		public static void AddBlock(Block block) {
			Blocks[block.TypeId] = block;
		}

		public int TypeId;
		// null for none, everything else means some light is emitted and needs to be calculated
		public int? Luminosity = null;
		// whether light goes through this block
		public bool Transparent = true;

		public virtual void OnBlockCreated(BlockUpdateEventArgs args) {
			//Console.WriteLine("Block create");
			//args.World.SetBlockType(args.X, args.Y, args.Z, (int)BlockType.Water, false);
		}

		public virtual void OnBlockDestroyed(BlockUpdateEventArgs args) {
			/*args.World.EntityHandler.AddEntity(new ItemEntity {
				TypeId = (int)BlockType.Dirt,
				Count = 1,
				X = args.X + 0.5,
				Y = args.Y,
				Z = args.Z + 0.5
			});*/
		}

		static Block() {
			AddBlock(new Block {
				TypeId = (int)BlockType.Grass
			});
		}
	}

	public class DroppingBlock : Block {
		public int DropsItem;

		public override void OnBlockDestroyed(BlockUpdateEventArgs args) {
			Console.WriteLine("Drop item " + DropsItem);
		}
	}

	public enum BlockType {
		Air = 0,
		Stone = 1,
		Grass = 2,
		Dirt = 3,
		Cobblestone = 4,
		Planks = 5,
		Plant = 6,
		Admincrete = 7,
		Water = 8,
		StillWater = 9,
		Lava = 10,
		StillLava = 11,
		Sand = 12,
		Gravel = 13,
		GoldOre = 14,
		IronOre = 15,
		Coal = 16,
		Log = 17,
		Leaves = 18,
		Sponge = 19,
		Glass = 20,

		Cloth = 35,

		YellowFlower = 37,
		RedFlower = 38,
		BrownMushroom = 39,
		RedMushroom = 40,

		Gold = 41,
		Steel = 42,
		DoubleStair = 43,
		Stair = 44,
		Brick = 45,
		TNT = 46,
		Books = 47,
		MossyRocks = 48,
		Obsidian = 49,

		Torch = 50,
		Fire = 51,
		MobSpawner = 52,
		WoodenStairs = 53,
		Chest = 54,
		RedstoneWire = 55,
		DiamondOre = 56,
		Diamond = 57,
		Workbench = 58,
		Crop = 59,
		Soil = 60,
		Furnace = 61,
		BurningFurnace = 62,
		SignPost = 63,
		WoodenDoor = 64,
		Ladder = 65,
		Rail = 66,
		CobblestoneStairs = 67,
		Sign = 68,
		Lever = 69,
		StonePressurePlate = 70,
		IronDoor = 71,
		WoodenPressurePlate = 72,
		RedstoneOre = 73,
		LightedRedstoneOre = 74,
		RedstoneTorchOff = 75,
		RedstoneTorchOn = 76,
		StoneButton = 77,
		Snow = 78,
		Ice = 79,
		SnowBlock = 80,
		Cactus = 81,
		Clay = 82,
		Reed = 83,
		Jukebox = 84,
		Fence = 85
	}

	
}
