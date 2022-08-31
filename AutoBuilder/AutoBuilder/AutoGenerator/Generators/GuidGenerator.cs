namespace AutoBuilder.AutoGenerator.Generators
{
    public class GuidGenerator
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.Uuid();
        }
    }
}