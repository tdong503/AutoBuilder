namespace AutoBuilder.AutoGenerator.Generators
{
    public class DateTimeGenerator
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            return context.Random.GetDate();
        }
    }
}