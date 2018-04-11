using InfoClientUWP.Helpers;
using InfoClientUWP.Model;
using InfoClientUWP.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace InfoClientUWP
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            GetCheckpoints();
            GetThisLocationAsync();
        }

        private ThreadPoolTimer backgroundTimer = null;
        private ThreadPoolTimer timer = null;
        private Guid routeId = Guid.NewGuid();
        private TimeUtils timeUtility = new TimeUtils();
        private LocationHelpers locationHelper = new LocationHelpers();
        private Converters converter = new Converters();
        private List<Geopoint> path = new List<Geopoint>();
        private List<CheckpointsClient> checkpointsList = new List<CheckpointsClient>();
        private CalendarViewDayItem date;
        private Geopoint myLocation;

        //Here we get the user's position using C++
        //private async Task GetUserPositionCPlusPlus()
        //{
        //    var cPlusPlusClassInstance = new CppProj.Class1();
        //    var position = await cPlusPlusClassInstance.ReturnPosition();

        //    var x = position.Coordinate;
        //    var speed = position.Coordinate.Speed;
        //}

        private async void ShowPath()
        {
            if (path.Count >= 2)
            {
                foreach (var geoPoint in path)
                {
                    AddMarkerUserPos(geoPoint, timeUtility.GetTime(DateTime.Now));                }

                MapRouteFinderResult routeResult = await MapRouteFinder.GetWalkingRouteFromWaypointsAsync(path);

                if (routeResult.Status == MapRouteFinderStatus.Success)
                {
                    MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                    viewOfRoute.RouteColor = Colors.Yellow;
                    viewOfRoute.OutlineColor = Colors.Black;

                    MapControl1.Routes.Add(viewOfRoute);
                    MapControl1.ZoomLevel = 17;

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

        private async void OnBackgroundEvent(ThreadPoolTimer backgroundTimer)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {                
                CheckIfBeenHereBefore();
            }); 
        }

        private async void OnTimedEvent(ThreadPoolTimer timer)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                UpdatePosition();
            }); 
        }

        private void ShowLiveRoute(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                LiveButton.Content = "Stop Tracking";
                timer = ThreadPoolTimer.CreatePeriodicTimer(OnTimedEvent, TimeSpan.FromSeconds(5));
                System.Diagnostics.Debug.WriteLine("starting timer");
            }
            else
            {
                LiveButton.Content = "Start Tracking";
                timer.Cancel();
                timer = null;
                System.Diagnostics.Debug.WriteLine("stopping timer");
            }
        }

        private void PreviousRoutesActivation(object sender, RoutedEventArgs e)
        {
            if (backgroundTimer == null)
            {
                PreviousRouteButton.Content = "Prev. Routes Search Off";
                backgroundTimer = ThreadPoolTimer.CreatePeriodicTimer(OnBackgroundEvent, TimeSpan.FromSeconds(5));
            }
            else
            {
                PreviousRouteButton.Content = "Prev. Routes Search On";
                backgroundTimer.Cancel();
                backgroundTimer = null;
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

        private async void GetThisLocationAsync()
        {
            var accesStatus = await Geolocator.RequestAccessAsync();

            switch (accesStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    Geolocator geolocator = new Geolocator();
                    Geoposition position = await geolocator.GetGeopositionAsync();
                    myLocation = position.Coordinate.Point;
                    break;

                case GeolocationAccessStatus.Denied:
                    break;

                case GeolocationAccessStatus.Unspecified:
                    break;
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

                if (PopupBeenHereInfo.IsOpen)
                {
                    PopupBeenHereInfo.IsOpen = false;
                    BeenHereListView.Items.Clear();
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(PreviousRouteButton);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
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

                    //Here we get the user's position using C++
                    var cPlusPlusClass = new CppProj.Class1();
                    var position = await cPlusPlusClass.ReturnPosition();

                    Geopoint thisLocation = position.Coordinate.Point;

                    double lat = thisLocation.Position.Latitude;
                    double lon = thisLocation.Position.Longitude;
                    double speed = (double)position.Coordinate.Speed;

                    if (double.IsNaN(speed))
                    {
                        speed = 0.0;
                    }

                    AddMarkerUserPos(thisLocation, "Lat: " + lat.ToString() + 
                                                   "\n" + " Long: " + lon.ToString() + 
                                                   "\n" + "Speed: " + speed.ToString());

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
                if (!checkpointsList.Contains(checkpoint))
                {
                    checkpointsList.Add(checkpoint);
                }
            }         
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
            converter.DisplayCalendarRouteInfo(InfoTextBlock);

            date = args.Item;

            SolidColorBrush greenBackground = new SolidColorBrush(Windows.UI.Colors.LightGreen);

            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(CalendarViewPreviousRouteInfo);
            }
            
            args.Item.PointerPressed += UpdateCalendarView;
        }

        private void CheckIfBeenHereBefore()
        {
            List<CheckpointsClient> tempCheckpointsList = new List<CheckpointsClient>();
            List<string> routeIdList = new List<string>();
            List<RouteClient> routes = new List<RouteClient>();
            List<RouteClient> previousRoutes;
            List<CheckpointsClient> routeCheckpoints = null;

            string checkpointId = "";

            foreach (var checkpoint in checkpointsList)
            {
                if (myLocation == null)
                {
                    InfoTextBlock.Text = "Getting this Location...";
                    break;
                }
                else
                {
                    if (checkpoint.Latitude.Equals(converter.FromDoubleToString(myLocation.Position.Latitude)))
                    {
                        if (!PopupBeenHereInfo.IsOpen)
                        {
                            PopupBeenHereInfo.IsOpen = true;
                        }

                        checkpointId = checkpoint.RouteID;
                    }
                    else
                    {
                        converter.DisplayCalendarRouteInfo(InfoTextBlock);
                    }
                }
            }

            foreach (var checkpoint in checkpointsList)
            {
                if (checkpointId.Equals(checkpoint.RouteID))
                {
                    if (!tempCheckpointsList.Contains(checkpoint))
                    {
                        tempCheckpointsList.Add(checkpoint);
                    }
                }
            }

            locationHelper.GetRouteId(tempCheckpointsList, routeIdList);
            locationHelper.CreateRoute(tempCheckpointsList, routeCheckpoints, routes, routeIdList);
            previousRoutes = locationHelper.GetPreviousRoutes(routes, routeIdList);
            locationHelper.DisplayRoutesInListView(BeenHereListView, previousRoutes, routeIdList);            

            BeenHereListView.ItemClick += ShowWayPointsAsync;
        }

        private void OpenPopupRouteList(object sender, PointerRoutedEventArgs e)
        {
            List<CheckpointsClient> tempCheckpointsList = new List<CheckpointsClient>();
            List<string> routeIdList = new List<string>();
            List<RouteClient> routes = new List<RouteClient>();
            List<RouteClient> previousRoutes;
            List<CheckpointsClient> routeCheckpoints = null;

            OpenPopupRouteInfo(sender, e);

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

            locationHelper.GetRouteId(tempCheckpointsList, routeIdList);
            locationHelper.CreateRoute(tempCheckpointsList, routeCheckpoints, routes, routeIdList);
            previousRoutes = locationHelper.GetPreviousRoutes(routes, routeIdList);
            locationHelper.DisplayRoutesInListView(RouteListView, previousRoutes, routeIdList);

            RouteListView.ItemClick += ShowWayPointsAsync;
        }

        private void ClosePopupRouteInfo(object sender, RoutedEventArgs e)
        {
            if (PopupRouteInfo.IsOpen)
            {
                PopupRouteInfo.IsOpen = false;
            }
        }

        private void OpenPopupRouteInfo(object sender, RoutedEventArgs e)
        {
            if (!PopupRouteInfo.IsOpen)
            {
                PopupRouteInfo.IsOpen = true;
            }
        }

        private void ClosePopupBeenHereInfo(object sender, RoutedEventArgs e)
        {
            if (PopupBeenHereInfo.IsOpen)
            {
                PopupBeenHereInfo.IsOpen = false;
            }
        }

        private void OpenPopupBeenHereInfo(object sender, RoutedEventArgs e)
        {
            if (!PopupBeenHereInfo.IsOpen)
            {
                PopupBeenHereInfo.IsOpen = true;
            }
        }
    }
}
