namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class LayoutDivider : ContentView
	{
		public event Action DragCompleted = delegate { };

		public const double NORMAL_SIZE = 8;
		public const double EDIT_SIZE = 24;

		public BoxView? Handle { get; private set; }

		private Grid SplitGrid { get; }
		private LayoutNode SplitNode { get; }
		private int ChildIndex { get; }
		private bool IsLeftToRight { get; }
		private double StartWeightBefore { get; set; }
		private double StartWeightAfter { get; set; }
		private double PixelsPerWeight { get; set; }
		private double MinDragDistance { get; set; }
		private double MaxDragDistance { get; set; }
		private double CurrentDragDistance { get; set; }

		private const double MIN_PANEL_SIZE = 120;
		private const double HANDLE_THICKNESS = 4;
		private const double DRAG_HANDLE_THICKNESS = 6;
		private const string HANDLE_COLOR = "#ac99ea";

		public LayoutDivider (Grid splitGrid, LayoutNode splitNode, int childIndex, bool isEditMode)
		{
			SplitGrid = splitGrid;
			SplitNode = splitNode;
			ChildIndex = childIndex;
			IsLeftToRight = splitNode.Axis == SplitAxis.LEFT_TO_RIGHT;

			if (isEditMode == true)
			{
				SetupDragging();
			}
		}

		private void SetupDragging ()
		{
			BackgroundColor = Colors.Transparent;
			Handle = CreateHandle();

			PanGestureRecognizer panGestureRecognizer = new();
			panGestureRecognizer.PanUpdated += OnPanUpdated;
			GestureRecognizers.Add(panGestureRecognizer);
		}

		private BoxView CreateHandle ()
		{
			BoxView handle = new()
			{
				Color = Color.FromArgb(HANDLE_COLOR),
				InputTransparent = true
			};

			if (IsLeftToRight == true)
			{
				handle.HorizontalOptions = LayoutOptions.Center;
				handle.VerticalOptions = LayoutOptions.Fill;
			}
			else
			{
				handle.HorizontalOptions = LayoutOptions.Fill;
				handle.VerticalOptions = LayoutOptions.Center;
			}

			SetHandleThickness(handle, HANDLE_THICKNESS);
			return handle;
		}

		private void SetHandleThickness (BoxView handle, double thickness)
		{
			handle.CornerRadius = thickness / 2;

			if (IsLeftToRight == true)
			{
				handle.WidthRequest = thickness;
			}
			else
			{
				handle.HeightRequest = thickness;
			}
		}

		private void OnPanUpdated (object? sender, PanUpdatedEventArgs e)
		{
			switch (e.StatusType)
			{
				case GestureStatus.Started:
					StartDrag();
					break;
				case GestureStatus.Running:
					UpdateDrag(IsLeftToRight == true ? e.TotalX : e.TotalY);
					break;
				case GestureStatus.Completed:
					CompleteDrag();
					break;
				case GestureStatus.Canceled:
					ResetDrag();
					break;
			}
		}

		private void StartDrag ()
		{
			List<LayoutNode> childCollection = SplitNode.ChildCollection!;
			StartWeightBefore = childCollection[ChildIndex].Weight;
			StartWeightAfter = childCollection[ChildIndex + 1].Weight;
			CurrentDragDistance = 0;

			double gridSize = IsLeftToRight == true ? SplitGrid.Width : SplitGrid.Height;
			double dividerSize = IsLeftToRight == true ? Width : Height;
			double panelSize = gridSize - (childCollection.Count - 1) * dividerSize;
			double totalWeight = childCollection.Sum(child => child.Weight);
			PixelsPerWeight = totalWeight > 0 ? panelSize / totalWeight : 0;

			if (PixelsPerWeight <= 0)
			{
				return;
			}

			double pairWeight = StartWeightBefore + StartWeightAfter;
			double minWeight = Math.Min(MIN_PANEL_SIZE / PixelsPerWeight, pairWeight / 2);
			MinDragDistance = Math.Min(0, (minWeight - StartWeightBefore) * PixelsPerWeight);
			MaxDragDistance = Math.Max(0, (pairWeight - minWeight - StartWeightBefore) * PixelsPerWeight);

			if (Handle != null)
			{
				SetHandleThickness(Handle, DRAG_HANDLE_THICKNESS);
			}
		}

		private void UpdateDrag (double dragDistance)
		{
			if (PixelsPerWeight <= 0 || Handle == null)
			{
				return;
			}

			CurrentDragDistance = Math.Clamp(dragDistance, MinDragDistance, MaxDragDistance);

			if (IsLeftToRight == true)
			{
				Handle.TranslationX = CurrentDragDistance;
			}
			else
			{
				Handle.TranslationY = CurrentDragDistance;
			}
		}

		private void CompleteDrag ()
		{
			if (PixelsPerWeight <= 0)
			{
				ResetDrag();
				return;
			}

			double pairWeight = StartWeightBefore + StartWeightAfter;
			double weightBefore = StartWeightBefore + CurrentDragDistance / PixelsPerWeight;

			SetChildWeight(ChildIndex, weightBefore);
			SetChildWeight(ChildIndex + 1, pairWeight - weightBefore);
			ResetDrag();
			DragCompleted();
		}

		private void ResetDrag ()
		{
			PixelsPerWeight = 0;
			CurrentDragDistance = 0;

			if (Handle == null)
			{
				return;
			}

			Handle.TranslationX = 0;
			Handle.TranslationY = 0;
			SetHandleThickness(Handle, HANDLE_THICKNESS);
		}

		private void SetChildWeight (int childIndex, double weight)
		{
			SplitNode.ChildCollection![childIndex].Weight = weight;
			GridLength length = new(weight, GridUnitType.Star);
			int definitionIndex = childIndex * 2;

			if (IsLeftToRight == true)
			{
				SplitGrid.ColumnDefinitions[definitionIndex].Width = length;
			}
			else
			{
				SplitGrid.RowDefinitions[definitionIndex].Height = length;
			}
		}
	}
}
