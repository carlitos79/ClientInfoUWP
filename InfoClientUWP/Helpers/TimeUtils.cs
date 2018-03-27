using InfoClientUWP.Model;
using System;
using Windows.Devices.Geolocation;

namespace InfoClientUWP.Helpers
{
    class TimeUtils
    {
        public string GetTime(DateTime dateTime)
        {
            return dateTime.ToString("t");
        }

        public string GetDate(DateTime dateTime)
        {
            return dateTime.ToString("d");
        }

        public double ComputeSpeed(CheckpointsClient cp1, CheckpointsClient cp2)
        {
            var pos1 = new BasicGeoposition()
            {
                Latitude = Convert.ToDouble(cp1.Latitude),
                Longitude = Convert.ToDouble(cp1.Longitude)
            };
            var pos2 = new BasicGeoposition()
            {
                Latitude = Convert.ToDouble(cp2.Latitude),
                Longitude = Convert.ToDouble(cp2.Longitude)
            };
            double R = 6371;
            double dLat = ToRadian(pos2.Latitude - pos1.Latitude);
            double dLon = ToRadian(pos2.Longitude - pos1.Longitude);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(this.ToRadian(pos1.Latitude)) * Math.Cos(this.ToRadian(pos2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = (R * c) * 1000;

            return d / Math.Abs(((cp1.CPDateTime.Second + cp1.CPDateTime.Minute * 60 + cp1.CPDateTime.Hour * 3600) -
                (cp2.CPDateTime.Second + cp2.CPDateTime.Minute * 60 + cp2.CPDateTime.Hour * 3600)));
        }

        private double ToRadian(double val)
        {
            return (Math.PI / 180) * val;
        }
    }
}
