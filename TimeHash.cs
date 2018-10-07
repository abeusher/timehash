using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeHash
{
    /**
     * For .NET Core 2.1
     * This struct creates a fuzzy precision representation of a time interval.
        - It makes calculations based on a 64 year representation of time from January 1, 1970 to January 1, 2098.
        - Values are encoded with a number of bits(represented by an ASCII character) that indicate the amount of time to add to 1970.
        - Times prior to 1970 or after 2098 are not accounted for by this scale.
        - Each character added to the timehash reduces the time interval ambiguity by a factor of 8.
        - Valid characters for encoding the floating point time into ASCII characters include {01abcdef}
        0 +/- 64 years
        1 +/- 8 years
        2 +/- 1 years
        3 +/- 45.65625 days
        4 +/- 5.707 days
        5 +/- 0.71337 days = 17.121 hours
        6 +/- 2.14013671875 hours
        7 +/- 0.26751708984375 hours = 16.05 minutes
        8 +/- 2.006378173828125 minutes
        9 +/- 0.2507 minutes = 15 seconds
        10 +/- 1.88097 seconds
     */
    public struct TimeHash
    {
        #region [ Static ]

        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const string BASE32 = "01abcdef";
        private const string BEFORE = "f01abcde";
        private const string AFTER = "1abcdef0";
        private static readonly Dictionary<string, int> DECODE_MAP = new Dictionary<string, int>();
        private static readonly Dictionary<string, Tuple<char, char>> NEIGHBOR_MAP = new Dictionary<string, Tuple<char, char>>();
        private static readonly int[] MaskArray = { 4, 2, 1 };

        public static readonly double TimeIntervalStart = 0.0; // from January 1, 1970
        public static readonly double TimeIntervalEnd = 4039372800.0; // to January 1, 2098

        static TimeHash()
        {
            for (int i = 0; i < BASE32.Length; i++)
            {
                DECODE_MAP.Add(BASE32.Substring(i, 1), i);
                NEIGHBOR_MAP.Add(BASE32.Substring(i, 1), new Tuple<char, char>(BEFORE[i], AFTER[i]));
            }
        }

        private static double ToEpochSeconds(DateTime time)
        {
            return (time.ToUniversalTime() - EPOCH).TotalSeconds;
        }

        private static DateTime FromEpochSeconds(double epochSeconds)
        {
            return new DateTime(TimeSpan.FromSeconds(epochSeconds).Ticks, DateTimeKind.Utc);
        }

        /// <summary>
        /// Validates a given string to determine if it is a valid hash code
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public static bool Validate(string hashCode)
        {
            return hashCode.All(c => BASE32.Contains(c));
        }

        /// <summary>
        /// Encodes a given unix time to a TimeHash hash code using the given number of characters for precision
        /// </summary>
        /// <param name="epochTime">A unix time in seconds</param>
        /// <param name="precision">The number of characters in the resulting hash code</param>
        /// <returns></returns>
        public static string Encode(double epochTime, int precision)
        {
            var start = TimeIntervalStart;
            var end = TimeIntervalEnd;

            StringBuilder timeHash = new StringBuilder();
            int bit = 0;
            int ch = 0;

            while (timeHash.Length < precision)
            {
                double mid = (start + end) * 0.5;
                if (epochTime > mid)
                {
                    ch |= MaskArray[bit];
                    start = mid;
                }
                else
                {
                    end = mid;
                }

                if (bit < 2)
                {
                    bit++;
                }
                else
                {
                    timeHash.Append(BASE32.Substring(ch, 1));
                    bit = 0;
                    ch = 0;
                }
            }

            return timeHash.ToString();
        }

        /// <summary>
        /// Encodes a given DateTime to a TimeHash hash code
        /// </summary>
        /// <param name="time">A DateTime</param>
        /// <param name="precision">The number of characters in the resulting hash code</param>
        /// <returns></returns>
        public static string Encode(DateTime time, int precision)
        {
            return Encode(ToEpochSeconds(time), precision);
        }

        /// <summary>
        /// Decodes a given TimeHash hash code to a TimeHash struct
        /// </summary>
        /// <param name="hashCode">A valid TimeHash hash code</param>
        /// <returns>The TimeHash instance representing the hash code</returns>
        public static TimeHash DecodeExactly(string hashCode)
        {
            var start = TimeIntervalStart;
            var end = TimeIntervalEnd;
            var timeError = (start + end) * 0.5;                        

            for (int i = 0; i < hashCode.Length; i++)
            {
                var c = hashCode.Substring(i, 1);
                int cd = DECODE_MAP[c];

                for (int j = 0; j < 3; j++)
                {
                    timeError = timeError * 0.5;
                    double mid = (start + end) * 0.5;

                    if ((cd & MaskArray[j]) == 0)
                    {
                        end = mid;
                    }
                    else
                    {
                        start = mid;
                    }
                }
            }

            double timeValue = (start + end) * 0.5;

            return new TimeHash(hashCode, timeValue, timeError);
        }

        /// <summary>
        /// Decodes a given TimeHash hash code to a unix time
        /// </summary>
        /// <param name="hashCode">A valid TimeHash hash code</param>
        /// <returns>The Center of the time range represented by the hash code</returns>
        public static double Decode(string hashCode)
        {
            return DecodeExactly(hashCode).Center;
        }

        /// <summary>
        /// Gets the hash code that preceeds the given hash code
        /// </summary>
        /// <param name="hashCode">A valid TimeHash hash code</param>
        /// <returns>The hash code that preceeds hashCode at the same level of precision</returns>
        public static string Before(string hashCode)
        {
            int i = 1;
            var reversedHash = hashCode.Reverse();
            var ret = string.Empty;

            foreach (var c in reversedHash)
            {
                if (c != '0')
                {
                    var padding = new String('f', i - 1);
                    var pos = hashCode.Length - i;
                    ret = $"{hashCode.Substring(0, pos)}{NEIGHBOR_MAP[c.ToString()].Item1}{padding}";
                    break;
                }
                else
                {
                    i++;
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the hash code that follows the given hash code
        /// </summary>
        /// <param name="hashCode">A valid TimeHash hash code</param>
        /// <returns>The hash code that follows hashCode at the same level of precision</returns>
        public static string After(string hashCode)
        {
            int i = 1;
            var reversedHash = hashCode.Reverse();
            var ret = string.Empty;

            foreach (var c in reversedHash)
            {
                if (c != 'f')
                {
                    var padding = new String('0', i - 1);
                    var pos = hashCode.Length - i;
                    ret = $"{hashCode.Substring(0, pos)}{NEIGHBOR_MAP[c.ToString()].Item2}{padding}";
                    break;
                }
                else
                {
                    i++;
                }
            }

            return ret;
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a TimeHash struct by decoding the given hash code
        /// </summary>
        /// <param name="hashCode">A valid TimeHash hash code</param>
        public TimeHash(string hashCode)
        {
            var tmp = DecodeExactly(hashCode);
            HashCode = hashCode;
            Center = tmp.Center;
            Error = tmp.Error;
        }

        private TimeHash(string hashCode, double center, double error)
        {
            HashCode = hashCode;
            Center = center;
            Error = error;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// The hash code that was used to create this TimeHash instance
        /// </summary>
        public string HashCode { get; }

        /// <summary>
        /// The center of the time range defined by this TimeHash in unix seconds
        /// </summary>
        public double Center { get; }

        /// <summary>
        /// 1/2 of the time range in unix seconds
        /// </summary>
        public double Error { get; }

        /// <summary>
        /// The start of the time range defined by this TimeHash in unix seconds.
        /// Calculated by: Start = Center - Error
        /// </summary>
        public double Start => Center - Error;

        /// <summary>
        /// The end of the time range defined by this TimeHash in unix seconds.
        /// Calculated by: End = Center + Error
        /// </summary>
        public double End => Center + Error;

        #endregion

        #region [ Methods ]

        public override string ToString()
        {
            return HashCode;
        }

        public override int GetHashCode()
        {
            return HashCode.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is TimeHash hash)
            {
                return this == hash;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region [ Operators ]

        public static bool operator ==(TimeHash left, TimeHash right)
        {
            return left.HashCode == right.HashCode;
        }

        public static bool operator !=(TimeHash left, TimeHash right)
        {
            return left.HashCode != right.HashCode;
        }

        public static bool operator <(TimeHash left, TimeHash right)
        {
            return left.Center < right.Center;
        }

        public static bool operator <=(TimeHash left, TimeHash right)
        {
            return left.Center <= right.Center;
        }

        public static bool operator >(TimeHash left, TimeHash right)
        {
            return left.Center > right.Center;
        }

        public static bool operator >=(TimeHash left, TimeHash right)
        {
            return left.Center >= right.Center;
        }

        /// <summary>
        /// Finds the nth TimeHash before the given TimeHash (maintaining precision)
        /// </summary>
        /// <param name="hash">The original TimeHash instance</param>
        /// <param name="n">The number of iterations of Before</param>
        /// <returns>The TimeHash n ranges to the left of the original TimeHash</returns>
        public static TimeHash operator <<(TimeHash hash, int n)
        {
            var tmp = hash.HashCode;
            foreach (int i in Enumerable.Range(0, n))
            {
                tmp = Before(tmp);
            }
            return new TimeHash(tmp);
        }

        /// <summary>
        /// Finds the nth TimeHash after the given TimeHash (maintaining precision)
        /// </summary>
        /// <param name="hash">The original TimeHash instance</param>
        /// <param name="n">The number of iterations of After</param>
        /// <returns>The TimeHash n ranges to the right of the original TimeHash</returns>
        public static TimeHash operator >>(TimeHash hash, int n)
        {
            var tmp = hash.HashCode;
            foreach (int i in Enumerable.Range(0, n))
            {
                tmp = After(tmp);
            }
            return new TimeHash(tmp);
        }

        #endregion
    }
}
