using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoBuilder.AutoGenerator.Generators;

namespace AutoBuilder.AutoGenerator
{
    public sealed class Binder : IBinder
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public T CreateInstance<T>(AutoGenerateContext context)
        {
            if (context != null)
            {
                var type = typeof(T);
                var constructor = GetConstructor<T>();

                if (constructor != null)
                {
                    // If a constructor is found generate values for each of the parameters
                    var parameters = (from p in constructor.GetParameters()
                        let g = GetParameterGenerator(p)
                        select g.Generate(context)).ToArray();

                    return (T)constructor.Invoke(parameters);
                }
            }

            return default;
        }

        public void PopulateInstance<T>(object instance, AutoGenerateContext context,
            IEnumerable<MemberInfo> members = null)
        {
            var type = typeof(T);

            // We can only populate non-null instances
            if (instance == null || context == null)
            {
                return;
            }

            // Iterate the members and bind a generated value
            var autoMembers = GetMembersToPopulate(type, members);

            foreach (var member in autoMembers)
            {
                if (member.Type != null)
                {
                    //Check if the member has a skip config or the type has already been generated as a parent
                    //If so skip this generation otherwise track it for use later in the object tree
                    if (context.SkipMembers.Contains($"{type.FullName}.{member.Name}"))
                    {
                        continue;
                    }

                    context.GenerateType = member.Type;

                    // Generate a random value and bind it to the instance
                    var generator = AutoGeneratorFactory.GetGenerator(member.Type);
                    var value = generator.Generate(context);

                    try
                    {
                        if (!member.IsReadOnly)
                        {
                            member.Setter.Invoke(instance, value);
                        }
                        else if (ReflectionHelper.IsDictionary(member.Type))
                        {
                            PopulateDictionary(value, instance, member);
                        }
                        else if (ReflectionHelper.IsCollection(member.Type))
                        {
                            PopulateCollection(value, instance, member);
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception();
                    }
                }
            }
        }

        private static ConstructorInfo GetConstructor<TType>()
        {
            var type = typeof(TType);
            var constructors = type.GetConstructors();

            // For dictionaries and enumerables locate a constructor that is used for populating as well
            if (ReflectionHelper.IsDictionary(type))
            {
                return ResolveTypedConstructor(typeof(IDictionary<,>), constructors);
            }

            if (ReflectionHelper.IsEnumerable(type))
            {
                return ResolveTypedConstructor(typeof(IEnumerable<>), constructors);
            }

            // Attempt to find a default constructor
            // If one is not found, simply use the first in the list
            var defaultConstructor = (from c in constructors
                let p = c.GetParameters()
                where p.Length == 0
                select c).SingleOrDefault();

            return defaultConstructor ?? constructors.FirstOrDefault();
        }

        private static ConstructorInfo ResolveTypedConstructor(Type type, IEnumerable<ConstructorInfo> constructors)
        {
            // Find the first constructor that matches the passed generic definition
            return (from c in constructors
                let p = c.GetParameters()
                where p.Length == 1
                let m = p.Single()
                where ReflectionHelper.IsGenericType(m.ParameterType)
                let d = ReflectionHelper.GetGenericTypeDefinition(m.ParameterType)
                where d == type
                select c).SingleOrDefault();
        }

        private static IAutoGenerator GetParameterGenerator(ParameterInfo parameter)
        {
            return AutoGeneratorFactory.GetGenerator(parameter.ParameterType);
        }

        private void PopulateDictionary(object value, object parent, AutoMember member)
        {
            var instance = member.Getter(parent);
            var argTypes = GetAddMethodArgumentTypes(member.Type);
            var addMethod = GetAddMethod(member.Type, argTypes);

            if (instance != null && addMethod != null && value is IDictionary dictionary)
            {
                foreach (var key in dictionary.Keys)
                {
                    addMethod.Invoke(instance, new[] { key, dictionary[key] });
                }
            }
        }

        private void PopulateCollection(object value, object parent, AutoMember member)
        {
            var instance = member.Getter(parent);
            var argTypes = GetAddMethodArgumentTypes(member.Type);
            var addMethod = GetAddMethod(member.Type, argTypes);

            if (instance != null && addMethod != null && value is ICollection collection)
            {
                foreach (var item in collection)
                {
                    addMethod.Invoke(instance, new[] { item });
                }
            }
        }

        private static Type[] GetAddMethodArgumentTypes(Type type)
        {
            var types = new[] { typeof(object) };

            if (ReflectionHelper.IsGenericType(type))
            {
                var generics = ReflectionHelper.GetGenericArguments(type);
                types = generics.ToArray();
            }

            return types;
        }

        private MethodInfo GetAddMethod(Type type, Type[] argTypes)
        {
            // First try directly on the type
            var method = type.GetMethod("Add", argTypes);

            if (method == null)
            {
                // Then traverse the type interfaces
                return (from i in type.GetInterfaces()
                    let m = GetAddMethod(i, argTypes)
                    where m != null
                    select m).FirstOrDefault();
            }

            return method;
        }


        private static IEnumerable<AutoMember> GetMembersToPopulate(Type type, IEnumerable<MemberInfo> members)
        {
            // If a list of members is provided, no others should be populated
            if (members != null)
            {
                return members.Select(member => new AutoMember(member));
            }

            // Get the baseline members resolved by Bogus
            var autoMembers = (from m in GetMembers(type)
                select new AutoMember(m.Value)).ToList();

            foreach (var member in type.GetMembers(bindingFlags))
            {
                // Then check if any other members can be populated
                var autoMember = new AutoMember(member);

                if (autoMembers.All(baseMember => autoMember.Name != baseMember.Name))
                {
                    // A readonly dictionary or collection member can use the Add() method
                    if (autoMember.IsReadOnly && ReflectionHelper.IsDictionary(autoMember.Type))
                    {
                        autoMembers.Add(autoMember);
                    }
                    else if (autoMember.IsReadOnly && ReflectionHelper.IsCollection(autoMember.Type))
                    {
                        autoMembers.Add(autoMember);
                    }
                }
            }

            return autoMembers;
        }

        private static Dictionary<string, MemberInfo> GetMembers(Type t)
        {
            return t?.GetAllMembers(bindingFlags)
                .Where(m =>
                {
                    if (m.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true)
                        .Any())
                        return false;
                    var propertyInfo = m as PropertyInfo;
                    if ((object)propertyInfo != null && propertyInfo.GetMethod?.IsVirtual == false)
                        return propertyInfo.CanWrite;
                    var fieldInfo = m as FieldInfo;
                    return (object)fieldInfo != null && !fieldInfo.IsPrivate;
                }).GroupBy((Func<MemberInfo, string>)(mi => mi.Name))
                .ToDictionary(
                    k => k.Key,
                    g => g.First());
        }
    }
}