using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.IO;
using System.Runtime.InteropServices;



namespace FirstScraping
{
    public partial class Form1 : Form
    {
        public static ChromiumWebBrowser chromeBrowser;

        public void InitializeChromium()
        {

            CefSettings settings = new CefSettings();
            // Initialize cef with the provided settings
            Cef.Initialize(settings);
            // Create a browser component
            const string initialUrl = "https://www.cars.com/signin/?redirect_path=%2Fshopping%2Fresults%2F%3Flist_price_max%3D100000%26makes%255B%255D%3Dtesla%26maximum_distance%3Dall%26models%255B%255D%3Dtesla-model_s%26stock_type%3Dused%26zip%3D94596";
            chromeBrowser = new ChromiumWebBrowser(initialUrl);
            chromeBrowser.LoadingStateChanged += BrowserLoadingStateChanged;

            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;
        }
        static bool isLoggedIn = false;
        private static void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                //MessageBox.Show(isLoggedIn.ToString(), "hello" );

                if (!isLoggedIn)
                {
                    // fill the username and password fields with their respective values, then click the submit button
                    var loginScript = @"document.querySelector('#email').value = 'johngerson808@gmail.com';
                               document.querySelector('#password').value = 'test8008';
                               document.querySelector('button[type=submit]').click();";

                    chromeBrowser.EvaluateScriptAsync(loginScript).ContinueWith(u => {
                        isLoggedIn = true;
                        Console.WriteLine("User Logged in.n");
                    });
                }
                else
                {

                }
            }
        }
        public 
        public Form1()
        {
            InitializeComponent();
            // Start the browser after initialize global component
            InitializeChromium();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

    }
}
