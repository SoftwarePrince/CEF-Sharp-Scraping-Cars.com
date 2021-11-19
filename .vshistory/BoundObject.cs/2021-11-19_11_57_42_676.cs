namespace FirstScraping
{
    internal class BoundObject
    {
        public class BoundObject
        {
            public void showMessage(string msg)
            {
                MessageBox.Show(msg);
            }
            public void sleep(int time)
            {
                Console.WriteLine("Bot Sleeping " + time);
                Thread.Sleep(time);
                Console.WriteLine("Bot Waking from sleep " + time);
            }
        }
    }
}