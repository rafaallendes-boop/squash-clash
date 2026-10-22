using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SquashClash.Core.Config
{
    public sealed class ParamSet
    {
        private readonly Dictionary<string, double> values;
        public IReadOnlyList<KeyValuePair<string, double>> Ordered { get; }
        public string Hash { get; }

        internal ParamSet(IEnumerable<KeyValuePair<string, double>> source)
        {
            values = source.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
            Ordered = Array.AsReadOnly(values.OrderBy(entry => entry.Key, StringComparer.Ordinal).ToArray());
            Hash = ComputeHash(Ordered);
        }

        public double Get(string key) => values[key];
        public bool GetBool(string key)
        {
            var value = Get(key);
            if (value != 0 && value != 1) throw new InvalidOperationException(key + " is not a boolean value.");
            return value == 1;
        }
        public int GetInt(string key)
        {
            var value = Get(key);
            if (value != Math.Truncate(value)) throw new InvalidOperationException(key + " is not an integer value.");
            return checked((int)value);
        }

        public ParamSet With(string key, double value)
        {
            if (!ParamCatalog.TryGet(key, out var def)) throw new ArgumentException("Unknown parameter: " + key, nameof(key));
            var error = def.Error(value);
            if (error != null) throw new ArgumentOutOfRangeException(nameof(value), error);
            var changed = new Dictionary<string, double>(values, StringComparer.Ordinal) { [key] = value };
            return new ParamSet(changed);
        }

        public string HashOf(params string[] prefixes) => ComputeHash(Ordered.Where(entry =>
            prefixes.Any(prefix => entry.Key.StartsWith(prefix, StringComparison.Ordinal))));

        private static string ComputeHash(IEnumerable<KeyValuePair<string, double>> entries)
        {
            var canonical = new StringBuilder();
            foreach (var entry in entries)
                canonical.Append(entry.Key).Append('=').Append(entry.Value.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
                return BitConverter.ToString(hash, 0, 6).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
