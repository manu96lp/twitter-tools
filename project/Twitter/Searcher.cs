using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tweetinvi;

namespace Twitter
{
    static class Searcher
    {
        private static Tweetinvi.Streaming.IFilteredStream stream;
        
        public static event EventHandler<Tweetinvi.Events.DisconnectedEventArgs> DisconnectedMessageReceived;
        public static event EventHandler<Tweetinvi.Events.WarningFallingBehindEventArgs> WarningFallingBehind;

        public static int TweetCount { get; private set; }

        public static List<string> Languages { get; set; }
        public static List<string> Keywords { get; set; }
        public static List<Coordinates> Coords { get; set; }

        public static Tweetinvi.Streaming.Parameters.StreamFilterLevel FilterLevel { get; set; }
        public static DBConnection Database { get; set; }

        private static void OnTweetReceived( object sender, Tweetinvi.Events.MatchedTweetReceivedEventArgs e )
        {
            TweetCount++;
            Database.SaveTweet( new Tweet( e.Tweet.CreatedBy.ScreenName, e.Tweet.FullText, e.Tweet.FavoriteCount, e.Tweet.RetweetCount ) );
        }

        public static Tweetinvi.Models.StreamState StreamState
        {
            get
            {
                return (stream == null) ? Tweetinvi.Models.StreamState.Stop : stream.StreamState;
            }
            set
            {
                if( stream != null && (stream.StreamState != value) )
                {
                    switch( value )
                    {
                        case Tweetinvi.Models.StreamState.Running: stream.ResumeStream( ); break;
                        case Tweetinvi.Models.StreamState.Pause: stream.PauseStream( ); break;
                        case Tweetinvi.Models.StreamState.Stop: stream.StopStream( ); break;
                    }
                }
            }
        }

        public static void Start( )
        {
            stream = Tweetinvi.Stream.CreateFilteredStream( );
            
            stream.FilterLevel = FilterLevel;
            
            stream.MatchingTweetReceived += OnTweetReceived;
            stream.WarningFallingBehindDetected += WarningFallingBehind;
            stream.DisconnectMessageReceived += DisconnectedMessageReceived;

            foreach( string keyword in Keywords ) stream.AddTrack( keyword );
            foreach( string language in Languages ) stream.AddTweetLanguageFilter( language );
            foreach( Coordinates coord in Coords ) stream.AddLocation( coord, new Coordinates( coord.Latitude + 1.0, coord.Longitude + 1.0 ) );

            TweetCount = 0;

            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            Task streamAsync = Sync.ExecuteTaskAsync( ( ) => stream.StartStreamMatchingAllConditionsAsync( ) );
        }

        public static List<Tweet> GetTimeLine( string screenName, DateTime initDate, DateTime endDate )
        {
            int tweetCount = 0;
            bool endDateFound = false;

            var timelineParams = new Tweetinvi.Parameters.UserTimelineParameters( );
            var lastTweets = Timeline.GetUserTimeline( screenName, timelineParams );

            if( lastTweets == null )
                return null;
            
            var tweets = new List<Tweet>( );

            while( lastTweets.Count( ) > 0 && tweetCount < 3200 )
            {
                tweetCount += lastTweets.Count( );

                foreach( Tweetinvi.Models.ITweet tweet in lastTweets )
                {
                    if( tweet.CreatedAt > endDate )
                        continue;

                    if( tweet.CreatedAt >= initDate )
                    {
                        Tweet commonTweet = new Tweet
                        (
                            tweet.CreatedBy.ScreenName,
                            tweet.FullText,
                            tweet.CreatedAt,
                            tweet.FavoriteCount,
                            tweet.RetweetCount
                        );

                        tweets.Add( commonTweet );
                    }
                    else
                    {
                        endDateFound = true; break;
                    }
                }
                
                if( endDateFound )
                    break;

                timelineParams.MaxId = lastTweets.Select( x => x.Id ).Min( );
                timelineParams.MaximumNumberOfTweetsToRetrieve = 200;

                lastTweets = Timeline.GetUserTimeline( screenName, timelineParams );
            }

            return tweets;
        }

        public static bool SetCredentials( string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret )
        {
            Auth.SetUserCredentials( consumerKey, consumerSecret, accessToken, accessTokenSecret );

            return ( Search.SearchUsers( "@Twitter" ) != null );
        }
    }
}
