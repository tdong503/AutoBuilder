namespace AutoBuilder.AutoGenerator.Generators
{
    public class BoolGenerator : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.Bool();
        }
    }
}