using SDL2;
using System;
using System.Drawing;
using System.IO;

namespace RPGEngine
{
	public class Text
	{
		IntPtr renderer;
		IntPtr font;
		IntPtr surface;

		SDL.SDL_Rect target, source;
		SDL.SDL_Color textcolor, backcolor;

		IntPtr texture;
		string text;
		uint format;
		TextMode mode;

		string fontfile;
		int fontsize, access;

		public Text(IntPtr renderer, string fontfile, int fontsize, Color fontcolor, Color backcolor)
		{
			if (SDL_ttf.TTF_Init() != 0)
				Game.Print(LogType.Error, this.GetType().ToString(), "TTF_Init(): {0}".F(SDL.SDL_GetError()));

			if (renderer == IntPtr.Zero)
				Game.Print(LogType.Error, this.GetType().ToString(), "Got NULL Renderer");

			this.renderer = renderer;

			if (!File.Exists(fontfile))
				Game.Print(LogType.Error, this.GetType().ToString(), "File not found: {0}".F(fontfile));

			this.fontfile = fontfile;
			this.fontsize = fontsize;
this.mode = TextMode.Solid;


			this.text = string.Empty;
			this.OpenFont(fontfile, fontsize);

			this.target.x = 18;
			this.target.y = 40;

			this.textcolor.a = fontcolor.A;
			this.textcolor.r = fontcolor.R;
			this.textcolor.g = fontcolor.G;
			this.textcolor.b = fontcolor.B;

			this.backcolor.a = backcolor.A;
			this.backcolor.r = backcolor.R;
			this.backcolor.g = backcolor.G;
			this.backcolor.b = backcolor.B;

			this.source = this.target;
		}

		/// <summary>
		///	Returns the current Font 
		/// </summary>
		public IntPtr Font { get { return this.font; } }
		public IntPtr Texture { get { return this.texture; } }

		public void OpenFont(string filename, int size)
		{
			this.font = SDL_ttf.TTF_OpenFont(this.fontfile, this.fontsize);

			if (this.font == IntPtr.Zero)
				Game.Print(LogType.Error, this.GetType().ToString(), "TTF_OpenFont(): {0}".F(SDL.SDL_GetError()));
		}

		public void CloseFont()
		{
			SDL_ttf.TTF_CloseFont(this.font);
		}

		public void SetSize(int size)
		{
			this.CloseFont();
			this.OpenFont(this.fontfile, size);
		}

		public void Close()
		{
			this.CloseFont();

			SDL_ttf.TTF_Quit();
		}

		public void Update()
		{

		}

		public void Render(IntPtr renderer)
		{
			this.renderer = renderer;

			if (this.surface != IntPtr.Zero)
				SDL.SDL_FreeSurface(this.surface);

			if (this.texture != IntPtr.Zero)
				SDL.SDL_DestroyTexture(this.texture);

			if (this.renderer != IntPtr.Zero)
			{
				switch (this.mode)
				{
					case TextMode.Solid:
						this.surface = SDL_ttf.TTF_RenderText_Solid(this.font, this.text, this.textcolor);
						break;
					case TextMode.Blended:
						this.surface = SDL_ttf.TTF_RenderText_Blended(this.font, this.text, this.textcolor);
						break;
					case TextMode.Wrapped:
						this.surface = SDL_ttf.TTF_RenderText_Blended_Wrapped(this.font, this.text, this.textcolor, 180);
						break;
					case TextMode.Shaded:
						this.surface = SDL_ttf.TTF_RenderText_Shaded(this.font, this.text, this.textcolor, this.backcolor);
						break;
					default:
						break;
				}

				if (this.surface != IntPtr.Zero)
				{
					this.texture = SDL.SDL_CreateTextureFromSurface(this.renderer, this.surface);
					if (this.texture != IntPtr.Zero)
					{
						if (SDL.SDL_RenderCopy(this.renderer, this.texture, ref this.source, ref this.target) != 0)
							Game.Print(LogType.Error, this.GetType().ToString(), "SDL_RenderCopy(): {0}".F(SDL.SDL_GetError()));

						SDL.SDL_DestroyTexture(this.texture);
						SDL.SDL_FreeSurface(this.surface);
					}
				}
			}
		}

		public void print(string text, TextMode mode, int x = 20, int y = 20)
		{
			this.mode = mode;
			this.text = text;

			if (this.text.Length > 0)
				if (this.renderer != IntPtr.Zero && this.surface != IntPtr.Zero)
				{
					this.texture = SDL.SDL_CreateTextureFromSurface(this.renderer, this.surface);
					if (this.texture == IntPtr.Zero)
						Game.Print(LogType.Error, this.GetType().ToString(), "SDL_CreateTextureFromSurface(): {0}".F(SDL.SDL_GetError()));
					else
					{
						this.target.x = x;
						this.target.y = y;

						if (SDL.SDL_QueryTexture(this.texture, out this.format, out this.access, out this.target.w, out this.target.h) != 0)
							Game.Print(LogType.Error, this.GetType().ToString(), SDL.SDL_GetError());
					}
				}
		}

		public void Events(SDL.SDL_Event e)
		{

		}
	}
}
