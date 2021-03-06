﻿using System;


namespace Swarmops.Common.ExtensionMethods
{
    public static partial class DummyWrapper
    {
        public static UInt64 ToUnix (this DateTime source)
        {
            // Assumes DateTime is already UTC

            return (UInt64)(source.Subtract(new DateTime(1970, 1, 1, 0, 0, 0,DateTimeKind.Utc))).TotalSeconds;
        }

        public static bool IsHigh(this DateTime source)
        {
            if (source.Year > 2299) return true;

            return false;
        }

        public static bool IsLow(this DateTime source)
        {
            if (source.Year < 1801) return true;

            return false;
        }

        public static bool IsDefined(this DateTime source)
        {
            if (source.IsHigh()) return false;
            if (source.IsLow()) return false;

            return true;
        }
    }
}
