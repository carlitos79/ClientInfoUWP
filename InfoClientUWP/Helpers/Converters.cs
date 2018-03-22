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
    }
}
