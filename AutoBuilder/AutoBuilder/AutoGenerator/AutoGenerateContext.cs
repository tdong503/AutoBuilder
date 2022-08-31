using System;
using System.Collections.Generic;

namespace AutoBuilder.AutoGenerator
{
    public sealed class AutoGenerateContext
    {
        public Type GenerateType { get; internal set; }

        public IEnumerable<string> SkipMembers { get; internal set; }

        public static IBinder Binder => new Binder();

        private Randomizer randomizer;

        public Randomizer Random
        {
            get => randomizer ?? (Random = new Randomizer());
            set => randomizer = value;
        }

        public T Generate<T>()
        {
            // Set the generate type for the current request
            GenerateType = typeof(T);

            // Get the type generator and return a value
            var generator = AutoGeneratorFactory.GetGenerator(GenerateType);
            return (T)generator.Generate(this);
        }

        public List<TType> GenerateUniqueMany<TType>(int? count = 1)
        {
            var items = new List<TType>();

            GenerateMany(count, items);

            return items;
        }

        public List<TType> GenerateMany<TType>(int? count = 1)
        {
            var items = new List<TType>();

            GenerateMany(count, items);

            return items;
        }

        private void GenerateMany<T>(int? count, List<T> items, Func<T> generate = null)
        {
            generate ??= Generate<T>;

            // Generate a list of items
            var required = count - items.Count;

            for (var index = 0; index < required; index++)
            {
                var item = generate.Invoke();

                // Ensure the generated value is not null (which means the type couldn't be generated)
                if (item != null)
                {
                    items.Add(item);
                }
            }
        }
    }
}