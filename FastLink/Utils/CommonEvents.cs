using System.Windows;
using System.Windows.Input;

namespace FastLink.Utils
{
    public static class CommonEvents
    {
        public static void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (sender is System.Windows.Window window)
                    window.Hide();
                e.Handled = true;
            }
        }

        public static void KeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox textBox) return;

            // Ignore modifiers
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                e.Handled = true;
                return;
            }

            // Backspace : clear textbox
            if (e.Key == Key.Back)
            {
                textBox.Clear();
                e.Handled = true;
                return;
            }

            if (e.Key >= Key.F1 && e.Key <= Key.F24)
                textBox.Text = e.Key.ToString();
            else if (e.Key >= Key.A && e.Key <= Key.Z)
                textBox.Text = e.Key.ToString();
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
                textBox.Text = e.Key.ToString().Substring(1);
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                textBox.Text = "Num" + (e.Key - Key.NumPad0);
            else textBox.Text = e.Key.ToString();

            textBox.CaretIndex = textBox.Text.Length;
            e.Handled = true;
        }
    }
}