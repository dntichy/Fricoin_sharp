using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Wallet
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        TextBoxOutputter outputter;

        public ConsoleWindow()
        {
            DataContext = this;
            InitializeComponent();
            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);
            Console.WriteLine("----------Debug console started----------");
        }
    }

    internal class TextBoxOutputter : TextWriter
    {
        TextBlock textBlock = null;

        public TextBoxOutputter(TextBlock output)
        {
            textBlock = output;
        }
        public override void Write(char value)
        {
            base.Write(value);
            textBlock.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBlock.Text += value.ToString();

            }));


        }
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;

            }
        }
    }

    public static class Helper
    {
        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(Helper), new PropertyMetadata(false, AutoScrollPropertyChanged));

        private static void AutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;

            if (scrollViewer != null && (bool)e.NewValue)
            {
                scrollViewer.ScrollToBottom();
            }
        }
    }
}
