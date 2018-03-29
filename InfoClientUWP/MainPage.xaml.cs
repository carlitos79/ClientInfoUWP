using InfoClientUWP.Helpers;
using InfoClientUWP.Model;
using InfoClientUWP.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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
            GetCheckpoints();
            CheckIfBeenHereBefore();

            // Create timer to call OnBackgroundEvent every 1 sec.
            backgroundTimer = ThreadPoolTimer.CreatePeriodicTimer(OnBackgroundEvent, TimeSpan.FromSeconds(1));
        }

        private ThreadPoolTimer backgroundTimer;
        private ThreadPoolTimer timer = null;
        private Guid routeId = Guid.NewGuid();
        private TimeUtils timeUtility = new TimeUtils();
        private Converters converter = new Converters();
        private List<Geopoint> path = new List<Geopoint>();
        private List<CheckpointsClient> checkpointsList = new List<CheckpointsClient>();
        private CalendarViewDayItem date;
        private Geopoint myLocation;

        private async void ShowPath()
        {
            if (path.Count >= 2)
            {
                foreach (var geoPoint in path)
                {
                    AddMarkerUserPos(geoPoint, timeUtility.GetTime(DateTime.Now));
                }

                MapRouteFinderResult routeResult = await MapRouteFinder.GetWalkingRouteFromWaypointsAsync(path);

                if (routeResult.Status == MapRouteFinderStatus.Success)
                {
                    MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                    viewOfRoute.RouteColor = Colors.Yellow;
                    viewOfRoute.OutlineColor = Colors.Black;                    

                    MapControl1.Routes.Add(viewOfRoute);
                    MapControl1.ZoomLevel = 7;

                    await MapControl1.TrySetViewBoundsAsync(
                          routeResult.Route.BoundingBox,
                          null,
                          Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
                }
            }           
        }

        private async void UpdatePosition()
        {
            var accesStatus = await Geolocator.RequestAccessAsync();

            switch (accesStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    Geolocator geolocator = new Geolocator();
                    Geoposition position = await geolocator.GetGeopositionAsync();
                    Geopoint thisLocation = position.Coordinate.Point;

                    path.Add(thisLocation);

                    double lat = thisLocation.Position.Latitude;
                    double lon = thisLocation.Position.Longitude;

                    ShowPath();

                    Converters converter = new Converters();

                    CheckpointsClient checkPoint = new CheckpointsClient
                    {
                        RouteID = routeId.ToString(),
                        Latitude = converter.FromDoubleToString(lat),
                        Longitude = converter.FromDoubleToString(lon),
                        CPDateTime = DateTime.Now
                    };

                    RequestHandler handler = new RequestHandler();
                    await handler.PostDataToAPI(checkPoint);

                    break;

                case GeolocationAccessStatus.Denied:
                    break;

                case GeolocationAccessStatus.Unspecified:
                    break;
            }
        }

        // Timer method to be called every 1 sec.
        private async void OnBackgroundEvent(ThreadPoolTimer timer)
        {
            // Call CheckIfBeenHereBefore() from the UI thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                CheckIfBeenHereBefore();
            }); 
        }

        // Timer method to be called every 15 sec.
        private async void OnTimedEvent(ThreadPoolTimer timer)
        {
            // Call UpdatePosition from the UI thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                UpdatePosition();
            }); 
        }

        private void ShowLiveRoute(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                LiveButton.Content = "Stop Tracking";
                // Create timer to call OnTimedEvent every 15 sec.
                timer = ThreadPoolTimer.CreatePeriodicTimer(OnTimedEvent, TimeSpan.FromSeconds(15));
                System.Diagnostics.Debug.WriteLine("starting timer");
            }
            else
            {
                LiveButton.Content = "Start Tracking";
                // Stop the timer.
                timer.Cancel();
                timer = null;
                System.Diagnostics.Debug.WriteLine("stopping timer");
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

        private async Task<Geopoint> GetThisLocationAsync()
        {
            var accesStatus = await Geolocator.RequestAccessAsync();
            Geopoint thisLocation;

            if (accesStatus == GeolocationAccessStatus.Allowed)
            {
                Geolocator geolocator = new Geolocator();
                Geoposition position = await geolocator.GetGeopositionAsync();
                thisLocation = position.Coordinate.Point;

                return thisLocation;
            }
            else if (accesStatus == GeolocationAccessStatus.Denied || accesStatus == GeolocationAccessStatus.Unspecified)
            {
                return null; 
            }

            return null;
        }

        private void CheckIfBeenHereBefore()
        {
            foreach (var checkpoint in checkpointsList)
            {
                if (myLocation.Position.Equals(checkpoint))
                {
                    InfoTextBlock.Text = "You have been here before";
                }
            }
        }

        private async void ShowWayPointsAsync(object sender, ItemClickEventArgs e)
        {
            var route = (RouteClient)e.ClickedItem;            

            Geolocator locator = new Geolocator();
            locator.DesiredAccuracyInMeters = 1;

            BasicGeoposition point;
            var path2 = new List<Geopoint>();

            var newRoute = new RouteClient()
            {
                RouteID = route.RouteID,
                Route = route.Route
            };

            for(int i = 0; i < newRoute.Route.Count - 1; i++)
            {
                var cp1 = newRoute.Route[i];
                var cp2 = newRoute.Route[i + 1];

                point = new BasicGeoposition() {
                    Latitude = converter.FromStringToDouble(cp1.Latitude),
                    Longitude = converter.FromStringToDouble(cp1.Longitude)
                };

                string speedString = Math.Abs(timeUtility.ComputeSpeed(cp1, cp2)).ToString();
                string speed = speedString.Substring(0, 6);

                AddMarker(point, "Date: " + timeUtility.GetDate(cp1.CPDateTime) + "\n" + "Speed: " + speed + " m/s");
                path2.Add(new Geopoint(point));
            }
            var last = newRoute.Route[newRoute.Route.Count - 1];
            point = new BasicGeoposition() { Latitude = converter.FromStringToDouble(last.Latitude), Longitude = converter.FromStringToDouble(last.Longitude) };
            AddMarker(point, "Date: " + timeUtility.GetDate(last.CPDateTime) + "\n" + "Speed: 0 m/s");
            path2.Add(new Geopoint(point));            

            MapRouteFinderResult routeResult = await MapRouteFinder.GetWalkingRouteFromWaypointsAsync(path2);

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                if (PopupRouteInfo.IsOpen)
                {
                    PopupRouteInfo.IsOpen = false;
                    RouteListView.Items.Clear();
                }

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

                    Converters converter = new Converters();

                    CheckpointsClient checkPoint = new CheckpointsClient
                    {
                        RouteID = routeId.ToString(),
                        Latitude = converter.FromDoubleToString(lat),
                        Longitude = converter.FromDoubleToString(lon),
                        CPDateTime = DateTime.Now
                    };

                    RequestHandler handler = new RequestHandler();
                    await handler.PostDataToAPI(checkPoint);

                    break;

                case GeolocationAccessStatus.Denied:
                    break;

                case GeolocationAccessStatus.Unspecified:
                    break;
            }
        }

        private async void GetCheckpoints()
        {
            RequestHandler handler = new RequestHandler();
            IEnumerable<CheckpointsClient> checkpoints = await handler.GetDataFromAPI<CheckpointsClient>("Checkpoints");

            List<CheckpointsClient> tempCheckpointsList = new List<CheckpointsClient>();

            foreach (var checkpoint in checkpoints)
            {
                tempCheckpointsList.Add(checkpoint);
            }

            foreach (var checkpoint in tempCheckpointsList)
            {
                if (!checkpointsList.Contains(checkpoint))
                {
                    checkpointsList.Add(checkpoint);
                }
            }
            myLocation = await GetThisLocationAsync();
        }

        private void UpdateCalendarView(object sender, PointerRoutedEventArgs e)
        {
            SolidColorBrush greenBackground = new SolidColorBrush(Windows.UI.Colors.LightGreen);

            date = sender as CalendarViewDayItem;

            foreach (var checkpoint in checkpointsList)
            {
                string rightFormat = converter.SetStringRightFormat(checkpoint.CPDateTime);
                DateTime dateTime = DateTime.ParseExact(rightFormat, "MM-dd-yyyy", CultureInfo.InvariantCulture);
                DateTime checkDate = dateTime;

                if ((sender as CalendarViewDayItem).Date.Date.Equals(checkDate))
                {
                    (sender as CalendarViewDayItem).Background = greenBackground;
                    (sender as CalendarViewDayItem).BorderThickness = new Thickness(1);
                    (sender as CalendarViewDayItem).PointerPressed += OpenPopupRouteList;
                }
            }
        }

        private void CalendarViewPreviousRouteInfo(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            InfoTextBlock.Text = "Dates highlighted in green represent" + "\n" + "previous routes." + "\n" +
                                 "Press on the highlighted date to see" + "\n" + "the route(s) for that date.";

            date = args.Item;

            SolidColorBrush greenBackground = new SolidColorBrush(Windows.UI.Colors.LightGreen);

            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(CalendarViewPreviousRouteInfo);
            }
            
            args.Item.PointerPressed += UpdateCalendarView;
        }

        private void OpenPopupRouteList(object sender, PointerRoutedEventArgs e)
        {
            List<CheckpointsClient> tempCheckpointsList = new List<CheckpointsClient>();
            List<string> routeIdList = new List<string>();
            List<RouteClient> routes = new List<RouteClient>();
            List<CheckpointsClient> routeCheckpoints = null;

            if (!PopupRouteInfo.IsOpen)
            {
                PopupRouteInfo.IsOpen = true;
            }

            foreach (var checkpoint in checkpointsList)
            {
                string rightFormat = converter.SetStringRightFormat(checkpoint.CPDateTime);
                DateTime dateTime = DateTime.ParseExact(rightFormat, "MM-dd-yyyy", CultureInfo.InvariantCulture);
                DateTime checkpointDate = dateTime;

                if ((sender as CalendarViewDayItem).Date.Date.Equals(checkpointDate))
                {
                    tempCheckpointsList.Add(checkpoint);               
                }                
            }

            foreach (var checkpoint in tempCheckpointsList)
            {
                if (!routeIdList.Contains(checkpoint.RouteID))
                {
                    routeIdList.Add(checkpoint.RouteID);
                }
            }

            for (int i = 0; i < tempCheckpointsList.Count; i++)
            {
                routeCheckpoints = (from c in tempCheckpointsList where c.RouteID == routeIdList.ElementAtOrDefault(i) select c).ToList();

                if (routeCheckpoints != null)
                {
                    routes.Add(new RouteClient
                    {
                        RouteID = "Route " + (i + 1).ToString(),
                        Route = routeCheckpoints
                    });
                }
            }

            var route = (from r in routes
                        from c in r.Route
                        from rId in routeIdList
                        where c.RouteID == rId
                        select r).ToList();

            foreach (var r in route.Distinct())
            {
                RouteListView.Items.Add(r);
            }

            int routeIdListLength = routeIdList.Count();

            if (routeIdListLength >= 2)
            {
                RouteListView.Items.RemoveAt(0);
            }

            int ListViewLength = RouteListView.Items.Count() - routeIdListLength;

            for (int i = 0; i < ListViewLength; i++)
            {
                RouteListView.Items.RemoveAt(i);
            }

            RouteListView.ItemClick += ShowWayPointsAsync;
        }

        private void ClosePopupRouteInfo(object sender, RoutedEventArgs e)
        {
            if (PopupRouteInfo.IsOpen)
            {
                PopupRouteInfo.IsOpen = false;
            }
        }
    }
}
