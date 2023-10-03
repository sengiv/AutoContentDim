namespace AutoContentDim
{
    internal class Program
    {

        [STAThread]
        static async Task Main(string[] args)
        {
            try
            {
                Application.Run(new HelloWorldTaskbarApp());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                //hold control if error
                Console.ReadLine();
            }
        }

        

    }
}
