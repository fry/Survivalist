using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class LiquidBlock : Block {
		public int SourceType;
		public int FlowingType;
		public int MaxHeight = 7;

		public LiquidBlock() {
			Delay = 250;
			Dynamic = true;
		}

		public override void OnUpdate(World world, int x, int y, int z) {
			// Get all kinds of required data
			var type = world.GetBlockType(x, y, z);
			var groundtype = world.GetBlockType(x, y - 1, z);
			var metaData = world.GetBlockData(x, y, z);
			var height = GetHeight(metaData);
			var isFalling = IsFalling(metaData);
			var onWater = groundtype == SourceType || groundtype == FlowingType;

			// Still water drains and has some restriction the way it can flow:
			// 1) It can't flow on top of water tiles
			// 2) It can't spread if it is falling
			if (type == FlowingType) {
				// Not falling tiles adjust their size depending on their neighbors' height
				if (!isFalling) {
					var maxNHeight = FindMaxNHeight(world, x, y, z);
					if (maxNHeight >= height) { // this block is higher than our highest neighbor and needs to shrink
						// If this already is the smallest block, disappear, otherwise shrink by 1
						if (height == MaxHeight)
							world.SetBlockType(x, y, z, (int)BlockType.Air);
						else {
							world.SetBlockData(x, y, z, height + 1);
							ActivateNeighbors(world, x, y, z);
							ActivateBlock(world, x, y, z);
						}
						return;
					} else if (height != maxNHeight + 1) { // this block needs to grow
						// Never grow higher than the max height
						world.SetBlockData(x, y, z, Math.Min(MaxHeight, maxNHeight + 1));
						ActivateNeighbors(world, x, y, z);
						ActivateBlock(world, x, y, z);
					}
				} else {
					// Falling water shrinks if there is no water block above it
					var aboveType = world.GetBlockType(x, y + 1, z);
					if (aboveType != FlowingType && aboveType != SourceType) {
						// Falling water shrinks faster than normal tiles
						if (height >= MaxHeight)
							world.SetBlockType(x, y, z, (int)BlockType.Air);
						else {
							world.SetBlockData(x, y, z, MaxHeight);
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
			if (height > MaxHeight)
				return;
			// Source blocks also spread on top of water
			bool flowDown = DoFlow(world, x, y - 1, z, 8);
			if (!flowDown || (onWater && type == SourceType)) {
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

			if (flowDown && y == 0)
				return;
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
			if (type == FlowingType)
				height = Math.Min(height, GetHeight(world.GetBlockData(x, y, z)));
			else if (type == SourceType)
				height = 0;
		}

		// Return whether this type is interpreted as a "drop" and should be flowed towards
		protected bool IsDrop(int type) {
			return type == (int)BlockType.Air || type == SourceType || type == FlowingType;
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
			if (type == SourceType || type == FlowingType)
				return true;
			if (type == (int)BlockType.Air) {
				if (metaData == 7)
					return false;
				world.SetBlock(x, y, z, FlowingType, metaData);
				ActivateNeighbors(world, x, y, z);
				return true;
			}
			return false;
		}
	}
}
