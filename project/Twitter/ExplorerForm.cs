using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using MetroFramework;

namespace Twitter
{
    public partial class ExplorerForm : MetroFramework.Forms.MetroForm
    {
        private string appPath;

        public ExplorerForm( IEnumerable<string> searchs, string appPath )
        {
            InitializeComponent( );

            this.appPath = appPath;

            selectSearchComboBox.Items.AddRange( searchs.ToArray( ) );
            selectSearchComboBox.SelectedIndex = 0;
        }

        private void selectSearchComboBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            if( selectSearchComboBox.SelectedIndex == 0 )
            {
                return;
            }

            selectSearchComboBox.Enabled = false;

            var db = new DBConnection( String.Format( @"{0}\Busquedas\{1}\Tweets.db", appPath, (string)selectSearchComboBox.SelectedItem ) );
            var tweets = db.GetTweets( 0 );

            tweetsListView.Items.Clear( );
            tweetsListView.Items.AddRange( tweets.Select( x => new ListViewItem( new string[] { x.User, x.Content } ) ).Take( 1000 ).ToArray( ) );

            selectSearchComboBox.Enabled = true;

            MetroMessageBox.Show( this, "\nSe encontraron " + tweetsListView.Items.Count + " tweets en la búsqueda.\nLa cantidad de tweets mostrados está limitada a 1000.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information, 150 );
        }
    }
}
