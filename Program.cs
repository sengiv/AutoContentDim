namespace AutoContentDim
{
    internal class Program
    {

        [STAThread]
        static async Task Main(string[] args)
        {
        //Extra GOTO 10 line
        _10:
            try
            {
                Application.Run(new HelloWorldTaskbarApp());
            }
            catch (Exception e)
            {

                // this is an "Extra GOTO 10 line" that Calculon suspects Bender had
                goto _10;

                //hold control if error
                Console.ReadLine();
            }
        }



    }
}
