namespace DLS.Description
{
	public enum ChipType
	{
		Custom,

		// ---- Basic Chips ----
		Nand,
		TriStateBuffer,
		Clock,
		Pulse,
		Detector,

		// ---- Memory ----
		Ram_256x8,
		Ram_65536x16,
		Rom_256x16,
		Rom_65536x16,
		EEPROM_256x16,
		EEPROM_65536x16,

		// ---- Displays ----
		SevenSegmentDisplay,
		DisplayRGB,
		DisplayDot,
		DisplayLED,
		DisplayRGBTouch,

		// ---- Merge / Split ----
		Merge_Pin,
		Split_Pin,

		// ---- In / Out Pins ----
		In_Pin,
		Out_Pin,

        Key,

		Button,
		Toggle,

		Constant_8Bit,

        // ---- Buses ----
        Bus,
		BusTerminus,
		
		// ---- Audio ----
		Buzzer,

		// ---- Time ----
		RTC,

		// ---- Clock ----
		SPS,
	}
}