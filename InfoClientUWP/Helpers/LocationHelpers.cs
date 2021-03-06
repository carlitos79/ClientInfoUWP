﻿using InfoClientUWP.Model;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace InfoClientUWP.Helpers
{
    class LocationHelpers
    {
        public void GetRouteId(List<CheckpointsClient> source, List<string> target)
        {
            foreach (var checkpoint in source)
            {
                if (!target.Contains(checkpoint.RouteID))
                {
                    target.Add(checkpoint.RouteID);
                }
            }
        }

        public void CreateRoute(List<CheckpointsClient> checkpointsList, List<CheckpointsClient> route, List<RouteClient> routes, List<string> routeIdList)
        {
            for (int i = 0; i < checkpointsList.Count; i++)
            {
                route = (from c in checkpointsList where c.RouteID == routeIdList.ElementAtOrDefault(i) select c).ToList();

                if (route != null)
                {
                    routes.Add(new RouteClient
                    {
                        RouteID = "Route " + (i + 1).ToString(),
                        Route = route
                    });
                }
            }
        }

        public List<RouteClient> GetPreviousRoutes(List<RouteClient> routes, List<string> routeIdList)
        {
            var route = (from r in routes
                         from c in r.Route
                         from rId in routeIdList
                         where c.RouteID == rId
                         select r).ToList();

            return route;
        }

        public void DisplayRoutesInListView(ListView listView, List<RouteClient> previousRoutes, List<string> routeIdList)
        {
            foreach (var r in previousRoutes.Distinct())
            {
                listView.Items.Add(r);
            }

            int routeIdListLength = routeIdList.Count();

            if (routeIdListLength >= 2)
            {
                listView.Items.RemoveAt(0);
            }

            int ListViewLength = listView.Items.Count() - routeIdListLength;

            for (int i = 0; i < ListViewLength; i++)
            {
                listView.Items.RemoveAt(i);
            }
        }
    }
}
