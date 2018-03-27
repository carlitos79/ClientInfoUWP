using System.Collections.Generic;

namespace InfoClientUWP.Model
{
    class RouteClient
    {
        public string RouteID { get; set; }
        public List<CheckpointsClient> Route { get; set; }
    }
}
