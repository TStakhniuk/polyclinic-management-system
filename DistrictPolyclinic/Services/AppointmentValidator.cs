using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistrictPolyclinic.Services
{
    public class AppointmentValidator
    {
        public static bool IsWithinWorkingHours(DateTime startTime, DateTime endTime)
        {
            TimeSpan workStart = new TimeSpan(8, 0, 0);  // 08:00
            TimeSpan workEnd = new TimeSpan(17, 0, 0);   // 17:00

            return startTime.TimeOfDay >= workStart && endTime.TimeOfDay <= workEnd;
        }

        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsStartBeforeEnd(DateTime start, DateTime end)
        {
            return start < end;
        }
    }


}
