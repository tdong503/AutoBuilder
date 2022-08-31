namespace AutoBuilder.AutoGenerator.Generators
{
    public class DoubleGenerator
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.GetDouble();
        }
    }
}