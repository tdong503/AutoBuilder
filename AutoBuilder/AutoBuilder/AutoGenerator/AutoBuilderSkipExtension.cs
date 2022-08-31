using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace AutoBuilder.AutoGenerator
{
    public static class AutoBuilderSkipExtension
    {
        public static IAutoBuilderSkip<TEntity, TProperty> WithSkip<TEntity, TProperty>(
            [NotNull] this IAutoBuilder<TEntity> autoBuilder, Expression<Func<TEntity, TProperty>> member)
        {
            var memberName = GetMemberName(member);
            var type = typeof(TEntity);
            if (!string.IsNullOrWhiteSpace(memberName))
            {
                var path = $"{type.FullName}.{memberName}";
                var existing = autoBuilder.SkipMembers.Any(s => s == path);

                if (!existing)
                {
                    autoBuilder.ParentMemberName = memberName;
                    autoBuilder.SkipMembers.Add(path);
                }
            }

            return new AutoBuilderSkip<TEntity, TProperty>(autoBuilder);
        }

        public static IAutoBuilderSkip<TEntity, TProperty> WithSpecificFiled<TEntity, TPreviousProperty, TProperty>(
            [NotNull] this IAutoBuilderSkip<TEntity, IEnumerable<TPreviousProperty>> autoBuilder,
            Expression<Func<TPreviousProperty, TProperty>> member)
        {
            var memberName = GetMemberName(member);
            var type = typeof(TPreviousProperty);
            var currentSkipMember = $"{type.FullName}.{memberName}";

            var typeParent = typeof(TEntity);
            var parentSkipMember = typeParent.FullName + "." + autoBuilder.ParentMemberName;

            var index = autoBuilder.SkipMembers.FindIndex(s => s == parentSkipMember);
            if (index > -1)
            {
                autoBuilder.SkipMembers[index] = currentSkipMember;
            }

            return new AutoBuilderSkip<TEntity, TProperty>(autoBuilder);
        }

        public static IAutoBuilderSkip<TEntity, TProperty> WithSpecificFiled<TEntity, TPreviousProperty, TProperty>(
            [NotNull] this IAutoBuilderSkip<TEntity, TPreviousProperty> autoBuilder,
            Expression<Func<TPreviousProperty, TProperty>> member)
        {
            var memberName = GetMemberName(member);
            var type = typeof(TPreviousProperty);
            var currentSkipMember = $"{type.FullName}.{memberName}";

            var typeParent = typeof(TEntity);
            var parentSkipMember = typeParent.FullName + "." + autoBuilder.ParentMemberName;

            var index = autoBuilder.SkipMembers.FindIndex(s => s == parentSkipMember);
            if (index > -1)
            {
                autoBuilder.SkipMembers[index] = currentSkipMember;
            }

            return new AutoBuilderSkip<TEntity, TProperty>(autoBuilder);
        }

        private static string GetMemberName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> member)
        {
            if (member != null)
            {
                MemberExpression expression = null;

                if (member.Body is UnaryExpression unary)
                {
                    expression = unary.Operand as MemberExpression;
                }
                else if (member.Body is MemberExpression)
                {
                    expression = member.Body as MemberExpression;
                }

                if (expression != null)
                {
                    var memberInfo = expression.Member;

                    if (ReflectionHelper.IsField(memberInfo) || ReflectionHelper.IsProperty(memberInfo))
                    {
                        return memberInfo.Name;
                    }
                }
            }

            return null;
        }

        private sealed class AutoBuilderSkip<TEntity, TProperty> : IAutoBuilderSkip<TEntity, TProperty>
        {
            private readonly IAutoBuilder<TEntity> autoBuilder;

            public AutoBuilderSkip(IAutoBuilder<TEntity> autoBuilder)
            {
                this.autoBuilder = autoBuilder;
                SkipMembers = autoBuilder.SkipMembers;
                ParentMemberName = autoBuilder.ParentMemberName;
            }

            public List<string> SkipMembers { get; }
            public string ParentMemberName { get; set; }

            public TEntity Generate()
            {
                return autoBuilder.Generate();
            }
        }
    }
}