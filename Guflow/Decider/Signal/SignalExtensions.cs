using System.Linq;

namespace Guflow.Decider
{
    internal static class SignalExtensions
    {
        public static string[] CombinedValidEventNames(this string name, params string[] names)
            => new[] { name }.Concat(names).Where(n => !string.IsNullOrEmpty(n)).ToArray();
    }
}