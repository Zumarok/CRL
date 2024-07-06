using UnityEngine;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;

namespace Crux.CRL.Utils
{
    public static class StaticUtils
    {
        public static Color ColorFromHex(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length == 6 || hex.Length == 8)
            {
                var rInt = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                var gInt = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                var bInt = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                var aInt = hex.Length == 8
                    ? int.Parse(hex.Substring(6, 2), NumberStyles.HexNumber)
                    : 255;

                // Convert integer values to floats in the range [0, 1]
                return new Color(rInt / 255f, gInt / 255f, bInt / 255f, aInt / 255f);
            }

            Debug.LogError("Hex to Color string must be in format such as '#FFFFFF' or 'FFFFFFFF'");
            return new Color();
        }

        /// <summary>
        /// Perform a deep copy of the object via serialization.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>A deep copy of the object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            using (var stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
