using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter
{
    class Coordinates : Tweetinvi.Models.ICoordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Coordinates( double p1, double p2 )
        {
            this.Latitude = p1;
            this.Longitude = p2;
        }
    }
}
