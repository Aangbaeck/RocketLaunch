using System;
using System.IO;
using System.Windows;
using Serilog;
using SharpVectors.Converters;

namespace RocketLaunch.Helper
{
    public class SvgViewboxAttachedProperties : DependencyObject
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source",
                typeof(string), typeof(SvgViewboxAttachedProperties),
                // default value: null
                new PropertyMetadata(null, OnSourceChanged));

        public static string GetSource(DependencyObject obj)
        {
            return (string) obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var svgControl = obj as SvgViewbox;
            if (svgControl != null)
            {
                string s = (string) e.NewValue;
                try
                {
                    Uri uri;
                    if (!Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out uri))
                    {
                        Log.Error(string.Format("'{0}' is not a valid URI", s));
                    }
                    else if (uri.IsAbsoluteUri)
                    {
                        svgControl.Source = new Uri(s, UriKind.Absolute);
                    }
                    else
                    {
                        s = Directory.GetCurrentDirectory() + s;
                        svgControl.Source = new Uri(s, UriKind.RelativeOrAbsolute);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(string.Format("Looking for file in path: '{0}', errror message: ", s), exception);
                    Console.WriteLine(@"Looking for file in path: '{0}', errror message: " + s + exception.Message);
                }
            }
        }
    }
}