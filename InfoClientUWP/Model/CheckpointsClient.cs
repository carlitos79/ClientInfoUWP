using System;

namespace InfoClientUWP.Model
{
    class CheckpointsClient
    {
        public int RouteID { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime CPDateTime { get; set; }
    }
}
