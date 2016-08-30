using System;

namespace RPGEngine
{
	public static class Exts
	{
		public static string F(this string fmt, params object[] objs)
		{
			return string.Format(fmt, objs);
		}

		public static int[] ToIntArray(this string value, char separator)
		{
			return Array.ConvertAll(value.Split(separator), s => int.Parse(s));
		}

		public static int TileSubdivision(this int me, int subdivisionSize)
		{
			return (me / subdivisionSize) * subdivisionSize;
		}
	}
}
