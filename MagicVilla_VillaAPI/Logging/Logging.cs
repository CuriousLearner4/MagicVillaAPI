namespace MagicVilla_VillaAPI.Logging
{
    public class Logging : ILogging
    {
        public void log(string message, string type)
        {
            if (type == "error")
            {
                Console.WriteLine("Error - {0}", message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
