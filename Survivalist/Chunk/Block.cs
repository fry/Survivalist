using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class Block {
		public static Block[] Blocks = new Block[256];
		public static bool[] DynamicBlocks = new bool[256];

		public static void AddBlock(Block block) {
			//if (Blocks[block.TypeId] != null)
			//	throw new ArgumentException("Tried to register a duplicate block: " + block.TypeId);
			Blocks[block.TypeId] = block;
			DynamicBlocks[block.TypeId] = block.Dynamic;
		}

		public int TypeId;
		// null for none, everything else means some light is emitted and needs to be calculated
		public int? Luminosity = null;
		// whether light goes through this block
		public bool Transparent = true;
		// whether this block can schedule updates
		public bool Dynamic = false;
		// the default delay this block is updated at if it is dynamic
		public int Delay = 500;

		public virtual void OnCreated(World world, int x, int y, int z) {
			//Console.WriteLine("Block create");
			//args.World.SetBlockType(args.X, args.Y, args.Z, (int)BlockType.Water, false);
			
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

			AddBlock(new Block {
				TypeId = (int)BlockType.Grass
			});
			AddBlock(new LiquidBlock {
				TypeId = (int)BlockType.StillWater,
				Dynamic = true
			});
			AddBlock(new LiquidBlock {
				TypeId = (int)BlockType.Water,
				Dynamic = true
			});
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

	public class LiquidBlock : Block {
		public LiquidBlock() {
			Delay = 250;
		}

		public override void OnUpdate(World world, int x, int y, int z) {
			// Get all kinds of required data
			var type = world.GetBlockType(x, y, z);
			var groundtype = world.GetBlockType(x, y - 1, z);
			var metaData = world.GetBlockData(x, y, z);
			var height = GetHeight(metaData);
			var isFalling = IsFalling(metaData);
			var onWater = groundtype == (int)BlockType.Water || groundtype == (int)BlockType.StillWater;

			// Still water drains and has some restriction the way it can flow:
			// 1) It can't flow on top of water tiles
			// 2) It can't spread if it is falling
			if (type == (int)BlockType.Water) {
				// Not falling tiles adjust their size depending on their neighbors' height
				if (!isFalling) {
					var maxNHeight = FindMaxNHeight(world, x, y, z);
					if (maxNHeight >= height) { // this block is higher than our highest neighbor and needs to shrink
						// If this already is the smallest block, disappear, otherwise shrink by 1
						if (height == 7)
							world.SetBlockType(x, y, z, (int)BlockType.Air);
						else {
							world.SetBlockData(x, y, z, height + 1);
							ActivateNeighbors(world, x, y, z);
							ActivateBlock(world, x, y, z);
						}
						return;
					} else if (height != maxNHeight + 1) { // this block needs to grow
						// Never grow higher than the max height
						world.SetBlockData(x, y, z, Math.Min(7, maxNHeight + 1));
						ActivateNeighbors(world, x, y, z);
						ActivateBlock(world, x, y, z);
					}
				} else {
					// Falling water shrinks if there is no water block above it
					var aboveType = world.GetBlockType(x, y + 1, z);
					if (aboveType != (int)BlockType.Water && aboveType != (int)BlockType.StillWater) {
						// Falling water shrinks faster than normal tiles
						if (height >= 6)
							world.SetBlockType(x, y, z, (int)BlockType.Air);
						else {
							world.SetBlockData(x, y, z, height + 2);
							ActivateNeighbors(world, x, y, z);
							ActivateBlock(world, x, y, z);
						}
						return;
					}
				}

				// Flowing water doesn't spread on top of water
				if (onWater)
					return;
			}

			// Set height of all following tiles and abort if this tile is already the smallest
			height++;
			if (height >= 8)
				return;
			// Source blocks also spread on top of water
			bool flowDown = DoFlow(world, x, y - 1, z, 8);
			if (!flowDown || (onWater && type == (int)BlockType.StillWater)) {
				// Try flowing towards the nearest drop in 4 tiles radius	
				if (!FlowRadius(world, x, y, z, 1, height))
					if (!FlowRadius(world, x, y, z, 2, height))
						if (!FlowRadius(world, x, y, z, 3, height))
							if (!FlowRadius(world, x, y, z, 4, height)) {
								// No nearby drop down, flow in all directions
								DoFlow(world, x - 1, y, z, height);
								DoFlow(world, x + 1, y, z, height);
								DoFlow(world, x, y, z - 1, height);
								DoFlow(world, x, y, z + 1, height);
							}
				return;
			}

			// Schedule new update for this block if we flowed once (down or sideways)
			ActivateBlock(world, x, y, z);
		}

		// The first 7 bits of the meta data specify the height (0 is a full block, 7 is a nearly empty block)
		protected int GetHeight(int metaData) {
			return metaData & 7;
		}

		// The 8th bit of the meta data specifies whether this is a falling tile
		protected bool IsFalling(int metaData) {
			return (metaData & 8) != 0;
		}

		// Return the height of the highest tile adjacent to this tile
		protected int FindMaxNHeight(World world, int x, int y, int z) {
			int maxNHeight = 7;
			CheckTile(world, x - 1, y, z, ref maxNHeight);
			CheckTile(world, x + 1, y, z, ref maxNHeight);
			CheckTile(world, x, y, z - 1, ref maxNHeight);
			CheckTile(world, x, y, z + 1, ref maxNHeight);

			return maxNHeight;
		}

		// If the passed tile is higher than the passed height, adjust that height
		protected void CheckTile(World world, int x, int y, int z, ref int height) {
			var type = world.GetBlockType(x, y, z);
			if (type == (int)BlockType.Water)
				height = Math.Min(height, GetHeight(world.GetBlockData(x, y, z)));
			else if (type == (int)BlockType.StillWater)
				height = 0;
		}

		// Return whether this type is interpreted as a "drop" and should be flowed towards
		protected bool IsDrop(int type) {
			return type == (int)BlockType.Air || type == (int)BlockType.Water || type == (int)BlockType.StillWater;
		}

		// Look for a drop on the sides of a rectangle with the passed radius and flow towards it
		// Also return whether we did find a drop
		protected bool FlowRadius(World world, int x, int y, int z, int radius, int metaData) {
			bool didFlow = false;
			int checkY = y - 1;
			// Flow north?
			for (int i = x - radius; i < x + radius; i++) {
				if (IsDrop(world.GetBlockType(i, checkY, z - radius)))
					didFlow = DoFlow(world, x, y, z - 1, metaData) || didFlow;
			}
			// Flow south?
			for (int i = x - radius; i < x + radius; i++) {
				if (IsDrop(world.GetBlockType(i, checkY, z + radius)))
					didFlow = DoFlow(world, x, y, z + 1, metaData) || didFlow;
			}
			// Flow west?
			for (int i = z - radius; i < z + radius; i++) {
				if (IsDrop(world.GetBlockType(x - radius, checkY, i)))
					didFlow = DoFlow(world, x - 1, y, z, metaData) || didFlow;
			}
			// Flow east?
			for (int i = z - radius; i < z + radius; i++) {
				if (IsDrop(world.GetBlockType(x + radius, checkY, i)))
					didFlow = DoFlow(world, x + 1, y, z, metaData) || didFlow;
			}
			return didFlow;
		}

		// Try filling the passed tile with water and return whether we're successful
		protected bool DoFlow(World world, int x, int y, int z, int metaData) {
			var type = world.GetBlockType(x, y, z);
			if (type == (int)BlockType.Water || type == (int)BlockType.StillWater)
				return true;
			if (type == (int)BlockType.Air) {
				if (metaData == 7)
					return false;
				world.SetBlock(x, y, z, (int)BlockType.Water, metaData);
				ActivateNeighbors(world, x, y, z);
				return true;
			}
			return false;
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
