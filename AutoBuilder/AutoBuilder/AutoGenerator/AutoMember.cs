using System;
using System.Reflection;

namespace AutoBuilder.AutoGenerator
{
    internal sealed class AutoMember
    {
        internal AutoMember(MemberInfo memberInfo)
        {
            Name = memberInfo.Name;

            // Extract the required member info
            if (ReflectionHelper.IsField(memberInfo))
            {
                var fieldInfo = memberInfo as FieldInfo;

                Type = fieldInfo.FieldType;
                IsReadOnly = !fieldInfo.IsPrivate && fieldInfo.IsInitOnly;
                Getter = fieldInfo.GetValue;
                Setter = fieldInfo.SetValue;
            }
            else if (ReflectionHelper.IsProperty(memberInfo))
            {
                var propertyInfo = memberInfo as PropertyInfo;

                Type = propertyInfo.PropertyType;
                IsReadOnly = !propertyInfo.CanWrite;
                Getter = obj => propertyInfo.GetValue(obj, Array.Empty<object>());
                Setter = (obj, value) => propertyInfo.SetValue(obj, value, Array.Empty<object>());
            }
        }

        internal string Name { get; }
        internal Type Type { get; }
        internal bool IsReadOnly { get; }
        internal Func<object, object> Getter { get; }
        internal Action<object, object> Setter { get; }
    }
}