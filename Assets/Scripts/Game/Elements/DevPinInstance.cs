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
		public PinBitCount BitCount => Pin.bitCount;
		public readonly char[] DecimalDisplayCharBuffer = new char[16];

		public Vector2 FaceDir;

		public readonly bool IsInputPin;
		public string Name => Pin.Name;
		public readonly PinInstance Pin;
		public Vector2Int StateGridDimensions;
		public Vector2 StateGridSize;

		public PinValueDisplayMode PinValueDisplayMode;
		public bool IsHorizontal => FaceDir == Vector2.right || FaceDir == Vector2.left;

		private Vector2 _position;

		public DevPinInstance(PinDescription pinDescription, bool isInput)
		{
			ID = pinDescription.ID;
			IsInputPin = isInput;

			Pin = new PinInstance(pinDescription, new PinAddress(ID, 0), this, isInput);
			PinValueDisplayMode = pinDescription.ValueDisplayMode;

			// Calculate layout info
			FaceDir = pinDescription.DevPinFacingDirection;
			Position = pinDescription.Position;
			StateGridDimensions = GridHelper.GetStateGridDimension(BitCount);
			StateGridSize = BitCount == 1 ? 
				Vector2.one * (DevPinStateDisplayRadius * 2 + DevPinStateDisplayOutline * 2) :
				(Vector2)StateGridDimensions * MultiBitPinStateDisplaySquareSize + Vector2.one * DevPinStateDisplayOutline;
		}

		public Vector2 Position
		{
			get => _position;
			set
			{
				_position = value;
				UpdatePinPosition();
			}
		}
		public Vector2 HandlePosition => Position;
		public Vector2 StateDisplayPosition => (Position + PinPosition) / 2;
		public Vector2 PinPosition;

		public Vector2 MoveStartPosition { get; set; }
		public Vector2 StraightLineReferencePoint { get; set; }
		public int ID { get; }

		public bool IsSelected { get; set; }
		public bool HasReferencePointForStraightLineMovement { get; set; }
		public bool IsValidMovePos { get; set; }

		public Bounds2D SelectionBoundingBox => CreateBoundingBox(SelectionBoundsPadding);
		public Bounds2D BoundingBox => CreateBoundingBox(0);
		public Vector2 SnapPoint => PinPosition;

		private void UpdatePinPosition()
		{
			PinPosition = BitCount.BitCount switch
			{
				1 or 4 => Position + FaceDir * (GridSize * 6),
				8 => Position + FaceDir * (GridSize * 9),
				_ => Position + FaceDir * (GridSize * 2 + StateGridSize.x)
			};
		}

		public bool ShouldBeIncludedInSelectionBox(Vector2 selectionCentre, Vector2 selectionSize)
		{
			Bounds2D selfBounds = SelectionBoundingBox;
			return Maths.BoxesOverlap(selectionCentre, selectionSize, selfBounds.Centre, selfBounds.Size);
		}

		public int GetStateDecimalDisplayValue()
		{
			uint rawValue = Pin.State.GetValue();
			int displayValue = (int)rawValue;

			if (PinValueDisplayMode == PinValueDisplayMode.SignedDecimal)
			{
				displayValue = Maths.TwosComplement(rawValue, Math.Min((int)BitCount,32));
			}

			return displayValue;
		}

		Bounds2D CreateBoundingBox(float pad)
		{
			if (IsHorizontal)
			{
				float dir = FaceDir.x;
				float x1 = HandlePosition.x - dir * DevPinHandleWidth / 2;
				float x2 = PinPosition.x + dir * PinRadius;
				float minX = Mathf.Min(x1, x2);
				float maxX = Mathf.Max(x1, x2);

				Vector2 centre = new((minX + maxX) / 2, HandlePosition.y);
				Vector2 size = new Vector2(maxX - minX, BoundsHeight()) + Vector2.one * pad;
				return Bounds2D.CreateFromCentreAndSize(centre, size);
			}
			else
			{
				float dir = FaceDir.y;
				float x1 = HandlePosition.y - dir * DevPinHandleWidth / 2;
				float x2 = PinPosition.y + dir * PinRadius;
				float minX = Mathf.Min(x1, x2);
				float maxX = Mathf.Max(x1, x2);

				Vector2 centre = new(HandlePosition.x, (minX + maxX) / 2);
				Vector2 size = new Vector2(BoundsHeight(), maxX - minX ) + Vector2.one * pad;
				return Bounds2D.CreateFromCentreAndSize(centre, size);
			}
		}

		public Bounds2D HandleBounds() => Bounds2D.CreateFromCentreAndSize(HandlePosition, GetHandleSize());

		public float BoundsHeight() => StateGridSize.y;

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
			FaceDir = GetFacingDirection(orientation);
			UpdatePinPosition();
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
			if (FaceDir == Vector2.up)
				FaceDir = clockwise ? Vector2.right : Vector2.left;
			
			else if (FaceDir == Vector2.right)
				FaceDir = clockwise ? Vector2.down : Vector2.up;

			else if (FaceDir == Vector2.down)
				FaceDir = clockwise ? Vector2.left : Vector2.right;
			
			else if  (FaceDir == Vector2.left) 
				FaceDir = clockwise ? Vector2.up : Vector2.down;
			
			UpdatePinPosition();
		}
	}
}