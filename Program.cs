using System;

namespace DevCommuBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                new DevCommuBot().StartAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}