using System.IO;
using DLS.Game;
using DLS.SaveSystem;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics
{
	public static class ExternalRomFileMenu
	{
		static SubChipInstance romChip;
		static string romFolderPath;
		static string[] binFileNames;
		static bool hasFiles;

		static readonly UIHandle ID_FileSelector = new("ExternalRomFileSelect_FileWheel");

		public static string RomFolderPath => Path.Combine(SavePaths.AllData, "ROM");

		public static void OnMenuOpened()
		{
			romChip = (SubChipInstance)ContextMenu.interactionContext;
			romFolderPath = RomFolderPath;
			Directory.CreateDirectory(romFolderPath);

			string[] filePaths = Directory.GetFiles(romFolderPath, "*.bin");
			hasFiles = filePaths.Length > 0;

			if (hasFiles)
			{
				binFileNames = new string[filePaths.Length];
				for (int i = 0; i < filePaths.Length; i++)
				{
					binFileNames[i] = Path.GetFileName(filePaths[i]);
				}

				// Pre-select the currently loaded file if there is one
				int preselect = 0;
				if (!string.IsNullOrEmpty(romChip.Label))
				{
					for (int i = 0; i < binFileNames.Length; i++)
					{
						if (binFileNames[i] == romChip.Label)
						{
							preselect = i;
							break;
						}
					}
				}

				UI.GetWheelSelectorState(ID_FileSelector).index = preselect;
			}
			else
			{
				binFileNames = new string[] { "No .bin files found" };
			}
		}

		public static void DrawMenu()
		{
			MenuHelper.DrawBackgroundOverlay();
			Draw.ID panelID = UI.ReservePanel();
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			Vector2 pos = UI.Centre + Vector2.up * (UI.HalfHeight * 0.25f);

			using (UI.BeginBoundsScope(true))
			{
				UI.DrawText("Load External ROM", theme.FontBold, theme.FontSizeRegular, pos, Anchor.TextCentre, Color.white * 0.9f);

				Vector2 subtitlePos = UI.PrevBounds.CentreBottom + Vector2.down * 1.5f;
				string subtitle = "Place .bin files in: " + romFolderPath;
				UI.DrawText(subtitle, theme.FontRegular, theme.FontSizeRegular * 0.8f, subtitlePos, Anchor.TextCentre, Color.white * 0.5f);

				Vector2 wheelPos = UI.PrevBounds.CentreBottom + Vector2.down * DrawSettings.VerticalButtonSpacing;
				Vector2 wheelSize = new(UI.Width * 0.35f, DrawSettings.SelectorWheelHeight);
				int selectedIndex = UI.WheelSelector(ID_FileSelector, binFileNames, wheelPos, wheelSize, MenuHelper.Theme.OptionsWheel, Anchor.CentreTop);

				MenuHelper.CancelConfirmResult result = MenuHelper.DrawCancelConfirmButtons(UI.GetCurrentBoundsScope().BottomLeft, UI.GetCurrentBoundsScope().Width, true);
				MenuHelper.DrawReservedMenuPanel(panelID, UI.GetCurrentBoundsScope());

				if (result == MenuHelper.CancelConfirmResult.Cancel || KeyboardShortcuts.CancelShortcutTriggered())
				{
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
				else if (result == MenuHelper.CancelConfirmResult.Confirm && hasFiles)
				{
					string selectedFileName = binFileNames[selectedIndex];
					romChip.Label = selectedFileName;
					LoadFileIntoChip(romChip, selectedFileName);
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
			}
		}

		/// <summary>
		/// Loads a .bin file from the ROM folder into the chip's InternalData.
		/// </summary>
		public static void LoadFileIntoChip(SubChipInstance chip, string fileName)
		{
			string filePath = Path.Combine(RomFolderPath, fileName);
			if (!File.Exists(filePath)) return;

			byte[] bytes = File.ReadAllBytes(filePath);

			uint[] data = new uint[256];
			int byteCount = Mathf.Min(bytes.Length, 512);

			for (int i = 0; i < byteCount / 2; i++)
			{
				uint low = bytes[i * 2];
				uint high = (i * 2 + 1 < byteCount) ? (uint)bytes[i * 2 + 1] : 0u;
				data[i] = (high << 8) | low;
			}

			// Handle odd trailing byte
			if (byteCount % 2 == 1)
			{
				int addr = byteCount / 2;
				if (addr < 256)
				{
					data[addr] = bytes[byteCount - 1];
				}
			}

			for (int i = 0; i < 256; i++)
			{
				chip.InternalData[i] = data[i];
			}

			Project.ActiveProject.NotifyRomContentsEdited(chip);
		}

		/// <summary>
		/// Reloads all ExternalROM chips in the current view from their associated .bin files.
		/// Called on window focus to pick up external file changes.
		/// </summary>
		public static void ReloadAllExternalRoms()
		{
			if (Project.ActiveProject == null) return;

			DevChipInstance viewedChip = Project.ActiveProject.ViewedChip;
			if (viewedChip == null) return;

			foreach (SubChipInstance subChip in viewedChip.GetSubchips())
			{
				if (subChip.ChipType == DLS.Description.ChipType.ExternalRom_256x16 && !string.IsNullOrEmpty(subChip.Label))
				{
					LoadFileIntoChip(subChip, subChip.Label);
				}
			}
		}
	}
}
