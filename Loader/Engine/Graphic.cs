using System;
using System.Drawing;
using static SDL2.SDL;
using static SDL2.SDL_image;

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

	public class Graphic
	{
		public enum LoadIMGFlags { normal, texture }
		public delegate void VideoInitDoneEventHandler(object source, FinishEventArgs args);
		public delegate void VideoInitErrorEventHandler(object source, ErrorEventArgs args);
		public delegate void VideoInitStateEventHandler(object source, StateEventArgs args);

		public event VideoInitDoneEventHandler VideoInitDone;
		public event VideoInitStateEventHandler VideoInitState;
		public event VideoInitErrorEventHandler VideoInitError;

		#region private Fields
		IntPtr window, renderer;
		RenderType renderType;
		SDL_WindowFlags flags;
		int width;
		int height;
		string title;

		#endregion

		/// <summary>
		/// Bit per Pixel (from Primary Display)
		/// </summary>
		public int BitsPerPixel { get { return 32; } }

		/// <summary>
		///	Returns the current Window context 
		/// </summary>
		public IntPtr Window { get { return this.window; } }

		/// <summary>
		///	Returns the current Window surface
		/// </summary>
		public IntPtr WindowSurface { get { return SDL_GetWindowSurface(this.window); } }

		/// <summary>
		///	Returns the current Renderer context 
		/// </summary>
		public IntPtr Renderer { get { return this.renderer; } }

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

			OnVideoInitState(GetType().ToString(), "Waiting for the Video subsystem...");
			this.renderType = RenderType.SDL;

			var errnum = -1;
			this.flags = config.Engine.WindowFlags;

			errnum = SDL_Init(SDL_INIT_VIDEO);
			if (errnum == -1)
				OnVideoInitError(GetType().ToString(), "SDL_Init(): " + SDL_GetError());

			errnum = IMG_Init(IMG_InitFlags.IMG_INIT_PNG);
			if (errnum == -1)
				OnVideoInitError(GetType().ToString(), "IMG_Init(): " + SDL_GetError());

			OnVideoInitState(GetType().ToString(), "Image subsystem initialized!");

			this.window = SDL_CreateWindow(this.title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, this.width, this.height, this.flags);

			if (this.window == null)
				OnVideoInitError(GetType().ToString(), "SDL_CreateWindow(): " + SDL_GetError());
			else
			{
				OnVideoInitState(GetType().ToString(), "Window created!");

				this.renderer = SDL_CreateRenderer(this.window, -1, 6);

				if (this.renderer == IntPtr.Zero)
					OnVideoInitError(GetType().ToString(), "SDL_CreateRenderer():" + SDL_GetError());
				else
				{
					errnum = 0;
					OnVideoInitState(GetType().ToString(), "Renderer created!");

					if (errnum == 0)
						OnVideoInitDone("Video Subsystem initialized...");
				}
			}
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
					return IMG_LoadTexture(renderer, filename);
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
		}

		#region Events
		protected virtual void OnVideoInitDone(string message)
		{
			var fe = new FinishEventArgs();
			fe.Source = GetType().ToString();
			fe.Message = message;

			VideoInitDone?.Invoke(this, fe);
		}

		protected virtual void OnVideoInitError(string source, string ErrorMessage)
		{
			var errvtargs = new ErrorEventArgs();

			errvtargs.Message = ErrorMessage;
			errvtargs.Source = source;

			VideoInitError?.Invoke(this, errvtargs);
		}

		protected virtual void OnVideoInitState(string source, string ErrorMessage)
		{
			var statevtargs = new StateEventArgs();

			statevtargs.Message = ErrorMessage;
			statevtargs.Source = source;

			VideoInitState?.Invoke(this, statevtargs);
		}
#endregion
		
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
		public void Begin(Color color)
		{
			SDL_SetRenderDrawColor(this.renderer, color.R, color.G, color.B, color.A);
			SDL_RenderClear(this.renderer);
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
		public static void DrawRect(IntPtr renderer, int x, int y, int w, int h, Color color)
		{
			var rect = new SDL_Rect();
			rect.h = h;
			rect.w = w;

			rect.x = x;
			rect.y = y;

			SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
			SDL_RenderDrawRect(renderer, ref rect);
			SDL_SetRenderDrawColor(renderer, byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);
		}
	}
}
