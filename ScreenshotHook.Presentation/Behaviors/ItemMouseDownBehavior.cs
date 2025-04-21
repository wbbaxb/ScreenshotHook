using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace ScreenshotHook.Presentation.Behaviors
{
    public class ItemMouseDownBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    Application.Current.MainWindow.DragMove();
                }
            };
        }
    }
}