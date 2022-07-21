using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TestPictureFormat
{
	public class SUP
	{
		public char[] SUPH_Header { get; set; }
		
		public Size ImageSize { get; set; }
		public class Size
		{
			public int Width { get; set; }
			public int Height { get; set; }

			public Size(int Input_Width, int Input_Height)
			{
				Width = Input_Width;
				Height = Input_Height;
			}

			public void ReadSize(BinaryReader br)
			{
				Width = br.ReadInt32();
				Height = br.ReadInt32();
			}

			public void WriteSize(BinaryWriter bw)
			{
				bw.Write(Width);
				bw.Write(Height);
			}
		}

		public CP CPData { get; set; }
		public class CP
		{
			public char[] CP_header { get; set; } //0x2

			public Flags Flag { get; set; }
			public class Flags
			{
				public byte ColorPalletFlag { get; set; } //0x2
				public UsingAlpha UsingAlphaSetting => GetUsingAlpha();
				public RGBType RGBTypeSetting => GetRGBType();

				public UsingAlpha GetUsingAlpha()
				{
					return (UsingAlpha)(ColorPalletFlag & 0x0F);
				}

				public RGBType GetRGBType()
				{
					return (RGBType)(ColorPalletFlag & 0xF0);
				}

				public enum UsingAlpha
				{
					No = 0,
					Yes = 1
				}

				public enum RGBType
				{
					RGBA = 0,
					ARGB = 16
					//32, 48, 64...
				}

				public Flags(byte Input)
				{
					ColorPalletFlag = Input;
				}

				public Flags(UsingAlpha usingAlpha, RGBType RGBTypes)
				{
					ColorPalletFlag = (byte)((int)RGBTypes ^ (int)usingAlpha);
				}

				public void ReadFlag(BinaryReader br)
				{
					ColorPalletFlag = br.ReadByte();
				}

				public void Write(BinaryWriter bw)
				{
					bw.Write(ColorPalletFlag);
				}
			}

			public int ColorPalletCount { get; set; } //0x4, 65536

			public List<SUPColor> SUPColorList { get; set; }
			public class SUPColor
			{
				public byte ColorR { get; set; }
				public byte ColorG { get; set; }
				public byte ColorB { get; set; }
				public byte ColorA { get; set; }

				public SUPColor()
				{
					ColorR = 0;
					ColorG = 0;
					ColorB = 0;
					ColorA = 0;
				}

				public void ReadSUPColor(BinaryReader br, Flags.RGBType RGBType)
				{
					if (RGBType == Flags.RGBType.RGBA)
					{
						ColorR = br.ReadByte();
						ColorG = br.ReadByte();
						ColorB = br.ReadByte();
						ColorA = br.ReadByte();
					}
					else if(RGBType == Flags.RGBType.ARGB)
					{
						ColorA = br.ReadByte();
						ColorR = br.ReadByte();
						ColorG = br.ReadByte();
						ColorB = br.ReadByte();
					}
				}

				public void WriteSUPColor(BinaryWriter bw, Flags.RGBType RGBType)
				{
					if (RGBType == Flags.RGBType.RGBA)
					{
						bw.Write(ColorR);
						bw.Write(ColorG);
						bw.Write(ColorB);
						bw.Write(ColorA);
					}
					else if (RGBType == Flags.RGBType.ARGB)
					{
						bw.Write(ColorA);
						bw.Write(ColorR);
						bw.Write(ColorG);
						bw.Write(ColorB);
					}
				}
			}

			public void ReadCP(BinaryReader br, Flags.RGBType RGBType)
			{
				CP_header = br.ReadChars(2);
				if (new string(CP_header) != "CP") throw new Exception("Error : CP");
				Flag.ReadFlag(br);
				ColorPalletCount = br.ReadInt32();
				
				if (ColorPalletCount != 0)
				{
					for (int i = 0; i < ColorPalletCount; i++)
					{
						SUPColor sUPColor = new SUPColor();
						sUPColor.ReadSUPColor(br, RGBType);

						SUPColorList.Add(sUPColor);
					}
				}
			}

			public void WriteCP(BinaryWriter bw, Flags.RGBType rGBType)
			{
				bw.Write(CP_header);
				Flag.Write(bw);
				bw.Write(ColorPalletCount);

				if (ColorPalletCount != 0)
				{
					for (int i = 0; i < ColorPalletCount; i++)
					{
						SUPColorList[i].WriteSUPColor(bw, rGBType);
					}
				}
			}

			public CP()
			{
				CP_header = "CP".ToCharArray();
				Flag = new Flags(00);
				ColorPalletCount = 0;
				SUPColorList = new List<SUPColor>();
			}
		}

		public IMGP IMGPData { get; set; }
		public class IMGP
		{
			public char[] IMGP_Header { get; set; } //0x4
			public int IMGPSize { get; set; } //0x4

			public MAPS MAP { get; set; }
			public class MAPS
			{
				public char[] MAPS_header { get; set; } //0x4
				public int MAPCount { get; set; } //0x4

				public List<List<Bit>> Bits { get; set; }
				public class Bit
				{
					public byte LookupBit { get; set; } //LookupColorPallet

					public void ReadBit(BinaryReader br)
					{
						LookupBit = br.ReadByte();
					}

					public void WriteBit(BinaryWriter bw)
					{
						bw.Write(LookupBit);
					}

					public Bit(byte In)
					{
						LookupBit = In;
					}
				}

				public void ReadMAPS(BinaryReader br, Size size)
				{
					MAPS_header = br.ReadChars(4);
					if (new string(MAPS_header) != "MAPS") throw new Exception("Error : MAPS");
					MAPCount = br.ReadInt32();

					for (int h = 0; h < size.Height; h++)
					{
						List<Bit> bits = new List<Bit>();

						for (int w = 0; w < size.Width; w++)
						{
							bits.Add(new Bit(br.ReadByte()));
						}

						Bits.Add(bits);
					}
				}

				public void WriteMAPS(BinaryWriter bw, Size size)
				{
					bw.Write(MAPCount);
					bw.Write(MAPCount);

					for (int h = 0; h < size.Height; h++)
					{
						for (int w = 0; w < size.Width; w++)
						{
							bw.Write(Bits[h][w].LookupBit);
						}
					}
				}

				public int GetLength()
				{
					return MAPS_header.Length + 4 + Bits.Count;
				}

				public MAPS()
				{
					MAPS_header = "MAPS".ToCharArray();
					MAPCount = 0;
					Bits = new List<List<Bit>>();
				}
			}

			public void ReadIMGP(BinaryReader br, Size size)
			{
				IMGP_Header = br.ReadChars(4);
				if (new string(IMGP_Header) != "IMGP") throw new Exception("Error : IMGP");
				IMGPSize = br.ReadInt32();
				MAP.ReadMAPS(br, size);
			}

			public void WriteIMGP(BinaryWriter bw, Size size)
			{
				bw.Write(IMGP_Header);
				bw.Write(IMGPSize);
				MAP.WriteMAPS(bw, size);
			}

			public IMGP()
			{
				IMGP_Header = "IMGP".ToCharArray();
				IMGPSize = 0;
				MAP = new MAPS();
			}
		}

		public void ReadSUPH(BinaryReader br)
		{
			SUPH_Header = br.ReadChars(4);
			if (new string(SUPH_Header) != "SUPH") throw new Exception("Error : SUPH");
			ImageSize.ReadSize(br);
			CPData.ReadCP(br, CPData.Flag.RGBTypeSetting);
			IMGPData.ReadIMGP(br, ImageSize);
		}

		public void WriteSUPH(BinaryWriter bw)
		{
			bw.Write(SUPH_Header);
			ImageSize.WriteSize(bw);
			CPData.WriteCP(bw, CPData.Flag.RGBTypeSetting);
			IMGPData.WriteIMGP(bw, ImageSize);
		}

		public SUP(int Width, int Height)
		{
			SUPH_Header = "SUPH".ToCharArray();
			ImageSize = new Size(Width, Height);
			CPData = new CP();
			IMGPData = new IMGP();
		}
	}
}
