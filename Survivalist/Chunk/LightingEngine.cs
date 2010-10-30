using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Survivalist {
	public enum LightType {
		Sky, Block
	}

	public struct Box {
		public int X1, Y1, Z1, X2, Y2, Z2;

		public Box(int x1, int y1, int z1, int x2, int y2, int z2) {
			X1 = x1; Y1 = y1; Z1 = z1;
			X2 = x2; Y2 = y2; Z2 = z2;
		}

		public Box(int x, int y, int z) {
			X1 = x; Y1 = y; Z1 = z;
			X2 = x; Y2 = y; Z2 = z;
		}

		public int Size {
			get {
				return (X2 - X1) * (Y2 - Y1) * (Z2 - Z1);
			}
		}

		public bool Contains(Box box) {
			return box.X1 >= X1 && box.X2 <= X2 &&
				   box.Y1 >= Y1 && box.Y2 <= Y2 &&
				   box.Z1 >= Z1 && box.Z2 <= Z2;
		}

		public void Extend(int radius) {
			X1 -= radius;
			X2 += radius;
			Y1 -= radius;
			Y2 += radius;
			Z1 -= radius;
			Z2 += radius;
		}

		public void Extend(Box box) {
			X1 = Math.Min(X1, box.X1);
			X2 = Math.Max(X2, box.X2);
			Y1 = Math.Min(Y1, box.Y1);
			Y2 = Math.Max(Y2, box.Y2);
			Z1 = Math.Min(Z1, box.Z1);
			Z2 = Math.Max(Z2, box.Z2);
		}
	}

	public class LightingUpdate {
		public LightType Type;
		public Box Area;

		LightingEngine engine;

		public LightingUpdate(LightingEngine engine) {
			this.engine = engine;
		}

		public bool TryExtend(Box box) {
			if (Area.Contains(box))
				return true;

			// New area has to be within 1 tile
			var testBox = Area;
			testBox.Extend(1);

			if (!testBox.Contains(Area))
				return false;

			var newBox = Area;
			newBox.Extend(box);
			if (newBox.Size - Area.Size > 2)
				return false;

			Area = newBox;
			return true;
		}

		public void Run(World world) {
			if (Area.Size > 16 * 16 * 128) {
				Console.WriteLine("[LightingEngine] Tried to run a too big update");
				return;
			}

			for (int x = Area.X1; x <= Area.X2; x++) {
				for (int z = Area.Z1; z <= Area.Z2; z++) {
					if (!world.IsLoaded(x, z))
						continue;
					for (int y = Area.Y1; y <= Area.Y2; y++) {
						int oldLight = world.GetLight(Type, x, y, z);
						int newLight = 0;

						int blockType = world.GetBlockType(x, y, z);
						int lightBlocked = Math.Max(1, Block.LightAbsorbs[blockType]);
						int lightEmitted = 0;
						if (Type == LightType.Sky) {
							// Blocks directly hit by sky light scatter light
							if (world.RecievesSkyLight(x, y, z))
								lightEmitted = 15;
						} else if (Type == LightType.Block) {
							lightEmitted = Block.LightEmits[blockType];
						}

						// All light gets absorbed, no need to check neighbors
						if (lightBlocked >= 15 && lightEmitted == 0)
							newLight = 0;
						else {
							// Find the highest light from the neighbors
							newLight = Math.Max(0, GetHighestNeighbor(world, x, y, z) - lightBlocked);

							// Emitted light always takes precedence
							if (lightEmitted > newLight)
								newLight = lightEmitted;
						}

						// Light changed, scatter it to neighbors
						if (oldLight != newLight) {
							world.SetLight(Type, x, y, z, newLight);

							int neighborLight = Math.Max(0, newLight - 1);
							// Definitely update blocks we already passed in the loop
							engine.TryUpdateTile(Type, x - 1, y, z, neighborLight);
							engine.TryUpdateTile(Type, x, y - 1, z, neighborLight);
							engine.TryUpdateTile(Type, x, y, z - 1, neighborLight);

							// Only update following blocks if we aren't going to update them in this loop
							if (x + 1 >= Area.X2)
								engine.TryUpdateTile(Type, x + 1, y, z, neighborLight);
							if (y + 1 >= Area.Y2)
								engine.TryUpdateTile(Type, x, y + 1, z, neighborLight);
							if (z + 1 >= Area.Z2)
								engine.TryUpdateTile(Type, x, y, z + 1, neighborLight);
						}
					}
				}
			}
		}

		protected int GetHighestNeighbor(World world, int x, int y, int z) {
			int highest = world.GetLight(Type, x - 1, y, z);
			highest = Math.Max(highest, world.GetLight(Type, x + 1, y, z));
			highest = Math.Max(highest, world.GetLight(Type, x, y - 1, z));
			highest = Math.Max(highest, world.GetLight(Type, x, y + 1, z));
			highest = Math.Max(highest, world.GetLight(Type, x, y, z - 1));
			highest = Math.Max(highest, world.GetLight(Type, x, y, z + 1));
			return highest;
		}
	}

	public class LightingEngine {
		LinkedList<LightingUpdate> scheduledUpdates = new LinkedList<LightingUpdate>();
		World world;

		public LightingEngine(World world) {
			this.world = world;
		}

		public void RunUpdates() {
			int i = 1000;
			while (i > 0 && scheduledUpdates.Count > 0) {
				i--;

				var update = scheduledUpdates.Last.Value;
				scheduledUpdates.RemoveLast();
				update.Run(world);
			}
		}

		public void ScheduleUpdate(LightType type, int x, int y, int z) {
			ScheduleUpdate(type, new Box(x, y, z));
		}

		public void ScheduleUpdate(LightType type, Box area) {
			int centerX = (area.X1 + area.X2) / 2;
			int centerY = (area.Y1 + area.Y2) / 2;

			if (!world.IsLoaded(centerX, centerY))
				return;

			// Try to merge this update in one of the more recent updates
			int checkUpdates = 4;
			
			foreach(var update in scheduledUpdates) {
				if (checkUpdates <= 0)
					break;
				checkUpdates--;
				if (update.Type == type && update.TryExtend(area))
					return;
			}

			var newUpdate = new LightingUpdate(this);
			newUpdate.Area = area;
			newUpdate.Type = type;
			scheduledUpdates.AddFirst(newUpdate);
		}

		public void TryUpdateTile(LightType type, int x, int y, int z, int newLight) {
			Debug.Assert(newLight >= 0 && newLight <= 15);
			if (!world.IsLoaded(x, y, z))
				return;

			if (type == LightType.Sky) {
				if (world.RecievesSkyLight(x, y, z))
					newLight = 15;
			} else if (type == LightType.Block) {
				int blockType = world.GetBlockType(x, y, z);
				int emitsLight = Block.LightEmits[blockType];
				newLight = Math.Max(newLight, emitsLight);
			}

			var oldLight = world.GetLight(type, x, y, z);
			if (oldLight != newLight)
				ScheduleUpdate(type, x, y, z);
		}

		public void OnTileChanged(int tileX, int y, int tileZ) {
			int x = tileX & 0xF;
			int z = tileZ & 0xF;
			var chunk = world.GetChunk(tileX >> 4, tileZ >> 4);
			var type = chunk.GetBlock(x, y, z);
			var height = chunk.GetHeight(x, z);
			if (Block.LightAbsorbs[type] > 0) {
				// Block absorbs light, and placed in the way of the skylight so cast a new ray
				if (y >= height)
					DoSkyLight(chunk, x, y + 1, z);
			} else if (y == height - 1) {
				// block is placed just below the max height
				DoSkyLight(chunk, x, y, z);
			}

			ScheduleUpdate(LightType.Sky, tileX, y, tileZ);
			ScheduleUpdate(LightType.Block, tileX, y, tileZ);
		}

		public void RecalculateLighting(ChunkData chunk) {
			for (int x = 0; x < 16; x++) {
				for (int z = 0; z < 16; z++) {
					chunk.SetHeight(x, z, 0);
					DoSkyLight(chunk, x, 127, z);
				}
			}
		}

		protected void DoSkyLight(ChunkData chunk, int x, int ystart, int z) {
			var tileX = chunk.X * 16 + x;
			var tileZ = chunk.Y * 16 + z;

			var height = chunk.GetHeight(x, z);

			// From where should we start calculating
			int newHeight = Math.Max(height, ystart);

			// We need to cast light through transparent blocks
			while (newHeight > 0 && Block.LightAbsorbs[chunk.GetBlock(x, newHeight - 1, z)] == 0)
				newHeight--;

			// Nothing changed, we don't need to calculate
			if (newHeight == height)
				return;

			chunk.SetHeight(x, z, newHeight);

			/* The new height is lower than the last, that means all the tiles
			 * above are fully lit */
			if (newHeight < height) {
				for (int y = newHeight; y < height; y++) {
					chunk.SkyLight.SetValue(x, y, z, 15);
				}
			} else if (newHeight > height) {
				ScheduleUpdate(LightType.Sky, new Box(tileX, height, tileZ, tileX, newHeight, tileZ));
				for (int y = height; y < newHeight; y++) {
					chunk.SkyLight.SetValue(x, y, z, 0);
				}
			}
		}
	}
}
