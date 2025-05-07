using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenshotHook.Presentation.Behaviors
{
    public class ScrollOnMouseWheelBehavior : Behavior<ScrollViewer>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            AssociatedObject.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            AssociatedObject.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            base.OnDetaching();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}