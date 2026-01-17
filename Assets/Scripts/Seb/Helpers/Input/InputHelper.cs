using System.Data;
using Seb.Helpers.InputHandling;
using Seb.Types;
using UnityEngine;
using System.Collections.Generic;

namespace Seb.Helpers
{
	public enum MouseButton
	{
		Left = 0,
		Right = 1,
		Middle = 2
	}

	public static class InputHelper
	{
		public static IInputSource InputSource = new UnityInputSource();
		static Camera _worldCam;
		static Vector2 prevWorldMousePos;
		static int prevWorldMouseFrame = -1;
		static int leftMouseDownConsumeFrame = -1;
		static int rightMouseDownConsumeFrame = -1;
		static int middleMouseDownConsumeFrame = -1;
		public static Vector2 MousePos => InputSource.MousePosition; // Screen-space mouse position
		public static string InputStringThisFrame => InputSource.InputString;
		public static bool AnyKeyOrMouseDownThisFrame => InputSource.AnyKeyOrMouseDownThisFrame;
		public static bool AnyKeyOrMouseHeldThisFrame => InputSource.AnyKeyOrMouseHeldThisFrame;
		public static Vector2 MouseScrollDelta => InputSource.MouseScrollDelta;
		public static bool ModifierKeysOff = false; // So we can use special keys for the key chip

		// Allows me to rename these keys to be more readable versions
		public static readonly Dictionary<string, string> KeysRenameMap = new Dictionary<string, string>
		{
			{ "Alpha0", "0" },
			{ "Alpha1", "1" },
			{ "Alpha2", "2" },
			{ "Alpha3", "3" },
			{ "Alpha4", "4" },
			{ "Alpha5", "5" },
			{ "Alpha6", "6" },
			{ "Alpha7", "7" },
			{ "Alpha8", "8" },
			{ "Alpha9", "9" },
			{ "A", "A" },
			{ "B", "B" },
			{ "C", "C" },
			{ "D", "D" },
			{ "E", "E" },
			{ "F", "F" },
			{ "G", "G" },
			{ "H", "H" },
			{ "I", "I" },
			{ "J", "J" },
			{ "K", "K" },
			{ "L", "L" },
			{ "M", "M" },
			{ "N", "N" },
			{ "O", "O" },
			{ "P", "P" },
			{ "Q", "Q" },
			{ "R", "R" },
			{ "S", "S" },
			{ "T", "T" },
			{ "U", "U" },
			{ "V", "V" },
			{ "W", "W" },
			{ "X", "X" },
			{ "Y", "Y" },
			{ "Z", "Z" },
			{ "BackQuote", "`" },
			{ "Minus", "-" },
			{ "Equals", "=" },
			{ "LeftBracket", "[" },
			{ "RightBracket", "]" },
			{ "Semicolon", ";" },
			{ "Quote", "'" },
			{ "Comma", "," },
			{ "Period", "." },
			{ "Slash", "/" },
			{ "Keypad0", "Keypad 0" },
			{ "Keypad1", "Keypad 1" },
			{ "Keypad2", "Keypad 2" },
			{ "Keypad3", "Keypad 3" },
			{ "Keypad4", "Keypad 4" },
			{ "Keypad5", "Keypad 5" },
			{ "Keypad6", "Keypad 6" },
			{ "Keypad7", "Keypad 7" },
			{ "Keypad8", "Keypad 8" },
			{ "Keypad9", "Keypad 9" },
			{ "KeypadDivide", "Keypad /" },
			{ "KeypadEnter", "Keypad Enter" },
			{ "KeypadEquals", "Keypad =" },
			{ "KeypadMinus", "Keypad -" },
			{ "KeypadPlus", "Keypad +" },
			{ "KeypadMultiply", "Keypad *" },
			{ "KeypadPeriod", "Keypad ." },
			{ "Tab", "Tab" },
			{ "Return", "Enter" },
			{ "Escape", "Escape" },
			{ "Space", "Space" },
			{ "Delete", "Delete" },
			{ "Backspace", "Backspace" },
			{ "Insert", "Insert" },
			{ "Home", "Home" },
			{ "End", "End" },
			{ "PageUp", "Page Up" },
			{ "PageDown", "Page Down" },
			{ "LeftArrow", "Left Arrow" },
			{ "RightArrow", "Right Arrow" },
			{ "UpArrow", "Up Arrow" },
			{ "DownArrow", "Down Arrow" },
			{ "CapsLock", "Caps Lock" },
			{ "Numlock", "Num Lock" },
			{ "ScrollLock", "Scroll Lock" },
			{ "Print", "Print Screen" },
			{ "Pause", "Pause" },
			{ "Clear", "Clear" },
			{ "LeftControl", "Left Control" },
			{ "RightControl", "Right Control" },
			{ "LeftShift", "Left Shift" },
			{ "RightShift", "Right Shift" },
			{ "LeftAlt", "Left Alt" },
			{ "RightAlt", "Right Alt" },
			{ "LeftMeta", "Left Windows" },
			{ "RightMeta", "Right Windows" },
			{ "Backslash", "\\" },
			{ "F1", "F1" },
			{ "F2", "F2" },
			{ "F3", "F3" },
			{ "F4", "F4" },
			{ "F5", "F5" },
			{ "F6", "F6" },
			{ "F7", "F7" },
			{ "F8", "F8" },
			{ "F9", "F9" },
			{ "F10", "F10" },
			{ "F11", "F11" },
			{ "F12", "F12" }
		};

		// List of keys that can be used as input to key chips
		public static readonly KeyCode[] ValidInputKeys =
		{
			// Letters
			KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
			KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
			KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
			KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,

			// Numbers
			KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
			KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,

			// Symbols
			KeyCode.BackQuote, KeyCode.Minus, KeyCode.Equals, KeyCode.LeftBracket,
			KeyCode.RightBracket, KeyCode.Semicolon, KeyCode.Quote, KeyCode.Comma,
			KeyCode.Period, KeyCode.Slash, KeyCode.Backslash,

			// Keypad
			KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3,
			KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7,
			KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.KeypadDivide, KeyCode.KeypadEnter,
			KeyCode.KeypadEquals, KeyCode.KeypadMinus, KeyCode.KeypadPlus, KeyCode.KeypadMultiply,
			KeyCode.KeypadPeriod,

			// Controls
			KeyCode.Tab, KeyCode.Return, KeyCode.Escape, KeyCode.Space, KeyCode.Delete,
			KeyCode.Backspace, KeyCode.Insert, KeyCode.Home, KeyCode.End, KeyCode.PageUp,
			KeyCode.PageDown, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow,
			KeyCode.DownArrow, KeyCode.CapsLock, KeyCode.Numlock, KeyCode.ScrollLock,
			KeyCode.Print, KeyCode.Pause, KeyCode.Clear, KeyCode.LeftControl, KeyCode.RightControl,
			KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftMeta,
			KeyCode.RightMeta,

			// Function Keys
			KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6,
			KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,
		};

		// List of keys that can be used when the special keys are turned off (and otherwise can't)
		public static readonly KeyCode[] ModifierKeys =
		{
			KeyCode.LeftControl, KeyCode.RightControl,
			KeyCode.LeftShift, KeyCode.RightShift,
			KeyCode.LeftAlt, KeyCode.RightAlt
		};

		static int[] keycodes = GetKeyCodeValues();
		static bool[] keysPressed;

		public static Camera WorldCam
		{
			get
			{
				if (_worldCam == null) _worldCam = Camera.main;
				return _worldCam;
			}
		}

		public static Vector2 MousePosWorld
		{
			get
			{
				if (Time.frameCount != prevWorldMouseFrame)
				{
					prevWorldMousePos = WorldCam.ScreenToWorldPoint(MousePos);
					prevWorldMouseFrame = Time.frameCount;
				}

				return prevWorldMousePos;
			}
		}

		public static bool ShiftIsHeld => IsKeyHeld(KeyCode.LeftShift) || IsKeyHeld(KeyCode.RightShift);
		public static bool CtrlIsHeld => IsKeyHeld(KeyCode.LeftControl) || IsKeyHeld(KeyCode.RightControl);
		public static bool AltIsHeld => IsKeyHeld(KeyCode.LeftAlt) || IsKeyHeld(KeyCode.RightAlt);
		public static bool RightAltIsHeld => IsKeyHeld(KeyCode.RightAlt);

		public static KeyCode GetKeyCodePressedThisFrame()
		{
			// Get first key pressed this frame
			if (ValidInputKeys == null) return KeyCode.None;
			foreach (KeyCode k in ValidInputKeys)
			{
				if (IsKeyHeld(k)) return k;
			}
			return KeyCode.None;
		}

		public static bool IsKeyDownThisFrame(KeyCode key) => InputSource.IsKeyDownThisFrame(key);
		public static bool IsKeyUpThisFrame(KeyCode key) => InputSource.IsKeyUpThisFrame(key);
		public static bool IsKeyHeld(KeyCode key) => InputSource.IsKeyHeld(key);

		public static bool IsMouseInGameWindow()
		{
			Vector2 mousePos = MousePos;
			return mousePos.x >= 0 && mousePos.y >= 0 && mousePos.x < Screen.width && mousePos.y < Screen.height;
		}

		public static bool MouseInBounds_ScreenSpace(Vector2 centre, Vector2 size)
		{
			if (!Application.isPlaying) return false;
			Vector2 offset = MousePos - centre;
			return Mathf.Abs(offset.x) < size.x / 2 && Mathf.Abs(offset.y) < size.y / 2;
		}

		public static bool MouseInBounds_ScreenSpace(Bounds2D bounds)
		{
			if (!Application.isPlaying) return false;
			return bounds.PointInBounds(MousePos);
		}

		public static bool MouseInPoint_ScreenSpace(Vector2 centre, float radius)
		{
			if (!Application.isPlaying) return false;
			Vector2 offset = MousePos - centre;
			return offset.sqrMagnitude < radius * radius;
		}

		public static bool MouseInsidePoint_World(Vector2 centre, float radius)
		{
			if (!Application.isPlaying) return false;
			Vector2 offset = MousePosWorld - centre;
			return offset.sqrMagnitude < radius * radius;
		}

		public static bool MouseInsideBounds_World(Vector2 centre, Vector2 size)
		{
			if (!Application.isPlaying) return false;
			Vector2 offset = MousePosWorld - centre;
			return Mathf.Abs(offset.x) < size.x / 2 && Mathf.Abs(offset.y) < size.y / 2;
		}

		public static bool MouseInsideBounds_World(Bounds2D bounds)
		{
			if (!Application.isPlaying) return false;
			return bounds.PointInBounds(MousePosWorld);
		}

		public static bool IsMouseHeld(MouseButton button)
		{
			if (!Application.isPlaying) return false;
			return InputSource.IsMouseHeld(button);
		}

		// Check if mouse button was pressed this frame. Optionally consume the event, so it will return false for other callers this frame.
		public static bool IsMouseDownThisFrame(MouseButton button, bool consumeEvent = false)
		{
			if (!Application.isPlaying) return false;
			if (MouseDownEventIsConsumed(button)) return false;

			if (consumeEvent)
			{
				ConsumeMouseButtonDownEvent(button);
			}

			return InputSource.IsMouseDownThisFrame(button);
		}


		// Check if any mouse button was pressed this frame, even if the event was consumed.
		public static bool IsAnyMouseButtonDownThisFrame_IgnoreConsumed()
		{
			if (!Application.isPlaying) return false;
			return InputSource.IsMouseDownThisFrame(MouseButton.Left) || InputSource.IsMouseDownThisFrame(MouseButton.Right) || InputSource.IsMouseDownThisFrame(MouseButton.Middle);
		}

		// Consume mouse down event (the mouse event will report false on all subsequent calls this frame)
		public static void ConsumeMouseButtonDownEvent(MouseButton button)
		{
			if (button == MouseButton.Left)
			{
				leftMouseDownConsumeFrame = Time.frameCount;
			}
			else if (button == MouseButton.Right)
			{
				rightMouseDownConsumeFrame = Time.frameCount;
			}
			else if (button == MouseButton.Middle)
			{
				middleMouseDownConsumeFrame = Time.frameCount;
			}
		}

		static bool MouseDownEventIsConsumed(MouseButton button)
		{
			int lastConsumedFrame = button switch
			{
				MouseButton.Left => leftMouseDownConsumeFrame,
				MouseButton.Right => rightMouseDownConsumeFrame,
				MouseButton.Middle => middleMouseDownConsumeFrame,
				_ => -1
			};
			return Time.frameCount == lastConsumedFrame;
		}

		public static bool IsMouseUpThisFrame(MouseButton button)
		{
			if (!Application.isPlaying) return false;
			return InputSource.IsMouseUpThisFrame(button);
		}

		public static void TickPreciseKeyLogging() // Only tick when necessary (for exemple, controls screen)
		{
			for (int i = 0; i < keycodes.Length; i++)
			{
				keysPressed[i] = Input.GetKey((KeyCode)keycodes[i]);
			}
		}

		public static KeyCode GetFirstValidKeyCodePressed() // returns the first valid keycode pressed in the order provided in the enum
		{
			KeyCode pressed = KeyCode.None;
			for(int i = 0; i < keysPressed.Length; ++i)
			{
				if (keysPressed[i]) { pressed = (KeyCode)keycodes[i]; break; }
			}
			return pressed;
		}


		public static void CopyToClipboard(string s) => GUIUtility.systemCopyBuffer = s;
		public static string GetClipboardContents() => GUIUtility.systemCopyBuffer;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Reset()
		{
			_worldCam = null;
			prevWorldMouseFrame = -1;
			leftMouseDownConsumeFrame = -1;
			rightMouseDownConsumeFrame = -1;
			middleMouseDownConsumeFrame = -1;
			InputSource = new UnityInputSource();
		}

		static int[] GetKeyCodeValues()
		{
			int[] keys = (int[])System.Enum.GetValues(typeof(KeyCode));
			int keyCount = 0;

			for (int i = 0; i < keys.Length; i++) {
				if (!(keys[i] >= 323 & keys[i] <= 509)) // removes unwanted mouse and joystick values (keeps only keyboard)
				{
					keyCount++;
				}
			}

			int[] validKeys = new int[keyCount];
			int j = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (!(keys[i] >= 323 & keys[i] <= 509)) // removes unwanted mouse and joystick values (keeps only keyboard)
                {
					validKeys[j] = keys[i];
					j++;
                }
            }

			return validKeys;
        }

		public static void InitiatePreciseKeyLogging()
		{
			keycodes = GetKeyCodeValues();
			keysPressed = new bool[keycodes.Length];
		}

		public static string UintToKeyName(uint InputUint)
		{
			KeyCode keyCodeInput = (KeyCode)InputUint;

			if (KeysRenameMap.TryGetValue(keyCodeInput.ToString(), out string correctName))
				return correctName;

			UnityEngine.Debug.LogError("Error getting value from KeysRenameMap in UintToKeyName, with input: " + InputUint);
			return null;
		}
	}
}