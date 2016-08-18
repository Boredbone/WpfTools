using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTools.Controls
{
    /// <summary>
    /// http://proprogrammer.hatenadiary.jp/entry/2014/12/25/014448
    /// </summary>
    public class FastCanvas : Control
    {
        public static Point GetLocation(DependencyObject obj)
        {
            return (Point)obj.GetValue(LocationProperty);
        }

        public static void SetLocation(DependencyObject obj, Point value)
        {
            obj.SetValue(LocationProperty, value);
        }
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.RegisterAttached("Location", typeof(Point), typeof(FastCanvas),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsArrange));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ObservableCollection<UIElement> Children { get; private set; }

        private Border mainPanel = null;


        static FastCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FastCanvas),
                new FrameworkPropertyMetadata(typeof(FastCanvas)));
        }

        public FastCanvas()
        {
            Children = new ObservableCollection<UIElement>();

            Children.CollectionChanged += Children_CollectionChanged;
        }

        void Children_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (UIElement oldItem in e.OldItems)
                {
                    RemoveVisualChild(oldItem);
                }
            }

            if (e.NewItems != null)
            {
                foreach (UIElement newItem in e.NewItems)
                {
                    AddVisualChild(newItem);
                }
            }
        }

        /// <summary>
        /// Remove all VisualChild
        /// </summary>
        public void ClearChildren()
        {
            var oldItems = this.Children.ToArray();

            this.Children.Clear();
            foreach (UIElement oldItem in oldItems)
            {
                RemoveVisualChild(oldItem);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.mainPanel = this.GetTemplateChild("PART_RootBorder") as Border;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return Children.Count + 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                return this.mainPanel;
            }
            return Children[index - 1];
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }
                var location = GetLocation(child);

                child.Arrange(new Rect(location, child.DesiredSize));
            }

            return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureOverride(Size availableSize)
        {

            foreach (var child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }
                var fe = child as FrameworkElement;

                if (fe != null)
                {
                    //var margin = fe.Margin;
                    //child.Measure(new Size
                    //    (fe.Width + margin.Left + margin.Right, fe.Height + margin.Top + margin.Bottom));
                    child.Measure(new Size(fe.Width, fe.Height));
                }
            }

            return base.MeasureOverride(availableSize);
        }
    }


    public class FastCanvas2 : Canvas
    {
        private HashSet<UIElement> _virtualChildren = new HashSet<UIElement>();

        public void AddElement(UIElement element)
        {
            _virtualChildren.Add(element);
        }

        public void RemoveElement(UIElement element)
        {
            _virtualChildren.Remove(element);
        }

        public void SetViewport(Rect rect)
        {
            foreach (FrameworkElement child in _virtualChildren)
            {
                var childRect = new Rect(Canvas.GetLeft(child), Canvas.GetTop(child), child.Width, child.Height);
                if (!rect.IntersectsWith(childRect))
                {
                    if (Children.Contains(child))
                        Children.Remove(child);
                }
                else
                {
                    if (!Children.Contains(child))
                        Children.Add(child);
                }
            }
        }
    }
}
