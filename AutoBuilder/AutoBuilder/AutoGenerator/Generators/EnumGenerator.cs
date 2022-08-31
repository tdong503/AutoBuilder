using System;

namespace AutoBuilder.AutoGenerator.Generators
{
    public class EnumGenerator<T>
        : IAutoGenerator
        where T: struct, Enum
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.Enum<T>();
        }
    }
}