using ReactNative.Reflection;
using ReactNative.UIManager;
using ReactNative.UIManager.Annotations;
using System.Runtime.CompilerServices;
#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.Foundation.Metadata;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
#endif

namespace ReactNative.Views.View
{
    /// <summary>
    /// View manager for React view instances.
    /// </summary>
    public class ReactViewManager : ViewParentManager<BorderedCanvas>
    {
        private enum Radius
        {
            All,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        private class BackgroundBrushProperties
        {
            public uint? BackgroundColor;
            public bool RevealBrush;
            public double? AcrylicOpacity;
            public uint? AcrylicTintColor;
            public string AcrylicSource;
        }

        private readonly bool _isFluentSupported =
#if WINDOWS_UWP
            ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5, 0);
#else
            false;
#endif

        private readonly ConditionalWeakTable<DependencyObject, BackgroundBrushProperties> _backgroundBrushProperties =
            new ConditionalWeakTable<DependencyObject, BackgroundBrushProperties>();

        /// <summary>
        /// Default brush for the view borders.
        /// </summary>
        protected static readonly Brush s_defaultBorderBrush = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// The name of this view manager. This will be the name used to 
        /// reference this view manager from JavaScript.
        /// </summary>
        public override string Name
        {
            get
            {
                return "RCTView";
            }
        }

        /// <summary>
        /// Once any of the background-brush-related properties are changed,
        /// this method is invoked to update the XAML brush. If acrylic-related
        /// properties are set on the React view, they arrive here first, but
        /// the brush can be created only when backgroundColor is set.
        /// </summary>
        private void UpdateBackgroundBrush(BorderedCanvas view)
        {
            var border = GetOrCreateBorder(view);
            var props = _backgroundBrushProperties.GetOrCreateValue(view);

            if (props.BackgroundColor != null)
            {
                if (_isFluentSupported && props.RevealBrush)
                {
                    border.Background = new RevealBackgroundBrush
                    {
                        Color = ColorHelpers.Parse(props.BackgroundColor.Value),
                        FallbackColor = ColorHelpers.Parse(props.BackgroundColor.Value),
                    };
                }
                else if (_isFluentSupported && (props.AcrylicOpacity != null || props.AcrylicTintColor != null))
                {
#if WINDOWS_UWP
                    border.Background = new AcrylicBrush
                    {
                        BackgroundSource = props.AcrylicSource == "app" ?
                            AcrylicBackgroundSource.Backdrop :
                            AcrylicBackgroundSource.HostBackdrop,
                        TintColor = ColorHelpers.Parse(props.AcrylicTintColor ?? props.BackgroundColor.Value),
                        FallbackColor = ColorHelpers.Parse(props.BackgroundColor.Value),
                        TintOpacity = props.AcrylicOpacity ?? 1.0,
                    };
#endif
                }
                else
                {
                    border.Background = new SolidColorBrush(ColorHelpers.Parse(props.BackgroundColor.Value));
                }
            }
        }

        /// <summary>
        /// Checks if the Canvas has a Border already.
        /// </summary>
        protected bool HasBorder(BorderedCanvas view)
        {
            return view.Border != null;
        }

        /// <summary>
        /// Adds a Border to a Canvas if it hasn't been added already.
        /// </summary>
        protected Border GetOrCreateBorder(BorderedCanvas view)
        {
            if (view.Border == null)
            {
                view.Border = new Border { BorderBrush = s_defaultBorderBrush };

                // Layout animations bypass SetDimensions, hence using XAML bindings.

                view.Border.SetBinding(FrameworkElement.WidthProperty, new Binding
                {
                    Source = view,
                    Path = new PropertyPath("Width")
                });

                view.Border.SetBinding(FrameworkElement.HeightProperty, new Binding
                {
                    Source = view,
                    Path = new PropertyPath("Height")
                });
            }

            return view.Border;
        }

        /// <summary>
        /// Creates a new view instance of type <see cref="Canvas"/>.
        /// </summary>
        /// <param name="reactContext">The React context.</param>
        /// <returns>The view instance.</returns>
        protected override BorderedCanvas CreateViewInstance(ThemedReactContext reactContext)
        {
            return new BorderedCanvas();
        }

        /// <summary>
        /// Sets whether or not the view is an accessibility element.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="accessible">A flag indicating whether or not the view is an accessibility element.</param>
        [ReactProp("accessible")]
        public void SetAccessible(BorderedCanvas view, bool accessible)
        {
            // TODO: #557 Provide implementation for View's accessible prop

            // We need to have this stub for this prop so that Views which
            // specify the accessible prop aren't considered to be layout-only.
            // The proper implementation is still to be determined.
        }

        /// <summary>
        /// Set the pointer events handling mode for the view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="pointerEventsValue">The pointerEvents mode.</param>
        [ReactProp("pointerEvents")]
        public void SetPointerEvents(BorderedCanvas view, string pointerEventsValue)
        {
            var pointerEvents = EnumHelpers.ParseNullable<PointerEvents>(pointerEventsValue) ?? PointerEvents.Auto;
            view.SetPointerEvents(pointerEvents);
        }

        /// <summary>
        /// Sets the border radius of the view.
        /// </summary>
        /// <param name="view">The view panel.</param>
        /// <param name="index">The property index.</param>
        /// <param name="radius">The border radius value.</param>
        [ReactPropGroup(
            ViewProps.BorderRadius,
            ViewProps.BorderTopLeftRadius,
            ViewProps.BorderTopRightRadius,
            ViewProps.BorderBottomLeftRadius,
            ViewProps.BorderBottomRightRadius)]
        public void SetBorderRadius(BorderedCanvas view, int index, double radius)
        {
            var border = GetOrCreateBorder(view);
            var cornerRadius = border.CornerRadius == null ? new CornerRadius() : border.CornerRadius;

            switch ((Radius)index)
            {
                case Radius.All:
                    cornerRadius = new CornerRadius(radius);
                    break;
                case Radius.TopLeft:
                    cornerRadius.TopLeft = radius;
                    break;
                case Radius.TopRight:
                    cornerRadius.TopRight = radius;
                    break;
                case Radius.BottomLeft:
                    cornerRadius.BottomLeft = radius;
                    break;
                case Radius.BottomRight:
                    cornerRadius.BottomRight = radius;
                    break;
            }

            border.CornerRadius = cornerRadius;
        }

        /// <summary>
        /// Sets the background color of the view.
        /// </summary>
        /// <param name="view">The view panel.</param>
        /// <param name="color">The masked color value.</param>
        [ReactProp(
            ViewProps.BackgroundColor,
            CustomType = "Color",
            DefaultUInt32 = ColorHelpers.Transparent)]
        public void SetBackgroundColor(BorderedCanvas view, uint color)
        {
            _backgroundBrushProperties.GetOrCreateValue(view).BackgroundColor = color;
            UpdateBackgroundBrush(view);
        }

        [ReactProp(ViewProps.AcrylicOpacity)]
        public void SetAcrylicOpacity(BorderedCanvas view, double value)
        {
            _backgroundBrushProperties.GetOrCreateValue(view).AcrylicOpacity = value;
            UpdateBackgroundBrush(view);
        }

        [ReactProp(ViewProps.AcrylicSource)]
        public void SetAcrylicSource(BorderedCanvas view, string value)
        {
            _backgroundBrushProperties.GetOrCreateValue(view).AcrylicSource = value;
            UpdateBackgroundBrush(view);
        }

        [ReactProp(ViewProps.AcrylicTintColor)]
        public void SetAcrylicTintColor(BorderedCanvas view, uint? value)
        {
            _backgroundBrushProperties.GetOrCreateValue(view).AcrylicTintColor = value;
            UpdateBackgroundBrush(view);
        }

        [ReactProp("reveal", DefaultBoolean = false)]
        public void SetRevealHighlightEnabled(BorderedCanvas view, bool value)
        {
            _backgroundBrushProperties.GetOrCreateValue(view).RevealBrush = value;
            UpdateBackgroundBrush(view);

            if (_isFluentSupported)
            {
                if (value)
                {
                    view.PointerEntered += Reveal_PointerEntered;
                    view.PointerExited += Reveal_PointerExited;
                }
                else
                {
                    view.PointerEntered -= Reveal_PointerEntered;
                    view.PointerExited -= Reveal_PointerExited;
                }
            }
        }

        private void Reveal_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var canvas = sender as BorderedCanvas;
            var border = GetOrCreateBorder(canvas);
            RevealBrush.SetState(border, RevealBrushState.PointerOver);
        }

        private void Reveal_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var canvas = sender as BorderedCanvas;
            var border = GetOrCreateBorder(canvas);
            border.ClearValue(RevealBrush.StateProperty);
        }

        /// <summary>
        /// Set the border color of the view.
        /// </summary>
        /// <param name="view">The view panel.</param>
        /// <param name="color">The color hex code.</param>
        [ReactProp("borderColor", CustomType = "Color")]
        public void SetBorderColor(BorderedCanvas view, uint? color)
        {
            var border = GetOrCreateBorder(view);
            border.BorderBrush = color.HasValue
                ? new SolidColorBrush(ColorHelpers.Parse(color.Value))
                : s_defaultBorderBrush;
            if (_backgroundBrushProperties.GetOrCreateValue(view).RevealBrush)
            {
                border.BorderBrush = new RevealBorderBrush
                {
                    Color = ColorHelpers.Parse(color.Value),
                    FallbackColor = ColorHelpers.Parse(color.Value),
                };
            }
        }

        /// <summary>
        /// Sets the border thickness of the view.
        /// </summary>
        /// <param name="view">The view panel.</param>
        /// <param name="index">The property index.</param>
        /// <param name="width">The border width in pixels.</param>
        [ReactPropGroup(
            ViewProps.BorderWidth,
            ViewProps.BorderLeftWidth,
            ViewProps.BorderRightWidth,
            ViewProps.BorderTopWidth,
            ViewProps.BorderBottomWidth)]
        public void SetBorderWidth(BorderedCanvas view, int index, double width)
        {
            var border = GetOrCreateBorder(view);
            border.SetBorderWidth(ViewProps.BorderSpacingTypes[index], width);
        }

        /// <summary>
        /// Sets whether the view is collapsible.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="collapsible">The flag.</param>
        [ReactProp(ViewProps.Collapsible)]
        public void SetCollapsible(BorderedCanvas view, bool collapsible)
        {
            // no-op: it's here only so that "collapsable" property is exported to JS. The value is actually
            // handled in NativeViewHierarchyOptimizer
        }

        /// <summary>
        /// Adds a child at the given index.
        /// </summary>
        /// <param name="parent">The parent view.</param>
        /// <param name="child">The child view.</param>
        /// <param name="index">The index.</param>
        public override void AddView(BorderedCanvas parent, DependencyObject child, int index)
        {
            if (HasBorder(parent))
            {
                index++;
            }

            var uiElementChild = child.As<UIElement>();
            parent.Children.Insert(index, uiElementChild);
        }

        /// <summary>
        /// Gets the child at the given index.
        /// </summary>
        /// <param name="parent">The parent view.</param>
        /// <param name="index">The index.</param>
        /// <returns>The child view.</returns>
        public override DependencyObject GetChildAt(BorderedCanvas parent, int index)
        {
            if (HasBorder(parent))
            {
                index++;
            }

            return parent.Children[index];
        }

        /// <summary>
        /// Gets the number of children in the view parent.
        /// </summary>
        /// <param name="parent">The view parent.</param>
        /// <returns>The number of children.</returns>
        public override int GetChildCount(BorderedCanvas parent)
        {
            var count = parent.Children.Count;

            if (HasBorder(parent))
            {
                count--;
            }

            return count;
        }

        /// <summary>
        /// Removes all children from the view parent.
        /// </summary>
        /// <param name="parent">The view parent.</param>
        public override void RemoveAllChildren(BorderedCanvas parent)
        {
            if (HasBorder(parent))
            {
                for (var i = parent.Children.Count - 1; i > 0; i--)
                {
                    parent.Children.RemoveAt(i);
                }
            }
            else
            {
                parent.Children.Clear();
            }
        }

        /// <summary>
        /// Removes the child at the given index.
        /// </summary>
        /// <param name="parent">The view parent.</param>
        /// <param name="index">The index.</param>
        public override void RemoveChildAt(BorderedCanvas parent, int index)
        {
            if (HasBorder(parent))
            {
                index++;
            }

            parent.Children.RemoveAt(index);
        }
    }
}
