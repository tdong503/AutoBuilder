namespace AutoBuilder.AutoGenerator.Generators
{
    public class StringGenerator
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.GetString2(10);
        }
    }
}