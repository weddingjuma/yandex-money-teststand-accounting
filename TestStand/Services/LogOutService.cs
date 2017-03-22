using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestStand.Controls;
using TestStand.View;
using Windows.UI.Xaml;
using Jupiter.Application;

namespace TestStand.Services
{
    public class LogOutService
    {
        private static DispatcherTimer _timer = new DispatcherTimer();

        static LogOutService()
        {
            _timer.Tick += TimerTick;
            _timer.Interval = TimeSpan.FromMinutes(3);
        }

        public static void Start()
        {
            _timer.Start();
        }

        public static void Stop()
        {
            _timer.Stop();
        }
        public static void Restart()
        {
            Stop();
            Start();
        }

        private static void TimerTick(object sender, object e)
        {
            _timer.Stop();
            Popup.Show(new UserTimeoutView());
        }
    }
}
