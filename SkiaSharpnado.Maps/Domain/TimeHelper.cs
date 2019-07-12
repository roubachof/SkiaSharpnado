using System;
using System.Globalization;

namespace SkiaSharpnado.Maps.Domain
{
    public static class TimeHelper
    {
        /// <summary>
        /// Converts the specified input value into smart date string [local time].
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <returns>The smart date string</returns>
        public static string ToSmartDate(this DateTime inputValue)
        {
            string res;

            var months = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames;
            var dayNames = CultureInfo.CurrentUICulture.DateTimeFormat.DayNames;

            var dfi = DateTimeFormatInfo.CurrentInfo;
            var calendar = dfi.Calendar;

            var now = DateTime.Now;

            bool isToday = inputValue.Year == now.Year && inputValue.Month == now.Month && inputValue.Day == now.Day;

            bool isThisWeek = inputValue.Year == now.Year
                && calendar.GetWeekOfYear(inputValue, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)
                == calendar.GetWeekOfYear(now, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

            bool isThisYear = inputValue.Year == now.Year;

            string time = inputValue.ToString("t");
            string day = dayNames[(int)inputValue.DayOfWeek];
            string month = inputValue.ToString("m");
            int year = inputValue.Year;

            if (isToday)
            {
                return time;
            }

            if (isThisWeek)
            {
                return $"{day} {time}";
            }

            if (isThisYear)
            {
                return $"{day} {month} {time}";
            }

            return $"{day} {month} {year} {time}";
        }

        public static string ToSmartShortDate(this DateTime inputValue)
        {
            string res;

            var currentCulture = CultureInfo.CurrentUICulture;

            var abbreviatedMonths = currentCulture.DateTimeFormat.AbbreviatedMonthNames;
            var months = currentCulture.DateTimeFormat.MonthNames;

            var dfi = DateTimeFormatInfo.CurrentInfo;
            var calendar = dfi.Calendar;

            var now = DateTime.Now;
            if (inputValue.Year == now.Year && inputValue.Month == now.Month && inputValue.Day == now.Day)
            {
                res = inputValue.ToString("t");
            }
            else if (inputValue.Year == now.Year && calendar.GetWeekOfYear(inputValue, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) == calendar.GetWeekOfYear(now, dfi.CalendarWeekRule, dfi.FirstDayOfWeek))
            {
                var abbreviatedDayNames = currentCulture.DateTimeFormat.DayNames;
                res = abbreviatedDayNames[(int)inputValue.DayOfWeek];
            }
            else if (inputValue.Year == now.Year)
            {
                res =
                    inputValue.ToString("M", currentCulture)
                        .Replace(months[inputValue.Month - 1], abbreviatedMonths[inputValue.Month - 1]);
            }
            else
            {
                var abbreviatedMonthAndDay = inputValue.ToString("M", currentCulture)
                    .Replace(months[inputValue.Month - 1], abbreviatedMonths[inputValue.Month - 1]);
                res = string.Format("{0} {1}", abbreviatedMonthAndDay, inputValue.Year);
            }

            return res;
        }
    }
}