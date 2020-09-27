using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PartsCatalog.View.AttachedProperties
{
    public static class TextBoxInputFilter
    {
        public static readonly DependencyProperty CheckInputIntegerProperty =
            DependencyProperty.RegisterAttached("CheckInputInteger",
            typeof(bool), typeof(TextBoxInputFilter), new PropertyMetadata(false, CheckInputInteger));

        public static void SetCheckInputInteger(DependencyObject dp, bool value) =>
            dp.SetValue(CheckInputIntegerProperty, value);

        public static bool GetCheckInputInteger(DependencyObject dp) =>
            (bool)dp.GetValue(CheckInputIntegerProperty);

        private static void CheckInputInteger(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is TextBox textBox))
                return;

            if ((bool)e.OldValue)
            {
                textBox.PreviewTextInput -= PreviewTextInput;
                textBox.PreviewKeyDown -= PreviewKeyDown;
                DataObject.RemovePastingHandler(textBox, Paste);
            }

            if ((bool)e.NewValue)
            {
                textBox.PreviewTextInput += PreviewTextInput;
                textBox.PreviewKeyDown += PreviewKeyDown;
                DataObject.AddPastingHandler(textBox, Paste);
            }
        }

        private static void Paste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                if (!(sender is TextBox textBox))
                    return;
                string input = (string)e.DataObject.GetData(typeof(string));
                if (!int.TryParse(input, out _))
                    e.CancelCommand();
                else
                {
                    int currPos = textBox.SelectionStart;
                    string Txt = textBox.Text;
                    if (textBox.SelectionLength > 0)
                        Txt = Txt.Remove(currPos, textBox.SelectionLength).Insert(currPos, input);
                    else
                        Txt = Txt.Insert(currPos, input);
                    if (!int.TryParse(Txt, out _))
                        e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox textBox))
                return;

            e.Handled = false;
            if (e.Key == Key.Space)
                e.Handled = true;

            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                int cursorPos = textBox.SelectionStart;
                string text = textBox.Text;
                bool updateText = true;

                if (textBox.SelectionLength > 0)
                {
                    if (textBox.SelectionLength < text.Length)
                        text = text.Remove(cursorPos, textBox.SelectionLength);
                    else
                        text = "0";
                }

                else if (e.Key == Key.Delete)
                {
                    if (cursorPos == 0 && text.Length == 1)
                        text = "0";
                    else if (cursorPos < text.Length)
                        text = text.Remove(cursorPos, 1);
                    else
                        updateText = false;
                }
                else if (cursorPos > 0)
                {
                    if (text.Length == 1)
                        text = "0";
                    else
                        text = text.Remove(--cursorPos, 1);
                }
                else
                    updateText = false;

                if (updateText && int.TryParse(text, out _))
                {
                    textBox.Text = text;
                    if (text == "0")
                    {
                        textBox.SelectionStart = 0;
                        textBox.SelectionLength = 1;
                    }
                    else
                        textBox.SelectionStart = cursorPos;
                }
                e.Handled = true;
            }
        }

        private static void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textBox))
                return;
            if (!int.TryParse(e.Text, out _))
                e.Handled = true;

            int cursorPos = textBox.SelectionStart;
            string text = textBox.Text;

            if (textBox.SelectionLength > 0)
                text = text.Remove(cursorPos, textBox.SelectionLength).Insert(cursorPos, e.Text);
            else
                text = text.Insert(cursorPos, e.Text);

            e.Handled = !int.TryParse(text, out _);
        }
    }
}
