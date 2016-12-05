using System;

namespace Guflow
{
    public static class Ensure
    {
        public static void NotNull(object argument, string argumentName)
        {
            That(argument != null, () => new ArgumentNullException(argumentName));

        }
        public static void NotNull<T>(object argument, Func<T> exception) where T : Exception
        {
            That(argument != null, exception);
        }
        public static void NotNullAndEmpty(string argument, string argumentName)
        {
            That(!string.IsNullOrEmpty(argument), () => new ArgumentException(argumentName));
        }
        public static void NotNullAndEmpty<T>(string argument, Func<T> exception) where T : Exception
        {
            That(!string.IsNullOrEmpty(argument), exception);
        }
        public static void That<T>(bool condition, params object[] argumentNames) where T : Exception
        {
            if (!condition)
            {
                throw Activator.CreateInstance(typeof(T), argumentNames) as T;
            }
        }
        public static void That<T>(bool condition, Func<T> exception) where T : Exception
        {
            if (!condition)
            {
                throw exception();
            }
        }
        public static void ToThrowWhen<T>(bool condition, Func<T> exception) where T : Exception
        {
            if (condition)
            {
                throw exception();
            }
        }
    }
}