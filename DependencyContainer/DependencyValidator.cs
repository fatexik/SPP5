namespace DependencyContainer
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;


    class DependencyValidator
    {
        private IEnumerable<Dependency> dependencies;

        internal DependencyValidator()
        {
        }

        internal bool Validate(DependenciesConfiguration config)
        {
            dependencies = config.Dependencies;
            foreach (Dependency dependency in dependencies)
            {
                if (dependency.KeyValueTypePair.Key.IsGenericTypeDefinition)
                {
                    if (!dependency.KeyValueTypePair.Value.IsGenericTypeDefinition)
                        return false;
                }
                else if (dependency.KeyValueTypePair.Key != dependency.KeyValueTypePair.Value &&
                         !dependency.KeyValueTypePair.Key.IsAssignableFrom(dependency.KeyValueTypePair.Value))
                    return false;

                if (dependency.KeyValueTypePair.Value.IsAbstract)
                    return false;
            }

            foreach (Dependency dependency in dependencies)
            {
                if (!dependency.KeyValueTypePair.Key.IsGenericTypeDefinition)
                {
                    bool fl = false;
                    Type typeForCreate = GetLastType(dependency.KeyValueTypePair.Value) ??
                                         dependency.KeyValueTypePair.Value;
                    List<Type> bannedTypes = new List<Type>();
                    bannedTypes.Add(typeForCreate);
                    foreach (ConstructorInfo constructorInfo in typeForCreate.GetConstructors())
                    {
                        fl = CheckConstructor(constructorInfo, bannedTypes);
                        if (fl)
                            break;
                    }

                    if (!fl)
                        return false;
                }
            }

            return true;
        }

        private Type GetLastType(Type type, bool isFirst = true)
        {
            foreach (Dependency dependency in dependencies)
            {
                if (dependency.KeyValueTypePair.Key == type)
                    return dependency.KeyValueTypePair.Value != type
                        ? GetLastType(dependency.KeyValueTypePair.Value, false)
                        : dependency.KeyValueTypePair.Value;
            }

            return isFirst ? null : type;
        }

        private bool CheckConstructor(ConstructorInfo constructor, List<Type> bannedTypes)
        {
            foreach (var parameterInfo in constructor.GetParameters())
            {
                bool fl = false;
                List<Type> curr;
                Type type = GetLastType(parameterInfo.ParameterType);
                Type[] genericArgs = null;

                if (type == null && parameterInfo.ParameterType.IsGenericType)
                {
                    genericArgs = parameterInfo.ParameterType.GenericTypeArguments;
                    type = GetLastType(parameterInfo.ParameterType.GetGenericTypeDefinition());
                }

                if (type == null || bannedTypes.Contains(type))
                    return false;

                if (type.IsGenericTypeDefinition)
                {
                    try
                    {
                        type = type.MakeGenericType(genericArgs);
                    }
                    catch
                    {
                        return false;
                    }
                }


                foreach (ConstructorInfo constructorInfo in type.GetConstructors())
                {
                    curr = new List<Type>(bannedTypes);
                    curr.Add(type);
                    fl = CheckConstructor(constructorInfo, curr);
                    if (fl)
                        break;
                }

                if (!fl)
                    return false;
            }

            return true;
        }
    }
}
