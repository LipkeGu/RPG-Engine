using SDL2;
using System;
using System.Drawing;

namespace RPGEngine
{
	public class ErrorEventArgs : EventArgs
	{
		public string Source { get; set; }
		public string Message { get; set; }
	}

	public class StateEventArgs : EventArgs
	{
		public string Source { get; set; }
		public string Message { get; set; }
	}

	public class FinishEventArgs : EventArgs
	{
		public string Source { get; set; }
		public string Message { get; set; }
	}

	public struct Vector2<T>
	{
		public T X, Y;
		public Vector2(T x, T y)
		{
			X = x;
			Y = y;
		}
	}

	public class Portal
	{
		public RectangleF Position;
		public string Map;
		public bool Enabled;

		public Portal(Vector2<ulong> pos, string mapname, Vector2<uint> size, bool enable = true)
		{
			this.Map = mapname;
			this.Position = new RectangleF(pos.X, pos.Y, size.X, size.Y);
			this.Enabled = enable;
		}
	}


	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
		None
	}

	public enum TextMode
	{
		Solid,
		Blended,
		Wrapped,
		Shaded
	}

	public enum TileType
	{
		
		Clear,
		Grass,
		Water,
		Road,
		None
	}

	public enum Worldtype
	{
		Normal,
		Editor,
		Debug,
		Menu
	}

	public enum LogType
	{
		Info,
		Warn,
		Error,
		Debug,
		Notice
	}

	public enum MovingType
	{
		Walk,
		Bike,
		Dive
	}

	public enum RenderType
	{
		SDL,
		OpenGL,
	}

	public enum LayerType
	{
		Ground,
		Ground_Overlay,
		Collision,
		PlayerOverlay
	}

	interface IGame
	{
		void Render();
		void Update();
		void Events(ref SDL.SDL_Event e);
		void Close();
	}
}
