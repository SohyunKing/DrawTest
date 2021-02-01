using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace DrawTest.Class
{
    public class DrawProvider<T> : RandomProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawProvider{T}"/> class.
        /// </summary>
        public DrawProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawProvider{T}"/> class with a <see cref="RandomNumberGenerator"/>.
        /// </summary>
        /// <param name="randomNumberGenerator">The <see cref="RandomNumberGenerator"/>.</param>
        public DrawProvider(RandomNumberGenerator randomNumberGenerator)
            : base(randomNumberGenerator)
        {
        }

        /// <summary>
        /// Shuffles elements of the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The elements list.</param>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        public virtual void Shuffle(IList<T> list)
        {
            Shuffle(list, null);
        }

        /// <summary>
        /// Shuffles elements of the <paramref name="list"/> with specified indexes of seed elements.
        /// </summary>
        /// <param name="list">The element list.</param>
        /// <param name="seedIndexes">Indexes of seed elements, the seed element's index cannot be changed. (Invalid seed indexes will be ignored)</param>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        public virtual void Shuffle(IList<T> list, ISet<int> seedIndexes)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var count = list.Count;
            if (count == 0)
            {
                return;
            }

            if (seedIndexes != null && seedIndexes.Count > 0)
            {
                // indexes of elements which are not seeds
                var normalIndexes = new List<int>(count);
                for (int i = 0; i < count; ++i)
                {
                    if (!seedIndexes.Contains(i))
                    {
                        normalIndexes.Add(i);
                    }
                }

                // shuffle the normal elements
                var normalIndexesCount = normalIndexes.Count;
                var randomIndexes = new int[normalIndexesCount];
                Next(randomIndexes, normalIndexesCount);
                var j = 0;
                foreach (var normalIndex in normalIndexes)
                {
                    var swapIndex = normalIndexes[randomIndexes[j++]];
                    Swap(list, normalIndex, swapIndex);
                }
            }
            else
            {
                var randomIndexes = new int[count];
                Next(randomIndexes, count);
                for (int i = 0; i < count; ++i)
                {
                    var swapIndex = randomIndexes[i];
                    Swap(list, i, swapIndex);
                }
            }
        }

        /// <summary>
        /// Swaps two elements of a <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The elements list.</param>
        /// <param name="index1">The first element index.</param>
        /// <param name="index2">The second element index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index1"/> or <paramref name="index2"/> is less than zero 
        /// or greater than the count of <paramref name="list"/> minus 1.
        /// </exception>
        public virtual void Swap(IList<T> list, int index1, int index2)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var count = list.Count;
            if (index1 < 0 || index1 >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index1));
            }

            if (index2 < 0 || index2 >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index2));
            }

            if (index1 != index2)
            {
                var value = list[index1];
                list[index1] = list[index2];
                list[index2] = value;
            }
        }

        /// <summary>
        /// Draws out a collection of elements from a <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The elements list.</param>
        /// <returns>A collection of elements that are drawn out.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        public virtual IEnumerable<T> Draw(IList<T> list)
        {
            return Draw(list, null);
        }

        /// <summary>
        /// Draws out a collection of elements from a <paramref name="list"/> with specified indexes of seed elements.
        /// </summary>
        /// <param name="list">The elements list.</param>
        /// <param name="seedIndexes">Indexes of seed elements, the seed element's index cannot be changed. (Invalid seed indexes will be ignored)</param>
        /// <returns>A collection of elements that are drawn out.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        public virtual IEnumerable<T> Draw(IList<T> list, ISet<int> seedIndexes)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var count = list.Count;
            if (count == 0)
            {
                yield break;
            }

            List<int> availableIndexes;
            if (seedIndexes != null && seedIndexes.Count > 0)
            {
                availableIndexes = new List<int>(count - seedIndexes.Count);
                for (int i = 0; i < count; ++i)
                {
                    if (!seedIndexes.Contains(i))
                    {
                        availableIndexes.Add(i);
                    }
                }

                for (int i = 0; i < count; ++i)
                {
                    if (seedIndexes.Contains(i))
                    {
                        yield return list[i];
                    }
                    else
                    {
                        var index = Next(availableIndexes.Count);
                        var resultIndex = availableIndexes[index];
                        availableIndexes.RemoveAt(index);
                        yield return list[resultIndex];
                    }
                }
            }
            else
            {
                availableIndexes = new List<int>(count);
                for (int i = 0; i < count; ++i)
                {
                    availableIndexes.Add(i);
                }

                for (int i = 0; i < count; ++i)
                {
                    var index = Next(availableIndexes.Count);
                    var resultIndex = availableIndexes[index];
                    availableIndexes.RemoveAt(index);
                    yield return list[resultIndex];
                }
            }
        }
    }
}
