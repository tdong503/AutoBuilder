using System;
using System.Collections.Generic;

namespace AutoBuilder.AutoGenerator.Generators
{
    public class DictionaryGenerator<TKey, TValue>
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            // Create an instance of a dictionary (public and non-public)
            IDictionary<TKey, TValue> items;
            try
            {
                items = (IDictionary<TKey, TValue>)Activator.CreateInstance(context.GenerateType, true);
            }
#pragma warning disable CA1031
            catch (Exception)
#pragma warning restore CA1031
            {
                items = new Dictionary<TKey, TValue>();
            }

            // Get a list of keys
            var keys = context.GenerateUniqueMany<TKey>();

            foreach (var key in keys)
            {
                // Get a matching value for the current key and add to the dictionary
                var value = context.Generate<TValue>();

                if (value != null)
                {
                    items.Add(key, value);
                }
            }

            return items;
        }
    }
}