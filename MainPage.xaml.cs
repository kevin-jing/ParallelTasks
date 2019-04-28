using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ParallelTasks
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            CreateErrorFile();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // exception cannot be catched without await.
            try
            {
                GetAsync();
            }
            catch(Exception ex)
            {
                tbError.Text = ex.ToString();
            }
        }
        StorageFile _errorFile;
        async void GetAsync()
        {
            string e = null;
            List<Task> parallelTasks = new List<Task>();
            var t1 = Get1Async(1.5);
            parallelTasks.Add(t1);
            var t2 = Get2Async(2);
            parallelTasks.Add(t2);

            try
            {
                await Task.WhenAll(parallelTasks);
            }
            catch (Exception ex)
            {
                await WriteMessage(ex.StackTrace);
                e = ex.ToString();
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (e == null)
                {
                    tb1.Text = t1.Result.ToString();
                    tb2.Text = t2.Result;
                }
                else
                {
                    tbError.Text = e;
                }
            });
        }
        async Task<double> Get1Async (double p)
        {
            await Task.Delay((int)(p * 1000));
            return p;
        }
        async Task<string> Get2Async(double p)
        {
            await Task.Delay((int)(p * 1000));
            throw new NotImplementedException();
            return p.ToString();
        }

        private async void CreateErrorFile()
        {
            try
            {
                _errorFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("error.log", CreationCollisionOption.ReplaceExisting);
                await WriteMessage("App started.");
            }
            catch (Exception)
            {
                // If cannot open our error file, then that is a shame. This should always succeed 
                // you could try and log to an internet serivce(i.e. Azure Mobile Service) here so you have a record of this failure.
            }
        }

        //C:\Users\qfjing\AppData\Local\Packages\StockMonitor_mx3h8d1kvnvcy\LocalState
        private SemaphoreSlim _errorFileSemaphore = new SemaphoreSlim(1);
        public async Task WriteMessage(string strMessage)
        {
            if (_errorFile != null)
            {
                await _errorFileSemaphore.WaitAsync();
                try
                { // Run asynchronously
                    await FileIO.AppendTextAsync(_errorFile, $"\r\n{DateTime.Now.ToLocalTime().ToString()}\r\n{strMessage}\r\n");
                }
                catch (Exception)
                { // If another option is available to the app to log error(i.e. Azure Mobile Service, etc...) then try that here
                }
                finally
                {
                    _errorFileSemaphore.Release();
                }
            }
        }
    }
}
