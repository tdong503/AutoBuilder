namespace AutoBuilder.AutoGenerator.Generators
{
    public class ArrayGenerator<T>
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            var items = context.GenerateMany<T>();
            return items.ToArray();
        }
    }
}