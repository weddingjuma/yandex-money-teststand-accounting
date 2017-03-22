using System;
using TestStand.Enum;
using Windows.UI.Xaml.Data;

namespace TestStand.Converters
{
    public class DeviceTypeToIconConverter : IValueConverter
    {
        public Uri PhoneIconPath { get; set; }

        public Uri TabletIconPath { get; set; }

        public Uri WearableIconPath { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var deviceType = (DeviceType)value;

            switch (deviceType)
            {
                case DeviceType.Phone:
                    return PhoneIconPath;
                case DeviceType.Tablet:
                    return TabletIconPath;
                case DeviceType.Wearable:
                    return WearableIconPath;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
