using System.Collections.Generic;

namespace InfoClientUWP.Model
{
    class RouteClient
    {
        public int RouteID { get; set; }
        public List<CheckpointsClient> Route { get; set; }
    }
}
