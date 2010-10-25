using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class Block {
		public static Block[] Blocks = new Block[256];
		public static bool[] DynamicBlocks = new bool[256];
		public static int[] LightAbsorbs = new int[256];
		public static int[] LightEmits = new int[256];

		public static void AddBlock(Block block) {
			//if (Blocks[block.TypeId] != null)
			//	throw new ArgumentException("Tried to register a duplicate block: " + block.TypeId);
			Blocks[block.TypeId] = block;
			DynamicBlocks[block.TypeId] = block.Dynamic;
			LightEmits[block.TypeId] = block.LightEmitted;
			LightAbsorbs[block.TypeId] = block.LightAbsorbed;
		}

		public int TypeId;
		// whether this block can schedule updates
		public bool Dynamic = false;
		// the default delay this block is updated at if it is dynamic
		public int Delay = 500;
		// how much light this block absorbs
		public int LightAbsorbed = 0;
		// how much light this block emits
		public int LightEmitted = 0;

		public virtual void OnCreated(World world, int x, int y, int z) {
			if (Dynamic)
				ActivateBlock(world, x, y, z);
		}

		public virtual void OnPlaced(World world, int x, int y, int z) {
			ActivateBlock(world, x, y, z);
			ActivateNeighbors(world, x, y, z);
		}

		public virtual void OnDestroyed(World world, int x, int y, int z) {
			ActivateNeighbors(world, x, y, z);
		}

		public virtual void OnUpdate(World world, int x, int y, int z) {
		}

		protected void ActivateNeighbors(World world, int x, int y, int z) {
			ActivateBlock(world, x - 1, y, z);
			ActivateBlock(world, x + 1, y, z);
			ActivateBlock(world, x, y - 1, z);
			ActivateBlock(world, x, y + 1, z);
			ActivateBlock(world, x, y, z - 1);
			ActivateBlock(world, x, y, z + 1);
		}

		protected bool ActivateBlock(World world, int x, int y, int z) {
			int type = world.GetBlockType(x, y, z);
			var block = Blocks[type];
			if (block != null && block.Dynamic) {
				world.ChunkPool.ScheduleUpdate(block.Delay, x, y, z);
				return true;
			}
			return false;
		}

		static Block() {
			for (int i = 0; i < 256; i++) {
				AddBlock(new Block { TypeId = i });
			}

			AddBlock(new SolidBlock {
				TypeId = (int)BlockType.Grass
			});
			AddBlock(new SolidBlock {
				TypeId = (int)BlockType.Dirt
			});
			AddBlock(new Block {
				TypeId = (int)BlockType.Air,
				LightAbsorbed = 0
			});
			AddBlock(new Block {
				TypeId = (int)BlockType.Leaves,
				LightAbsorbed = 1
			});
			AddBlock(new LiquidBlock {
				TypeId = (int)BlockType.StillLava,
				FlowingType = (int)BlockType.Lava,
				SourceType = (int)BlockType.StillLava,
				Delay = 1000,
				LightAbsorbed = 15
			});
			AddBlock(new LiquidBlock {
				TypeId = (int)BlockType.Lava,
				FlowingType = (int)BlockType.Lava,
				SourceType = (int)BlockType.StillLava,
				Delay = 1000,
				LightAbsorbed = 15
			});
			AddBlock(new LiquidBlock {
				TypeId = (int)BlockType.StillWater,
				FlowingType = (int)BlockType.Water,
				SourceType = (int)BlockType.StillWater,
				LightAbsorbed = 3
			});
			AddBlock(new LiquidBlock {
				TypeId = (int)BlockType.Water,
				FlowingType = (int)BlockType.Water,
				SourceType = (int)BlockType.StillWater,
				LightAbsorbed = 3
			});
		}
	}

	public class SolidBlock: Block {
		public SolidBlock() {
			LightAbsorbed = 15;
		}
	}

	public class DroppingBlock : Block {
		public int DropsItem;

		public override void OnDestroyed(World world, int x, int y, int z) {
			Console.WriteLine("Drop item " + DropsItem);
			world.EntityHandler.AddEntity(new ItemEntity {
				TypeId = (int)BlockType.Dirt,
				Count = 1,
				X = x + 0.5,
				Y = y,
				Z = z + 0.5
			});
		}
	}

	public class TestDirtBlock : Block {
		public override void OnPlaced(World world, int x, int y, int z) {
			world.ChunkPool.ScheduleUpdate(500, x, y, z);
		}

		public override void OnUpdate(World world, int x, int y, int z) {
			world.SetBlockType(x, y + 1, z, (int)BlockType.StillWater);
		}
	}

	// Stolen from "MySMP"
	public enum BlockType: byte {
		Air = 0,
		Stone = 1,
		Grass = 2,
		Dirt = 3,
		Cobblestone = 4,
		Planks = 5,
		Plant = 6,
		Bedrock = 7,
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
