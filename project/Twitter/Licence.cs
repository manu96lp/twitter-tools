using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Security
{
    static class Licence
    {
        public static bool Valid { get; private set; }
        public static string Owner { get; private set; }
        public static DateTime Expiration { get; private set; }
        
        public static void Update( string licence )
        {
            string decrypted = Encriptor.Decrypt( licence );

            Licence.Valid = false;

            if( !String.IsNullOrWhiteSpace( decrypted ) )
            {
                string[ ] data = decrypted.Split( '|' );
                int value = 0;

                if( data.Length > 2 && data[ 0 ].Equals( "Secure" ) && int.TryParse( data[ 1 ], out value ) )
                {
                    Licence.Valid = true;
                    Licence.Owner = data[ 2 ].Trim( );
                    Licence.Expiration = new DateTime( 1970, 1, 1 ).AddSeconds( value );
                }
            }
        }
    }

    static class Encriptor
    {
        private static byte[ ] key = new byte[ 8 ] { 33, 56, 125, 85, 44, 87, 45, 71 };
        private static byte[ ] iv = new byte[ 8 ] { 84, 75, 11, 64, 26, 113, 3, 45 };

        public static string Crypt( string text )
        {
            SymmetricAlgorithm algorithm = DES.Create( );
            ICryptoTransform transform = algorithm.CreateEncryptor( key, iv );

            byte[ ] inputBuffer = Encoding.Unicode.GetBytes( text );
            byte[ ] outputBuffer = transform.TransformFinalBlock( inputBuffer, 0, inputBuffer.Length );

            return Convert.ToBase64String( outputBuffer );
        }

        public static string Decrypt( string text )
        {
            SymmetricAlgorithm algorithm = DES.Create( );
            ICryptoTransform transform = algorithm.CreateDecryptor( key, iv );

            string result = "";

            try
            {
                byte[ ] inputBuffer = Convert.FromBase64String( text );
                byte[ ] outputBuffer = null;

                outputBuffer = transform.TransformFinalBlock( inputBuffer, 0, inputBuffer.Length );

                result = Encoding.Unicode.GetString( outputBuffer );
            }
            catch( Exception ) { }

            return result;
        }
    }
}