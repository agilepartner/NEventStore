// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Reflection;

namespace System
{
    internal static class ExceptionBuilder
    {
        public static Exception CreateDiscoveryException(string messageFormat, params string[] arguments)
        {
            // DiscoveryError (Dev10:602872): This should go through the discovery error reporting when 
            // we add a way to report discovery errors properly.
            return new InvalidOperationException(Format(messageFormat, arguments));
        }

        public static ArgumentException CreateContainsNullElement(string parameterName)
        {
            Assumes.NotNull(parameterName);

            string message = Format(Strings.Argument_NullElement, parameterName);

            return new ArgumentException(message, parameterName);
        }

        public static ObjectDisposedException CreateObjectDisposed(object instance)
        {
            Assumes.NotNull(instance);

            return new ObjectDisposedException(instance.GetType().ToString());
        }

        public static NotImplementedException CreateNotOverriddenByDerived(string memberName)
        {
            Assumes.NotNullOrEmpty(memberName);

            string message = Format(Strings.NotImplemented_NotOverriddenByDerived, memberName);

            return new NotImplementedException(message);
        }

        private static string Format(string format, params string[] arguments)
        {
            return String.Format(CultureInfo.CurrentCulture, format, arguments);
        }
    }
}
