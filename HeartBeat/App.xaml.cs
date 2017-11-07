using Xamarin.Forms;

namespace HeartBeat
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new HeartBeatPage();
        }//patientId
        //primary key: pEGwJ6Iyvuf+TkxzgVjgz2K2ftqhXYKKQJpahjCHIys=
        // primary key - connection string: HostName=HearbeatXL.azure-devices.net;DeviceId=patientId;SharedAccessKey=pEGwJ6Iyvuf+TkxzgVjgz2K2ftqhXYKKQJpahjCHIys=


        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
