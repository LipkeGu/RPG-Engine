namespace RPGEngine
{
	public struct Vector2<T>
	{
		public T X, Y;
		public Vector2(T x, T y)
		{
			X = x;
			Y = y;
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

	public enum Worldtype
	{
		Normal,
		Editor,
		Debug
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
}
