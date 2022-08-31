namespace AutoBuilder.AutoGenerator.Generators
{
    public class EnumerableGenerator<T>
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.GenerateMany<T>();
        }
    }
}