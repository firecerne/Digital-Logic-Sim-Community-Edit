using System.Data;
using Seb.Helpers.InputHandling;
using Seb.Types;
using UnityEngine;

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

		// Numbers first
		public enum KeyNumberEnum : uint
		{
			Alpha0 = 0,
			Alpha1 = 1,
			Alpha2 = 2,
			Alpha3 = 3,
			Alpha4 = 4,
			Alpha5 = 5,
			Alpha6 = 6,
			Alpha7 = 7,
			Alpha8 = 8,
			Alpha9 = 9,
			A = 10,
			B = 11,
			C = 12,
			D = 13,
			E = 14,
			F = 15,
			G = 16,
			H = 17,
			I = 18,
			J = 19,
			K = 20,
			L = 21,
			M = 22,
			N = 23,
			O = 24,
			P = 25,
			Q = 26,
			RightArrow = 27,
			S = 28,
			T = 29,
			U = 30,
			V = 31,
			W = 32,
			X = 33,
			Y = 34,
			Z = 35,
			BackQuote = 36,
			Minus = 37,
			Equals = 38,
			LeftBracket = 39,
			RightBracket = 40,
			Semicolon = 41,
			Quote = 42,
			Comma = 43,
			Period = 44,
			Slash = 45,
			Keypad0 = 46,
			Keypad1 = 47,
			Keypad2 = 48,
			Keypad3 = 49,
			Keypad4 = 50,
			Keypad5 = 51,
			Keypad6 = 52,
			Keypad7 = 53,
			Keypad8 = 54,
			Keypad9 = 55,
			KeypadDivide = 56,
			KeypadEnter = 57,
			KeypadEquals = 58,
			KeypadMinus = 59,
			KeypadPlus = 60,
			KeypadMultiply = 61,
			KeypadPeriod = 62,
			Tab = 63,
			Return = 64,
			Escape = 65,
			Space = 66,
			Delete = 67,
			Backspace = 68,
			Insert = 69,
			Home = 70,
			End = 71,
			PageUp = 72,
			PageDown = 73,
			LeftArrow = 74,
			R = 75, // So we can make it start at R
			UpArrow = 76,
			DownArrow = 77,
			CapsLock = 78,
			Numlock = 79,
			ScrollLock = 80,
			Print = 81,
			Pause = 82,
			Clear = 83,
			LeftControl = 84,
			RightControl = 85,
			LeftShift = 86,
			RightShift = 87,
			LeftAlt = 88,
			RightAlt = 89,
			LeftMeta = 90,
			RightMeta = 91,
			F1 = 92,
			F2 = 93,
			F3 = 94,
			F4 = 95,
			F5 = 96,
			F6 = 97,
			F7 = 98,
			F8 = 99,
			F9 = 100,
			F10 = 101,
			F11 = 102,
			F12 = 103,
			Backslash = 104
		}

		// Allows me to rename these keys to be more readable versions
		public static class KeyRenameNames
		{
			public static string Alpha0 = "0";
			public static string Alpha1 = "1";
			public static string Alpha2 = "2";
			public static string Alpha3 = "3";
			public static string Alpha4 = "4";
			public static string Alpha5 = "5";
			public static string Alpha6 = "6";
			public static string Alpha7 = "7";
			public static string Alpha8 = "8";
			public static string Alpha9 = "9";
			public static string A = "A";
			public static string B = "B";
			public static string C = "C";
			public static string D = "D";
			public static string E = "E";
			public static string F = "F";
			public static string G = "G";
			public static string H = "H";
			public static string I = "I";
			public static string J = "J";
			public static string K = "K";
			public static string L = "L";
			public static string M = "M";
			public static string N = "N";
			public static string O = "O";
			public static string P = "P";
			public static string Q = "Q";
			public static string R = "R";
			public static string S = "S";
			public static string T = "T";
			public static string U = "U";
			public static string V = "V";
			public static string W = "W";
			public static string X = "X";
			public static string Y = "Y";
			public static string Z = "Z";
			public static string BackQuote = "`";
			public static string Minus = "-";
			public static new string Equals = "="; // Gave warning with "new"
			public static string LeftBracket = "[";
			public static string RightBracket = "]";
			public static string Semicolon = ";";
			public static string Quote = "'";
			public static string Comma = ",";
			public static string Period = ".";
			public static string Slash = "/";
			public static string Keypad0 = "Keypad 0";
			public static string Keypad1 = "Keypad 1";
			public static string Keypad2 = "Keypad 2";
			public static string Keypad3 = "Keypad 3";
			public static string Keypad4 = "Keypad 4";
			public static string Keypad5 = "Keypad 5";
			public static string Keypad6 = "Keypad 6";
			public static string Keypad7 = "Keypad 7";
			public static string Keypad8 = "Keypad 8";
			public static string Keypad9 = "Keypad 9";
			public static string KeypadDivide = "Keypad /";
			public static string KeypadEnter = "Keypad Enter";
			public static string KeypadEquals = "Keypad =";
			public static string KeypadMinus = "Keypad -";
			public static string KeypadPlus = "Keypad +";
			public static string KeypadMultiply = "Keypad *";
			public static string KeypadPeriod = "Keypad .";
			public static string Tab = "Tab";
			public static string Return = "Enter";
			public static string Escape = "Escape";
			public static string Space = "Space";
			public static string Delete = "Delete";
			public static string Backspace = "Backspace";
			public static string Insert = "Insert";
			public static string Home = "Home";
			public static string End = "End";
			public static string PageUp = "Page Up";
			public static string PageDown = "Page Down";
			public static string LeftArrow = "Left Arrow";
			public static string RightArrow = "Right Arrow";
			public static string UpArrow = "Up Arrow";
			public static string DownArrow = "Down Arrow";
			public static string CapsLock = "Caps Lock";
			public static string Numlock = "Num Lock";
			public static string ScrollLock = "Scroll Lock";
			public static string Print = "Print Screen";
			public static string Pause = "Pause";
			public static string Clear = "Clear";
			public static string LeftControl = "Left Control";
			public static string RightControl = "Right Control";
			public static string LeftShift = "Left Shift";
			public static string RightShift = "Right Shift";
			public static string LeftAlt = "Left Alt";
			public static string RightAlt = "Right Alt";
			public static string LeftMeta = "Left Windows";
			public static string RightMeta = "Right Windows";
			public static string Backslash = "\\";
			public static string F1 = "F1";
			public static string F2 = "F2";
			public static string F3 = "F3";
			public static string F4 = "F4";
			public static string F5 = "F5";
			public static string F6 = "F6";
			public static string F7 = "F7";
			public static string F8 = "F8";
			public static string F9 = "F9";
			public static string F10 = "F10";
			public static string F11 = "F11";
			public static string F12 = "F12";
		}

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
	}
}