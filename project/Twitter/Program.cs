using System;
using System.Threading;
using System.Windows.Forms;

namespace Twitter
{
    static class Program
    {
        static Mutex mutex = new Mutex( true, "{Y8A7RRNJ-ZF5MS9WC-68938VX3-GDPPATG6}" );
        
        [STAThread]
        static void Main()
        {
            if( mutex.WaitOne( TimeSpan.Zero, true ) )
            {
                Application.EnableVisualStyles( );
                Application.SetCompatibleTextRenderingDefault( false );
                Application.Run( new MainForm( ) );

                mutex.ReleaseMutex( );
            }
        }
    }
}
