using System;
using DLS.Description;
using Seb.Helpers;
using Seb.Types;
using UnityEngine;
using static DLS.Graphics.DrawSettings;

namespace DLS.Game
{
	public class DevPinInstance : IMoveable
	{
		public PinBitCount BitCount;
		public readonly char[] decimalDisplayCharBuffer = new char[16];

		public Vector2 faceDir;

		public readonly bool IsInputPin;
		public readonly string Name;
		public readonly PinInstance Pin;
		public Vector2Int StateGridDimensions;
		public Vector2 StateGridSize;

		public PinValueDisplayMode pinValueDisplayMode;
		public bool IsHorizontal => faceDir == Vector2.right || faceDir == Vector2.left;


		public DevPinInstance(PinDescription pinDescription, bool isInput)
		{
			Name = pinDescription.Name;
			ID = pinDescription.ID;
			IsInputPin = isInput;
			Position = pinDescription.Position;
			BitCount = pinDescription.BitCount;

			Pin = new PinInstance(pinDescription, new PinAddress(ID, 0), this, isInput);
			pinValueDisplayMode = pinDescription.ValueDisplayMode;

			// Calculate layout info
			faceDir = pinDescription.DevPinFacingDirection;
			StateGridDimensions = GridHelper.GetStateGridDimension(BitCount.BitCount);
			StateGridSize = BitCount.BitCount == 1 ? Vector2.one * (DevPinStateDisplayRadius * 2 + DevPinStateDisplayOutline * 2) : (Vector2)StateGridDimensions * MultiBitPinStateDisplaySquareSize + Vector2.one * DevPinStateDisplayOutline;
		}

		public Vector2 HandlePosition => Position;
		public Vector2 StateDisplayPosition => (Position + PinPosition) / 2;

		public Vector2 PinPosition
		{
			get	
			{
				if(BitCount.BitCount is 1 or 4 or 8)
				{
                    return Position + faceDir * (GridSize * (BitCount.BitCount is 1 or 4 ? 6 : 9));
                }

				return Position + faceDir * (StateGridSize.x + 2 * GridSize);
			}
		}

		public Vector2 Position { get; set; }
		public Vector2 MoveStartPosition { get; set; }
		public Vector2 StraightLineReferencePoint { get; set; }
		public int ID { get; }

		public bool IsSelected { get; set; }
		public bool HasReferencePointForStraightLineMovement { get; set; }
		public bool IsValidMovePos { get; set; }

		public Bounds2D SelectionBoundingBox => CreateBoundingBox(SelectionBoundsPadding);

		public Bounds2D BoundingBox => CreateBoundingBox(0);


		public Vector2 SnapPoint => Pin.GetWorldPos();

		public bool ShouldBeIncludedInSelectionBox(Vector2 selectionCentre, Vector2 selectionSize)
		{
			Bounds2D selfBounds = SelectionBoundingBox;
			return Maths.BoxesOverlap(selectionCentre, selectionSize, selfBounds.Centre, selfBounds.Size);
		}

		public int GetStateDecimalDisplayValue()
		{
			uint rawValue = Pin.State.GetValue();
			int displayValue = (int)rawValue;

			if (pinValueDisplayMode == PinValueDisplayMode.SignedDecimal)
			{
				displayValue = Maths.TwosComplement(rawValue, Math.Min((int)BitCount,32));
			}

			return displayValue;
		}

		Bounds2D CreateBoundingBox(float pad)
		{
			var isHorizontal = IsHorizontal;
			var handlePosition = isHorizontal ? HandlePosition.x : HandlePosition.y;
			var pinPosition = isHorizontal ? PinPosition.x : PinPosition.y;
			var dir = isHorizontal ? faceDir.x : faceDir.y;
			float x1 = handlePosition - dir * DevPinHandleWidth / 2;
			float x2 = pinPosition + dir * PinRadius;
			float minX = Mathf.Min(x1, x2);
			float maxX = Mathf.Max(x1, x2);

			Vector2 centre = new((minX + maxX) / 2, !isHorizontal ? HandlePosition.x : HandlePosition.y);
			Vector2 size = new Vector2(maxX - minX, BoundsHeight()) + Vector2.one * pad;
			if (!isHorizontal)
			{
				(size.x, size.y) = (size.y, size.x);
				(centre.x, centre.y) = (centre.y, centre.x);
			}
			return Bounds2D.CreateFromCentreAndSize(centre, size);
		}

		private Bounds2D HandleBounds() => Bounds2D.CreateFromCentreAndSize(HandlePosition, GetHandleSize());

		private float BoundsHeight() => StateGridSize.y;

		public Vector2 GetHandleSize() => IsHorizontal ? new(DevPinHandleWidth, BoundsHeight()): new(BoundsHeight(), DevPinHandleWidth);

		public void ToggleState(int bitIndex)
		{
			Pin.PlayerInputState.ToggleBit(bitIndex);
		}

		public bool PointIsInInteractionBounds(Vector2 point) => PointIsInHandleBounds(point) || PointIsInStateIndicatorBounds(point);

		public bool PointIsInStateIndicatorBounds(Vector2 point) => Maths.PointInCircle2D(point, StateDisplayPosition, DevPinStateDisplayRadius);

		public bool PointIsInHandleBounds(Vector2 point) => HandleBounds().PointInBounds(point);


		public void SetOrientation(Orientation orientation)
		{
			faceDir = GetFacingDirection(orientation);
		}

		public Vector2 GetFacingDirection(Orientation orientation) => orientation switch
		{
			Orientation.Top => Vector2.up,
			Orientation.Right => Vector2.right,
			Orientation.Bottom => Vector2.down,
			Orientation.Left => Vector2.left,
			_ => throw new ArgumentOutOfRangeException(nameof(orientation), $"No corresponding Vector2 for Orientation {orientation} implemented!")
		};

		public void Rotate(bool clockwise = true)
		{
			if (faceDir == Vector2.up)
			{
				faceDir = clockwise ? Vector2.right : Vector2.left;
				return;
			}

			if (faceDir == Vector2.right)
			{
				faceDir = clockwise ? Vector2.down : Vector2.up;
				return;
			}

			if (faceDir == Vector2.down)
			{
				faceDir = clockwise ? Vector2.left : Vector2.right;
			}
			else
			{
				faceDir = clockwise ? Vector2.up : Vector2.down;
			}
		}
	}
}