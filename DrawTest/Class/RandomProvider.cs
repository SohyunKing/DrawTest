using System;
using System.Security.Cryptography;

namespace DrawTest.Class
{
    public class RandomProvider
    {
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private const double MaxInt = int.MaxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomProvider"/> class.
        /// </summary>
        public RandomProvider() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomProvider"/> class with specified <see cref="RandomNumberGenerator"/>.
        /// </summary>
        /// <param name="randomNumberGenerator">The <see cref="RandomNumberGenerator"/>.</param>
        public RandomProvider(RandomNumberGenerator randomNumberGenerator)
        {
            _randomNumberGenerator = randomNumberGenerator ?? new RNGCryptoServiceProvider();
        }

        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0.</returns>
        public virtual int Next()
        {
            var values = new int[1];
            Next(values);
            return values[0];
        }

        /// <summary>
        /// Returns a non-negative random integer that is less than the specified <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number to be generated.
        /// <paramref name="maxValue"/> must be greater than or equal to 0.
        /// </param>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public virtual int Next(int maxValue)
        {
            var values = new int[1];
            Next(values, maxValue);
            return values[0];
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number returned.
        /// <paramref name="maxValue"/> must greater than or equal to <paramref name="minValue"/>.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> 
        /// and less than <paramref name="maxValue"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public virtual int Next(int minValue, int maxValue)
        {
            var values = new int[1];
            Next(values, minValue, maxValue);
            return values[0];
        }

        /// <summary>
        /// Fills an array of integers with random numbers that are greater or equal to 0.
        /// </summary>
        /// <param name="values">The array of numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        public virtual void Next(int[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            InternalNext(values, v => Math.Abs(v));
        }

        /// <summary>
        /// Fills an array of integers with random numbers which are greater or equal to 0, and less than <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="values">The array of numbers.</param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random numbers to be generated.
        /// <paramref name="maxValue"/> must be greater than or equal to 0.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public virtual void Next(int[] values, int maxValue)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue),
                    $"{nameof(maxValue)} is less than 0");
            }

            if (maxValue == 0)
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    values[i] = 0;
                }
            }
            else
            {
                InternalNext(values, v => Math.Abs(v % maxValue));
            }
        }

        /// <summary>
        /// Fills an array of integers with random numbers which are greater or equal than <paramref name="minValue"/>
        /// and less than <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="values">The array of numbers.</param>
        /// <param name="minValue">The inclusive lower bound of the random numbers to be generated.</param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random numbers to be generated.
        /// <paramref name="maxValue"/> must greater than or equal to <paramref name="minValue"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than <paramref name="minValue"/>.</exception>
        public virtual void Next(int[] values, int minValue, int maxValue)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue),
                    $"{nameof(minValue)} is greater than {nameof(maxValue)}");
            }

            if (minValue == maxValue)
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    values[i] = minValue;
                }
            }
            else
            {
                var range = maxValue - minValue;
                InternalNext(values, v => (int)(Math.Abs(v) / MaxInt * range + minValue));
            }
        }

        /// <summary>
        /// Returns a random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer.</returns>
        public virtual int NextRaw()
        {
            var values = new int[1];
            InternalNext(values);
            return values[0];
        }

        /// <summary>
        /// Fills an array of integers with random numbers.
        /// </summary>
        /// <param name="values">The array of numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        public virtual void NextRaw(int[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            InternalNext(values);
        }

        /// <summary>
        /// Fills an array of integers with random numbers which are less than <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="values">The array of numbers.</param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random numbers to be generated.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        public virtual void NextRaw(int[] values, int maxValue)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            InternalNext(values, v => v % maxValue);
        }

        /// <summary>
        /// Fills an array of bytes with random values.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        public virtual void NextBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            _randomNumberGenerator.GetBytes(bytes);
        }

        /// <summary>
        /// Returns a random double value that is greater or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A double value that is greater or equal to 0.0, and less than 1.0.</returns>
        public virtual double NextDouble()
        {
            var values = new int[1];
            InternalNext(values);
            return Math.Abs(values[0]) / MaxInt;
        }

        /// <summary>
        /// Fills an array of integers with random numbers with specified conversion method.
        /// </summary>
        /// <param name="values">The array of numbers.</param>
        /// <param name="conversion">The method that will be used to convert the numbers when filling them to the array.</param>
        protected void InternalNext(int[] values, Func<int, int> conversion = null)
        {
            var length = values.Length;
            var bytes = new byte[length * 4];
            _randomNumberGenerator.GetBytes(bytes);

            if (conversion == null)
            {
                for (int i = 0; i < length; ++i)
                {
                    values[i] = BitConverter.ToInt32(bytes, i + 4);
                }
            }
            else
            {
                for (int i = 0, startIndex = 0; i < length; ++i, startIndex += 4)
                {
                    values[i] = conversion(BitConverter.ToInt32(bytes, startIndex));
                }
            }
        }
    }
}
