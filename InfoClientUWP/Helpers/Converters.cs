using System;

namespace InfoClientUWP.Helpers
{
    class Converters
    {
        public double FromStringToDouble(string s)
        {
            return Double.Parse(s);
        }

        public string FromDoubleToString(double d)
        {
            return d.ToString();
        }

        public string SetStringRightFormat(DateTime dateTime)
        {
            string month = dateTime.Month.ToString();
            string day = dateTime.Day.ToString();
            string year = dateTime.Year.ToString();

            if (month.Length < 2)
            {
                month = "0" + month;
            }

            if (day.Length < 2)
            {
                day = "0" + day;
            }

            return month + "-" + day + "-" + year;
        }
    }
}
