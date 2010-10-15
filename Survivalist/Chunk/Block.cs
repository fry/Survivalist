﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class Block {
		public static Block[] Blocks = new Block[256];
		public static bool[] DynamicBlocks = new bool[256];

		public static void AddBlock(Block block) {
			if (Blocks[block.TypeId] != null)
				throw new ArgumentException("Tried to register a duplicate block: " + block.TypeId);
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
		public override void OnPlaced(World world, int x, int y, int z) {
			world.ChunkPool.ScheduleUpdate(500, x, y, z);
		}

		public override void OnUpdate(World world, int x, int y, int z) {
			var type = world.GetBlockType(x, y, z);
			var metaData = world.GetBlockData(x, y, z);
			var height = GetHeight(metaData);
			var isFalling = IsFalling(metaData);

			if (type == (int)BlockType.Water) {
				var groundtype = world.GetBlockType(x, y - 1, z);
				var onWater = groundtype == (int)BlockType.Water || groundtype == (int)BlockType.StillWater;
				if (!isFalling) {
					var maxNHeight = FindMaxNHeight(world, x, y, z);
					if (maxNHeight > height) { // this block is higher than our highest neighbor
						if (height == 7)
							world.SetBlockType(x, y, z, (int)BlockType.Air);
						else {
							world.SetBlockData(x, y, z, height + 1);
							world.ChunkPool.ScheduleUpdate(500, x, y, z);
							ActivateNeighbors(world, x, y, z);
						}
						return;
					} else if (height != maxNHeight + 1) { // this block needs to grow
						world.SetBlockData(x, y, z, Math.Min(7, maxNHeight + 1));
						ActivateNeighbors(world, x, y, z);
					}
				} else {
					var aboveType = world.GetBlockType(x, y + 1, z);
					if (aboveType != (int)BlockType.Water && aboveType != (int)BlockType.StillWater) {
						world.SetBlockType(x, y, z, (int)BlockType.Air);
						return;
					}
				}

				if (onWater)
					return;
			}

			if (height == 7) {
				world.SetBlockType(x, y, z, (int)BlockType.Air);
				return;
			}

			var twobelowType = world.GetBlockType(x, y - 2, z);
			var fallMetaData = twobelowType == (int)BlockType.Air ? 8 : 0;
			height++;
			if (!DoFlow(world, x, y - 1, z, fallMetaData)) {
				DoFlow(world, x - 1, y, z, height);
				DoFlow(world, x + 1, y, z, height);
				DoFlow(world, x, y, z - 1, height);
				DoFlow(world, x, y, z + 1, height);
			}
		}

		protected int GetHeight(int metaData) {
			return metaData & 7;
		}

		protected bool IsFalling(int metaData) {
			return (metaData & 8) != 0;
		}

		protected int FindMaxNHeight(World world, int x, int y, int z) {
			int maxNHeight = 7;

			CheckTile(world, x - 1, y, z, ref maxNHeight);
			CheckTile(world, x + 1, y, z, ref maxNHeight);
			CheckTile(world, x, y, z - 1, ref maxNHeight);
			CheckTile(world, x, y, z + 1, ref maxNHeight);

			return maxNHeight;
		}

		protected void CheckTile(World world, int x, int y, int z, ref int height) {
			var type = world.GetBlockType(x, y, z);
			if (type == (int)BlockType.Water)
				height = Math.Min(height, GetHeight(world.GetBlockData(x, y, z)));
			else if (type == (int)BlockType.StillWater)
				height = 0;
		}

		protected bool DoFlow(World world, int x, int y, int z, int metaData) {
			var type = world.GetBlockType(x, y, z);
			if (type == (int)BlockType.Air) {
				if (metaData == 7)
					return false;
				world.SetBlock(x, y, z, (int)BlockType.Water, metaData);
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
