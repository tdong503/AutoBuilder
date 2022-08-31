using System.Collections.Generic;
using System.Reflection;

namespace AutoBuilder.AutoGenerator
{
    public interface IBinder
    {
        public T CreateInstance<T>(AutoGenerateContext context);

        public void PopulateInstance<TType>(object instance, AutoGenerateContext context,
            IEnumerable<MemberInfo> members = null);
    }
}