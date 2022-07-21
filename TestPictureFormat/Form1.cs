using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestPictureFormat
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog()
			{
				Title = "SUPを開く",
				InitialDirectory = @"C:\Users\User\Desktop",
				Filter = "SUP file|*.sup"
			};

			if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

			System.IO.FileStream fs1 = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read);
			BinaryReader br1 = new BinaryReader(fs1);

			SUP sUP = new SUP(0, 0);
			sUP.ReadSUPH(br1);

			Bitmap bitmap = new Bitmap(sUP.ImageSize.Width, sUP.ImageSize.Height);

			for (int Pixel_X = 0; Pixel_X < bitmap.Width; Pixel_X++)
			{
				for (int Pixel_Y = 0; Pixel_Y < bitmap.Height; Pixel_Y++)
				{
					byte Col_R = sUP.CPData.SUPColorList[sUP.IMGPData.MAP.Bits[Pixel_Y][Pixel_X].LookupBit].ColorR;
					byte Col_G = sUP.CPData.SUPColorList[sUP.IMGPData.MAP.Bits[Pixel_Y][Pixel_X].LookupBit].ColorG;
					byte Col_B = sUP.CPData.SUPColorList[sUP.IMGPData.MAP.Bits[Pixel_Y][Pixel_X].LookupBit].ColorB;
					byte Col_A = sUP.CPData.SUPColorList[sUP.IMGPData.MAP.Bits[Pixel_Y][Pixel_X].LookupBit].ColorA;

					bitmap.SetPixel(Pixel_X, Pixel_Y, Color.FromArgb(Col_A, Col_R, Col_G, Col_B));
				}
			}

			pictureBox1.Image = bitmap;
		}
	}
}
