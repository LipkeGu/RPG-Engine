using SDL2;
using System;
using System.IO;

namespace RPGEngine
{
	public class Sprite
	{
		IntPtr image;
		int width, height;
		
		Vector2<int> frameSize;
		public Vector2<int> Offset, Frames;
		public SDL.SDL_Rect SourceRect, TargetRect;

		string filename;

		public Sprite(string filename, ref IntPtr renderer, ref Vector2<int> frames, ref Vector2<int> offset)
		{
			if (!File.Exists(filename))
				Game.Print(LogType.Error, this.GetType().ToString(), "File not Found: {0}".F(filename));

			this.filename = filename;

			var format = uint.MinValue;
			var access = 0;
			var retval = -1;

			this.image = SDL_image.IMG_LoadTexture(renderer, this.filename);

			retval = SDL.SDL_QueryTexture(this.image, out format,
				out access, out this.width, out this.height);

			if (retval != 0)
				Game.Print(LogType.Error, this.GetType().ToString(), SDL.SDL_GetError());

			this.Frames = frames;
			this.Offset = offset;

			this.frameSize = new Vector2<int>((this.width / frames.X), (this.height / frames.Y));

			this.TargetRect.h = this.SourceRect.h = this.frameSize.Y;
			this.TargetRect.w = this.SourceRect.w = this.frameSize.X;

			this.TargetRect.x = this.SourceRect.x = (this.Offset.X * this.frameSize.X);
			this.TargetRect.y = this.SourceRect.y = (this.Offset.Y * this.frameSize.Y);
		}

		/// <summary>
		/// The Size of the Frame 
		/// </summary>
		public Vector2<int> FrameSize
		{
			set
			{
				this.frameSize.X = (this.width / value.X);
				this.frameSize.Y = (this.height / value.Y);
			}

			get { return this.frameSize; }
		}

		/// <summary>
		/// The entire width of the Texture
		/// </summary>
		public int Width
		{
			get { return this.width; }
		}

		/// <summary>
		/// The entire height of the Texture
		/// </summary>
		public int Height
		{
			get { return this.height; }
		}

		public string Filename
		{
			get { return this.filename; }
		}

		public IntPtr Image
		{
			set { this.image = value; }
			get { return this.image; }
		}

		public int Render(ref IntPtr renderer)
		{
			return SDL.SDL_RenderCopy(renderer, this.image, ref this.SourceRect, ref this.TargetRect);
		}

		public void Update()
		{
			this.frameSize = new Vector2<int>((this.width / this.Frames.X), (this.height / this.Frames.Y));

			this.TargetRect.h = this.SourceRect.h = this.frameSize.Y;
			this.TargetRect.w = this.SourceRect.w = this.frameSize.X;
		}

		public void Events(SDL.SDL_Event e)
		{

		}

		public void Close()
		{
			SDL.SDL_DestroyTexture(this.image);
		}
	}
}
