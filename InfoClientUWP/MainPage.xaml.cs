﻿using InfoClientUWP.Helpers;
using InfoClientUWP.Model;
using InfoClientUWP.Services;
using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InfoClientUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private double BorasLat = 57.72103500;
        private double BorasLong = 12.93981900;

        private double UlricehamnLat = 57.79159;
        private double UlricehamnLong = 13.41422;

        private double JonkopingLat = 57.78145;
        private double JonkopingLong = 14.15618;

        private void ShowBoras(object sender, RoutedEventArgs e)
        {            
            BasicGeoposition boras = new BasicGeoposition() { Latitude = BorasLat, Longitude = BorasLong };
            Geopoint cityCenter = new Geopoint(boras);

            MapControl1.Center = cityCenter;
            MapControl1.ZoomLevel = 10;
            MapControl1.LandmarksVisible = true;
        }

        private void ShowUlricehamn(object sender, RoutedEventArgs e)
        {
            BasicGeoposition ulricehamn = new BasicGeoposition() { Latitude = UlricehamnLat, Longitude = UlricehamnLong };
            Geopoint cityCenter = new Geopoint(ulricehamn);

            MapControl1.Center = cityCenter;
            MapControl1.ZoomLevel = 10;
            MapControl1.LandmarksVisible = true;
        }

        private async void ShowRouteStartEndAsync(object sender, RoutedEventArgs e)
        {
            BasicGeoposition startLocation = new BasicGeoposition() { Latitude = JonkopingLat, Longitude = JonkopingLong };
            BasicGeoposition endLocation = new BasicGeoposition() { Latitude = BorasLat, Longitude = BorasLong };

            MapRouteFinderResult routeResult =
                  await MapRouteFinder.GetDrivingRouteAsync(
                  new Geopoint(startLocation),
                  new Geopoint(endLocation),
                  MapRouteOptimization.Time,
                  MapRouteRestrictions.None);

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;

                MapControl1.Routes.Add(viewOfRoute);

                await MapControl1.TrySetViewBoundsAsync(
                      routeResult.Route.BoundingBox,
                      null,
                      Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
            }
        }

        public void AddMarker(BasicGeoposition pos, string text)
        {
            Geopoint geoForPos = new Geopoint(pos);
            MapIcon pin = new MapIcon
            {
                Location = geoForPos,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                Title = text,
                ZIndex = 0
            };
            MapControl1.MapElements.Add(pin);
        }

        public void AddMarkerUserPos(Geopoint point, string text)
        {
            MapIcon pin = new MapIcon
            {
                Location = point,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                Title = text,
                ZIndex = 0
            };

            MapControl1.MapElements.Add(pin);
        }

        private async void ShowWayPointsAsync(object sender, RoutedEventArgs e)
        {
            Geolocator locator = new Geolocator();
            locator.DesiredAccuracyInMeters = 1;

            BasicGeoposition point1 = new BasicGeoposition() { Latitude = JonkopingLat, Longitude = JonkopingLong };
            BasicGeoposition point2 = new BasicGeoposition() { Latitude = UlricehamnLat, Longitude = UlricehamnLong };
            BasicGeoposition point3 = new BasicGeoposition() { Latitude = BorasLat, Longitude = BorasLong };

            AddMarker(point1, "0.016 m/s" + "\n" + "10:10 A.M.");
            AddMarker(point2, "Speed: 0.016 m/s" + "\n" + "Time: 13:20 P.M.");
            AddMarker(point3, "0.016 m/s" + "\n" + "15:30 P.M.");

            var path = new List<EnhancedWaypoint>();

            path.Add(new EnhancedWaypoint(new Geopoint(point1), WaypointKind.Stop)); //WaypointKind.Stop = 0
            path.Add(new EnhancedWaypoint(new Geopoint(point2), WaypointKind.Via));  //WaypointKind.Via = 1
            path.Add(new EnhancedWaypoint(new Geopoint(point3), WaypointKind.Stop));

            MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteFromEnhancedWaypointsAsync(path);

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;

                MapControl1.Routes.Add(viewOfRoute);

                await MapControl1.TrySetViewBoundsAsync(
                      routeResult.Route.BoundingBox,
                      null,
                      Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
            }
        }

        private async void ShowThisLocationAsync(object sender, RoutedEventArgs e)
        {
            var accesStatus = await Geolocator.RequestAccessAsync();

            switch (accesStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    Geolocator geolocator = new Geolocator();
                    Geoposition position = await geolocator.GetGeopositionAsync();
                    Geopoint thisLocation = position.Coordinate.Point;

                    double lat = thisLocation.Position.Latitude;
                    double lon = thisLocation.Position.Longitude;

                    AddMarkerUserPos(thisLocation, lat.ToString() + " " + lon.ToString());

                    MapControl1.Center = thisLocation;
                    MapControl1.ZoomLevel = 17;
                    MapControl1.LandmarksVisible = true;

                    // COMMUNICATION TEST

                    Converters converter = new Converters();

                    CheckpointsClient checkPoint = new CheckpointsClient
                    {
                        RouteID = 1,
                        Latitude = converter.FromDoubleToString(lat),
                        Longitude = converter.FromDoubleToString(lon),
                        CPDateTime = DateTime.Now
                    };

                    RequestHandler handler = new RequestHandler();
                    await handler.PostDataToAPI(checkPoint);

                    // END OF TEST

                    break;

                case GeolocationAccessStatus.Denied:
                    break;

                case GeolocationAccessStatus.Unspecified:
                    break;
            }
        }
    }
}