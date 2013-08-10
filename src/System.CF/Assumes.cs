// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.Diagnostics;

namespace System
{
    public static partial class Assumes
    {
        [DebuggerStepThrough]
        internal static void NotNull<T>(T value)
            where T : class
        {
            IsTrue(value != null);
        }

        [DebuggerStepThrough]
        internal static void NotNull<T1, T2>(T1 value1, T2 value2)
            where T1 : class
            where T2 : class
        {
            NotNull(value1);
            NotNull(value2);
        }

        [DebuggerStepThrough]
        internal static void NotNull<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            NotNull(value1);
            NotNull(value2);
            NotNull(value3);
        }

        [DebuggerStepThrough]
        internal static void NotNullOrEmpty<T>(T[] values)
        {
            Assumes.NotNull(values);
            Assumes.IsTrue(values.Length > 0);
        }

        [DebuggerStepThrough]
        internal static void NotNullOrEmpty(string value)
        {
            NotNull(value);
            IsTrue(value.Length > 0);
        }

        [DebuggerStepThrough]
        internal static void Null<T>(T value)
            where T : class
        {
            IsTrue(value == null);
        }

        [DebuggerStepThrough]
        internal static void IsFalse(bool condition)
        {
            if (condition)
            {
                Fail(null);
            }
        }

        [DebuggerStepThrough]
        internal static void IsTrue(bool condition)
        {
            if (!condition)
            {
                Fail(null);
            }
        }

        [DebuggerStepThrough]
        internal static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                Fail(message);
            }
        }

        [DebuggerStepThrough]
        internal static void Fail(string message)
        {
            throw new InvalidOperationException(message);
        }

        [DebuggerStepThrough]
        internal static T NotReachable<T>()
        {
            throw new InvalidOperationException("Code path should never be reached!");
        }
    }
}
