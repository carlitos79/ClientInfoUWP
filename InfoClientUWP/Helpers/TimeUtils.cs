using System;

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
    }
}
