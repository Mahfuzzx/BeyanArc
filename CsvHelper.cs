using System.Globalization;
using System.Reflection;
using System.Text;

namespace BeyanArc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class CsvHelper
    {
        public static List<T> readCsv<T>(string filePath, bool hasHeader = true, char separator = ';') where T : new()
        {
            var result = new List<T>();
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Dosya bulunamadı: " + filePath);
                return result;
            }

            var lines = File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length == 0) return result;

            string[] headers;
            int startLine = 0;

            if (hasHeader)
            {
                headers = lines[0].Split(separator).Select(h => h.Trim()).ToArray();
                startLine = 1;
            }
            else
            {
                headers = typeof(T).GetProperties().Select(p => p.Name).ToArray();
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = startLine; i < lines.Length; i++)
            {
                var values = lines[i].Split(separator).Select(v => v.Trim()).ToArray();
                var obj = new T();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase));
                    if (prop != null && prop.CanWrite)
                    {
                        try
                        {
                            object value;
                            if (string.IsNullOrEmpty(values[j]))
                            {
                                value = GetDefault(prop.PropertyType);
                            }
                            else
                            {
                                value = Convert.ChangeType(values[j], prop.PropertyType, CultureInfo.InvariantCulture);
                            }

                            prop.SetValue(obj, value);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Satır {i + 1}, Sütun {headers[j]}: {ex.Message}");
                        }
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }
    }
}