using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace myRaiHelper
{
	public static class DateTimeHelpers
	{

		public static DateTime ToDateTime ( this string s,
							  string format = "ddMMyyyy", string cultureString = "it-IT" )
		{
			try
			{
				var r = DateTime.ParseExact(
					s: s,
					format: format,
					provider: CultureInfo.GetCultureInfo( cultureString ) );
				return r;
			}
			catch ( FormatException exception )
			{
				throw exception;
			}
			catch ( CultureNotFoundException )
			{
				throw; // Given Culture is not supported culture
			}
		}

		public static DateTime ToDateTime ( this string s,
					string format, CultureInfo culture )
		{
			try
			{
				var r = DateTime.ParseExact( s: s, format: format,
										provider: culture );
				return r;
			}
			catch ( FormatException )
			{
				throw;
			}
			catch ( CultureNotFoundException )
			{
				throw; // Given Culture is not supported culture
			}

		}


		static GregorianCalendar _gc = new GregorianCalendar();

		public static int GetWeekOfMonth ( this DateTime time )
		{
			DateTime first = new DateTime( time.Year, time.Month, 1 );
			return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
		}

		public static int GetWeekOfYear ( this DateTime time )
		{
			return _gc.GetWeekOfYear( time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday );
		}
	}


    public struct DateTimeSpan
    {
        public int Years { get; }
        public int Months { get; }
        public int Days { get; }
        public int Hours { get; }
        public int Minutes { get; }
        public int Seconds { get; }
        public int Milliseconds { get; }

        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            Years = years;
            Months = months;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
        }

        enum Phase { Years, Months, Days, Done }

        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            DateTime current = date1;
            int years = 0;
            int months = 0;
            int days = 0;

            Phase phase = Phase.Years;
            DateTimeSpan span = new DateTimeSpan();
            int officialDay = current.Day;

            while (phase != Phase.Done)
            {
                switch (phase)
                {
                    case Phase.Years:
                        if (current.AddYears(years + 1) > date2)
                        {
                            phase = Phase.Months;
                            current = current.AddYears(years);
                        }
                        else
                        {
                            years++;
                        }
                        break;
                    case Phase.Months:
                        if (current.AddMonths(months + 1) > date2)
                        {
                            phase = Phase.Days;
                            current = current.AddMonths(months);
                            if (current.Day < officialDay && officialDay <= DateTime.DaysInMonth(current.Year, current.Month))
                                current = current.AddDays(officialDay - current.Day);
                        }
                        else
                        {
                            months++;
                        }
                        break;
                    case Phase.Days:
                        if (current.AddDays(days + 1) > date2)
                        {
                            current = current.AddDays(days);
                            var timespan = date2 - current;
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                            phase = Phase.Done;
                        }
                        else
                        {
                            days++;
                        }
                        break;
                }
            }

            return span;
        }
    }
}