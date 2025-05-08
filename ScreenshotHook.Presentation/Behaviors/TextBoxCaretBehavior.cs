using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace ScreenshotHook.Presentation.Behaviors
{
    public class TextBoxCaretBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).IsFocused)
            {
                TextBox textBox = sender as TextBox;
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
    }
} 