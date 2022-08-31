namespace AutoBuilder.AutoGenerator.Generators
{
    public class DecimalGenerator
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.GetDecimal();
        }
    }
}