# Terraria-Hardware-Cursor
Instructions for manually patching your Terraria to use hardware cursor on windows.

I have tested this patch and it does not stop me from joining my patchless server, with or without imposed security.

Requires Terraria and dnSpy (free)

Instructions:

Make a backup of Terraria.exe unless you really really don't feel like it.

To obtain the ReLogic and NewtonSoft DLLs, navigate to Terraria.exe in dnSpy, there should be a "Resources" folder near "PE", "References", "{ } -", "{ } Extensions", etc. Open up "Resources", there should be a list of json and dll files here. Click "Terraria.Libraries.ReLogic.ReLogic.dll" & "Terraria.Libraries.JSON.NET.Newtonsoft.Json.dll" and now there should be a "save" button in the contents window. Save the DLLs next to Terraria.exe on your drive and now drag both DLLs from file explorer into your dnSpy window.

1. Open Terraria.exe using dnSpy. Also make sure the 2 DLLs are open in the same dnSpy window (necessary to be able to recompile C# code and avoid needing assembly edits)
2. Edit(on the menu bar) -> Merge with Assembly... -> Choose "Terraria hardware cursor.dll" (shipped with this repo). Now there should be a HardwareCursor class within the Terraria namespace.
3. Ctrl+Shift+S to save assembly (this step is required here in order to reference members of the merged DLL in the next steps)
4. Still within dnSpy, navigate to Terraria.Main.Initialize_AlmostEverything()
5. Right-click inside the function body and "Edit Method (C#)...", scroll to the bottom
6. After "ItemSorting.SetupWhiteLists();", add: "HardwareCursor.APPEND_METHOD_Main_Initialize_AlmostEverything();" and press compile
7. In Terraria.Main.DoUpdate(ref GameTime), search for "base.IsMouseVisible = false;" (there should only be 1 result) and replace it with with: HardwareCursor.AMMEND_METHOD_Main_DoUpdate_where_base_IsMouseVisible_is_set_to_false(this);
8. In Terraria.Main.DrawCursor(Vector2, bool) replace the first 2 calls to Main.spriteBatch.Draw() with this 1 line of code: HardwareCursor.AMMEND_METHOD_Main_DrawCursor_First_Two_Calls_To_Main_spriteBatch_Draw(bonus, color, num);
9. In Terraria.Main.DrawThickCursor(bool) right near the bottom, replace the single call to Main.spriteBatch.Draw() with this: HardwareCursor.AMMEND_METHOD_Main_DrawThickCursor_Call_To_Main_spriteBatch_Draw(num, vector, color, origin, scale);
10. Ctrl+Shift+S to save assembly
