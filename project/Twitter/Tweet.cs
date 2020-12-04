using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter
{
    class Tweet
    {
        private static DateTime epochTime = new DateTime( 1970, 1, 1 );

        public string User { get; }
        public string Content { get; }
        public int Timestamp { get; }
        public int Likes { get; }
        public int Retweets { get; }

        public Tweet( string user, string content, int timestamp, int likes, int retweets )
        {
            this.User = user;
            this.Content = content;
            this.Timestamp = timestamp;
            this.Likes = likes;
            this.Retweets = retweets;
        }

        public Tweet( string user, string content, DateTime date, int likes, int retweets )
        {
            this.User = user;
            this.Content = content;
            this.Likes = likes;
            this.Retweets = retweets;
            this.Timestamp = Convert.ToInt32( date.Subtract( epochTime ).TotalSeconds );
        }

        public Tweet( string user, string content, int likes, int retweets )
        {
            this.User = user;
            this.Content = content;
            this.Likes = likes;
            this.Retweets = retweets;
            this.Timestamp = Convert.ToInt32( DateTime.Now.Subtract( epochTime ).TotalSeconds );
        }
    }
}
