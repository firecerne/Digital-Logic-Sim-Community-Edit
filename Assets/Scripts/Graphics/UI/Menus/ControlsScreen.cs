using System.Collections.Generic;
using DLS.Description;
using DLS.Game;
using Seb.Helpers;
using Seb.Types;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics
{
    public static class ControlsScreen
    {
        public static Dictionary<string, Shortcut> configurableShortcuts = new()
        {
            { "New Project", Main.ActiveShortcutSettings.MainMenu_NewProjectShortcutTriggered },
            { "Open Project", Main.ActiveShortcutSettings.MainMenu_OpenProjectShortcutTriggered },
            { "Open Settings", Main.ActiveShortcutSettings.MainMenu_SettingsShortcutTriggered },
            { "Quit Game", Main.ActiveShortcutSettings.MainMenu_QuitShortcutTriggered },

            { "Save Chip", Main.ActiveShortcutSettings.SaveShortcutTriggered },
            { "Open Library", Main.ActiveShortcutSettings.LibraryShortcutTriggered },
            { "Preferences", Main.ActiveShortcutSettings.PreferencesShortcutTriggered },
            { "Stats Menu", Main.ActiveShortcutSettings.StatsShortcutTriggered },
            { "New Chip", Main.ActiveShortcutSettings.CreateNewChipShortcutTriggered },
            { "Quit to Main Menu", Main.ActiveShortcutSettings.QuitToMainMenuShortcutTriggered },
            { "Search", Main.ActiveShortcutSettings.SearchShortcutTriggered },
            { "Special Chips Menu", Main.ActiveShortcutSettings.SpecialChipsShortcutTriggered },

            { "Duplicate Selection", Main.ActiveShortcutSettings.DuplicateShortcutTriggered },
            { "Toggle Grid", Main.ActiveShortcutSettings.ToggleGridShortcutTriggered },
            { "Reset Camera", Main.ActiveShortcutSettings.ResetCameraShortcutTriggered },
            { "Undo", Main.ActiveShortcutSettings.UndoShortcutTriggered },
            { "Redo", Main.ActiveShortcutSettings.RedoShortcutTriggered },

            { "Cancel", Main.ActiveShortcutSettings.CancelShortcutTriggered },
            { "Confirm", Main.ActiveShortcutSettings.ConfirmShortcutTriggered },
            { "Delete", Main.ActiveShortcutSettings.DeleteShortcutTriggered },
            { "Next Step (Step by Step)", Main.ActiveShortcutSettings.SimNextStepShortcutTriggered },
            { "Pause Sim", Main.ActiveShortcutSettings.SimPauseToggleShortcutTriggered },

            { "Open Save Data Folder", Main.ActiveShortcutSettings.OpenSaveDataFolderShortcutTriggered },
        };

        static string[] modifierNames = new[] {"None","Control","Shift","Alt","Ctrl+Shift","Ctrl+Shift+Alt" };

        static List<ShortcutEditingCollapsable> collapsables = new List<ShortcutEditingCollapsable>();
        static List<ShortcutEditingCollapsable> startingCollapsable;

        static UI.ScrollViewDrawContentFunc controlsScrollViewDrawer = DrawAllShortcutsCollapsableInScrollview;
        static UIHandle ID_ControlsScrollView = new("MainMenu_ControlsScrollView");

        static DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

        static string[] cancelSave = new[] { "Cancel", "Save" };

        class ShortcutEditingCollapsable
        {
            public Shortcut relatedShortcut;
            public string name; //Also used as ID
            public bool isOpen; //Opening the collapsable allows the user to modify the shortcut

            string shortcutDesc;

            // UI States
            bool keyBeingAssigned;
            bool altKeyBeingAssigned;


            public ShortcutEditingCollapsable(Shortcut relatedShortcut, string name)
            {
                this.relatedShortcut = relatedShortcut.Copy();
                this.name = name;
                this.shortcutDesc = MenuHelper.GetComplexStringRepresentationOfShortcut(this.relatedShortcut);
            }

            public void RegenerateShortcutDesc()
            {
                shortcutDesc = MenuHelper.GetComplexStringRepresentationOfShortcut(relatedShortcut);
            }

            public void Draw(ref Vector2 topLeft, float width)
            {

                // First, draw the entry in the scroll 
                const float spacing = 1.5f;
                const float panelSpacing = 1f;

                string rolldown = isOpen ? "v" : ">";
                Seb.Vis.Text.Rendering.TextRenderer.BoundingBox bounds = Seb.Vis.Draw.CalculateTextBounds(shortcutDesc, theme.FontRegular, theme.FontSizeRegular, topLeft + (width - spacing / 2f) * Vector2.right, Anchor.TopRight);
                float height = bounds.BoundsMax.y - bounds.BoundsMin.y;
                Bounds2D correctedBounds = new(new Vector2(bounds.BoundsMin.x - panelSpacing, topLeft.y - height/2f - panelSpacing),
                    new Vector2(bounds.BoundsMax.x + panelSpacing, topLeft.y - height / 2f + panelSpacing)
                    );

                UI.DrawPanel(correctedBounds, theme.InfoBarCol);
                UI.DrawText(shortcutDesc, theme.FontRegular, theme.FontSizeRegular, pos: topLeft + (width - spacing/2f) * Vector2.right, anchor: Anchor.TopRight, col: Color.white);
                bool button = UI.Button(rolldown, theme.ButtonTheme, pos: topLeft + (-height / 2f + panelSpacing) * Vector2.up, anchor: Anchor.TopLeft, fitTextX: false, fitTextY: false,
                    size: Vector2.one * 2f, enabled: true
                    );
                UI.DrawText(name, theme.FontRegular, theme.FontSizeRegular, pos: topLeft + 4f * Vector2.right, anchor: Anchor.TopLeft, col: Color.white);

                isOpen ^= button;

                topLeft = UI.PrevBounds.BottomLeft + spacing * Vector2.down + 4f * Vector2.left;

                // Then, if the entry is open, draw the shortcut editor

                if (isOpen)
                {
                    const float openPanelSize = 7f;
                    Bounds2D openPanelBounds = new Bounds2D(topLeft + openPanelSize * Vector2.down, topLeft + width * Vector2.right );
                    UI.DrawPanel(openPanelBounds, theme.InfoBarCol);

                    const float gapBetweenItems = 10f;
                    Vector2 originalTextPos = topLeft + new Vector2(3.5f * spacing, -spacing);

                    DrawText("Modifier", originalTextPos);
                    float modifierTextXSize = CalculateBounds("Modifier",originalTextPos).Size.x;
                    Vector2 secondModifierPosition = originalTextPos + (modifierTextXSize + gapBetweenItems) * Vector2.right;
                    float secondModifierTextSize;

                    Vector2 modWheelSize = new Vector2(17f, 2.5f);

                    // center the selection wheels to the middle of their "title".
                    Vector2 ModWheelPosition = originalTextPos + (modifierTextXSize / 2f) * Vector2.right + 2 * spacing * Vector2.down;
                    
                    int firstModWheelSelected = DrawModWheel((int)relatedShortcut.Modifier, ModWheelPosition);
                    int altModWheelSelected = (int)relatedShortcut.AlternativeModifier;
                    int forbiddenModWheelSelected = (int)relatedShortcut.ForbiddenModifier;

                    if (firstModWheelSelected != 0) { // Draw only either Alt or Forbidden because Forbidden only makes sense for None Mod and Alt only for NonNone Mod
                        DrawText("Alt Modifier", secondModifierPosition);
                        secondModifierTextSize = CalculateBounds("Alt Modifier", secondModifierPosition).Size.x;
                        Vector2 secondModWheelPosition = secondModifierPosition + (secondModifierTextSize / 2f) * Vector2.right + 2 * spacing * Vector2.down;
                        altModWheelSelected = DrawModWheel(altModWheelSelected, secondModWheelPosition);
                    }
                    else {
                        DrawText("NOT Modifier", secondModifierPosition);
                        secondModifierTextSize = CalculateBounds("NOT Modifier", secondModifierPosition).Size.x;
                        Vector2 secondModWheelPosition = secondModifierPosition + (secondModifierTextSize / 2f) * Vector2.right + 2 * spacing * Vector2.down;
                        forbiddenModWheelSelected = DrawModWheel(forbiddenModWheelSelected, secondModWheelPosition);
                    }

                    bool modifierChanged = (int)relatedShortcut.Modifier != firstModWheelSelected || (int)relatedShortcut.AlternativeModifier != altModWheelSelected
                        || (int)relatedShortcut.ForbiddenModifier != forbiddenModWheelSelected;

                    relatedShortcut.Modifier = (ShortcutModifier)firstModWheelSelected;
                    // Alt and Forbidden can't both have a value (because forbidden with non null modifier is nonsensical, alt with null mod is nonsensical)
                    relatedShortcut.AlternativeModifier = (ShortcutModifier)(firstModWheelSelected == 0 ? 0 : altModWheelSelected);
                    relatedShortcut.ForbiddenModifier = (ShortcutModifier)(firstModWheelSelected == 0 ? forbiddenModWheelSelected: 0);

                    if (modifierChanged) { RegenerateShortcutDesc(); }

                    // Now draw the key selectors

                    Vector2 keyTextPosition = secondModifierPosition + (secondModifierTextSize + gapBetweenItems) * Vector2.right;
                    DrawText("Key", keyTextPosition);
                    float keyTextSize = CalculateBounds("Key", keyTextPosition).Size.x;

                    Vector2 altKeyTextPosition = keyTextPosition + (keyTextSize + gapBetweenItems) * Vector2.right;
                    DrawText("Alt Key", altKeyTextPosition);
                    float altKeyTextSize = CalculateBounds("Alt Key", altKeyTextPosition).Size.x;

                    Vector2 keyButtonSize = new Vector2(10f, 2.5f);

                    if (altKeyBeingAssigned || keyBeingAssigned) { InputHelper.TickPreciseKeyLogging(); }

                    if (keyBeingAssigned)
                    {
                        if (InputHelper.AnyKeyOrMouseDownThisFrame)
                        {
                            KeyCode keyPressed = InputHelper.GetFirstValidKeyCodePressed();
                            relatedShortcut.KeyCode = isKeyCodeModifierKeyCode(keyPressed) ? KeyCode.None : keyPressed;
                            RegenerateShortcutDesc() ;
                        }
                    }

                    Vector2 keySelectorButtonPos = keyTextPosition + keyTextSize / 2f * Vector2.right + 2 * spacing * Vector2.down;
                    bool keyButtonClicked = KeySelectorButton(text: relatedShortcut.KeyCode.ToString(), keySelectorButtonPos, keyBeingAssigned);
                    keyBeingAssigned = keyBeingAssigned ? !InputHelper.AnyKeyOrMouseDownThisFrame : keyButtonClicked;

                    if (altKeyBeingAssigned)
                    {
                        if (InputHelper.AnyKeyOrMouseDownThisFrame)
                        {
                            KeyCode keyPressed = InputHelper.GetFirstValidKeyCodePressed();
                            relatedShortcut.AlternativeKeyCode = isKeyCodeModifierKeyCode(keyPressed) ? KeyCode.None : keyPressed;
                            RegenerateShortcutDesc();
                        }
                    }

                    Vector2 altKeySelectorButtonPos = altKeyTextPosition + altKeyTextSize / 2f * Vector2.right + 2 * spacing * Vector2.down;
                    bool altKeyButtonClicked = KeySelectorButton(text: relatedShortcut.AlternativeKeyCode.ToString(), altKeySelectorButtonPos, altKeyBeingAssigned);
                    altKeyBeingAssigned = altKeyBeingAssigned ? !InputHelper.AnyKeyOrMouseDownThisFrame : altKeyButtonClicked;

                    topLeft += (spacing + openPanelSize) * Vector2.down;

                    int DrawModWheel(int indexSelected, Vector2 position)
                    {
                        return UI.WheelSelector(indexSelected, modifierNames, position, modWheelSize, theme.OptionsWheel);
                    }

                    void DrawText(string text, Vector2 position)
                    {
                        UI.DrawText(text, theme.FontRegular, theme.FontSizeRegular, position, Anchor.TopLeft, Color.white);
                    }

                    Seb.Vis.Text.Rendering.TextRenderer.BoundingBox CalculateBounds(string text, Vector2 pos)
                    {
                        return Seb.Vis.Draw.CalculateTextBounds(text, theme.FontRegular, theme.FontSizeRegular, pos, Anchor.TopLeft);
                    }

                    bool KeySelectorButton(string text, Vector2 pos, bool selected)
                    {
                        return UI.Button(text, theme.ButtonTheme, pos, size:keyButtonSize, fitToText:false, enabled:!selected);
                    }

                    bool isKeyCodeModifierKeyCode(KeyCode keyCode)
                    {
                        return keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt
                            || keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl
                            || keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift
                            ;
                    }
                }

            }
        }

        public static void DrawAllShortcutsCollapsableInScrollview(Vector2 topleft, float width, bool isLayoutPass)
        {
            Vector2 position = topleft + Vector2.down * 0.75f;
            foreach (ShortcutEditingCollapsable collapsable in collapsables)
            {
                collapsable.Draw(ref position, width);
            }

        }

        public static void DrawControlsScreen()
        {
            Vector2 pos = UI.Centre + new Vector2(0, -1);
            Vector2 size = new(68, 32);

            UI.DrawScrollView(ID_ControlsScrollView, pos, size, Anchor.Centre, theme.ScrollTheme, controlsScrollViewDrawer);

            Vector2 buttonPosition = pos + (size.y/2 + 2.5f) * Vector2.down;
            int buttonIndex = UI.HorizontalButtonGroup(cancelSave, theme.ButtonTheme, buttonPosition, size.x, 1f, 0f, Anchor.Centre);

            if (buttonIndex == 0)
            {
                MainMenu.BackToMain();
            }

            if (buttonIndex == 1)
            {
                Save();
                MainMenu.BackToMain();
            }
        }

        static void Save()
        {
            for (int i = 0; i < collapsables.Count; i++)
            {
                configurableShortcuts[collapsables[i].name].Reassign(collapsables[i].relatedShortcut);
            }

            Main.SaveAndApplyShortcutSettings(Main.ActiveShortcutSettings);
        }

        public static void OpenControlsScreen()
        {
            InputHelper.InitiatePreciseKeyLogging();
            collapsables = new();
            foreach (KeyValuePair<string, Shortcut> pair in configurableShortcuts)
            {
                collapsables.Add(new ShortcutEditingCollapsable(pair.Value, pair.Key));
            }
            startingCollapsable = collapsables;
        }
    }
}
