using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Terraria.GameContent;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/* C# project properties:

.NET Framework 4.8
32bit
windows class library

 */

/* C# project references:
	Microsoft.Xna.Framework
	Microsoft.Xna.Framework.Game
	Microsoft.Xna.Framework.Graphics
	System
	System.Drawing
	System.Windows
	System.Windows.Forms
	Terraria
	Terraria.Libraries.ReLogic.ReLogic
 */

namespace Terraria
{
    public class HardwareCursor
    {
        public static float masterCursorScale = 1.3f; // this will need tweaking to perfect the cursor size
		public static int cursorSize = 64; // this might need changing on earlier operating systems
        public static Bitmap cursorBitmap;
        public static Bitmap[] cursorBitmaps;
		public static Form formCached;
		public static IntPtr formCursorIconHandleCache = IntPtr.Zero;

		/* Instructions:
		 * 
		 * 1. Open Terraria.exe using dnSpy. Also open "Terraria.Libraries.ReLogic.ReLogic.dll" in the same dnSpy window (necessary to be able to recompile C# code and avoid needing assembly edits)
		 * 
		 *    - To obtain "Terraria.Libraries.ReLogic.ReLogic.dll", navigate to Terraria.exe in dnSpy, there should be a "Resources" folder near "PE", "References", "{ } -", "{ } Extensions", etc.
		 *      Open up "Resources", there should be a list of json and dll files here. Click "Terraria.Libraries.ReLogic.ReLogic.dll" and now there should be a "save" button in the contents window.
		 *      It's a bit weird but that's all there is to it. Save the dll next to Terraria.exe on your drive and now drag "Terraria.Libraries.ReLogic.ReLogic.dll" from file explorer into your dnSpy window.
		 *       
		 * 2. Edit(on the menu bar) -> Merge with Assembly... -> Choose "Terraria hardware cursor.dll" (shipped with this repo). Now there should be a HardwareCursor class within the Terraria namespace.
		 * 
		 * 3. Ctrl+Shift+S to save assembly (this step is required here in order to reference members of the merged DLL in the next steps)
		 * 
		 * 4. Still within dnSpy, navigate to Terraria.Main.Initialize_AlmostEverything()
		 * 
		 * 5. Right-click inside the function body and "Edit Method (C#)...", scroll to the bottom
		 * 
		 * 6. After "ItemSorting.SetupWhiteLists();", add: "HardwareCursor.APPEND_METHOD_Main_Initialize_AlmostEverything();" and press compile
		 * 
		 * 7. In Terraria.Main.DoUpdate(ref GameTime), search for "base.IsMouseVisible = false;" (there should only be 1 result) and replace it with with:
		 *      HardwareCursor.AMMEND_METHOD_Main_DoUpdate_where_base_IsMouseVisible_is_set_to_false(this);
		 *      
		 * 8. In Terraria.Main.DrawCursor(Vector2, bool) replace the first 2 calls to Main.spriteBatch.Draw() with this 1 line of code:
		 *      HardwareCursor.AMMEND_METHOD_Main_DrawCursor_First_Two_Calls_To_Main_spriteBatch_Draw(bonus, color, num);
		 *      
		 * 9. In Terraria.Main.DrawThickCursor(bool) right near the bottom, replace the single call to Main.spriteBatch.Draw() with this:
		 *      HardwareCursor.AMMEND_METHOD_Main_DrawThickCursor_Call_To_Main_spriteBatch_Draw(num, vector, color, origin, scale);
		 *      
		 * 10. Ctrl+Shift+S to save assembly
		 * 
		 */

		public static void AMMEND_METHOD_Main_DoUpdate_where_base_IsMouseVisible_is_set_to_false(Microsoft.Xna.Framework.Game _this)
		{
			formCached = Control.FromHandle(_this.Window.Handle) as Form;
			_this.IsMouseVisible = true;
		}

		public static void AMMEND_METHOD_Main_DrawThickCursor_Call_To_Main_spriteBatch_Draw(int num, Microsoft.Xna.Framework.Vector2 vector, Microsoft.Xna.Framework.Color color, Microsoft.Xna.Framework.Vector2 origin, float scale)
        {
			DrawCursorToBitmap(cursorBitmaps[num], vector, color, origin, scale * masterCursorScale);
		}

		public static void AMMEND_METHOD_Main_DrawCursor_First_Two_Calls_To_Main_spriteBatch_Draw(Microsoft.Xna.Framework.Vector2 bonus, Microsoft.Xna.Framework.Color color, int num)
        {
			DrawCursorToBitmap(cursorBitmaps[num], bonus + Microsoft.Xna.Framework.Vector2.One, new Microsoft.Xna.Framework.Color((int)((float)color.R * 0.2f), (int)((float)color.G * 0.2f), (int)((float)color.B * 0.2f), (int)((float)color.A * 0.6f)), default(Microsoft.Xna.Framework.Vector2), Main.cursorScale * masterCursorScale * 1.1f);
			color.A = 245;
			DrawCursorToBitmap(cursorBitmaps[num], bonus, color, default(Microsoft.Xna.Framework.Vector2), Main.cursorScale * masterCursorScale);
			SetCursorToBitmap();
		}

		public static void APPEND_METHOD_Main_Initialize_AlmostEverything()
		{
			cursorBitmap = new Bitmap(cursorSize, cursorSize);
			cursorBitmaps = new Bitmap[TextureAssets.Cursors.Length];
			for (int n = 0; n < TextureAssets.Cursors.Length; n++)
			{
				using (Stream stream = new MemoryStream())
				{
					TextureAssets.Cursors[n].Value.SaveAsPng(stream, TextureAssets.Cursors[n].Value.Width, TextureAssets.Cursors[n].Value.Height);
					cursorBitmaps[n] = new Bitmap(stream);
				}
			}
		}

		public static void DrawCursorToBitmap(Bitmap bitmapIn, Microsoft.Xna.Framework.Vector2 offset, Microsoft.Xna.Framework.Color color, Microsoft.Xna.Framework.Vector2 origin, float scale)
		{
			using (Bitmap bitmap = new Bitmap(bitmapIn))
			{
				for (int i = 0; i < bitmap.Width; i++)
				{
					for (int j = 0; j < bitmap.Height; j++)
					{
						System.Drawing.Color pixel = bitmap.GetPixel(i, j);
						if (pixel.A > 0)
						{
							Microsoft.Xna.Framework.Color color2 = new Microsoft.Xna.Framework.Color((int)pixel.R, (int)pixel.G, (int)pixel.B, (int)pixel.A).MultiplyRGBA(color);
							bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb((int)color2.A, (int)color2.R, (int)color2.G, (int)color2.B));
						}
					}
				}
				using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(cursorBitmap))
				{
					float width = (float)bitmap.Width * scale;
					float height = (float)bitmap.Height * scale;
					graphics.InterpolationMode = InterpolationMode.High;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.DrawImage(bitmap, offset.X - origin.X + (float)cursorBitmap.Width / 2f, offset.Y - origin.Y + (float)cursorBitmap.Height / 2f, width, height);
				}
			}
		}

		public static void SetCursorToBitmap()
		{
			IntPtr handle = formCached.Cursor.CopyHandle();
			IntPtr hicon = cursorBitmap.GetHicon();
			Cursor cursor = formCached.Cursor;
			formCached.Cursor = new Cursor(hicon);
			if (formCursorIconHandleCache != IntPtr.Zero)
			{
				DestroyIcon(formCursorIconHandleCache);
			}
			cursor.Dispose();
			DestroyCursor(handle);
			formCursorIconHandleCache = hicon;
			using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(cursorBitmap))
			{
				graphics.Clear(Color.FromArgb(0));
			}
		}


		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool DestroyCursor(IntPtr handle);


		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool DestroyIcon(IntPtr handle);
	}
}
