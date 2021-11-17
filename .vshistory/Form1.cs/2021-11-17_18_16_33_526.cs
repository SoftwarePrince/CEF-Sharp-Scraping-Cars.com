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

namespace FirstScraping
{
    public partial class Form1 : Form
    {
        public ChromiumWebBrowser chromeBrowser;

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            // Initialize cef with the provided settings
            Cef.Initialize(settings);
            // Create a browser component
            const string initialUrl = "http://testing-ground.scraping.pro/login";
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
                if (!isLoggedIn)
                {
                    // fill the username and password fields with their respective values, then click the submit button
                    var loginScript = @"document.querySelector('#usr').value = 'admin';
                               document.querySelector('#pwd').value = '12345';
                               document.querySelector('input[type=submit]').click();";

                    chr.EvaluateScriptAsync(loginScript).ContinueWith(u => {
                        isLoggedIn = true;
                        Console.WriteLine("User Logged in.n");
                    });
                }
                else
                {
                    // push the "success" field and the text from all 'li' elements into an array
                    var pageQueryScript = @"
                    (function(){
                        var lis = document.querySelectorAll('li');
                        var result = [];
                        result.push(document.querySelector('h3.success').innerText);
                        for(var i=0; i < lis.length; i++) { result.push(lis[i].innerText) } 
                        return result; 
                    })()";
                    var scriptTask = browser.EvaluateScriptAsync(pageQueryScript);
                    scriptTask.ContinueWith(u =>
                    {
                        if (u.Result.Success && u.Result.Result != null)
                        {
                            Console.WriteLine("Bot output received!nn");
                            var filePath = "output.txt";
                            var response = (List<dynamic>)u.Result.Result;
                            foreach (string v in response)
                            {
                                Console.WriteLine(v);
                            }
                            File.WriteAllLines(filePath, response.Select(v => (string)v).ToArray());
                            Console.WriteLine($"nnBot output saved to {filePath}");
                            Console.WriteLine("nnPress any key to close.");
                        }
                    });
                }
            }
        }
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
