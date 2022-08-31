using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace AutoBuilder.AutoGenerator
{
    public class Randomizer
    {
        private static readonly Random Seed = new Random();

        private static readonly Lazy<object> Locker =
            new Lazy<object>(() => new object(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Constructor that uses the global static `<see cref="Seed"/>.
        /// Changing the global static seed after this constructor runs
        /// will have no effect. A new randomizer is needed to capture a new
        /// global seed.
        /// </summary>
        public Randomizer()
        {
            this.localSeed = Seed;
        }

        /// <summary>
        /// Constructor that uses <see cref="localSeed"/> parameter as a seed.
        /// Completely ignores the global static <see cref="Seed"/>.
        /// </summary>
        public Randomizer(int localSeed)
        {
            this.localSeed = new Random(localSeed);
        }

        private readonly Random localSeed;

        /// <summary>
        /// Get an int from 0 to max.
        /// </summary>
        /// <param name="max">Upper bound, inclusive.</param>
        public int Number(int max)
        {
            return Number(0, max);
        }

        /// <summary>
        /// Get a random sequence of digits.
        /// </summary>
        /// <param name="count">How many</param>
        /// <param name="minDigit">minimum digit, inclusive</param>
        /// <param name="maxDigit">maximum digit, inclusive</param>
        public int[] Digits(int count, int minDigit = 0, int maxDigit = 9)
        {
            if (maxDigit > 9 || maxDigit < 0)
                throw new ArgumentException("max digit can't be lager than 9 or smaller than 0", nameof(maxDigit));
            if (minDigit > 9 || minDigit < 0)
                throw new ArgumentException("min digit can't be lager than 9 or smaller than 0", nameof(minDigit));

            var digits = new int[count];
            for (var i = 0; i < count; i++)
            {
                digits[i] = Number(min: minDigit, max: maxDigit);
            }

            return digits;
        }

        /// <summary>
        /// Get an int from min to max.
        /// </summary>
        /// <param name="min">Lower bound, inclusive</param>
        /// <param name="max">Upper bound, inclusive</param>
        public int Number(int min = 0, int max = 1)
        {
            //lock any seed access, for thread safety.
            lock (Locker.Value)
            {
                // Adjust the range as needed to make max inclusive. The Random.Next function uses exclusive upper bounds.

                // If max can be extended by 1, just do that.
                if (max < int.MaxValue) return localSeed.Next(min, max + 1);

                // If max is exactly int.MaxValue, then check if min can be used to push the range out by one the other way.
                // If so, then we can simply add one to the result to put it back in the correct range.
                if (min > int.MinValue) return 1 + localSeed.Next(min - 1, max);

                // If we hit this line, then min is int.MinValue and max is int.MaxValue, which mean the caller wants a
                // number from a range spanning all possible values of int. The Random class only supports exclusive
                // upper bounds, period, and the upper bound must be specified as an int, so the best we can get in a
                // single call is a value in the range (int.MinValue, int.MaxValue - 1). Instead, what we do is get two
                // samples, each of which has just under 31 bits of entropy, and use 16 bits from each to assemble a
                // single 16-bit number.
                int sample1 = localSeed.Next();
                int sample2 = localSeed.Next();

                int topHalf = (sample1 >> 8) & 0xFFFF;
                int bottomHalf = (sample2 >> 8) & 0xFFFF;

                return unchecked((topHalf << 16) | bottomHalf);
            }
        }

        /// <summary>
        /// Returns a random even number. If the range does not contain any even numbers, an <see cref="ArgumentException" /> is thrown.
        /// </summary>
        /// <param name="min">Lower bound, inclusive</param>
        /// <param name="max">Upper bound, inclusive</param>
        /// <exception cref="ArgumentException">Thrown if it is impossible to select an odd number satisfying the specified range.</exception>
        public int Even(int min = 0, int max = 1)
        {
            // Ensure that we have a valid range.
            if (min > max)
                throw new ArgumentException(
                    $"The min/max range is invalid. The minimum value '{min}' is greater than the maximum value '{max}'.",
                    nameof(max));
            if (((min & 1) == 1) && (max - 1 < min))
                throw new ArgumentException("The specified range does not contain any even numbers.", nameof(max));

            // Adjust the range to ensure that we always get the same number of even values as odd values.
            // For example,
            //   if the input is min = 1, max = 3, the new range should be min = 2, max = 3.
            //   if the input is min = 2, max = 3, the range should remain min = 2, max = 3.
            min = (min + 1) & ~1;
            max = max | 1;

            if (min > max)
                return min;

            // Strip off the last bit of a random number to make the number even.
            return Number(min, max) & ~1;
        }

        /// <summary>
        /// Returns a random odd number. If the range does not contain any odd numbers, an <see cref="ArgumentException" /> is thrown.
        /// </summary>
        /// <param name="min">Lower bound, inclusive</param>
        /// <param name="max">Upper bound, inclusive</param>
        /// <exception cref="ArgumentException">Thrown if it is impossible to select an odd number satisfying the specified range.</exception>
        public int Odd(int min = 0, int max = 1)
        {
            // Ensure that we have a valid range.
            if (min > max)
                throw new ArgumentException(
                    $"The min/max range is invalid. The minimum value '{min}' is greater than the maximum value '{max}'.",
                    nameof(max));
            if (((max & 1) == 0) && (min + 1 > max))
                throw new ArgumentException("The specified range does not contain any odd numbers.", nameof(max));

            // Special case where the math below breaks.
            if (max == int.MinValue)
                return int.MinValue | 1;

            // Adjust the range to ensure that we always get the same number of even values as odd values.
            // For example,
            //   if the input is min = 2, max = 4, the new range should be min = 2, max = 3.
            //   if the input is min = 2, max = 3, the range should remain min = 2, max = 3.
            min = min & ~1;
            max = (max - 1) | 1;

            if (min > max)
                return min | 1;

            // Ensure that the last bit is set in a random number to make the number odd.
            return Number(min, max) | 1;
        }


        /// <summary>
        /// Get a random double, between 0.0 and 1.0.
        /// </summary>
        /// <param name="min">Minimum, default 0.0</param>
        /// <param name="max">Maximum, default 1.0</param>
        public double GetDouble(double min = 0.0d, double max = 1.0d)
        {
            //lock any seed access, for thread safety.
            lock (Locker.Value)
            {
                if (min == 0.0d && max == 1.0d)
                {
                    //use default implementation
                    return localSeed.NextDouble();
                }

                return localSeed.NextDouble() * (max - min) + min;
            }
        }

        /// <summary>
        /// Get a random decimal, between 0.0 and 1.0.
        /// </summary>
        /// <param name="min">Minimum, default 0.0</param>
        /// <param name="max">Maximum, default 1.0</param>
        public decimal GetDecimal(decimal min = 0.0m, decimal max = 1.0m)
        {
            return Convert.ToDecimal(GetDouble()) * (max - min) + min;
        }

        /// <summary>
        /// Get a random float, between 0.0 and 1.0.
        /// </summary>
        /// <param name="min">Minimum, default 0.0</param>
        /// <param name="max">Maximum, default 1.0</param>
        public float GetFloat(float min = 0.0f, float max = 1.0f)
        {
            return Convert.ToSingle(GetDouble() * (max - min) + min);
        }

        /// <summary>
        /// Generate a random byte between 0 and 255.
        /// </summary>
        /// <param name="min">Min value, default 0</param>
        /// <param name="max">Max value, default 255</param>
        public byte Byte(byte min = byte.MinValue, byte max = byte.MaxValue)
        {
            return Convert.ToByte(Number(min, max));
        }

        /// <summary>
        /// Get a random sequence of bytes.
        /// </summary>
        /// <param name="count">The size of the byte array</param>
        public byte[] Bytes(int count)
        {
            var arr = new byte[count];
            lock (Locker.Value)
            {
                localSeed.NextBytes(arr);
            }

            return arr;
        }

        /// <summary>
        /// Generate a random sbyte between -128 and 127.
        /// </summary>
        /// <param name="min">Min value, default -128</param>
        /// <param name="max">Max value, default 127</param>
        public sbyte SByte(sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
        {
            return Convert.ToSByte(Number(min, max));
        }

        /// <summary>
        /// Generate a random int between MinValue and MaxValue.
        /// </summary>
        /// <param name="min">Min value, default MinValue</param>
        /// <param name="max">Max value, default MaxValue</param>
        public int GetInt(int min = int.MinValue, int max = int.MaxValue)
        {
            return this.Number(min, max);
        }

        /// <summary>
        /// Generate a random uint between MinValue and MaxValue.
        /// </summary>
        /// <param name="min">Min value, default MinValue</param>
        /// <param name="max">Max value, default MaxValue</param>
        public uint GetUInt(uint min = uint.MinValue, uint max = uint.MaxValue)
        {
            return Convert.ToUInt32(GetDouble() * (max - min) + min);
        }

        /// <summary>
        /// Generate a random ulong between -128 and 127.
        /// </summary>
        /// <param name="min">Min value, default -128</param>
        /// <param name="max">Max value, default 127</param>
        public ulong GetULong(ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
        {
            return Convert.ToUInt64(GetDouble() * (max - min) + min);
        }

        /// <summary>
        /// Generate a random long between MinValue and MaxValue.
        /// </summary>
        /// <param name="min">Min value, default MinValue</param>
        /// <param name="max">Max value, default MaxValue</param>
        public long GetLong(long min = long.MinValue, long max = long.MaxValue)
        {
            var range = (decimal)max - min; //use more bits?
            return Convert.ToInt64((decimal)GetDouble() * range + min);
        }

        /// <summary>
        /// Generate a random short between MinValue and MaxValue.
        /// </summary>
        /// <param name="min">Min value, default MinValue</param>
        /// <param name="max">Max value, default MaxValue</param>
        public short GetShort(short min = short.MinValue, short max = short.MaxValue)
        {
            return Convert.ToInt16(GetDouble() * (max - min) + min);
        }

        /// <summary>
        /// Generate a random ushort between MinValue and MaxValue.
        /// </summary>
        /// <param name="min">Min value, default MinValue</param>
        /// <param name="max">Max value, default MaxValue</param>
        public ushort GetUShort(ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
        {
            return Convert.ToUInt16(GetDouble() * (max - min) + min);
        }

        /// <summary>
        /// Generate a random char between MinValue and MaxValue.
        /// </summary>
        /// <param name="min">Min value, default MinValue</param>
        /// <param name="max">Max value, default MaxValue</param>
        public char GetChar(char min = char.MinValue, char max = char.MaxValue)
        {
            return Convert.ToChar(Number(min, max));
        }

        /// <summary>
        /// Generate a random chars between MinValue and MaxValue.
        /// </summary>
        /// <param name="min">Min value, default MinValue</param>
        /// <param name="max">Max value, default MaxValue</param>
        /// <param name="count">The length of chars to return</param>
        public char[] Chars(char min = char.MinValue, char max = char.MaxValue, int count = 5)
        {
            var arr = new char[count];
            for (var i = 0; i < count; i++)
                arr[i] = GetChar(min, max);
            return arr;
        }

        /// <summary>
        /// Get a string of characters of a specific length.
        /// Uses <seealso cref="Chars"/>.
        /// Note: This method can return ill-formed UTF16 Unicode strings with unpaired surrogates.
        /// Use <seealso cref="Utf16String"/> for technically valid Unicode.
        /// </summary>
        /// <param name="length">The exact length of the result string. If null, a random length is chosen between 40 and 80.</param>
        /// <param name="minChar">Min character value, default char.MinValue</param>
        /// <param name="maxChar">Max character value, default char.MaxValue</param>
        public string GetString(int? length = null, char minChar = char.MinValue, char maxChar = char.MaxValue)
        {
            var l = length ?? this.Number(40, 80);

            return new string(Chars(minChar, maxChar, l));
        }

        /// <summary>
        /// Get a string of characters between <paramref name="minLength" /> and <paramref name="maxLength"/>.
        /// Uses <seealso cref="Chars"/>.
        /// Note: This method can return ill-formed UTF16 Unicode strings with unpaired surrogates.
        /// Use <seealso cref="Utf16String"/> for technically valid Unicode.
        /// </summary>
        /// <param name="minLength">Lower-bound string length. Inclusive.</param>
        /// <param name="maxLength">Upper-bound string length. Inclusive.</param>
        /// <param name="minChar">Min character value, default char.MinValue</param>
        /// <param name="maxChar">Max character value, default char.MaxValue</param>
        public string GetString(int minLength, int maxLength, char minChar = char.MinValue,
            char maxChar = char.MaxValue)
        {
            var length = this.Number(minLength, maxLength);
            return GetString(length, minChar, maxChar);
        }

        /// <summary>
        /// Get a string of characters with a specific length drawing characters from <paramref name="chars"/>.
        /// The returned string may contain repeating characters from the <paramref name="chars"/> string.
        /// </summary>
        /// <param name="length">The length of the string to return.</param>
        /// <param name="chars">The pool of characters to draw from. The returned string may contain repeat characters from the pool.</param>
        public string GetString2(int length, string chars = "abcdefghijklmnopqrstuvwxyz")
        {
            var target = new char[length];

            for (var i = 0; i < length; i++)
            {
                if (chars == null) continue;
                var idx = Number(0, chars.Length - 1);
                target[i] = chars[idx];
            }

            return new string(target);
        }

        /// <summary>
        /// Get a string of characters with a specific length drawing characters from <paramref name="chars"/>.
        /// The returned string may contain repeating characters from the <paramref name="chars"/> string.
        /// </summary>
        /// <param name="minLength">The minimum length of the string to return.</param>
        /// <param name="maxLength">The maximum length of the string to return.</param>
        /// <param name="chars">The pool of characters to draw from. The returned string may contain repeat characters from the pool.</param>
        public string GetString2(int minLength, int maxLength, string chars = "abcdefghijklmnopqrstuvwxyz")
        {
            var length = this.Number(minLength, maxLength);
            return GetString2(length, chars);
        }

        /// <summary>
        /// Get a random boolean.
        /// </summary>
        public bool Bool()
        {
            return Number() == 0;
        }

        /// <summary>
        /// Get a random boolean.
        /// </summary>
        /// <param name="weight">The probability of true. Ranges from 0 to 1.</param>
        public bool Bool(float weight)
        {
            return GetFloat() < weight;
        }

        /// <summary>
        /// Get a random array element.
        /// </summary>
        public T ArrayElement<T>([NotNull] T[] array)
        {
            var r = Number(max: array.Length - 1);
            return array[r];
        }

        /// <summary>
        /// Get a random array element.
        /// </summary>
        public string ArrayElement(Array array)
        {
            array ??= new[] { "a", "b", "c" };

            var r = Number(max: array.Length - 1);

            return array.GetValue(r).ToString();
        }

        /// <summary>
        /// Get a random subset of an array.
        /// </summary>
        /// <param name="array">The source of items to pick from.</param>
        /// <param name="count">The number of elements to pick; otherwise, a random amount is picked.</param>
        public T[] ArrayElements<T>([NotNull] T[] array, int? count = null)
        {
            if (count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count is null)
                count = Number(0, array.Length - 1);

            return Shuffle(array).Take(count.Value).ToArray();
        }

        /// <summary>
        /// Get a random list item.
        /// </summary>
        public T ListItem<T>(List<T> list)
        {
            return ListItem(list as IList<T>);
        }

        /// <summary>
        /// Get a random list item.
        /// </summary>
        public T ListItem<T>([NotNull] IList<T> list)
        {
            var r = Number(max: list.Count - 1);
            return list[r];
        }

        /// <summary>
        /// Get a random subset of a List.
        /// </summary>
        /// <param name="items">The source of items to pick from.</param>
        /// <param name="count">The number of items to pick; otherwise, a random amount is picked.</param>
        public List<T> ListItems<T>([NotNull] IList<T> items, int? count = null)
        {
            if (count > items.Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count is null)
                count = Number(0, items.Count - 1);

            return Shuffle(items).Take(count.Value).ToList();
        }

        /// <summary>
        /// Get a random subset of a List.
        /// </summary>
        /// <param name="items">The source of items to pick from.</param>
        /// <param name="count">The number of items to pick; otherwise, a random amount is picked.</param>
        public IList<T> ListItems<T>(List<T> items, int? count = null)
        {
            return ListItems(items as IList<T>, count);
        }

        /// <summary>
        /// Get a random collection item.
        /// </summary>
        public T CollectionItem<T>([NotNull] ICollection<T> collection)
        {
            var r = Number(max: collection.Count - 1);
            return collection.Skip(r).First();
        }

        /// <summary>
        /// Replaces symbols with numbers.
        /// IE: ### -> 283
        /// </summary>
        /// <param name="format">The string format</param>
        /// <param name="symbol">The symbol to search for in format that will be replaced with a number</param>
        public string ReplaceNumbers(string format, char symbol = '#')
        {
            return ReplaceSymbols(format, symbol, () => Convert.ToChar('0' + Number(9)));
        }

        /// <summary>
        /// Replaces each character instance in a string.
        /// Func is called each time a symbol is encountered.
        /// </summary>
        /// <param name="format">The string with symbols to replace.</param>
        /// <param name="symbol">The symbol to search for in the string.</param>
        /// <param name="func">The function that produces a character for replacement. Invoked each time the replacement symbol is encountered.</param>
        public static string ReplaceSymbols(string format, char symbol, Func<char> func)
        {
            var chars = format.Select(c => c == symbol ? func() : c).ToArray();
            return new string(chars);
        }

        /// <summary>
        /// Replaces symbols with numbers and letters. # = number, ? = letter, * = number or letter.
        /// IE: ###???* -> 283QED4. Letters are uppercase.
        /// </summary>
        public string Replace(string format)
        {
            var chars = format.Select(c =>
                {
                    if (c == '*')
                    {
                        c = Bool() ? '#' : '?';
                    }

                    if (c == '#')
                    {
                        return Convert.ToChar('0' + Number(9));
                    }

                    if (c == '?')
                    {
                        return Convert.ToChar('A' + Number(25));
                    }

                    return c;
                })
                .ToArray();

            return new string(chars);
        }

        /// <summary>
        /// Clamps the length of a string between min and max characters.
        /// If the string is below the minimum, the string is appended with random characters up to the minimum length.
        /// If the string is over the maximum, the string is truncated at maximum characters; additionally, if the result string ends with
        /// whitespace, it is replaced with a random characters.
        /// </summary>
        public string ClampString([NotNull] string str, int? min = null, int? max = null)
        {
            if (max != null && str.Length > max)
            {
                str = str.Substring(0, max.Value).Trim();
            }

            if (min != null && min > str.Length)
            {
                var missingChars = min - str.Length;
                var fillerChars = this.Replace(string.Empty.PadRight(missingChars.Value, '?'));
                return str + fillerChars;
            }

            return str;
        }

        /// <summary>
        /// Picks a random enum value in T:Enum.
        /// </summary>
        /// <typeparam name="T">Must be an Enum</typeparam>
        /// <param name="exclude">Exclude enum values from being returned</param>
        public T Enum<T>(params T[] exclude) where T : struct, Enum
        {
            var e = typeof(T);
            if (!e.IsEnum)
                throw new ArgumentException("When calling Enum<T>() with no parameters T must be an enum.");

            var selection = System.Enum.GetNames(e);

            if (exclude.Any())
            {
                var excluded = exclude.Select(ex => System.Enum.GetName(e, ex));
                selection = selection.Except(excluded).ToArray();
            }

            if (!selection.Any())
            {
                throw new ArgumentException("There are no values after exclusion to choose from.");
            }

            var val = this.ArrayElement(selection);

            if (!System.Enum.TryParse(val, out T picked))
            {
                throw new FormatException();
            }

            return picked;
        }

        /// <summary>
        /// Picks a random subset of enum values in T:Enum.
        /// </summary>
        /// <typeparam name="T">The enum.</typeparam>
        /// <param name="count">The number of enums to pick.</param>
        /// <param name="exclude">Any enums that should be excluded before picking.</param>
        public T[] EnumValues<T>(int? count = null, params T[] exclude) where T : Enum
        {
            T[] enums;
            if (exclude.Length > 0)
            {
                enums = System.Enum.GetValues(typeof(T))
                    .OfType<T>()
                    .Except(exclude)
                    .ToArray();
            }
            else
            {
                enums = System.Enum.GetValues(typeof(T))
                    .OfType<T>()
                    .Except(exclude)
                    .ToArray();
            }

            if (count > enums.Length || count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count,
                    $"The {nameof(count)} parameter is {count} and the calculated set of enums has a length of {enums.Length}. It is impossible to pick {count} enums from a list of {enums.Length}.");
            }

            return this.ArrayElements(enums, count);
        }
        
        /// <summary>
        /// Shuffles an IEnumerable source.
        /// </summary>
        public IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
        {
            List<T> buffer = source.ToList();
            for (var i = 0; i < buffer.Count; i++)
            {
                int j;
                //lock any seed access, for thread safety.
                lock (Locker.Value)
                {
                    j = this.localSeed.Next(i, buffer.Count);
                }

                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        /// <summary>
        /// Get a random GUID.
        /// </summary>
        public Guid GetGuid()
        {
            var guidBytes = this.Bytes(16);
            return new Guid(guidBytes);
        }

        /// <summary>
        /// Get a random GUID. Alias for Randomizer.Guid().
        /// </summary>
        public Guid Uuid()
        {
            var guidBytes = this.Bytes(16);
            return new Guid(guidBytes);
        }

        private static readonly char[] AlphaChars =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
            'u', 'v', 'w', 'x', 'y', 'z'
        };

        /// <summary>
        /// Returns a random set of alpha numeric characters 0-9, a-z.
        /// </summary>
        public string AlphaNumeric(int length)
        {
            var sb = new StringBuilder();
            return Enumerable.Range(1, length)
                .Aggregate(sb, (b, i) => b.Append(ArrayElement(AlphaChars)), b => b.ToString());
        }

        private static readonly char[] HexChars =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f'
        };

        /// <summary>
        /// Generates a random hexadecimal string.
        /// </summary>
        public string Hexadecimal(int length = 1, string prefix = "0x")
        {
            var sb = new StringBuilder();
            return Enumerable.Range(1, length)
                .Aggregate(sb, (b, i) => b.Append(ArrayElement(HexChars)), b => $"{prefix}{b}");
        }

        /// <summary>
        /// Get a random <see cref="DateTime"/> within the last few days.
        /// </summary>
        /// <param name="days">Number of days to go back.</param>
        /// <param name="refDate">The date to start calculations. Default is <see cref="DateTime.Now"/>.</param>
        public DateTime GetDate(int days = 1, DateTime? refDate = null)
        {
            var maxDate = refDate ?? SystemClock();

            var minDate = days == 0 ? SystemClock().Date : maxDate.AddDays(-days);

            var totalTimeSpanTicks = (maxDate - minDate).Ticks;

            var partTimeSpan = RandomTimeSpanFromTicks(totalTimeSpanTicks);

            return maxDate - partTimeSpan;
        }

        private TimeSpan RandomTimeSpanFromTicks(long totalTimeSpanTicks)
        {
            //find % of the timespan
            var partTimeSpanTicks = GetDouble() * totalTimeSpanTicks;
            return TimeSpan.FromTicks(Convert.ToInt64(partTimeSpanTicks));
        }

        private static readonly Func<DateTime> SystemClock = () => DateTime.Now;
    }
}