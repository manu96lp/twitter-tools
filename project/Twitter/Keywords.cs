using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Twitter
{
    public class Keywords
    {
        public string Title { get; }
        public string[ ] Words { get; }

        public Keywords( string title, string[ ] words )
        {
            this.Title = title;
            this.Words = words;

            if( !this.IsValid( ) )
                throw new ArgumentException( );
        }

        public Keywords( string title, string text )
        {
            this.Title = title;
            this.Words = text.Split( ",".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Trim( ).ToLower( ) ).ToArray( );

            if( !this.IsValid( ) )
                throw new ArgumentException( );
        }

        public bool WithinText( string text )
        {
            foreach( string word in Words )
            {
                if( text.IndexOf( word, StringComparison.OrdinalIgnoreCase ) != -1 )
                    return true;
            }
            
            return false;
        }

        private bool IsValid( )
        {
            if( String.IsNullOrWhiteSpace( Title ) || Words.Length == 0 || Words.GroupBy( x => x ).Where( x => x.Count( ) > 1 ).Count( ) > 0 )
                return false;

            return true;
        }
    }

    public class KeywordsCollection : IEnumerable<Keywords>
    {
        private List<Keywords> list;

        public KeywordsCollection( )
        {
            list = new List<Keywords>( );
        }

        public IEnumerator<Keywords> GetEnumerator( )
        {
            return list.GetEnumerator( );
        }

        IEnumerator IEnumerable.GetEnumerator( )
        {
            return this.GetEnumerator( );
        }

        public Keywords this[ int index ]
        {
            get { return list[ index ]; }
            set { list[ index ] = value; }
        }

        public void Clear( )
        {
            list.Clear( );
        }

        public void Add( Keywords keywords )
        {
            list.Add( keywords );
        }
        
        public void Remove( Keywords keywords )
        {
            list.Remove( keywords );
        }

        public void RemoveAt( int index )
        {
            list.RemoveAt( index );
        }

        public int Count( )
        {
            return list.Count;
        }

        public int WordsCount( )
        {
            return list.Select( x => x.Words ).Sum( x => x.Count( ) );
        }

        public bool Export( string fileName )
        {
            if( File.Exists( fileName ) )
                File.Delete( fileName );
            
            try { File.WriteAllLines( fileName, list.Select( x => x.Title + ": " + String.Join( ",", x.Words ) ), Encoding.Default ); }
            catch( Exception ) { return false; }

            return true;
        }

        public bool Import( string filePath )
        {
            if( !File.Exists( filePath ) )
            {
                return false;
            }
            
            using( var sr = new StreamReader( filePath, Encoding.Default ) )
            {
                int count = 1;

                var keywordGroup = new List<Keywords>( );
                var line = sr.ReadLine( );

                while( line != null )
                {
                    if( !String.IsNullOrWhiteSpace( line ) && !line.Substring( 0, 2 ).Equals( "//" ) )
                    {
                        var data = line.Split( ":".ToCharArray( ) );

                        if( data.Length == 2 )
                        {
                            var title = data[0];
                            var words = data[1].Split( ",".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Trim( ) ).ToArray( );

                            if( !String.IsNullOrWhiteSpace( title ) && words.Length > 0 )
                            {
                                if( words.Where( x => x.Length > 20 ).Count( ) == 0 )
                                {
                                    list.Add( new Keywords( title, words ) );
                                }
                                else
                                {
                                    throw new InvalidDataException( String.Format( "Se encontró una palabra clave con mas de veinte letras.\n - Título del grupo: {0}\n - Línea: {1}", title, count ) );
                                }
                            }
                            else
                            {
                                throw new InvalidDataException( String.Format( "Se encontró un grupo de palabras claves sin título o sin palabras.\n - Título del grupo: {0}\n - Línea: {1}", title, count ) );
                            }
                        }
                        else
                        {
                            throw new InvalidDataException( String.Format( "Se encontró un grupo de palabras claves mal estructurado.\n - Linea: {0}", count ) );
                        }
                    }

                    count++;
                    line = sr.ReadLine( );
                }
            }

            if( list.Count( ) == 0 )
            {
                throw new InvalidDataException( String.Format( "No se encontraron grupos de palabras claves dentro del archivo.\nPosible carga de documento vacío o mal estructurado." ) );
            }

            return true;
        }
    }
}
