namespace AutoBuilder.AutoGenerator.Generators
{
    public class FloatGenerator
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.GetFloat();
        }
    }
}