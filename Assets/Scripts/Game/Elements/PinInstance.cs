using System;
using DLS.Description;
using DLS.Graphics;
using DLS.Simulation;
using UnityEngine;

namespace DLS.Game
{
	public class PinInstance : IInteractable
	{
		public readonly PinAddress Address;

		public PinBitCount bitCount;
		public readonly bool IsBusPin;
		public readonly bool IsSourcePin;

		// Pin may be attached to a chip or a devPin as its parent
		public readonly IMoveable parent;
		public PinStateValue State; // sim state
		public PinStateValue PlayerInputState;
		public PinColour Colour;
		public float LocalOffset;
		public string Name;
		public int face;
        public readonly int ID;
		

        public PinInstance(PinDescription desc, PinAddress address, IMoveable parent, bool isSourcePin)
		{
			this.parent = parent;
			bitCount = desc.BitCount;
			Name = desc.Name;
			Address = address;
			IsSourcePin = isSourcePin;
			Colour = desc.Colour;

            IsBusPin = parent is SubChipInstance { IsBus: true };
			face = desc.Face;
            ID = desc.ID;
            LocalOffset = desc.LocalOffset;
			State.MakeFromPinBitCount(bitCount);
			PlayerInputState.MakeFromPinBitCount(bitCount);
		}

        public Vector2 FacingDir => face == 1 ? Vector2.right : face == 3 ? Vector2.left : face == 2 ? Vector2.down : Vector2.up;

        public Vector2 GetWorldPos()
        {
            switch (parent)
            {
                case DevPinInstance devPin:
                    return devPin.PinPosition;
                case SubChipInstance subChip:
                    {
                        Vector2 chipSize = subChip.Size;
                        Vector2 chipPos = subChip.Position;

                        float halfWidth = chipSize.x / 2f;
                        float halfHeight = chipSize.y / 2f;
                        float inset = DrawSettings.SubChipPinInset;
                        float outlineOffset = DrawSettings.ChipOutlineWidth / 2f;

                        
                        float x;
                        float y;

                        switch (face)
                        {
                            case 0: // Top edge (Y fixed)
                                x = LocalOffset;
                                y = halfHeight + outlineOffset - inset;
                                break;

                            case 1: // Right edge (X fixed)
                                x = halfWidth + outlineOffset - inset;
                                y = LocalOffset;
                                break;

                            case 2: // Bottom edge (Y fixed)
                                x = LocalOffset;
                                y = -halfHeight - outlineOffset + inset;
                                break;

                            case 3: // Left edge (X fixed)
                                x = -halfWidth - outlineOffset + inset;
                                y = LocalOffset;
                                break;

                            default:
                                throw new Exception("Invalid pin face: " + face);
                        }

                        return chipPos + new Vector2(x, y);
                    }
                default:
                    throw new Exception("Parent type not supported");
            }
        }

        public void SetBusFlip(bool flipped)
		{
			face = IsSourcePin ^ flipped ? 1 : 3;
		}

		public Color GetColLow() => DrawSettings.ActiveTheme.StateLowCol[(int)Colour];
		public Color GetColHigh() => DrawSettings.ActiveTheme.StateHighCol[(int)Colour];

		public Color GetStateCol(int bitIndex, bool hover = false, bool canUsePlayerState = true, bool forWires = false)
		{
			PinStateValue pinState = IsSourcePin && canUsePlayerState ? PlayerInputState : State; // dev input pin uses player state (so it updates even when sim is paused)
			uint state = pinState.GetTristatedValue(bitIndex);
			if (state == PinStateValue.LOGIC_DISCONNECTED) return DrawSettings.ActiveTheme.StateDisconnectedCol;
			if(forWires && bitCount >= 64) { return DrawSettings.GetFlatColour(state == PinStateValue.LOGIC_HIGH, (uint)Colour, hover); }
			return DrawSettings.GetStateColour(state == PinStateValue.LOGIC_HIGH, (uint)Colour, hover);
			
		}
	}
}