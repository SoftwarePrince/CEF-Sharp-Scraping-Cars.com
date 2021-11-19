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
using System.Threading;


namespace FirstScraping
{


    public partial class Form1 : Form
    {
        public static ChromiumWebBrowser chromeBrowser;
        public static string[] searchRes = new[] { "" };
        public static bool sleep(int time)
        {
            var teststr = @"    ( function() {
        await CefSharp.BindObjectAsync('boundAsync', 'bound');
                boundAsync.sleep(" + time + "); })(); ";

            chromeBrowser.EvaluateScriptAsync(teststr);
            return true;
        }
        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            // Initialize cef with the provided settings
            Cef.Initialize(settings);
            // Create a browser component
            string initialUrl = "https://www.cars.com/signin/?redirect_path=%2Fshopping%2Fresults%2F%3Flist_price_max%3D100000%26makes%255B%255D%3Dtesla%26maximum_distance%3Dall%26models%255B%255D%3Dtesla-model_s%26stock_type%3Dused%26zip%3D94596";
            //remove after test
            //initialUrl = "https://www.cars.com/shopping/results/?list_price_max=100000&makes%5B%5D=tesla&maximum_distance=all&models%5B%5D=tesla-model_s&stock_type=used&zip=94596";

            chromeBrowser = new ChromiumWebBrowser(initialUrl);
            chromeBrowser.JavascriptObjectRepository.Register("boundAsync", new BoundObject(), true);

            chromeBrowser.LoadingStateChanged += BrowserLoadingStateChanged;

            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;
        }
        static bool isLoggedIn = false;
        static bool isScrapped = false;
        static int scrapingLoopCount=0 ;
        private static void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {

                if (!isLoggedIn)
                {
                    // fill the username and password fields, then click the submit button
                    var loginScript = @"document.querySelector('#email').value = 'johngerson808@gmail.com';
                               document.querySelector('#password').value = 'test8008';
                               document.querySelector('button[type=submit]').click();";

                    chromeBrowser.EvaluateScriptAsync(loginScript).ContinueWith(u =>
                    {
                        isLoggedIn = true;
                        Console.WriteLine("User Logged in.n");
                    });
                }
                else if (!isScrapped && scrapingLoopCount<2 )
                {
                    startScraping();
                }
            }
        }
        public static void startScraping()
        {
            // Get us off the main thread
            Task task = new Task(() =>
            {
                scrapingLoopCount++;
                scrapSearchResults();
            });
            task.Start();
        }
        public static void openSecondSearchPage()
        {
            if (scrapingLoopCount > 2) return;
            // push all search results into an array
            var pageQueryScript = @"(function(){
//open second page
document.querySelectorAll(`[aria-label='Go to Page 2']`)[0].click();
//wait 2nd page
})()";
             chromeBrowser.EvaluateScriptAsync(pageQueryScript);
        }

        private static async void scrapSearchResults()
        {
            // push all search results into an array
            var pageQueryScript = @"(function(){
var result = [];
 function scrapeSearch(){                      
var lis =  document.querySelectorAll('[data-linkname=vehicle-listing]');
                        for(var i=0; i < lis.length; i++) { result.push(lis[i].innerText +' || '+ lis[i].nextElementSibling.innerText +' || '+ lis[i].nextElementSibling.nextElementSibling.innerText ) } 

console.log('return scraping '+ result.length+'\n\n')
}
//scrape page
console.log('starting scraping search page\n\n')

scrapeSearch()
return result;

})()";

            JavascriptResponse u = await chromeBrowser.EvaluateScriptAsync(pageQueryScript);
            Console.WriteLine("Bot output received! before check \n\n");
            if (u.Result != null)
            {
                Console.WriteLine("\n\n Bot output received! \n\n", scrapingLoopCount);
                var response = (List<dynamic>)u.Result;
                foreach (string v in response)
                {
                    Console.WriteLine(v);
                    searchRes = searchRes.Concat(new[] { v }).ToArray();
                }
                Console.WriteLine($"\n\n Bot output saved to array");

                if (!isScrapped||(isScrapped && scrapingLoopCount>1) )
                {
                    openSecondSearchPage();
                    Console.WriteLine("\n\n Opening 2nd page");
                    isScrapped = true;
                    Thread.Sleep(10000);
                    Console.WriteLine($"\n\n Bot Scraping 2nd page");
                    scrapSearchResults();
                    Console.WriteLine("\n\n Done 2nd page");
                    Thread.Sleep(5000);
                    if (scrapingLoopCount > 1)
                    {
                        saveSearchArray();
                        //open random car
                        await chromeBrowser.EvaluateScriptAsync("document.querySelectorAll('[data-linkname=vehicle-listing]')[7].click();");
                        Thread.Sleep(10000);
                        scrapCarDetails();
                        //Thread.Sleep(1000);
                        //go back
                        // await chromeBrowser.EvaluateScriptAsync("history.back(); ");
                        //Thread.Sleep(10000);
                    }
                    else
                    {
                        //select model X
                        await chromeBrowser.EvaluateScriptAsync("document.querySelector('#model_tesla-model_x').click(); document.querySelector('#model_tesla-model_s').click(); ");
                        Thread.Sleep(10000);
                        startScraping();
                    }

                }
            }
        }
        static bool scrapedCarDetails = false;
        private static void scrapCarDetails()
        {
            if (scrapedCarDetails) return;
            scrapedCarDetails = true;
            // push all search results into an array
            var pageQueryScript = @" (function(){
                        var lis =  document.querySelectorAll(`[class='fancy-description-list']`);
                        var result = [];
                        result.push(lis[0].innerText);
                        result.push(lis[1].innerText);
                        return result; 
                    })()";
            var scriptTask = chromeBrowser.EvaluateScriptAsync(pageQueryScript);
            scriptTask.ContinueWith(u =>
            {
                // MessageBox.Show(u.Result.Success.ToString(), "hello");
                if (u.Result.Success && u.Result.Result != null)
                {
                    Console.WriteLine("Bot Car details output received! \n\n");
                    var filePath = "zzzCarOutput.txt";
                    var response = (List<dynamic>)u.Result.Result;
                    foreach (string v in response)
                    {
                        Console.WriteLine(v);
                    }
                    File.AppendAllLines(filePath, response.Select(v => (string)v).ToArray());
                    Console.WriteLine($"\n\nBot output saved to {filePath}");
                    //  MessageBox.Show(filePath, "scraping complete");
                }
            });
        }


        public Form1()
        {
            InitializeComponent();
            // Start the browser after initialize global component
            InitializeChromium();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("\n\nClosing app \n\n ");
            saveSearchArray();
            Cef.Shutdown();
        }
        public static bool alreadySaved = false;
        public static void saveSearchArray()
        {
            Console.WriteLine("\n\nsaveSearchArray() \n\n ");

            var filePath = "output.txt";
           if(!alreadySaved) File.AppendAllLines(filePath, searchRes);
            alreadySaved = true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

    }
}
