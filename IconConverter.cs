using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace FolderIconEditor
{
	public static class IconConverter
	{
		private static readonly int[] ICON_SIZES = [16, 32, 48, 64, 128, 256];

		// Function to convert a list of sourceImage file paths to ICO files
		public static void ConvertImagesToIco(IList<string> imageFilePaths, string outputDirectory)
		{
			foreach (var imagePath in imageFilePaths)
			{
				string fileName = Path.GetFileNameWithoutExtension(imagePath);
				string outputPath = Path.Combine(outputDirectory, $"{fileName}.ico");

				ConvertToIco2(imagePath, outputPath);
			}
		}

		// Function to convert a single source image to an ICO file with multiple sizes
		private static void ConvertToIco2(string imagePath, string outputPath)
		{
			using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
			using var writer = new BinaryWriter(stream);
			using var originalBitmap = new Bitmap(imagePath);

			// https://en.wikipedia.org/wiki/ICO_(file_format)
			// ICO header

			writer.Write((short)0); // Reserved
			writer.Write((short)1); // Image type (1 = icon)
			writer.Write((short)ICON_SIZES.Length); // Number of images

			// ICO Image Directory

			int imageDataOffset = 6 + (16 * ICON_SIZES.Length); // offset header and directory
			foreach (int size in ICON_SIZES)
			{
				int imageSize = (size * size * 4) + 40; // Image size (including header size)

				// Write image directory entry
				writer.Write((byte)size);
				writer.Write((byte)size);
				writer.Write((byte)0); // Color palette (not used)
				writer.Write((byte)0); // Reserved
				writer.Write((short)1); // Color planes
				writer.Write((short)32); // Bits per pixel
				writer.Write(imageSize); // Image size (including header size)
				writer.Write(imageDataOffset); // Offset to image data
				imageDataOffset += imageSize; // Add image data size and bitmap header size
			}

			// ICO Data

			foreach (int size in ICON_SIZES)
			{
				using var bitmap = new Bitmap(originalBitmap, (int)size, (int)size);
				var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				// Write BITMAPINFOHEADER
				// https://en.wikipedia.org/wiki/BMP_file_format#DIB_header_(bitmap_information_header)
				writer.Write((int)40); // Header size
				writer.Write(bitmap.Width);
				writer.Write(bitmap.Height * 2); // Height doubled for ICO format
				writer.Write((short)1); // Color planes
				writer.Write((short)32); // Bits per pixel
				writer.Write(0); // Compression (none)
				writer.Write((bitmap.Width * bitmap.Height * 4)); // Image size
				writer.Write(0); // Horizontal resolution (not used)
				writer.Write(0); // Vertical resolution (not used)
				writer.Write(0); // Colors in color palette (not used)
				writer.Write(0); // Important colors (not used)

				// Write pixel data
				for (int y = bitmap.Height - 1; y >= 0; y--)
				{
					IntPtr row = IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride);
					byte[] rowData = new byte[bitmap.Width * 4];
					System.Runtime.InteropServices.Marshal.Copy(row, rowData, 0, rowData.Length);
					writer.Write(rowData);
				}

				bitmap.UnlockBits(bitmapData);
			}
		}
	}
}