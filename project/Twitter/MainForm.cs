using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using Security;
using MetroFramework;

namespace Twitter
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        private DateTime epochTime = new DateTime( 1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc );

        private KeywordsCollection topics = new KeywordsCollection( );
        private KeywordsCollection categories = new KeywordsCollection( );

        private string appPath;
        private string currentSearch;
        private string lastCategoriesDir;
        private string lastTopicsDir;
        private int elapsedSeconds;

        private string[ ] availableLanguages = new string[ ] { "en", "es", "de" };

        public MainForm( )
        {
            InitializeComponent( );
        }

        private void MainForm_Load( object sender, EventArgs e )
        {
            appPath = System.Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) + @"\Twitter";

            if( !Directory.Exists( appPath ) )
            {
                Directory.CreateDirectory( appPath );
                Directory.CreateDirectory( appPath + @"\Busquedas" );
                Directory.CreateDirectory( appPath + @"\Reportes" );
            }

            if ( File.Exists( appPath + @"\Datos" ) )
            {
                var data = File.ReadAllLines( appPath + @"\Datos" );

                if( data.Length == 8 )
                {
                    consumerKeyTextBox.Text = data[0];
                    consumerSecretTextBox.Text = data[1];
                    accessTokenTextBox.Text = data[2];
                    accessSecretTextBox.Text = data[3];
                    licenceTextBox.Text = data[4];
                    copyRuteTextBox.Text = data[5];
                    lastCategoriesDir = data[6];
                    lastTopicsDir = data[7];

                    if( !Directory.Exists( lastCategoriesDir ) || !Directory.Exists( lastTopicsDir ) )
                    {
                        var desktopPath = Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory );

                        lastCategoriesDir = desktopPath;
                        lastTopicsDir = desktopPath;
                    }
                }
                else
                {
                    File.Delete( appPath + @"\Datos" );
                }
            }

            if ( !Directory.Exists( copyRuteTextBox.Text ) )
            {
                copyRuteTextBox.Text = Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory );
            }

            resetCredentialsButton.Enabled = false;
            searchExplorerButton.Enabled = false;

            reportDateTimePicker_1.Value = DateTime.Today;
            reportDateTimePicker_1.ShowUpDown = true;

            reportDateTimePicker_2.Value = DateTime.Today;
            reportDateTimePicker_2.ShowUpDown = true;

            mainToolTip.SetToolTip( reportDateTimePicker_1, "(TIP) La hora debe cambiarse por teclado." );
            mainToolTip.SetToolTip( reportDateTimePicker_2, "(TIP) La hora debe cambiarse por teclado." );
            mainToolTip.SetToolTip( authorizeButton, "Autorizar la aplicación para utilizar el programa." );
            mainToolTip.SetToolTip( resetCredentialsButton, "Devolver la aplicación a su estado inicial." );
            mainToolTip.SetToolTip( filterRadioButton_1, "Obtener pocos resultados." );
            mainToolTip.SetToolTip( filterRadioButton_2, "Obtener menos resultados de los disponibles." );
            mainToolTip.SetToolTip( filterRadioButton_3, "Obtener todos los resultados disponibles." );
            mainToolTip.SetToolTip( searchNameTextBox, "Unicamente letras, numeros y guiones." );
            mainToolTip.SetToolTip( userNameTextBox, "Nombre de usuario arrobado de la persona." );
            mainToolTip.SetToolTip( showWarningsCheckBox, "Pérdidas de datos, desconexiones, etc." );

            reportsTabControl.SelectedIndex = 0;

            selectSearchComboBox.Items.Clear( );
            selectSearchComboBox.Items.Add( "Sin seleccionar" );
            selectSearchComboBox.SelectedIndex = 0;

            selectSearchComboBox.Items.AddRange( Directory.GetDirectories( appPath + @"\Busquedas" ).Select( d => new DirectoryInfo( d ) ).OrderBy( d => d.CreationTime ).Reverse( ).Select( d => d.Name ).ToArray( ) );
            reportsListView.Items.AddRange( Directory.GetFiles( appPath + @"\Reportes" ).Select( f => new FileInfo( f ) ).OrderBy( f => f.CreationTime ).Reverse( ).Select( f => new ListViewItem( f.Name.Split( '.' ).ElementAt( 0 ) ) ).ToArray( ) );

            mainTabPage_1.Enabled = false;
            mainTabPage_2.Enabled = false;

            mainTabControl.SelectedTab = mainTabPage_3;

            this.ResetSearch( );
        }

        private void MainForm_FormClosing( object sender, FormClosingEventArgs e )
        {
            string[ ] data = new string[ ]
            {
                consumerKeyTextBox.Text,
                consumerSecretTextBox.Text,
                accessTokenTextBox.Text,
                accessSecretTextBox.Text,
                licenceTextBox.Text,
                copyRuteTextBox.Text,
                lastCategoriesDir,
                lastTopicsDir
            };

            File.WriteAllLines( appPath + @"\Datos", data );

            if( Searcher.StreamState == Tweetinvi.Models.StreamState.Running )
            {
                Searcher.StreamState = Tweetinvi.Models.StreamState.Stop;
            }
        }

        private void authorizeButton_Click( object sender, EventArgs e )
        {
            Licence.Update( licenceTextBox.Text );

            if( Licence.Valid && Licence.Expiration > DateTime.Now )
            {
                if( Searcher.SetCredentials( consumerKeyTextBox.Text, consumerSecretTextBox.Text, accessTokenTextBox.Text, accessSecretTextBox.Text ) )
                {
                    consumerKeyTextBox.Enabled = false;
                    consumerSecretTextBox.Enabled = false;
                    accessTokenTextBox.Enabled = false;
                    accessSecretTextBox.Enabled = false;
                    licenceTextBox.Enabled = false;

                    mainTabPage_1.Enabled = true;
                    mainTabPage_2.Enabled = true;

                    authorizeButton.Enabled = false;
                    resetCredentialsButton.Enabled = true;
                    searchExplorerButton.Enabled = true;

                    MetroMessageBox.Show( this, "\nSe autorizó correctamente la aplicación.", "Conexión aceptada", MessageBoxButtons.OK, MessageBoxIcon.Information, 125 );
                }
                else
                {
                    MetroMessageBox.Show( this, "\nNo se pudo establecer conexión con Twitter.\nVerifique las credenciales y la conexión a Internet.", "Conexión rechazada", MessageBoxButtons.OK, MessageBoxIcon.Error, 150 );
                }
            }
            else
            {
                MetroMessageBox.Show( this, "\nSe rechazó la licencia actual.\nPara saber mas acerca del por qué fue rechazada, haga clic en el botón \"Información sobre licencia\".", "Licencia rechazada", MessageBoxButtons.OK, MessageBoxIcon.Error, 175 );
            }
        }

        private void licenceInfoButton_Click( object sender, EventArgs e )
        {
            Licence.Update( licenceTextBox.Text );

            if( Licence.Valid )
            {
                if( Licence.Expiration < DateTime.Now )
                {
                    MetroMessageBox.Show( this, String.Format( "\nLa licencia pudo verificarse con éxito.\nLa licencia caducó en la fecha: {0}", Licence.Expiration ), "Licencia caducada", MessageBoxButtons.OK, MessageBoxIcon.Error, 150 );
                }
                else
                {
                    MetroMessageBox.Show( this, String.Format( "\nLa licencia pudo verificarse con éxito.\nCaducidad de la licencia: {0}\nDueño de la licencia: {1}", Licence.Expiration, Licence.Owner ), "Licencia válida", MessageBoxButtons.OK, MessageBoxIcon.Information, 175 );
                }
            }
            else
            {
                MetroMessageBox.Show( this, "\nLa licencia no se pudo verificar (mal estructurada).", "Licencia inválida", MessageBoxButtons.OK, MessageBoxIcon.Error, 125 );
            }
        }

        private void resetCredentialsButton_Click( object sender, EventArgs e )
        {
            if( MetroMessageBox.Show( this, "\nEsto detendrá todas las operaciones activas.\n¿Está seguro de reiniciar las credenciales?", "Confirmación", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, 150 ) != DialogResult.OK )
            {
                return;
            }

            if( Searcher.StreamState == Tweetinvi.Models.StreamState.Running )
            {
                Searcher.StreamState = Tweetinvi.Models.StreamState.Stop;
            }

            this.ResetSearch( );

            mainTabPage_1.Enabled = false;
            mainTabPage_2.Enabled = false;

            authorizeButton.Enabled = true;
            resetCredentialsButton.Enabled = false;

            consumerKeyTextBox.Enabled = true;
            consumerSecretTextBox.Enabled = true;
            accessTokenTextBox.Enabled = true;
            accessSecretTextBox.Enabled = true;
            licenceTextBox.Enabled = true;
        }

        private void searchExplorerButton_Click( object sender, EventArgs e )
        {
            ExplorerForm ef = new ExplorerForm( selectSearchComboBox.Items.Cast<string>( ).Where( x => !x.Equals( currentSearch ) ), appPath );

            ef.ShowDialog( );
            ef.Dispose( );
        }

        private void oldReportsButton_Click( object sender, EventArgs e )
        {
            Process.Start( "explorer.exe", appPath + @"\Reportes" );
        }

        private async void generateReportButton_Click( object sender, EventArgs e )
        {
            bool result = false;

            if( reportDateTimePicker_1.Value > reportDateTimePicker_2.Value )
            {
                MetroMessageBox.Show( this, "\nLa fecha de inicio del análisis debe ser menor (mas antigua) a la fecha de finalizado.", "Reporte no generado", MessageBoxButtons.OK, MessageBoxIcon.Error, 125 );

                return;
            }

            if( topics.Count( ) == 0 )
            {
                var questionRes = MetroMessageBox.Show( this, "\nAún no se han cargado temas para ser analizados.\n¿Desea generar el reporte de todas formas?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 150 );

                if( questionRes != DialogResult.Yes )
                {
                    return;
                }
            }

            generateReportButton.Enabled = false;
            reportsTabControl.Enabled = false;
            reportDateTimePicker_1.Enabled = false;
            reportDateTimePicker_2.Enabled = false;

            if( reportsTabControl.SelectedIndex == 0 )
            {
                if( (selectSearchComboBox.SelectedIndex > 0) && !(!String.IsNullOrEmpty( currentSearch ) && selectSearchComboBox.SelectedItem.Equals( currentSearch )) )
                {
                    string searchAlias = Convert.ToString( selectSearchComboBox.SelectedItem );

                    result = await Task.Run<bool>( ( ) => GenerateSearchReport( searchAlias, reportDateTimePicker_1.Value, reportDateTimePicker_2.Value ) );
                }
            }
            else
            {
                if( (reportDateTimePicker_1.Value != reportDateTimePicker_2.Value) && (userNameTextBox.Text.Length > 5) && (userNameTextBox.Text.Length <= 16) && userNameTextBox.Text.All( x => x == '@' || x == '_' || Char.IsLetterOrDigit( x ) ) )
                {
                    result = await Task.Run<bool>( ( ) => GenerateUserReport( userNameTextBox.Text, reportDateTimePicker_1.Value, reportDateTimePicker_2.Value ) );
                }
            }

            generateReportButton.Enabled = true;
            reportsTabControl.Enabled = true;
            reportDateTimePicker_1.Enabled = true;
            reportDateTimePicker_2.Enabled = true;

            topics.Clear( );

            if( result )
            {
                reportsListView.Items.Clear( );
                reportsListView.Items.AddRange( Directory.GetFiles( appPath + @"\Reportes" ).Select( f => new FileInfo( f ) ).OrderBy( n => n.CreationTime ).Reverse( ).Select( f => new ListViewItem( f.Name ) ).ToArray( ) );
            }
            else
            {
                if( reportsTabControl.SelectedIndex == 0 )
                {
                    MetroMessageBox.Show( this, "\nNo se pudo generar el reporte. Posibles causas:\n- La búsqueda seleccionada estaba siendo procesada.\n- No se encontraron tweets para procesar.\n- La ruta de copiado es inaccesible.", "Reporte no generado", MessageBoxButtons.OK, MessageBoxIcon.Error, 175 );
                }
                else
                {
                    MetroMessageBox.Show( this, "\nNo se pudo generar el reporte. Posibles causas:\n- El usuario no es válido o no existe.\n- No hay conexión a Internet.\n- No se encontraron tweets para procesar.\n- La ruta de copiado es inaccesible.\n- Las fechas están mal introducidas (deben ser distintas).", "Reporte no generado", MessageBoxButtons.OK, MessageBoxIcon.Error, 225 );
                }
            }
        }

        private void createSearchButton_Click( object sender, EventArgs e )
        {
            List<String> languages = new List<String>( );
            List<Coordinates> coords = new List<Coordinates>( );

            if( String.IsNullOrWhiteSpace( searchNameTextBox.Text ) )
            {
                MetroMessageBox.Show( this, "\nEl nombre de la búsqueda está vacío.", "Alias vacío", MessageBoxButtons.OK, MessageBoxIcon.Error, 150 );
                
                return;
            }

            if( !searchNameTextBox.Text.All( x => x == '_' || x == '-' || Char.IsLetterOrDigit( x ) ) )
            {
                MetroMessageBox.Show( this, "\nEl nombre de la búsqueda contiene carácteres inválidos", "Alias inválido", MessageBoxButtons.OK, MessageBoxIcon.Error, 125 );
                
                return;
            }

            string searchPath = appPath + String.Format( @"\Busquedas\{0}", searchNameTextBox.Text );

            if( Directory.Exists( searchPath ) )
            {
                MetroMessageBox.Show( this, "\nYa existe una búsqueda creada con el mismo alias.\nIntroduzca un alias distinto.", "Alias existente", MessageBoxButtons.OK, MessageBoxIcon.Error, 150 );
               
                return;
            }

            if( !String.IsNullOrWhiteSpace( placesTextBox.Text ) )
            {
                try
                {
                    coords = placesTextBox.Text.Split( ",".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Split( ' ' ).Select( y => Double.Parse( y ) ) ).Where( x => x.Count( ) == 2 ).Select( x => new Coordinates( x.ElementAt( 0 ), x.ElementAt( 1 ) ) ).ToList( );

                    if( coords.Count == 0 )
                    {
                        MetroMessageBox.Show( this, "\nLas coordenadas no estan correctamente ingresadas.", "Localizaciones mal ingresadas", MessageBoxButtons.OK, MessageBoxIcon.Error, 125 );
                        
                        return;
                    }
                }
                catch
                {
                    MetroMessageBox.Show( this, "\nNo se puede convertir los numeros en coordenadas.", "Localizaciones incorrectas", MessageBoxButtons.OK, MessageBoxIcon.Error, 125 );
                    
                    return;
                }
            }
            
            languages = languagesTextBox.Text.Split( ",".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Trim( ).ToLower( ) ).ToList( );

            if( !languages.All( x => availableLanguages.Contains( x ) ) )
            {
                MetroMessageBox.Show( this, "\nNo todos los lenguajes se encontraron en la lista de lenguajes soportados.", "Lenguajes no soportados", MessageBoxButtons.OK, MessageBoxIcon.Error, 150 );
                
                return;
            }

            if( categories.Count( ) == 0 )
            {
                MetroMessageBox.Show( this, "\nNo se puede iniciar una búsqueda sin categorías.", "Categorías sin cargar", MessageBoxButtons.OK, MessageBoxIcon.Error, 125 );
                
                return;
            }

            Directory.CreateDirectory( searchPath );

            filterRadioButton_1.Enabled = false;
            filterRadioButton_2.Enabled = false;
            filterRadioButton_3.Enabled = false;
            showWarningsCheckBox.Enabled = false;
            placesTextBox.Enabled = false;
            languagesTextBox.Enabled = false;
            createSearchButton.Enabled = false;
            searchNameTextBox.Enabled = false;
            selectCategoriesButton.Enabled = false;
            startSearchButton.Enabled = true;

            currentSearch = searchNameTextBox.Text;
            selectSearchComboBox.Items.Insert( 1, currentSearch );

            categories.Export( searchPath + @"\Categorias" );

            Searcher.Coords = coords;
            Searcher.Languages = languages;
            Searcher.Database = new DBConnection( searchPath + @"\Tweets.db" );
            Searcher.Keywords = categories.Select( x => x.Words ).SelectMany( x => x ).ToList( );
            Searcher.FilterLevel = Tweetinvi.Streaming.Parameters.StreamFilterLevel.None;

            Searcher.DisconnectedMessageReceived += ( obj, args ) =>
            {
                this.ResetSearch( );

                MetroMessageBox.Show( this, "\nLa búsqueda finalizó por la siguiente razón: " + args.DisconnectMessage.Reason, "Búsqueda finalizada", MessageBoxButtons.OK, MessageBoxIcon.Warning, 200 );
            };

            if( !filterRadioButton_3.Checked )
            {
                if( filterRadioButton_2.Checked )
                {
                    Searcher.FilterLevel = Tweetinvi.Streaming.Parameters.StreamFilterLevel.Medium;
                }
                else
                {
                    Searcher.FilterLevel = Tweetinvi.Streaming.Parameters.StreamFilterLevel.Low;
                }
            }

            if( showWarningsCheckBox.Checked )
            {
                Searcher.WarningFallingBehind += ( obj, args ) =>
                {
                    MetroMessageBox.Show( this, "\nLa búsqueda generó una advertencia: " + args.WarningMessage.Message, "Advertencia de búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Warning, 200 );
                };
            }
        }

        private void startSearchButton_Click( object sender, EventArgs e )
        {
            if( Searcher.StreamState != Tweetinvi.Models.StreamState.Pause )
            {
                Searcher.Start( );

                startSearchButton.Text = "Reanudar";
            }
            else
            {
                Searcher.StreamState = Tweetinvi.Models.StreamState.Running;
            }

            stopSearchButton.Enabled = false;
            pauseSearchButton.Enabled = true;
            startSearchButton.Enabled = false;

            searchTimer.Enabled = true;
        }

        private void pauseSearchButton_Click( object sender, EventArgs e )
        {
            Searcher.StreamState = Tweetinvi.Models.StreamState.Pause;

            searchInfoLabel.Text = String.Format( "Estado de búsqueda: en pausa\nTiempo transcurrido: {0:00}:{1:00}:{2:00} - Cantidad de tweets obtenidos: {3}", (elapsedSeconds / 3600), (elapsedSeconds / 60) % 60, elapsedSeconds % 60, Searcher.TweetCount );
            
            stopSearchButton.Enabled = true;
            pauseSearchButton.Enabled = false;
            startSearchButton.Enabled = true;

            searchTimer.Enabled = false;
        }

        private void stopSearchButton_Click( object sender, EventArgs e )
        {
            Searcher.StreamState = Tweetinvi.Models.StreamState.Stop;

            this.ResetSearch( );
        }

        private void searchTimer_Tick( object sender, EventArgs e )
        {
            elapsedSeconds++;
            searchInfoLabel.Text = String.Format( "Estado de búsqueda: en proceso...\nTiempo transcurrido: {0:00}:{1:00}:{2:00} - Cantidad de tweets obtenidos: {3}", (elapsedSeconds / 3600), (elapsedSeconds / 60) % 60, elapsedSeconds % 60, Searcher.TweetCount );
        }

        private void selectCategoriesButton_Click( object sender, EventArgs e )
        {
            OpenFileDialog fileDialog = new OpenFileDialog( );

            fileDialog.Filter = "Files|*.txt";
            fileDialog.InitialDirectory = lastCategoriesDir;
            fileDialog.Multiselect = false;
            fileDialog.Title = "Seleccione el archivo de categorías";

            if( fileDialog.ShowDialog( this ) == DialogResult.OK )
            {
                categories.Clear( );

                try
                {
                    categories.Import( fileDialog.FileName );

                    lastCategoriesDir = new FileInfo( fileDialog.FileName ).DirectoryName;
                }
                catch( Exception ex )
                {
                    MetroMessageBox.Show( this, "\nNo se pudo leer correctamente el archivo. Detalles del error:\n\n" + ex.Message, "Error de carga", MessageBoxButtons.OK, MessageBoxIcon.Error, 225 );
                    
                    return;
                }

                MetroMessageBox.Show( this, String.Format( "\nCantidad de categorías encontradas: {0}.\nCantidad de palabras claves leídas: {1}.", categories.Count( ), categories.WordsCount( ) ), "Categorías cargadas", MessageBoxButtons.OK, MessageBoxIcon.Information, 150 );
            }
        }

        private void selectTopicsButton_Click( object sender, EventArgs e )
        {
            OpenFileDialog fileDialog = new OpenFileDialog( );

            fileDialog.Filter = "Files|*.txt";
            fileDialog.InitialDirectory = lastTopicsDir;
            fileDialog.Multiselect = false;
            fileDialog.Title = "Seleccione el archivo de temas";

            if( fileDialog.ShowDialog( this ) == DialogResult.OK )
            {
                topics.Clear( );

                try
                {
                    topics.Import( fileDialog.FileName );

                    lastTopicsDir = new FileInfo( fileDialog.FileName ).DirectoryName;
                }
                catch( Exception ex )
                {
                    MetroMessageBox.Show( this, "\nNo se pudo leer correctamente el archivo. Detalles del error:\n\n" + ex.Message, "Error de carga", MessageBoxButtons.OK, MessageBoxIcon.Error, 225 );

                    return;
                }

                MetroMessageBox.Show( this, String.Format( "\nCantidad de temas encontrados: {0}.\nCantidad de palabras claves leídas: {1}.", topics.Count( ), topics.WordsCount( ) ), "Temas cargados", MessageBoxButtons.OK, MessageBoxIcon.Information, 150 );
            }
        }

        private void openReportButton_Click( object sender, EventArgs e )
        {
            if( reportsListView.SelectedItems.Count != 1 )
            {
                return;
            }

            string file = String.Format( @"{0}\Reportes\{1}.txt", appPath, reportsListView.SelectedItems[ 0 ].Text );
            
            if( File.Exists( file ) )
            {
                Process.Start( "wordpad.exe", String.Format( "\"{0}\"", file ) );
            }
        }

        private void deleteReportButton_Click( object sender, EventArgs e )
        {
            if( reportsListView.SelectedItems.Count > 0 )
            {
                if( MetroMessageBox.Show( this, "\nUn reporte eliminado no podrá ser restaurado en el futuro.\n¿Realmente desea eliminar el reporte?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 150 ) == DialogResult.Yes )
                {
                    string file = String.Format( @"{0}\Reportes\{1}.txt", appPath, reportsListView.SelectedItems[ 0 ].Text );

                    if( File.Exists( file ) )
                    {
                        File.Delete( file );

                        reportsListView.Items.Remove( reportsListView.SelectedItems[ 0 ] );
                    }
                }
            }
        }

        private void mainTabControl_Selecting( object sender, TabControlCancelEventArgs e )
        {
            if( !e.TabPage.Enabled )
            {
                e.Cancel = true;

                MetroMessageBox.Show( this, "\nNo puedes acceder a esta pestaña hasta que la aplicación esté correctamente autorizada.", "Autorización requerida", MessageBoxButtons.OK, MessageBoxIcon.Error, 150 );
            }
        }

        private void copyRuteTextBox_Leave( object sender, EventArgs e )
        {
            if( !Directory.Exists( copyRuteTextBox.Text ) )
            {
                copyRuteTextBox.Text = Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory );
            }
        }

        private void ResetSearch( )
        {
            currentSearch = null;

            categories.Clear( );

            startSearchButton.Text = "Iniciar";
            searchInfoLabel.Text = "Estado de búsqueda: en espera de creación\nTiempo transcurrido: 00:00:00 - Cantidad de tweets obtenidos: 0";

            startSearchButton.Enabled = false;
            stopSearchButton.Enabled = false;
            pauseSearchButton.Enabled = false;

            filterRadioButton_1.Enabled = true;
            filterRadioButton_2.Enabled = true;
            filterRadioButton_3.Enabled = true;
            showWarningsCheckBox.Enabled = true;
            placesTextBox.Enabled = true;
            languagesTextBox.Enabled = true;
            createSearchButton.Enabled = true;
            searchNameTextBox.Enabled = true;
            selectCategoriesButton.Enabled = true;

            filterRadioButton_1.Checked = false;
            filterRadioButton_2.Checked = false;
            filterRadioButton_3.Checked = true;
            showWarningsCheckBox.Checked = true;
            
            searchTimer.Enabled = false;
        }

        private bool GenerateSearchReport( string searchAlias, DateTime fromDate, DateTime toDate )
        {
            var dbConn = new DBConnection( appPath + String.Format( @"\Busquedas\{0}\Tweets.db", searchAlias ) );
            var tweets = (fromDate == toDate) ? dbConn.GetTweets( 0 ) : dbConn.GetTweets( 0, fromDate, toDate );

            if( tweets.Count == 0 )
            {
                return false;
            }

            int i, fromId = 0;

            var cellsToSum = new List<int>( );
            var reportCategories = new KeywordsCollection( );

            var firstTweetDate = epochTime.AddSeconds( tweets[0].Timestamp );
            var lastTweetDate = DateTime.Now;

            reportCategories.Import( appPath + String.Format( @"\Busquedas\{0}\Categorias", searchAlias ) );

            var topicsCounter = new int[ topics.Count( ) ];
            var categoriesCounter = new int[ reportCategories.Count( ) ];
            var relationsCounter = new int[ reportCategories.Count( ), topics.Count( ) ];

            while( tweets.Count > 0 )
            {
                foreach( Tweet tweet in tweets )
                {
                    cellsToSum.Clear( );

                    for( i = 0; i < reportCategories.Count( ); i++ )
                    {
                        if( reportCategories[ i ].WithinText( tweet.Content ) )
                        {
                            cellsToSum.Add( i );
                            categoriesCounter[ i ]++;
                        }
                    }

                    for( i = 0; i < topics.Count( ); i++ )
                    {
                        if( topics[ i ].WithinText( tweet.Content ) )
                        {
                            topicsCounter[ i ]++;

                            foreach( int j in cellsToSum )
                            {
                                relationsCounter[ j, i ]++;
                            }
                        }
                    }
                }

                fromId += tweets.Count;

                lastTweetDate = epochTime.AddSeconds( tweets[ tweets.Count - 1 ].Timestamp );
                tweets = ( fromDate == toDate ) ? dbConn.GetTweets( fromId ) : dbConn.GetTweets( fromId, fromDate, toDate ); ;
            }

            var data = new StringBuilder( "Búsqueda de la cual se obtuvieron los datos: " + searchAlias + ".\n" );

            data.Append( "\n[ Categorías que se incluyen en el reporte ]\n\n" );

            foreach( Keywords kw in reportCategories )
            {
                data.Append( String.Format( "\tCategoría: {0} - Palabras clave: {1}\n", kw.Title, String.Join( ", ", kw.Words ) ) );
            }

            data.Append( "\n[ Cantidad de ocurrencias de cada categoría ]\n\n" );

            for( i = 0; i < reportCategories.Count( ); i++ )
            {
                data.Append( String.Format( "\tCategoría: {0} - Ocurrencias: {1}.\n", reportCategories[ i ].Title, categoriesCounter[ i ] ) );
            }

            if( topics.Count( ) > 0 )
            {
                data.Append( "\n[ Temas que se analizan en el reporte ]\n\n" );

                foreach( Keywords kw in topics )
                {
                    data.Append( String.Format( "\tTema: {0} - Palabras clave: {1}\n", kw.Title, String.Join( ", ", kw.Words ) ) );
                }

                data.Append( "\n[ Cantidad de ocurrencias de cada tema ]\n\n" );

                for( i = 0; i < topics.Count( ); i++ )
                {
                    data.Append( String.Format( "\tTema: {0} - Ocurrencias: {1}.\n", topics[i].Title, topicsCounter[i] ) );
                }

                data.Append( "\n[ Relaciones entre categorías y temas ]\n" );

                for( i = 0; i < reportCategories.Count( ); i++ )
                {
                    data.Append( String.Format( "\n\t- Categoría: {0} - Ocurrencias de cada tema:\n", reportCategories[i].Title ) );

                    for( int j = 0; j < topics.Count( ); j++ )
                    {
                        data.Append( String.Format( "\t\t- {0}: {1}.\n", topics[j].Title, relationsCounter[i, j] ) );
                    }
                }
            }

            data.Append( "\n============================================================\n" );
            data.Append( String.Format( "Cantidad total de tweets analizados: {0}.\n\n", fromId ) );
            data.Append( String.Format( "Fecha y hora del primer tweet: {0}\n", firstTweetDate ) );
            data.Append( String.Format( "Fecha y hora del último tweet: {0}\n", lastTweetDate ) );
            data.Append( "============================================================" );

            try
            {
                string fileName = String.Format( "({0:MM-dd-yy}) Búsqueda {1}", DateTime.Now, searchAlias );

                File.WriteAllText( appPath + @"\Reportes\" + fileName + ".txt", data.ToString( ), Encoding.Default );
                File.WriteAllText( copyRuteTextBox.Text + @"\Reporte.txt", data.ToString( ), Encoding.Default );

                Process.Start( "wordpad.exe", copyRuteTextBox.Text + @"\Reporte.txt" );
            }
            catch( Exception )
            {
                return false;
            }

            return true;
        }

        private bool GenerateUserReport( string screenName, DateTime fromDate, DateTime toDate )
        {
            var tweets = Searcher.GetTimeLine( screenName, fromDate, toDate );

            if( tweets == null || tweets.Count == 0 )
            {
                return false;
            }

            int i, charId = 0, likes = 0, retweets = 0;

            var mentions = new Dictionary<string, int>( );
            var counter = new int[ topics.Count( ), 3 ];

            var firstTweetDate = epochTime.AddSeconds( tweets[0].Timestamp );
            var lastTweetDate = epochTime.AddSeconds( tweets[tweets.Count - 1].Timestamp );

            foreach( Tweet tweet in tweets )
            {
                likes += tweet.Likes;
                retweets += tweet.Retweets;

                for( i = 0; i < topics.Count( ); i++ )
                {
                    foreach( string word in topics[ i ].Words )
                    {
                        if( tweet.Content.IndexOf( word, StringComparison.OrdinalIgnoreCase ) != -1 )
                        {
                            counter[ i, 0 ]++;
                            counter[ i, 1 ] += tweet.Likes;
                            counter[ i, 2 ] += tweet.Retweets;

                            break;
                        }
                    }
                }

                for( i = 0; i < tweet.Content.Length; i++ )
                {
                    if( charId > 0 && (!(Char.IsLetterOrDigit( tweet.Content[ i ] ) || tweet.Content[ i ] == '_') || tweet.Content.Length == (i - 1)) )
                    {
                        if( i - charId <= 15 && i - charId >= 5 )
                        {
                            int occurrences = 0;
                            string user = tweet.Content.Substring( charId, i - charId );

                            mentions.TryGetValue( user, out occurrences );
                            mentions[ user.ToString( ) ] = occurrences + 1;
                        }

                        charId = 0;
                    }
                    else if( tweet.Content[ i ] == '@' )
                    {
                        charId = i + 1;
                    }
                }
            }

            var data = new StringBuilder( "Usuario del cual se obtuvieron los datos para el reporte: " + screenName + ".\n\n" );

            if( mentions.Count > 0 )
            {
                data.Append( "\n[ Usuarios mas mencionados ]\n\n" );

                int min = Math.Min( mentions.Count( ), 5 );
                var orderedMentions = mentions.OrderByDescending( x => x.Value );

                for( i = 0; i < min; i++ )
                {
                    data.Append( String.Format( "\t[{0}] Usuario: {1} - Ocurrencias: {2}\n", i + 1, orderedMentions.ElementAt( i ).Key, orderedMentions.ElementAt( i ).Value ) );
                }
            }
            else
            {
                data.Append( "\n[ No se encontraron menciones a usuarios ]\n\n" );
            }

            if( topics.Count( ) > 0 )
            {
                data.Append( "[ Temas que se analizan en el reporte ]\n\n" );

                foreach( Keywords kw in topics )
                {
                    data.Append( String.Format( "\tTema: {0} - Palabras clave: {1}\n", kw.Title, String.Join( ", ", kw.Words ) ) );
                }

                data.Append( "\n[ Cantidad de ocurrencias de cada tema ]\n\n" );

                for( i = 0; i < topics.Count( ); i++ )
                {
                    data.Append( String.Format( "\tTema: {0} - Ocurrencias: {1} | Favoritos: {2} | Retweets: {3}\n", topics[ i ].Title, counter[ i, 0 ], counter[ i, 1 ], counter[ i, 2 ] ) );
                }
            }

            data.Append( "\n============================================================\n" );
            data.Append( String.Format( "Cantidad total de tweets analizados: {0}.\n", tweets.Count ) );
            data.Append( String.Format( "Cantidad total de retweets: {0}.\n", likes ) );
            data.Append( String.Format( "Cantidad total de favoritos: {0}.\n\n", retweets ) );
            data.Append( String.Format( "Fecha y hora de inicio del analisis: {0}\n", fromDate ) );
            data.Append( String.Format( "Fecha y hora de finalizado del analisis: {0}\n", toDate ) );
            data.Append( "============================================================" );

            try
            {
                string fileName = String.Format( "({0:MM-dd-yy}) Usuario @{1}", DateTime.Now, screenName );

                File.WriteAllText( appPath + @"\Reportes\" + fileName + ".txt", data.ToString( ), Encoding.Default );
                File.WriteAllText( copyRuteTextBox.Text + @"\Reporte.txt", data.ToString( ), Encoding.Default );

                Process.Start( "wordpad.exe", copyRuteTextBox.Text + @"\Reporte.txt" );
            }
            catch( Exception )
            {
                return false;
            }

            return true;
        }
    }
}