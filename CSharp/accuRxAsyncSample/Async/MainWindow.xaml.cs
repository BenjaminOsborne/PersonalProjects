using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Async
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient _client = new HttpClient();

        public MainWindow() => InitializeComponent();

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var result = _GetResultFromTheInterWebs().Result;

            DemoTextBox.Text = $"Status Code: {result.StatusCode}";
        }

        private static Task<HttpResponseMessage> _GetResultFromTheInterWebs() =>
            _client.GetAsync(new Uri("https://www.google.com/"));
    }
}

#region Hide...

//.ConfigureAwait(continueOnCapturedContext: false);
//private static int _ThreadId => System.Threading.Thread.CurrentThread.ManagedThreadId;
//private static SynchronizationContext _GetSynchronizationContext() => SynchronizationContext.Current;

#endregion
