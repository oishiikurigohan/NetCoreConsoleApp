using System;

namespace NetCoreConsoleApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Run();
        }
    }
}
