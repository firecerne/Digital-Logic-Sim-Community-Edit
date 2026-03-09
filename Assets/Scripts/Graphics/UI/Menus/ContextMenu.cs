using System;
using System.Linq;
using DLS.Description;
using DLS.Game;
using Seb.Helpers;
using Seb.Types;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics
{
	public static class ContextMenu
	{
		const int pad = 10;
		const string menuDividerString = "#--#";
		static string interactionContextName;
		static bool bottomBarItemIsCollection;
		static Vector2 mouseOpenMenuPos;
		static Vector2 mouseOpenSubMenuPos;

		static MenuEntry[] activeContextMenuEntries;
		static MenuEntry[] activeSubMenuEntries;
		static readonly MenuEntry dividerMenuEntry = new(menuDividerString, null, null);
		static bool wasMouseOverMenu;
		static bool hasLastClickOpenedSubMenu;
		static string contextMenuHeader;

		const float TextOffsetX = 0.45f;
		const float MenuDividerHeight = 0.15f;
		const float MenuDividerMarginY = 0.5f;
		const float ButtonHeight = 2;
		const float OutlineThickness = 0.5f;
		const float BottomBarHeight = ButtonHeight + OutlineThickness * 2;

		static readonly MenuEntry[] pinColEntries = ((PinColour[])Enum.GetValues(typeof(PinColour))).Select(col =>
			new MenuEntry(Format(Enum.GetName(typeof(PinColour), col)), () => SetCol(col), CanSetCol)
		).ToArray();

		static readonly MenuEntry[] orientationEntries = ((Orientation[])Enum.GetValues(typeof(Orientation))).Select(orientation =>
			new MenuEntry(Format(Enum.GetName(typeof(Orientation), orientation)), () => SetOrientation(orientation), CanSetOrientation)
		).ToArray();
		
		static readonly MenuEntry deleteEntry = new(Format("DELETE"), Delete, CanDelete);
		static readonly MenuEntry openChipEntry = new(Format("OPEN"), OpenChip, CanOpenChip);
		static readonly MenuEntry labelChipEntry = new(Format("LABEL"), OpenChipLabelPopup, CanLabelChip);

		static readonly MenuEntry[] entries_customSubchip =
		{
			new(Format("VIEW"), EnterViewMode, CanEnterViewMode),
			openChipEntry,
			labelChipEntry,
			deleteEntry
		};

		static readonly MenuEntry[] entries_builtinSubchip =
		{
			labelChipEntry,
			deleteEntry
		};

		static readonly MenuEntry[] entries_builtinLED = entries_builtinSubchip.Concat(new[] { dividerMenuEntry }).Concat(pinColEntries).ToArray();

		static readonly MenuEntry[] entries_builtinButton = entries_builtinLED;

		static readonly MenuEntry[] entries_builtinBus =
		{
			new(Format("FLIP"), FlipBus, CanFlipBus),
			labelChipEntry,
			deleteEntry
		};

		static readonly MenuEntry[] entries_builtinKeySubchip =
		{
			new(Format("REBIND"), OpenKeyBindMenu, CanEditCurrentChip),
			labelChipEntry,
			deleteEntry
		};

		static readonly MenuEntry[] entries_builtinRomSubchip =
		{
			new(Format("EDIT"), OpenRomEditMenu, CanEditCurrentChip),
			labelChipEntry,
			deleteEntry
		};

		static readonly MenuEntry[] entries_builtinPulseChip =
		{
			new(Format("EDIT"), OpenPulseEditMenu, CanEditCurrentChip),
			labelChipEntry,
			deleteEntry
		};

        static readonly MenuEntry[] entries_builtinConstantChip =
		{
            new(Format("EDIT"), OpenConstantEditMenu, CanEditCurrentChip),
            labelChipEntry,
            deleteEntry
        };

        static readonly MenuEntry[] entries_subChipOutput = pinColEntries;

		static readonly MenuEntry[] entries_inputDevPin = new[]
		{
			new(Format("EDIT"), OpenPinEditMenu, CanEditCurrentChip),
			new(Format("DELETE"), Delete, CanDelete),
			dividerMenuEntry,
			new(Format("ORIENTATION >"), OpenOrientationSubMenu, CanSetOrientation)
		}.Concat(pinColEntries).ToArray();

		static readonly MenuEntry[] entries_outputDevPin = new[]
		{
			entries_inputDevPin[0],
			entries_inputDevPin[1],
			dividerMenuEntry
		}.Concat(orientationEntries).ToArray();

		static readonly MenuEntry[] entries_wire =
		{
			new(Format("EDIT"), EditWire, CanEditWire),
			new(Format("DELETE"), Delete, CanDelete)
		};

		static readonly MenuEntry[] entries_bottomBarChip =
		{
			openChipEntry,
			new(Format("UN-STAR"), UnstarBottomBarEntry, () => true)
		};

		static readonly MenuEntry[] entries_collectionPopupChip =
		{
			openChipEntry
		};

		static readonly MenuEntry[] entries_bottomBarCollection =
		{
			new(Format("UN-STAR"), UnstarBottomBarEntry, () => true)
		};

		public static bool IsOpen { get; private set; }
		public static bool IsSubMenuOpen { get; private set; }
		public static IInteractable interactionContext { get; private set; }


		static string Format(string s)
		{
			s = char.ToUpper(s[0]) + s.Substring(1).ToLower();
			return s.PadRight(pad);
		}

		public static void Update()
		{
			bool inMenu = !(UIDrawer.ActiveMenu is UIDrawer.MenuType.None or UIDrawer.MenuType.BottomBarMenuPopup or UIDrawer.MenuType.ChipCustomization);
			if (inMenu)
			{
				CloseContextMenu();
			}
			else
			{
				HandleOpenMenuInput();

				// Draw
				if (IsOpen)
				{
					var contextMenuInfo = DrawContextMenu();
					
					if (IsSubMenuOpen) DrawSubMenu(contextMenuInfo);
				}

				// Close menu input
				if (InputHelper.IsMouseDownThisFrame(MouseButton.Left) || KeyboardShortcuts.CancelShortcutTriggered())
				{
					if (hasLastClickOpenedSubMenu)
					{
						hasLastClickOpenedSubMenu = false;
						return;
					}
					CloseContextMenu();
				}
			}
		}

		static void HandleOpenMenuInput()
		{
			// Open menu input
			if (InputHelper.IsMouseDownThisFrame(MouseButton.Right) && !KeyboardShortcuts.CameraActionKeyHeld && !InteractionState.MouseIsOverUI && !InputHelper.LockMode)
			{
				bool inCustomizeMenu = UIDrawer.ActiveMenu == UIDrawer.MenuType.ChipCustomization;
				IInteractable hoverElement = InteractionState.ElementUnderMouse;

				bool openSubChipContextMenu = hoverElement is SubChipInstance && !inCustomizeMenu;
				bool openDevPinContextMenu = (hoverElement is PinInstance pin && pin.parent is DevPinInstance) || hoverElement is DevPinInstance;
				bool openWireContextMenu = hoverElement is WireInstance;
				bool openSubchipOutputPinContextMenu = hoverElement is PinInstance pin2 && pin2.parent is SubChipInstance && pin2.IsSourcePin && !pin2.IsBusPin;

				if (openSubChipContextMenu || openDevPinContextMenu || openWireContextMenu || openSubchipOutputPinContextMenu)
				{
					interactionContextName = string.Empty;
					interactionContext = hoverElement;
					string headerName = string.Empty;

					if (openSubChipContextMenu)
					{
						SubChipInstance subChip = (SubChipInstance)hoverElement;
						interactionContextName = subChip.Description.Name;

						if (subChip.ChipType == ChipType.Custom)
						{
							headerName = subChip.Description.Name;
							activeContextMenuEntries = entries_customSubchip;
						}
						else // builtin type
						{
							headerName = ChipTypeHelper.IsBusType(subChip.ChipType) ? "BUS" : subChip.Description.Name;
							if (subChip.ChipType is ChipType.Key) activeContextMenuEntries = entries_builtinKeySubchip;
							else if (ChipTypeHelper.IsRomType(subChip.ChipType)) activeContextMenuEntries = entries_builtinRomSubchip;
							else if (subChip.ChipType is ChipType.Pulse) activeContextMenuEntries = entries_builtinPulseChip;
							else if (ChipTypeHelper.IsBusType(subChip.ChipType)) activeContextMenuEntries = entries_builtinBus;
							else if (subChip.ChipType == ChipType.DisplayLED) activeContextMenuEntries = entries_builtinLED;
							else if (subChip.ChipType == ChipType.Button) activeContextMenuEntries = entries_builtinButton;
							else if (subChip.ChipType == ChipType.Constant_8Bit) activeContextMenuEntries = entries_builtinConstantChip;

							else activeContextMenuEntries = entries_builtinSubchip;
						}

						Project.ActiveProject.controller.Select(interactionContext as IMoveable, false);
					}
					else if (openDevPinContextMenu)
					{
						if (interactionContext is DevPinInstance devPinInstance) interactionContext = devPinInstance.Pin;

						PinInstance activePin = (PinInstance)interactionContext;
						headerName = CreatePinHeaderName(activePin.Name);
						interactionContextName = activePin.Name;
						Project.ActiveProject.controller.Select(activePin.parent, false);
						activeContextMenuEntries = activePin.IsSourcePin ? entries_inputDevPin : entries_outputDevPin;
					}
					else if (openWireContextMenu)
					{
						WireInstance wire = (WireInstance)interactionContext;
						if (wire.IsBusWire) headerName = "BUS LINE";
						else headerName = CreateWireHeaderString(wire);

						activeContextMenuEntries = entries_wire;
					}
					else if (openSubchipOutputPinContextMenu)
					{
						PinInstance pinContext = (PinInstance)interactionContext;
						headerName = CreatePinHeaderName(pinContext.Name);
						activeContextMenuEntries = entries_subChipOutput;
					}

					SetContextMenuOpen(headerName);
				}
				else
				{
					CloseContextMenu();
				}
			}
		}

		static string CreateWireHeaderString(WireInstance wire)
		{
			string pinName = wire.SourcePin.Name;
			if (string.IsNullOrWhiteSpace(pinName)) return "WIRE";

			return "WIRE: " + pinName;
		}

		static string CreatePinHeaderName(string pinName)
		{
			if (string.IsNullOrWhiteSpace(pinName)) return "PIN";

			return "PIN: " + pinName;
		}

		public static void OpenBottomBarContextMenu(string name, bool isCollection, bool isFromInsideCollection)
		{
			interactionContextName = name;
			bottomBarItemIsCollection = isCollection;
			interactionContext = null;
			SetContextMenuOpen(name);

			if (isCollection)
			{
				activeContextMenuEntries = entries_bottomBarCollection;
			}
			else
			{
				activeContextMenuEntries = isFromInsideCollection ? entries_collectionPopupChip : entries_bottomBarChip;
			}
		}

		static void SetContextMenuOpen(string header)
		{
			mouseOpenMenuPos = UI.ScreenToUISpace(InputHelper.MousePos);
			contextMenuHeader = header.PadRight(pad);
			IsOpen = true;
		}

		static ContextMenuInfo DrawContextMenu()
		{
			Draw.StartLayer(Vector2.zero, 1, true);

			ButtonTheme theme = DrawSettings.ActiveUITheme.MenuPopupButtonTheme;
			ButtonTheme headerTheme = DrawSettings.ActiveUITheme.MenuPopupButtonTheme;
			headerTheme.buttonCols.inactive = ColHelper.MakeCol(0.18f);
			headerTheme.textCols.inactive = Color.white;

			// Calculates how wide the menu entries should be. Total width of panel is slightly increased when finishing drawing.
			MenuEntry longestEntry = GetLongestMenuEntry(activeContextMenuEntries);
			float menuWidth = Draw.CalculateTextBoundsSize(longestEntry.Text, theme.fontSize, theme.font).x + 1;
			float menuWidthHeader = Draw.CalculateTextBoundsSize(contextMenuHeader, theme.fontSize, theme.font).x + 1;
			menuWidth = Mathf.Max(menuWidth, menuWidthHeader);

			Draw.ID panelID = UI.ReservePanel();
			Vector2 buttonSize = new(menuWidth, ButtonHeight);

			Vector2 pos = mouseOpenMenuPos;
			var clampRight = pos.x + menuWidth + OutlineThickness > UI.Width;
			if (clampRight)
			{
				pos.x = UI.Width - menuWidth - OutlineThickness;
			}

			float menuHeight = GetMenuHeight(activeContextMenuEntries);
			bool clampBottom = pos.y - menuHeight < BottomBarHeight;
			if (clampBottom)
			{
				pos.y = BottomBarHeight + OutlineThickness / 2;
			}
			float dirY = clampBottom ? 1 : -1;
			Anchor anchor = clampBottom ? Anchor.BottomLeft : Anchor.TopLeft;

			float openX = pos.x;
			float openY = pos.y;
			Vector2 menuSize = new(menuWidth, 0);
			using (UI.BeginBoundsScope(true))
			{
				for (int i = 0; i < activeContextMenuEntries.Length; i++)
				{
					int index = clampBottom ? activeContextMenuEntries.Length - i - 1 : i;
					MenuEntry entry = activeContextMenuEntries[index];

					if (index == 0 && !clampBottom) DrawHeader();

					if (entry.Text == menuDividerString)
					{
						pos.y += MenuDividerMarginY * dirY;
						UI.DrawPanel(pos, new Vector2(menuWidth, MenuDividerHeight), ColHelper.MakeCol(0.6f), Anchor.CentreLeft);
						pos.y += MenuDividerMarginY * dirY;
					}
					else
					{
						if (UI.Button(entry.Text, theme, pos, buttonSize, entry.IsEnabled(), false, false, anchor, true, TextOffsetX))
						{
							entry.OnPress();
							mouseOpenSubMenuPos = pos;
						}

						pos.y += buttonSize.y * dirY;
					}

					if (index == 0 && clampBottom) DrawHeader();
				}

				Bounds2D bounds = UI.GetCurrentBoundsScope();
				menuSize.y = bounds.Height;
				UI.ModifyPanel(panelID, bounds.Centre, menuSize + Vector2.one * OutlineThickness, ColHelper.MakeCol(0.91f));
			}

			wasMouseOverMenu = UI.MouseInsideBounds(UI.PrevBounds);
			return new(menuSize, new Vector2(openX, openY), clampRight, clampBottom);

			void DrawHeader()
			{
				UI.Button(contextMenuHeader, headerTheme, pos, buttonSize, false, false, false, anchor, true, TextOffsetX);
				pos.y += buttonSize.y * dirY;
			}
		}
		
		static void DrawSubMenu(ContextMenuInfo activeContextMenu)
		{
			Draw.StartLayer(Vector2.zero, 1, true);
			ButtonTheme theme = DrawSettings.ActiveUITheme.MenuPopupButtonTheme;

			// Calculates how wide the menu entries should be. Total width of panel is slightly increased when finishing drawing.
			MenuEntry longestEntry = GetLongestMenuEntry(activeSubMenuEntries);
			float menuWidth = Draw.CalculateTextBoundsSize(longestEntry.Text, theme.fontSize, theme.font).x + 1;

			Draw.ID panelID = UI.ReservePanel();
			Vector2 buttonSize = new(menuWidth, ButtonHeight);

			Vector2 pos = new Vector2(activeContextMenu.OpenedPosition.x + activeContextMenu.Size.x, mouseOpenSubMenuPos.y);
			if (activeContextMenu.ClampToBottom)
			{
				pos.y += ButtonHeight;
			}
			var shouldOpenToLeft = activeContextMenu.OpenedToLeft ||
			                       pos.x + menuWidth + OutlineThickness > UI.Width;
			if (shouldOpenToLeft)
			{
				pos.x = activeContextMenu.OpenedPosition.x - menuWidth - OutlineThickness;
			}

			float menuHeight = GetMenuHeight(activeSubMenuEntries, false);
			bool clampToBottom = pos.y - menuHeight - OutlineThickness < BottomBarHeight;
			if (clampToBottom)
			{
				pos.y = BottomBarHeight + OutlineThickness / 2;
			}
			float dirY = clampToBottom ? 1 : -1;
			Anchor anchor = clampToBottom ? Anchor.BottomLeft : Anchor.TopLeft;
			using (UI.BeginBoundsScope(true))
			{
				for (int i = 0; i < activeSubMenuEntries.Length; i++)
				{
					int index = clampToBottom ? activeSubMenuEntries.Length - i - 1 : i;
					MenuEntry entry = activeSubMenuEntries[index];

					if (entry.Text == menuDividerString)
					{
						pos.y += MenuDividerMarginY * dirY;
						UI.DrawPanel(pos, new Vector2(menuWidth, MenuDividerHeight), ColHelper.MakeCol(0.6f), Anchor.CentreLeft);
						pos.y += MenuDividerMarginY * dirY;
					}
					else
					{
						if (UI.Button(entry.Text, theme, pos, buttonSize, entry.IsEnabled(), false, false, anchor, true, TextOffsetX))
						{
							entry.OnPress();
						}

						pos.y += buttonSize.y * dirY;
					}
				}

				Bounds2D bounds = UI.GetCurrentBoundsScope();
				Vector2 menuSize = new(menuWidth, bounds.Height);
				UI.ModifyPanel(panelID, bounds.Centre, menuSize + Vector2.one * OutlineThickness, ColHelper.MakeCol(0.91f));
			}

			wasMouseOverMenu = UI.MouseInsideBounds(UI.PrevBounds);
		}

		static MenuEntry GetLongestMenuEntry(MenuEntry[] entries)
		{
			MenuEntry result = default;
			int maxLength = -1;
			for (int i = 0; i < entries.Length; i++)
			{
				var length = entries[i].Text.Length;
				if (length > maxLength)
				{
					maxLength = length;
					result = entries[i];
				}
			}

			return result;
		}

		static float GetMenuHeight(MenuEntry[] entries, bool hasHeader = true)
		{
			float height = 0f;

			if (hasHeader)
			{
				height += ButtonHeight;
			}
			for (int i = 0; i < entries.Length; i++)
			{
				if (entries[i].Text == menuDividerString)
				{
					height += MenuDividerMarginY + MenuDividerMarginY + MenuDividerHeight;
				}
				else
				{
					height += ButtonHeight;
				}
			}

			return height;
		}

		static bool IsCustomChip() => !Project.ActiveProject.chipLibrary.IsBuiltinChip(interactionContextName);
		static bool CanEnterViewMode() => IsCustomChip();
		static bool CanLabelChip() => Project.ActiveProject.CanEditViewedChip;
		static void EnterViewMode() => Project.ActiveProject.EnterViewMode(interactionContext as SubChipInstance);

		static bool CanDelete() => Project.ActiveProject.CanEditViewedChip;
		static bool CanFlipBus() => Project.ActiveProject.CanEditViewedChip;

		static bool CanSetCol()
		{
			if (!Project.ActiveProject.CanEditViewedChip || UIDrawer.ActiveMenu == UIDrawer.MenuType.ChipCustomization) return false;
			if (interactionContext is PinInstance pin) return pin.IsSourcePin;
			if (interactionContext is SubChipInstance subchip) return subchip.ChipType == ChipType.DisplayLED || subchip.ChipType == ChipType.Button;

			return false;
		}

		static void FlipBus()
		{
			((SubChipInstance)interactionContext).FlipBus();
		}

		static void SetCol(PinColour col)
		{
			if (interactionContext is PinInstance pin)
			{
				pin.Colour = col;
			}

			if(!(interactionContext is SubChipInstance subchip)) { return; }

			else if (subchip.ChipType == ChipType.DisplayLED)
			{
				Project.ActiveProject.NotifyLEDColourChanged(subchip, (uint)col);
			}
            else if (subchip.ChipType == ChipType.Button)
            {
                Project.ActiveProject.NotifyLEDColourChanged(subchip, (uint)col);
				subchip.OutputPins[0].Colour = col;
            }

        }

		static void OpenChipLabelPopup()
		{
			UIDrawer.SetActiveMenu(UIDrawer.MenuType.ChipLabelPopup);
		}

		public static void EditWire()
		{
			Project.ActiveProject.controller.EnterWireEditMode((WireInstance)interactionContext);
		}

		static void Delete()
		{
			if (interactionContext is IMoveable moveable)
			{
				Project.ActiveProject.controller.Delete(moveable);
			}
			else if (interactionContext is WireInstance wire)
			{
				Project.ActiveProject.controller.DeleteWire(wire);
			}
			else if (interactionContext is PinInstance pin)
			{
				Project.ActiveProject.controller.Delete(pin.parent);
			}
		}

		static void OpenKeyBindMenu()
		{
			UIDrawer.SetActiveMenu(UIDrawer.MenuType.RebindKeyChip);
		}

		static void OpenRomEditMenu() => UIDrawer.SetActiveMenu(UIDrawer.MenuType.RomEdit);

		static void OpenPulseEditMenu() => UIDrawer.SetActiveMenu(UIDrawer.MenuType.PulseEdit);

		static void OpenConstantEditMenu() => UIDrawer.SetActiveMenu(UIDrawer.MenuType.ConstantEdit);

		static bool CanEditCurrentChip() => Project.ActiveProject.CanEditViewedChip;

		static bool CanEditWire() => CanEditCurrentChip();

		static void OpenPinEditMenu()
		{
			PinEditMenu.SetTargetPin((DevPinInstance)((PinInstance)interactionContext).parent);
			UIDrawer.SetActiveMenu(UIDrawer.MenuType.PinRename);
		}

		static void OpenChip()
		{
			Project project = Project.ActiveProject;
			string chipToOpenName = interactionContextName;

			if (project.ActiveChipHasUnsavedChanges())
			{
				UnsavedChangesPopup.OpenPopup(OpenChipIfConfirmed);
			}
			else
			{
				OpenChipIfConfirmed(true);
			}

			void OpenChipIfConfirmed(bool confirm)
			{
				if (confirm)
				{
					project.LoadDevChipOrCreateNewIfDoesntExist(chipToOpenName);
				}
			}
		}

		static bool CanOpenChip() => IsCustomChip() && CanEditCurrentChip();

		public static void Reset()
		{
			CloseContextMenu();
		}

		public static void CloseContextMenu()
		{
			IsOpen = false;
			IsSubMenuOpen = false;
		}

		public static bool HasFocus() => IsOpen && wasMouseOverMenu;

		public static void UnstarBottomBarEntry()
		{
			Project.ActiveProject.SetStarred(interactionContextName, false, bottomBarItemIsCollection);
		}

		static void SetOrientation(Orientation orientation)
		{
			if (interactionContext is PinInstance { parent: DevPinInstance devPin }) devPin.SetOrientation(orientation);
			CloseContextMenu();
		}

		static bool CanSetOrientation()
		{
			if (!Project.ActiveProject.CanEditViewedChip) return false;
			return interactionContext is PinInstance { parent: DevPinInstance };
		}

		private static void OpenOrientationSubMenu()
		{
			SetSubMenuOpen(orientationEntries);
		}

		private static void SetSubMenuOpen(MenuEntry[] entries)
		{
			mouseOpenSubMenuPos = UI.ScreenToUISpace(InputHelper.MousePos);
			IsSubMenuOpen = true;
			hasLastClickOpenedSubMenu = true;
			activeSubMenuEntries = entries;
		}

		public readonly struct MenuEntry
		{
			public readonly string Text;
			public readonly Action OnPress;
			public readonly Func<bool> IsEnabled;

			public MenuEntry(string text, Action onPress, Func<bool> isEnabled)
			{
				Text = text;
				OnPress = onPress;
				IsEnabled = isEnabled;
			}
		}

		public readonly struct ContextMenuInfo
		{
			public readonly Vector2 Size;
			public readonly Vector2 OpenedPosition;
			public readonly bool OpenedToLeft;
			public readonly bool ClampToBottom;

			public ContextMenuInfo(Vector2 size, Vector2 openedPosition, bool openedToLeft, bool clampToBottom)
			{
				Size = size;
				OpenedPosition = openedPosition;
				OpenedToLeft = openedToLeft;
				ClampToBottom = clampToBottom;
			}
		}
	}
}