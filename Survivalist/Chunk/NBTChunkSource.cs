using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNbt;
using System.IO;
using LibNbt.Tags;

namespace Survivalist {
	public class NBTChunkSource : ChunkSource {
		string path;
		public NBTChunkSource(string path) {
			this.path = path;
		}

		protected string EncodeBase36(int val) {
			const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
			var builder = new StringBuilder();

			if (val == 0)
				return "0";

			string sign = "";
			if (val < 0) {
				sign = "-";
				val = -val;
			}

			while (val > 0) {
				int i = val % alphabet.Length;
				val /= alphabet.Length;
				builder.Insert(0, alphabet[i]);
			}

			return sign + builder.ToString();
		}

		public ChunkData Load(int x, int y) {
			NbtFile nbt;
			if (!OpenChunk(x, y, out nbt))
				return GenerateNewChunk(x, y);

			var root = nbt.RootTag;
			var level = root["Level"] as NbtCompound;

			var blocks = level["Blocks"] as NbtByteArray;
			var chunk = new ChunkData(blocks.Value);
			chunk.BlockLight.Data = (level["BlockLight"] as NbtByteArray).Value;
			chunk.SkyLight.Data = (level["SkyLight"] as NbtByteArray).Value;
			chunk.MetaData.Data = (level["Data"] as NbtByteArray).Value;

			nbt.Dispose();

			return chunk;
		}

		public void Save(int x, int y, ChunkData chunk) {
			var nbt = new NbtFile {
				RootTag = new NbtCompound("", new NbtTag[] {
					new NbtCompound("Level", new NbtTag[] {
						new NbtByteArray("Blocks", chunk.Blocks),
						new NbtByteArray("BlockLight", chunk.BlockLight.Data),
						new NbtByteArray("SkyLight", chunk.SkyLight.Data),
						new NbtByteArray("Data", chunk.MetaData.Data)
					})
				})
			};

			var path = GetChunkPath(x, y);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			nbt.SaveFile(path, true);
		}

		protected string GetChunkPath(int x, int y) {
			string firstDir = EncodeBase36((int)((uint)x % 64));
			string secondDir = EncodeBase36((int)((uint)y % 64));
			string fileName = String.Format("c.{0}.{1}.dat", EncodeBase36(x), EncodeBase36(y));
			return Path.Combine(path, firstDir, secondDir, fileName);
		}

		protected bool OpenChunk(int x, int y, out NbtFile file) {
			var realPath = GetChunkPath(x, y);			
			if (!File.Exists(realPath)) {
				file = null;
				return false;
			}

			var nbt = new NbtFile();
			nbt.LoadFile(realPath, true);
			file = nbt;
			return true;
		}

		protected void SaveChunk(int x, int y, NbtFile file) {
			var realPath = GetChunkPath(x, y);
			file.SaveFile(realPath, true);
		}

		protected ChunkData GenerateNewChunk(int x, int y) {
			var flatChunk = new ChunkData(new byte[16 * 16 * 128]);
			for (int tx = 0; tx < 16; tx++) {
				for (int tz = 0; tz < 16; tz++) {

					for (int ty = 0; ty < 5; ty++) {
						flatChunk.SetBlock(tx, ty, tz, (int)BlockType.Grass);
					}
					for (int ty = 5; ty < 128; ty++) {
						flatChunk.SkyLight.SetValue(tx, ty, tz, 15);
					}
				}
			}

			return flatChunk;
		}
	}
}
