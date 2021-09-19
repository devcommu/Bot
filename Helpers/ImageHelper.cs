using System;
using System.IO;
using System.Net;
using System.Text;

namespace DevCommuBot.Helpers
{
    internal abstract class ImageHelper
    {
		public static string ConvertImageURLToBase64(string url)
		{
			StringBuilder _sb = new();

            byte[] _byte = GetImage(url);

			_sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));

			return _sb.ToString();
		}

		private static byte[] GetImage(string url)
		{
            byte[] buf;

			try
			{
				//WebProxy myProxy = new();
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

				HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Stream stream = response.GetResponseStream();

                using (BinaryReader br = new(stream))
				{
					int len = (int)(response.ContentLength);
					buf = br.ReadBytes(len);
					br.Close();
				}

				stream.Close();
				response.Close();
			}
			catch (Exception e)
            {
				Console.WriteLine(e.Message);
				buf = null;
			}

			return (buf);
		}
	}
}