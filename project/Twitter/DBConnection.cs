using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Twitter
{
    class DBConnection
    {
        private static DateTime epochTime = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
        private string connString;

        public DBConnection( string dbFile )
        {
            connString = String.Format( "Data Source={0};Version=3;", dbFile );

            if( !File.Exists( dbFile ) )
            {
                SQLiteConnection.CreateFile( dbFile );

                using( var dbConnection = new SQLiteConnection( connString ) )
                {
                    dbConnection.Open( );

                    using( var dbCommand = new SQLiteCommand( dbConnection ) )
                    {
                        dbCommand.CommandText = "CREATE TABLE tweets ( "
                                                + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
                                                + "user TEXT, content TEXT, timestamp NUMERIC, "
                                                + "likes INTEGER, retweets INTEGER )";

                        dbCommand.ExecuteNonQuery( );
                    }

                    dbConnection.Close( );
                }
            }
        }

        public void SaveTweet( Tweet tweet )
        {
            using( var dbConnection = new SQLiteConnection( connString ) )
            {
                dbConnection.Open( );

                using( var dbCommand = new SQLiteCommand( dbConnection ) )
                {
                    dbCommand.CommandText = "INSERT INTO tweets VALUES (null,@user,@content,@timestamp,@likes,@retweets)";
                    
                    dbCommand.Parameters.Add( "user", System.Data.DbType.String ).Value = tweet.User;
                    dbCommand.Parameters.Add( "content", System.Data.DbType.String ).Value = tweet.Content;
                    dbCommand.Parameters.Add( "timestamp", System.Data.DbType.Int32 ).Value = tweet.Timestamp;
                    dbCommand.Parameters.Add( "likes", System.Data.DbType.Int32 ).Value = tweet.Likes;
                    dbCommand.Parameters.Add( "retweets", System.Data.DbType.Int32 ).Value = tweet.Retweets;

                    dbCommand.ExecuteNonQuery( );
                }

                dbConnection.Close( );
            }
        }

        public List<Tweet> GetTweets( int fromId )
        {
            List<Tweet> list = new List<Tweet>( );

            using( var dbConnection = new SQLiteConnection( connString ) )
            {
                dbConnection.Open( );

                using( var dbCommand = new SQLiteCommand( dbConnection ) )
                {
                    dbCommand.CommandText = "SELECT * FROM tweets WHERE (id > @fromId) LIMIT 100000";

                    dbCommand.Parameters.Add( "fromId", System.Data.DbType.Int32 ).Value = fromId;

                    using( var reader = dbCommand.ExecuteReader( ) )
                    {
                        while( reader.Read( ) )
                        {
                            Tweet tweet = new Tweet
                            (
                                Convert.ToString( reader["user"] ),
                                Convert.ToString( reader["content"] ),
                                Convert.ToInt32( reader["timestamp"] ),
                                Convert.ToInt32( reader["likes"] ),
                                Convert.ToInt32( reader["retweets"] )
                            );

                            list.Add( tweet );
                        }
                    }
                }

                dbConnection.Close( );
            }

            return list;
        }

        public List<Tweet> GetTweets( int fromId, DateTime fromDate, DateTime toDate )
        {
            List<Tweet> list = new List<Tweet>( );

            using( var dbConnection = new SQLiteConnection( connString ) )
            {
                dbConnection.Open( );

                using( var dbCommand = new SQLiteCommand( dbConnection ) )
                {
                    dbCommand.CommandText = "SELECT * FROM tweets WHERE (id > @fromId AND timestamp >= @initDate AND timestamp <= @endDate) LIMIT 100000";

                    dbCommand.Parameters.Add( "fromId", System.Data.DbType.Int32 ).Value = fromId;
                    dbCommand.Parameters.Add( "initDate", System.Data.DbType.Int32 ).Value = (int)fromDate.Subtract( epochTime ).TotalSeconds;
                    dbCommand.Parameters.Add( "endDate", System.Data.DbType.Int32 ).Value = (int)toDate.Subtract( epochTime ).TotalSeconds;

                    using( var reader = dbCommand.ExecuteReader( ) )
                    {
                        while( reader.Read( ) )
                        {
                            Tweet tweet = new Tweet
                            (
                                Convert.ToString( reader[ "user" ] ),
                                Convert.ToString( reader[ "content" ] ),
                                Convert.ToInt32( reader[ "timestamp" ] ),
                                Convert.ToInt32( reader[ "likes" ] ),
                                Convert.ToInt32( reader[ "retweets" ] )
                            );
                            
                            list.Add( tweet );
                        }
                    }
                }

                dbConnection.Close( );
            }

            return list;
        }
    }
}
