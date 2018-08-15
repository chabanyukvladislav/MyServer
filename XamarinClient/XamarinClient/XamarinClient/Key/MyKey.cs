using System;
using System.Threading;
using System.Threading.Tasks;

namespace XamarinClient.Key
{
    class MyKey
    {
        private static Guid _key;
        private static DateTime _date;

        public static Guid Key
        {
            get => _key;
            set
            {
                _key = value;
                _date = DateTime.Now;
                IsEnable = true;
                OnKeyChanged?.Invoke();
            }
        }

        public static bool IsEnable { get; private set; }

        static MyKey()
        {
            Worker();
        }

        private static async void Worker()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000 * 60 * 60 * 24);
                    if (_date.Day + 7 < DateTime.Now.Day)
                    {
                        IsEnable = false;
                        _key = Guid.Empty;
                    }
                }
            });
        }

        public static event Action OnKeyChanged;
    }
}
