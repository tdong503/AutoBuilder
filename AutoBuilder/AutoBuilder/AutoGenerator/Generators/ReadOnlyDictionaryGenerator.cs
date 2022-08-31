using System;
using System.Collections.Generic;

namespace AutoBuilder.AutoGenerator.Generators
{
    public class ReadOnlyDictionaryGenerator<TKey, TValue>
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            IAutoGenerator generator = new DictionaryGenerator<TKey, TValue>();

            var generateType = context.GenerateType;

            if (ReflectionHelper.IsInterface(generateType))
                generateType = typeof(Dictionary<TKey, TValue>);

            // Generate a standard dictionary and create the read only dictionary
            var items = generator.Generate(context) as IDictionary<TKey, TValue>;

            return Activator.CreateInstance(generateType, items);
        }
    }
}