using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TimerMain
{
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
    public sealed class InvertedBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public InvertedBooleanToVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible)
        { }
    }
}
