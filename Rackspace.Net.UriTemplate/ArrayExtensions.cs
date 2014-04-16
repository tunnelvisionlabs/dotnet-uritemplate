// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Net
{
    using System;

    /// <summary>
    /// Contains extension methods which support the use of arrays in the Portable Class Library build.
    /// </summary>
    internal static class ArrayExtensions
    {
#if PORTABLE
        /// <summary>
        /// Converts an array of one type to an array of another type.
        /// </summary>
        /// <typeparam name="TInput">The type of the elements of the source array.</typeparam>
        /// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
        /// <param name="array">The one-dimensional, zero-based <see cref="Array"/> to convert to a target type.</param>
        /// <param name="converter">A <see cref="Func{TInput, TOutput}"/> that converts each element from one type to another type.</param>
        /// <returns>An array of the target type containing the converted elements from the source array.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="array"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="converter"/> is <see langword="null"/>.</para>
        /// </exception>
        public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] array, Func<TInput, TOutput> converter)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (converter == null)
                throw new ArgumentNullException("converter");

            TOutput[] result = new TOutput[array.Length];
            for (int i = 0; i < array.Length; i++)
                result[i] = converter(array[i]);

            return result;
        }
#else
        /// <summary>
        /// Converts an array of one type to an array of another type.
        /// </summary>
        /// <typeparam name="TInput">The type of the elements of the source array.</typeparam>
        /// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
        /// <param name="array">The one-dimensional, zero-based <see cref="Array"/> to convert to a target type.</param>
        /// <param name="converter">A <see cref="Converter{TInput, TOutput}"/> that converts each element from one type to another type.</param>
        /// <returns>An array of the target type containing the converted elements from the source array.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="array"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="converter"/> is <see langword="null"/>.</para>
        /// </exception>
        public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] array, Converter<TInput, TOutput> converter)
        {
            return Array.ConvertAll(array, converter);
        }
#endif

        public static int FindIndex<T>(this T[] array, Predicate<T> predicate)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

#if !PORTABLE
            return Array.FindIndex(array, predicate);
#else
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                    return i;
            }

            return -1;
#endif
        }
    }
}
