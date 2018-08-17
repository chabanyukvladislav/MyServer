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
                OnKeyChanged?.Invoke();
            }
        }

        static MyKey()
        {
            Worker();
        }

        public static bool IsEnable()
        {
            return _key != Guid.Empty;
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
                        _key = Guid.Empty;
                    }
                }
            });
        }

        public static event Action OnKeyChanged;
    }
}
