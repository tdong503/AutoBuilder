namespace AutoBuilder.AutoGenerator.Generators
{
    public class NullableGenerator<T>
        : IAutoGenerator
        where T : struct
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Generate<T>();
        }
    }
}