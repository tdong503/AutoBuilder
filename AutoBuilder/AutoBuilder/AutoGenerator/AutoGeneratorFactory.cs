using System;
using System.Collections.Generic;
using System.Linq;
using AutoBuilder.AutoGenerator.Generators;

namespace AutoBuilder.AutoGenerator
{
    internal static class AutoGeneratorFactory
    {
        private static readonly IDictionary<Type, IAutoGenerator> Generators = new Dictionary<Type, IAutoGenerator>
        {
            { typeof(bool), new BoolGenerator() },
            { typeof(DateTime), new DateTimeGenerator() },
            { typeof(decimal), new DecimalGenerator() },
            { typeof(double), new DoubleGenerator() },
            { typeof(float), new FloatGenerator() },
            { typeof(Guid), new GuidGenerator() },
            { typeof(int), new IntGenerator() },
            { typeof(string), new StringGenerator() },
        };

        internal static IAutoGenerator GetGenerator(Type type)
        {
            var generator = ResolveGenerator(type);
            return generator;
        }

        private static IAutoGenerator ResolveGenerator(Type type)
        {
            // Need check if the type is an in/out parameter and adjusted accordingly
            if (type.IsByRef)
            {
                type = type.GetElementType();
            }

            // Do some type -> generator mapping
            if (type.IsArray)
            {
                type = type.GetElementType();
                return CreateGenericGenerator(typeof(ArrayGenerator<>), type);
            }

            if (ReflectionHelper.IsEnum(type))
            {
                return CreateGenericGenerator(typeof(EnumGenerator<>), type);
            }

            if (ReflectionHelper.IsNullable(type))
            {
                type = ReflectionHelper.GetGenericArguments(type).Single();
                return CreateGenericGenerator(typeof(NullableGenerator<>), type);
            }

            var genericCollectionType = ReflectionHelper.GetGenericCollectionType(type);

            if (genericCollectionType != null)
            {
                // For generic types we need to interrogate the inner types
                var generics = ReflectionHelper.GetGenericArguments(genericCollectionType);

                if (ReflectionHelper.IsReadOnlyDictionary(genericCollectionType))
                {
                    var keyType = generics.ElementAt(0);
                    var valueType = generics.ElementAt(1);

                    return CreateGenericGenerator(typeof(ReadOnlyDictionaryGenerator<,>), keyType, valueType);
                }

                if (ReflectionHelper.IsDictionary(genericCollectionType))
                {
                    return CreateDictionaryGenerator(generics);
                }

                if (ReflectionHelper.IsList(genericCollectionType))
                {
                    var elementType = generics.Single();
                    return CreateGenericGenerator(typeof(ListGenerator<>), elementType);
                }

                if (ReflectionHelper.IsCollection(genericCollectionType))
                {
                    var elementType = generics.Single();
                    return CreateGenericGenerator(typeof(ListGenerator<>), elementType);
                }

                if (ReflectionHelper.IsEnumerable(genericCollectionType))
                {
                    // Not a full list type, we can't fake it if it's anything other than
                    // the actual IEnumerable<T> interface itself.
                    if (type == genericCollectionType)
                    {
                        var elementType = generics.Single();
                        return CreateGenericGenerator(typeof(EnumerableGenerator<>), elementType);
                    }
                }
            }

            // Resolve the generator from the type
            if (Generators.ContainsKey(type))
            {
                return Generators[type];
            }

            return CreateGenericGenerator(typeof(TypeGenerator<>), type);
        }

        private static IAutoGenerator CreateDictionaryGenerator(IEnumerable<Type> generics)
        {
            var keyType = generics.ElementAt(0);
            var valueType = generics.ElementAt(1);

            return CreateGenericGenerator(typeof(DictionaryGenerator<,>), keyType, valueType);
        }

        private static IAutoGenerator CreateGenericGenerator(Type generatorType, params Type[] genericTypes)
        {
            var type = generatorType.MakeGenericType(genericTypes);
            return (IAutoGenerator)Activator.CreateInstance(type);
        }
    }
}