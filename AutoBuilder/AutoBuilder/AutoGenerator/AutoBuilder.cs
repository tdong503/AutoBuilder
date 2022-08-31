using System.Collections.Generic;

namespace AutoBuilder.AutoGenerator
{
    public class AutoBuilder<T> : IAutoBuilder<T>
    {
        public List<string> SkipMembers { get; }
        public string ParentMemberName { get; set; }

        public AutoBuilder()
        {
            SkipMembers = new List<string>();
        }

        public T Generate()
        {
            var context = new AutoGenerateContext
            {
                SkipMembers = SkipMembers
            };

            var generator = AutoGeneratorFactory.GetGenerator(typeof(T));
            return (T)generator.Generate(context);
        }
    }
}