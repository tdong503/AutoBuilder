namespace AutoBuilder.AutoGenerator.Generators
{
    public interface IAutoGenerator
    {
        object Generate(AutoGenerateContext context);
    }
}