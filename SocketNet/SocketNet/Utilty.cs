using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace SocketNet
{
	public static class Utilty
	{
		public static string ToBase64(Image img, ImageFormat format)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				img.Save(ms, format);
				byte[] b = ms.ToArray();
				return Convert.ToBase64String(b);
			}
		}

		public static Image ToImage(string base64)
		{
			byte[] b = Convert.FromBase64String(base64);
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(b, 0, b.Length);
				Image img = Image.FromStream(ms, true);
				return img;
			}
		}
	}
}
