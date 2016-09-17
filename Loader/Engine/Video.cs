using System;
using System.Drawing;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace RPGEngine
{
	public class Video
	{
		#region private Fields
		private IntPtr window, renderer;
		private SDL_WindowFlags flags;
		private int width, height;

		private string title;

		#endregion

		public delegate void VideoInitDoneEventHandler(object source, FinishEventArgs args);

		public delegate void VideoInitErrorEventHandler(object source, ErrorEventArgs args);

		public delegate void VideoInitStateEventHandler(object source, StateEventArgs args);

		public event VideoInitDoneEventHandler VideoInitDone;

		public event VideoInitStateEventHandler VideoInitState;

		public event VideoInitErrorEventHandler VideoInitError;

		public enum LoadIMGFlags
		{
			normal,
			texture
		}

		/// <summary>
		/// Bit per Pixel (from Primary Display)
		/// </summary>
		public int BitsPerPixel
		{
			get
			{
				return 32;
			}
		}

		/// <summary>
		///	Returns the current Window context 
		/// </summary>
		public IntPtr Window
		{
			get
			{
				return this.window;
			}
		}

		/// <summary>
		///	Returns the current Window surface
		/// </summary>
		public IntPtr WindowSurface
		{
			get
			{
				return SDL_GetWindowSurface(this.window);
			}
		}

		/// <summary>
		///	Returns the current Renderer context 
		/// </summary>
		public IntPtr Renderer
		{
			get
			{
				return this.renderer;
			}
		}

		/// <summary>
		/// Draws an Rectangle (SDL_Rect) on the screen (Window)
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="x">position X</param>
		/// <param name="y">position Y</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <param name="color">Color</param>
		public static int DrawRect(ref IntPtr renderer, int x, int y, int w, int h, Color color, bool fill = false)
		{
			var retval = -1;
			var rect = new SDL_Rect();
			rect.h = h;
			rect.w = w;

			rect.x = x;
			rect.y = y;

			retval = SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
			retval = SDL_RenderFillRect(renderer, ref rect);
			retval = SDL_SetRenderDrawColor(renderer, byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

			return retval;
		}

		/// <summary>
		/// Initialize the SDL Video Context
		/// </summary>
		/// <param name="title">Window Title</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <returns>-1 on Error or 0 on success</returns>
		public void Init(object obj)
		{
			var config = (Settings)obj;

			this.width = config.Engine.Width;
			this.height = config.Engine.Height;
			this.title = config.Game.Title;

			this.OnVideoInitState(this.GetType().ToString(), "Waiting for the Video Subsystem...");
			
			var errnum = -1;
			this.flags = config.Engine.WindowFlags;

			errnum = IMG_Init(IMG_InitFlags.IMG_INIT_PNG);
			if (errnum == -1)
				this.OnVideoInitError(this.GetType().ToString(), "IMG_Init(): {0}".F(SDL_GetError()));

			this.OnVideoInitState(this.GetType().ToString(), "Image subsystem initialized!");

			this.window = SDL_CreateWindow(this.title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, this.width, this.height, this.flags);

			if (this.window == IntPtr.Zero)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_CreateWindow(): {0}".F(SDL_GetError()));
			else
				this.OnVideoInitState(this.GetType().ToString(), "Window created!");

			this.renderer = SDL_CreateRenderer(this.window, 0, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

			if (this.renderer == IntPtr.Zero)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_CreateRenderer(): {0}".F(SDL_GetError()));
			else
				this.OnVideoInitState(this.GetType().ToString(), "Renderer created!");

			if (SDL_SetHint("SDL_HINT_RENDER_VSYNC", "1") == SDL_bool.SDL_FALSE)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_SetHint(): 'Vsync' is not enabled!");
			else
				this.OnVideoInitState(this.GetType().ToString(), "'VSync' enabled!");

			if (SDL_SetHint("SDL_HINT_RENDER_SCALE_QUALITY", "1") == SDL_bool.SDL_FALSE)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_SetHint(): 'Linear texture filtering' is not enabled!");
			else
				this.OnVideoInitState(this.GetType().ToString(), "'Linear texture filtering' enabled!");

			this.OnVideoInitDone("starting Game...");
		}

		/// <summary>
		/// Set (new) Window Title
		/// </summary>
		/// <param name="title">The (new) title of the Window</param>
		public void SetWindowTile(string title)
		{
			SDL_SetWindowTitle(this.Window, title);
		}

		/// <summary>
		/// Set Window position
		/// </summary>
		/// <param name="x">Horizontally Position</param>
		/// <param name="y">Vertical Position</param>
		public void SetWindowPosition(int x, int y)
		{
			SDL_SetWindowPosition(this.window, x, y);
		}

		/// <summary>
		///	Loads an Image from File and returns the desired type (Image or Texture). 
		/// </summary>
		/// <param name="filename">The Path to the Image</param>
		/// <param name="flags">Normal (Handle as Picture) or Texture (Handle as Texture)</param>
		/// <returns>On Normal a PNG, on Texture a Texture otherwise 0 for Error</returns>
		public IntPtr LoadPNG(string filename, LoadIMGFlags flags = LoadIMGFlags.texture)
		{
			switch (flags)
			{
				case LoadIMGFlags.normal:
					return IMG_Load(filename);
				case LoadIMGFlags.texture:
					return IMG_LoadTexture(this.renderer, filename);
				default:
					return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Closes the SDL Instances.
		/// </summary>
		public void Close()
		{
			IMG_Quit();
			SDL_VideoQuit();
			SDL_DestroyRenderer(this.renderer);
			SDL_DestroyWindow(this.window);
		}

		/// <summary>
		///	Reports the Video subsystem to flip / update the Window...
		/// </summary>
		/// <returns></returns>
		public void End()
		{
			SDL_RenderPresent(this.renderer);
		}

		/// <summary>
		/// Reports the Video subsystem to "begin" the rendering..
		/// </summary>
		/// <param name="color">Draw color</param>
		public int Begin(Color color)
		{
			var retval = -1;
			retval = SDL_SetRenderDrawColor(this.renderer, color.R, color.G, color.B, color.A);
			retval = SDL_RenderClear(this.renderer);

			return retval;
		}

		#region Events
		protected virtual void OnVideoInitDone(string message)
		{
			var fe = new FinishEventArgs();
			fe.Source = this.GetType().ToString();
			fe.Message = message;

			this.VideoInitDone?.Invoke(this, fe);
		}

		protected virtual void OnVideoInitError(string source, string message)
		{
			var errvtargs = new ErrorEventArgs();

			errvtargs.Message = message;
			errvtargs.Source = source;

			this.VideoInitError?.Invoke(this, errvtargs);
		}

		protected virtual void OnVideoInitState(string source, string message)
		{
			var statevtargs = new StateEventArgs();

			statevtargs.Message = message;
			statevtargs.Source = source;

			this.VideoInitState?.Invoke(this, statevtargs);
		}
		#endregion
	}
}
