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

		SDL.SDL_Rect target, source;
		SDL.SDL_Color textcolor, backcolor;

		Sprite texture;
		string text;
		uint format;
		TextMode mode;

		string alias;
		string fontfile;
		int fontsize, access;

		public Text(IntPtr renderer, string fontfile, string alias, int fontsize, Color fontcolor, Color backcolor)
		{
			if (SDL_ttf.TTF_Init() != 0)
				Game.Print(LogType.Error, this.GetType().ToString(), "TTF_Init(): {0}".F(SDL.SDL_GetError()));

			if (renderer == IntPtr.Zero)
				Game.Print(LogType.Error, this.GetType().ToString(), "Got NULL Renderer");

			this.renderer = renderer;

			if (!File.Exists(fontfile))
				Game.Print(LogType.Error, this.GetType().ToString(), "File not found: {0}".F(fontfile));

			this.fontfile = fontfile;
			this.alias = alias;

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

		public void Init(object obj)
		{

		}

		/// <summary>
		///	Returns the current Font 
		/// </summary>
		public IntPtr Font { get { return this.font; } }
		public Sprite Texture { get { return this.texture; } }

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
			if (this.renderer != IntPtr.Zero)
			{
				switch (this.mode)
				{
					case TextMode.Solid:
						Engine.ConvertSurface(SDL_ttf.TTF_RenderText_Solid(this.font, this.text, this.textcolor), renderer, "{0}_Solid".F(this.text), new Vector2<int>(1, 1));
						break;
					case TextMode.Blended:
						Engine.ConvertSurface(SDL_ttf.TTF_RenderText_Blended(this.font, this.text, this.textcolor), renderer, "{0}_Blended".F(this.text), new Vector2<int>(1, 1));
					break;
					case TextMode.Wrapped:
						Engine.ConvertSurface(SDL_ttf.TTF_RenderText_Blended_Wrapped(this.font, this.text, this.textcolor, 180), renderer, "{0}_Wrapped".F(this.text), new Vector2<int>(1, 1));
						break;
					case TextMode.Shaded:
						Engine.ConvertSurface(SDL_ttf.TTF_RenderText_Shaded(this.font, this.text, this.textcolor, this.backcolor), renderer, "{0}_Shaded".F(this.text), new Vector2<int>(1,1));
						break;
					default:
						break;
				}
				
				this.texture = Engine.GetTexture(this.alias, renderer, new Vector2<int>(1, 1));
				SDL.SDL_RenderCopy(renderer, this.texture.Image, ref this.source, ref this.target);
			}
		}

		public void print(string text, string alias, TextMode mode, int x = 20, int y = 20)
		{
			this.mode = mode;
			this.text = text;

			if (this.text.Length > 0)
				if (this.renderer != IntPtr.Zero)
				{
					this.texture = Engine.GetTexture(alias, renderer, new Vector2<int>(1,1));
					if (this.texture.Image == IntPtr.Zero)
						Game.Print(LogType.Error, this.GetType().ToString(), "Text->Print(): {0}".F(SDL.SDL_GetError()));
					else
					{
						this.target.x = x;
						this.target.y = y;

						if (SDL.SDL_QueryTexture(this.texture.Image, out this.format, out this.access, out this.target.w, out this.target.h) != 0)
							Game.Print(LogType.Error, this.GetType().ToString(), SDL.SDL_GetError());
					}
				}
		}

		public void Events(SDL.SDL_Event e)
		{

		}
	}
}
