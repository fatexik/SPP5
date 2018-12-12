namespace DependencyContainer
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;


    class DependencyValidator
    {
        private List<Dependency> dependencies;

        internal DependencyValidator()
        {
        }

        private bool createPossibility(Dependency dependency)
        {
            if (dependency.declaration.IsGenericTypeDefinition && !dependency.value.IsGenericTypeDefinition)
            {
                return false;
            }

            if (!dependency.declaration.IsGenericTypeDefinition && dependency.declaration != dependency.value &&
                !dependency.declaration.IsAssignableFrom(dependency.value))
            {
                return false;
            }

            if (dependency.value.IsAbstract)
            {
                return false;
            }

            return true;
        }

        internal bool Validate(DependenciesConfiguration config)
        {
            dependencies = config.Dependencies;
            foreach (Dependency dependency in dependencies)
            {
                if (!createPossibility(dependency))
                {
                    return false;
                }
            }

            foreach (Dependency dependency in dependencies)
            {
                if (!dependency.declaration.IsGenericTypeDefinition)
                {
                    bool existRightConstructor = false;
                    Type typeForCreate = getFromDependency(dependency.value, dependencies) ??
                                         dependency.value;
                    List<Type> bannedTypes = new List<Type>();
                    bannedTypes.Add(typeForCreate);
                    foreach (ConstructorInfo constructorInfo in typeForCreate.GetConstructors())
                    {
                        existRightConstructor = CheckConstructor(constructorInfo, bannedTypes);
                        if (existRightConstructor) break;
                    }

                    if (!existRightConstructor)
                        return false;
                }
            }

            return true;
        }

        public static Type getFromDependency(Type type, List<Dependency> dependencies)
        {
            foreach (Dependency dependency in dependencies)
            {
                if (dependency.declaration == type)
                    return dependency.value;
            }

            return null ;
        }

        private bool CheckConstructor(ConstructorInfo constructor, List<Type> bannedTypes)
        {
            foreach (var parameterInfo in constructor.GetParameters())
            {
                bool fl = false;
                List<Type> curr;
                Type type = getFromDependency(parameterInfo.ParameterType, dependencies);
                Type[] genericArgs = null;

                if (type == null && parameterInfo.ParameterType.IsGenericType)
                {
                    genericArgs = parameterInfo.ParameterType.GenericTypeArguments;
                    type = getFromDependency(parameterInfo.ParameterType.GetGenericTypeDefinition(), dependencies);
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
                        throw new ArgumentException();
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