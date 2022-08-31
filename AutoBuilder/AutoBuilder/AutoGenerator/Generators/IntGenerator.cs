namespace AutoBuilder.AutoGenerator.Generators
{
    public class IntGenerator
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.GetInt();
        }
    }
}