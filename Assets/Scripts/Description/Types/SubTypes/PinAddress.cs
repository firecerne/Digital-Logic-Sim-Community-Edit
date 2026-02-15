namespace DLS.Description
{
	public readonly struct PinAddress
	{
		// ----- Data -----
		public readonly int PinID; // ID for this pin (unique within its owner, but not globally unique)
		public readonly int PinOwnerID; // ID of the devPin or subChip to which this pin belongs (unique within its parent)

		// ---- Constructor and functions ----
		public PinAddress(int pinOwnerID, int pinID)
		{
			PinID = pinID;
			PinOwnerID = pinOwnerID;
		}

		public static bool Equals(PinAddress a, PinAddress b) => a.PinID == b.PinID && a.PinOwnerID == b.PinOwnerID;
		public override string ToString() => $"Address(OwnerID: {PinOwnerID}, PinID: {PinID})";
	}
}