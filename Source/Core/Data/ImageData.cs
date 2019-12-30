
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public abstract unsafe class ImageData : IDisposable
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		// Properties
		protected string name;
		protected long longname;
		protected int width;
		protected int height;
		protected Vector2D scale;
		protected bool worldpanning;
		private bool usecolorcorrection;
		protected string filepathname; //mxd. Absolute path to the image;
		protected string shortname; //mxd. Name in uppercase and clamped to DataManager.CLASIC_IMAGE_NAME_LENGTH
		protected string virtualname; //mxd. Path of this name is used in TextureBrowserForm
		protected string displayname; //mxd. Name to display in TextureBrowserForm
		protected bool isFlat; //mxd. If false, it's a texture
		protected bool istranslucent; //mxd. If true, has pixels with alpha > 0 && < 255 
		protected bool ismasked; //mxd. If true, has pixels with zero alpha
		protected bool hasLongName; //mxd. Texture name is longer than DataManager.CLASIC_IMAGE_NAME_LENGTH
		protected bool hasPatchWithSameName; //mxd
		protected int namewidth; // biwa
		protected int shortnamewidth; // biwa

		//mxd. Hashing
		private static int hashcounter;
		private readonly int hashcode;

        // Loading
        private ImageLoadState previewstate;
        private ImageLoadState imagestate;
        private bool loadfailed;

        // GDI bitmap
        private Bitmap bitmap;
        private Bitmap previewbitmap;

        // Direct3D texture
        private int mipmaplevels;	// 0 = all mipmaps
		protected bool dynamictexture;
		private Texture texture;
		
		// Disposing
		protected bool isdisposed;
		
		#endregion
		
		#region ================== Properties
		
		public string Name { get { return name; } }
		public long LongName { get { return longname; } }
		public string ShortName { get { return shortname; } } //mxd
		public string FilePathName { get { return filepathname; } } //mxd
		public string VirtualName { get { return virtualname; } } //mxd
		public string DisplayName { get { return displayname; } } //mxd
		public bool IsFlat { get { return isFlat; } } //mxd
		public bool IsTranslucent { get { return istranslucent; } } //mxd
		public bool IsMasked { get { return ismasked; } } //mxd
		public bool HasPatchWithSameName { get { return hasPatchWithSameName; } } //mxd
		internal bool HasLongName { get { return hasLongName; } } //mxd
		public bool UseColorCorrection { get { return usecolorcorrection; } set { usecolorcorrection = value; } }
		public Texture Texture { get { return GetTexture(); } }
		public bool IsPreviewLoaded { get { return (previewstate == ImageLoadState.Ready); } }
		public bool IsImageLoaded { get { return (imagestate == ImageLoadState.Ready); } }
		public bool LoadFailed { get { return loadfailed; } }
		public bool IsDisposed { get { return isdisposed; } }
		public bool AllowUnload { get; set; }
		public ImageLoadState ImageState { get { return imagestate; } internal set { imagestate = value; } }
		public ImageLoadState PreviewState { get { return previewstate; } internal set { previewstate = value; } }
		public bool UsedInMap { get; internal set; }
		public int MipMapLevels { get { return mipmaplevels; } set { mipmaplevels = value; } }
		public virtual int Width { get { return width; } }
		public virtual int Height { get { return height; } }
		//mxd. Scaled texture size is integer in ZDoom.
		public virtual float ScaledWidth { get { return (float)Math.Round(width * scale.x); } }
		public virtual float ScaledHeight { get { return (float)Math.Round(height * scale.y); } }
		public virtual Vector2D Scale { get { return scale; } }
		public bool WorldPanning { get { return worldpanning; } }
		public int NameWidth {  get { return namewidth; } } // biwa
		public int ShortNameWidth { get { return shortnamewidth; } } // biwa

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		protected ImageData()
		{
			// Defaults
			usecolorcorrection = true;
			AllowUnload = true;

			//mxd. Hashing
			hashcode = hashcounter++;
		}

		// Destructor
		~ImageData()
		{
			this.Dispose();
		}
		
		// Disposer
		public virtual void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				bitmap?.Dispose();
				texture?.Dispose();
				bitmap = null;
				texture = null;
					
				// Done
				imagestate = ImageLoadState.None;
				previewstate = ImageLoadState.None;
				isdisposed = true;
			}
		}
		
		#endregion
		
		#region ================== Management
		
		// This adds a reference
		// This sets the name
		protected virtual void SetName(string name)
		{
			this.name = name;
			this.filepathname = name; //mxd
			this.shortname = name; //mxd
			this.virtualname = name; //mxd
			this.displayname = name; //mxd
			this.longname = Lump.MakeLongName(name); //mxd

			ComputeNamesWidth(); // biwa
		}
		
		// biwa. Computing the widths in the constructor of ImageBrowserItem accumulates to taking forever when loading many images,
		// like when showing the texture browser of huge texture sets like OTEX
		internal void ComputeNamesWidth()
		{
			//mxd. Calculate names width
			namewidth = (int)Math.Ceiling(General.Interface.MeasureString(name, SystemFonts.MessageBoxFont, 10000, StringFormat.GenericTypographic).Width) + 6;
			shortnamewidth = (int)Math.Ceiling(General.Interface.MeasureString(shortname, SystemFonts.MessageBoxFont, 10000, StringFormat.GenericTypographic).Width) + 6;
		}

		// This returns the bitmap image
		public Bitmap GetBitmap()
		{
			// Image loaded successfully?
			if(!loadfailed && (imagestate == ImageLoadState.Ready) && (bitmap != null))
				return bitmap;
				
			// Image loading failed?
			return (loadfailed ? Properties.Resources.Failed : Properties.Resources.Hourglass);
		}

        // Loads the image directly. This is needed by the background loader for some patches.
        public Bitmap LocalGetBitmap()
        {
            // Note: if this turns out to be too slow, do NOT try to make it use GetBitmap or bitmap.
            // Create a cache for the local background loader thread instead.

            LocalLoadResult result = LocalLoadImage();
            if (result.messages.Any(x => x.Type == ErrorType.Error))
            {
                return Properties.Resources.Failed;
            }
            ConvertImageFormat(result);
            return result.bitmap;
        }
		
        public void LoadImage()
        {
            LoadImage(true);
        }

		// This loads the image
		public virtual void LoadImage(bool notify)
		{
            if (imagestate == ImageLoadState.Ready && previewstate != ImageLoadState.Loading)
                return;

            // Do the loading
            LocalLoadResult loadResult = LocalLoadImage();

            ConvertImageFormat(loadResult);
            MakeImagePreview(loadResult);

            // Save memory by disposing the original image immediately if we only used it to load a preview image
            bool onlyPreview = false;
            if (imagestate == ImageLoadState.Ready)
            {
                loadResult.bitmap?.Dispose();
                onlyPreview = true;
            }

            General.MainWindow.RunOnUIThread(() =>
            {
                if (imagestate != ImageLoadState.Ready && !onlyPreview)
                {
                    // Log errors and warnings
                    foreach (LogMessage message in loadResult.messages)
                    {
                        General.ErrorLogger.Add(message.Type, message.Text);
                    }

                    if (loadResult.messages.Any(x => x.Type == ErrorType.Error))
                    {
                        loadfailed = true;
                    }

                    bitmap?.Dispose();
                    texture?.Dispose();
                    imagestate = ImageLoadState.Ready;
                    bitmap = loadResult.bitmap;

                    if (loadResult.uiThreadWork != null)
                        loadResult.uiThreadWork();
                }
                else
                {
                    loadResult.bitmap?.Dispose();
                }

                if (previewstate == ImageLoadState.Loading)
                {
                    previewbitmap?.Dispose();
                    previewstate = ImageLoadState.Ready;
                    previewbitmap = loadResult.preview;
                }
                else
                {
                    loadResult.preview?.Dispose();
                }
            });

            // Notify the main thread about the change so that sectors can update their buffers
            if (notify) General.MainWindow.ImageDataLoaded(this.name);
		}

        protected class LocalLoadResult
        {
            public LocalLoadResult(Bitmap bitmap, string error = null, Action uiThreadWork = null)
            {
                this.bitmap = bitmap;
                messages = new List<LogMessage>();
                if (error != null)
                    messages.Add(new LogMessage(ErrorType.Error, error));
                this.uiThreadWork = uiThreadWork;
            }

            public LocalLoadResult(Bitmap bitmap, IEnumerable<LogMessage> messages, Action uiThreadWork = null)
            {
                this.bitmap = bitmap;
                this.messages = messages.ToList();
                this.uiThreadWork = uiThreadWork;
            }

            public Bitmap bitmap;
            public Bitmap preview;
            public List<LogMessage> messages;
            public Action uiThreadWork;
        }

        protected abstract LocalLoadResult LocalLoadImage();
		
        protected class LogMessage
        {
            public LogMessage(ErrorType type, string text) { Type = type; Text = text; }
            public ErrorType Type { get; set; }
            public string Text { get; set; }
        }

        void ConvertImageFormat(LocalLoadResult loadResult)
		{
            // Bitmap loaded successfully?
            Bitmap bitmap = loadResult.bitmap;
			if(bitmap != null)
			{
				// Bitmap has incorrect format?
				if(bitmap.PixelFormat != PixelFormat.Format32bppArgb)
				{
					if(dynamictexture)
						throw new Exception("Dynamic images must be in 32 bits ARGB format.");
						
					//General.ErrorLogger.Add(ErrorType.Warning, "Image '" + name + "' does not have A8R8G8B8 pixel format. Conversion was needed.");
					Bitmap oldbitmap = bitmap;
					try
					{
						// Convert to desired pixel format
						bitmap = new Bitmap(oldbitmap.Size.Width, oldbitmap.Size.Height, PixelFormat.Format32bppArgb);
						Graphics g = Graphics.FromImage(bitmap);
						g.PageUnit = GraphicsUnit.Pixel;
						g.CompositingQuality = CompositingQuality.HighQuality;
						g.InterpolationMode = InterpolationMode.NearestNeighbor;
						g.SmoothingMode = SmoothingMode.None;
						g.PixelOffsetMode = PixelOffsetMode.None;
						g.Clear(Color.Transparent);
						g.DrawImage(oldbitmap, 0, 0, oldbitmap.Size.Width, oldbitmap.Size.Height);
						g.Dispose();
						oldbitmap.Dispose();
					}
					catch(Exception e)
					{
						bitmap = oldbitmap;
						loadResult.messages.Add(new LogMessage(ErrorType.Warning, "Cannot lock image \"" + name + "\" for pixel format conversion. The image may not be displayed correctly.\n" + e.GetType().Name + ": " + e.Message));
					}
				}
					
				// This applies brightness correction on the image
				if(usecolorcorrection)
				{
					BitmapData bmpdata = null;
						
					try
					{
						// Try locking the bitmap
						bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
					}
					catch(Exception e)
					{
                        loadResult.messages.Add(new LogMessage(ErrorType.Warning, "Cannot lock image \"" + name + "\" for color correction. The image may not be displayed correctly.\n" + e.GetType().Name + ": " + e.Message));
					}

					// Bitmap locked?
					if(bmpdata != null)
					{
						// Apply color correction
						PixelColor* pixels = (PixelColor*)(bmpdata.Scan0.ToPointer());
						General.Colors.ApplyColorCorrection(pixels, bmpdata.Width * bmpdata.Height);
						bitmap.UnlockBits(bmpdata);
					}
				}
			}
			else
			{
				// Loading failed
				// We still mark the image as ready so that it will
				// not try loading again until Reload Resources is used
				bitmap = new Bitmap(Properties.Resources.Failed);
			}

			if(bitmap != null)
			{
				width = bitmap.Size.Width;
				height = bitmap.Size.Height;

				if(dynamictexture)
				{
					if((width != General.NextPowerOf2(width)) || (height != General.NextPowerOf2(height)))
						throw new Exception("Dynamic images must have a size in powers of 2.");
				}

				// Do we still have to set a scale?
				if((scale.x == 0.0f) && (scale.y == 0.0f))
				{
					if((General.Map != null) && (General.Map.Config != null))
					{
						scale.x = General.Map.Config.DefaultTextureScale;
						scale.y = General.Map.Config.DefaultTextureScale;
					}
					else
					{
						scale.x = 1.0f;
						scale.y = 1.0f;
					}
				}

				if(!loadfailed)
				{
					//mxd. Check translucency and calculate average color?
					if(General.Map != null && General.Map.Data != null && General.Map.Data.GlowingFlats != null &&
						General.Map.Data.GlowingFlats.ContainsKey(longname) &&
						General.Map.Data.GlowingFlats[longname].CalculateTextureColor)
					{
						BitmapData bmpdata = null;
						try
						{
							bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
						}
						catch(Exception e)
						{
                            loadResult.messages.Add(new LogMessage(ErrorType.Error, "Cannot lock image \"" + this.filepathname + "\" for glow color calculation. " + e.GetType().Name + ": " + e.Message));
						}

						if(bmpdata != null)
						{
							PixelColor* pixels = (PixelColor*)(bmpdata.Scan0.ToPointer());
							int numpixels = bmpdata.Width * bmpdata.Height;
							uint r = 0;
							uint g = 0;
							uint b = 0;

							for(PixelColor* cp = pixels + numpixels - 1; cp >= pixels; cp--)
							{
								r += cp->r;
								g += cp->g;
								b += cp->b;

								// Also check alpha
								if(cp->a > 0 && cp->a < 255) istranslucent = true;
								else if(cp->a == 0) ismasked = true;
							}

							// Update glow data
							int br = (int)(r / numpixels);
							int bg = (int)(g / numpixels);
							int bb = (int)(b / numpixels);

							int max = Math.Max(br, Math.Max(bg, bb));

							// Black can't glow...
							if(max == 0)
							{
								General.Map.Data.GlowingFlats.Remove(longname);
							}
							else
							{
								// That's how it's done in GZDoom (and I may be totally wrong about this)
								br = Math.Min(255, br * 153 / max);
								bg = Math.Min(255, bg * 153 / max);
								bb = Math.Min(255, bb * 153 / max);

								General.Map.Data.GlowingFlats[longname].Color = new PixelColor(255, (byte)br, (byte)bg, (byte)bb);
								General.Map.Data.GlowingFlats[longname].CalculateTextureColor = false;
								if(!General.Map.Data.GlowingFlats[longname].Fullbright) General.Map.Data.GlowingFlats[longname].Brightness = (br + bg + bb) / 3;
							}

							// Release the data
							bitmap.UnlockBits(bmpdata);
						}
					}
					//mxd. Check if the texture is translucent
					else
					{
						BitmapData bmpdata = null;
						try
						{
							bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
						}
						catch(Exception e)
						{
                            loadResult.messages.Add(new LogMessage(ErrorType.Error, "Cannot lock image \"" + this.filepathname + "\" for translucency check. " + e.GetType().Name + ": " + e.Message));
						}

						if(bmpdata != null)
						{
							PixelColor* pixels = (PixelColor*)(bmpdata.Scan0.ToPointer());
							int numpixels = bmpdata.Width * bmpdata.Height;

							for(PixelColor* cp = pixels + numpixels - 1; cp >= pixels; cp--)
							{
								// Check alpha
								if(cp->a > 0 && cp->a < 255) istranslucent = true;
								else if(cp->a == 0) ismasked = true;
							}

							// Release the data
							bitmap.UnlockBits(bmpdata);
						}
					}
				}
			}

            loadResult.bitmap = bitmap;
		}

        // Dimensions of a single preview image
        const int MAX_PREVIEW_SIZE = 256; //mxd

        // This makes a preview for the given image and updates the image settings
        private void MakeImagePreview(LocalLoadResult loadResult)
        {
            if (loadResult.bitmap == null)
                return;

            Bitmap image = loadResult.bitmap;
            Bitmap preview;

            int imagewidth = image.Width;
            int imageheight = image.Height;

            // Determine preview size
            float scalex = (imagewidth > MAX_PREVIEW_SIZE) ? (MAX_PREVIEW_SIZE / (float)imagewidth) : 1.0f;
            float scaley = (imageheight > MAX_PREVIEW_SIZE) ? (MAX_PREVIEW_SIZE / (float)imageheight) : 1.0f;
            float scale = Math.Min(scalex, scaley);
            int previewwidth = (int)(imagewidth * scale);
            int previewheight = (int)(imageheight * scale);
            if (previewwidth < 1) previewwidth = 1;
            if (previewheight < 1) previewheight = 1;

            //mxd. Expected and actual image sizes and format match?
            if (previewwidth == imagewidth && previewheight == imageheight && image.PixelFormat == PixelFormat.Format32bppArgb)
            {
                preview = new Bitmap(image);
            }
            else
            {
                // Make new image
                preview = new Bitmap(previewwidth, previewheight, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(preview);
                g.PageUnit = GraphicsUnit.Pixel;
                //g.CompositingQuality = CompositingQuality.HighQuality; //mxd
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                //g.SmoothingMode = SmoothingMode.HighQuality; //mxd
                g.PixelOffsetMode = PixelOffsetMode.None;
                //g.Clear(Color.Transparent); //mxd

                // Draw image onto atlas
                Rectangle atlasrect = new Rectangle(0, 0, previewwidth, previewheight);
                RectangleF imgrect = General.MakeZoomedRect(new Size(imagewidth, imageheight), atlasrect);
                if (imgrect.Width < 1.0f)
                {
                    imgrect.X -= 0.5f - imgrect.Width * 0.5f;
                    imgrect.Width = 1.0f;
                }
                if (imgrect.Height < 1.0f)
                {
                    imgrect.Y -= 0.5f - imgrect.Height * 0.5f;
                    imgrect.Height = 1.0f;
                }
                g.DrawImage(image, imgrect);
                g.Dispose();
            }

            loadResult.preview = preview;
        }

        Texture GetTexture()
		{
            if (texture != null)
                return texture;
            else if (imagestate == ImageLoadState.Loading)
                return General.Map.Data.LoadingTexture;
            else if (loadfailed)
                return General.Map.Data.FailedTexture;

            if (imagestate == ImageLoadState.None)
            {
                General.Map.Data.QueueLoadImage(this);
                return General.Map.Data.LoadingTexture;
            }

            texture = new Texture(General.Map.Graphics, bitmap);

            if (dynamictexture)
            {
                if ((width != texture.Width) || (height != texture.Height))
                    throw new Exception("Could not create a texture with the same size as the image.");
            }

#if DEBUG
			texture.Tag = name; //mxd. Helps with tracking undisposed resources...
#endif
            return texture;
		}

		// This updates a dynamic texture
		public void UpdateTexture()
		{
			if(!dynamictexture)
				throw new Exception("The image must be a dynamic image to support direct updating.");

			if((texture != null) && !texture.Disposed)
			{
                General.Map.Graphics.SetPixels(texture, bitmap);
			}
		}
		
		// This destroys the Direct3D texture
		public void ReleaseTexture()
		{
			texture?.Dispose();
			texture = null;
		}

		// This returns a preview image
		public virtual Image GetPreview()
		{
			// Preview ready?
			if(previewstate == ImageLoadState.Ready)
			{
				// Make a copy
				return new Bitmap(previewbitmap);
			}
				
			// Loading failed?
			if(loadfailed)
			{
				// Return error bitmap
				return Properties.Resources.Failed;
			}

			// Return loading bitmap
			return Properties.Resources.Hourglass;
		}

		//mxd. This greatly speeds up Dictionary lookups
		public override int GetHashCode()
		{
			return hashcode;
		}
		
		#endregion
	}
}
