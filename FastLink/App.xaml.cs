using System.Windows;

namespace FastLink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        Mutex mutex;

        // 중복 실행 방지
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string mutexName = "UniqueFastLink";
            mutex = new Mutex(true, mutexName, out bool createNew);
            if (!createNew) Shutdown();
        }
    }
}
