using System;
using System.Collections.Generic;

namespace AutoBuilder.AutoGenerator.Generators
{
    public class ListGenerator<T>
        : IAutoGenerator
    {
        object IAutoGenerator.Generate(AutoGenerateContext context)
        {
            IList<T> list;

            try
            {
                list = (IList<T>)Activator.CreateInstance(context.GenerateType);
            }
#pragma warning disable CA1031
            catch (Exception)
#pragma warning restore CA1031
            {
                list = new List<T>();
            }

            var items = context.GenerateMany<T>();

            foreach (var item in items)
            {
                list.Add(item);
            }

            return list;
        }
    }
}