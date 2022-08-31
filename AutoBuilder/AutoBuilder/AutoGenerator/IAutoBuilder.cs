using System.Collections.Generic;

namespace AutoBuilder.AutoGenerator
{
    public interface IAutoBuilder<out T>
    {
        public List<string> SkipMembers { get; }
        public string ParentMemberName { get; set; }
        public T Generate();
    }
}