namespace AutoBuilder.AutoGenerator.Generators
{
    public class TypeGenerator<T>
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            // Note that all instances are converted to object to cater for boxing and struct population
            // When setting a value via reflection on a struct a copy is made
            // This means the changes are applied to a different instance to the one created here
            object instance = AutoGenerateContext.Binder.CreateInstance<T>(context);

            // Populate the generated instance
            AutoGenerateContext.Binder.PopulateInstance<T>(instance, context);

            return instance;
        }
    }
}