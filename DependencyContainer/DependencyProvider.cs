using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyContainer
{
    public class DependencyProvider
    {
        private DependencyValidator validator;
        private List<Dependency> dependencies;
        private Dictionary<Dependency, object> singletons;

        public DependencyProvider(DependenciesConfiguration config)
        {
            singletons = new Dictionary<Dependency, object>();
            dependencies = new List<Dependency>(config.Dependencies);
            validator = new DependencyValidator();
            if (!validator.Validate(config))
                throw new ArgumentException("Not correct configuration");
        }

        public T Resolve<T>()
            where T : class
        {
            if (typeof(T).IsGenericTypeDefinition)
                return null;

            foreach (Dependency dependency in dependencies)
            {
                if (dependency.declaration == typeof(T))
                {
                    return (T) Generate(
                        DependencyValidator.getFromDependency(dependency.value, dependencies) ?? dependency.value,
                        dependency);
                }
            }

            if (typeof(T).IsGenericType)
            {
                foreach (Dependency dependency in dependencies)
                {
                    if (dependency.declaration == typeof(T).GetGenericTypeDefinition())
                    {
                        try
                        {
                            Type generic =
                                (DependencyValidator.getFromDependency(dependency.declaration, dependencies) ??
                                 dependency.value).MakeGenericType(
                                    typeof(T).GenericTypeArguments);
                            return (T) Generate(generic,
                                new Dependency(
                                    typeof(T),
                                    dependency.value.MakeGenericType(typeof(T).GenericTypeArguments),
                                    dependency._flagsContainer.getSingletoneFlague()));
                        }
                        catch
                        {
                            throw new ArgumentException();
                        }
                    }
                }
            }

            return null;
        }

        public IEnumerable<T> ResolveAll<T>()
            where T : class
        {
            if (typeof(T).IsGenericTypeDefinition)
                return null;

            List<T> result = new List<T>();
            foreach (Dependency dependency in dependencies)
            {
                if (dependency.declaration == typeof(T))
                {
                    result.Add((T) Generate(
                        DependencyValidator.getFromDependency(dependency.value, dependencies) ?? dependency.value,
                        dependency));
                }
            }

            if (typeof(T).IsGenericType)
            {
                foreach (Dependency dependency in dependencies)
                {
                    if (dependency.declaration == typeof(T).GetGenericTypeDefinition())
                    {
                        try
                        {
                            Type generic =
                                (DependencyValidator.getFromDependency(dependency.value, dependencies) ?? dependency.value)
                                .MakeGenericType(
                                    typeof(T).GenericTypeArguments);
                            result.Add((T) Generate(generic,
                                new Dependency(typeof(T),
                                    dependency.value.MakeGenericType(typeof(T).GenericTypeArguments),
                                    dependency._flagsContainer.getSingletoneFlague())));
                        }
                        catch
                        {
                            throw new ArgumentException();
                        }
                    }
                }
            }

            return result;
        }

        private object Generate(Type typeForGeneration, Dependency dependency)
        {
            if (!dependency._flagsContainer.getSingletoneFlague())
            {
                return Create(typeForGeneration);
            }

            if (dependency._flagsContainer.getSingletoneFlague())
            {
                object result;


                if (singletons.Keys.ToList()
                    .Exists(x => x.declaration == dependency.declaration && x.value == dependency.value))
                {
                    singletons.TryGetValue(dependency, out result);
                }
                else
                {
                    result = Create(typeForGeneration);
                    singletons.Add(dependency, result);
                }


                return result;
            }

            return null;
        }

        private object Create(Type typeForCreation)
        {
            object result = null;
            List<Type> bannedTypes = new List<Type>();
            bannedTypes.Add(typeForCreation);
            foreach (ConstructorInfo constructorInfo in typeForCreation.GetConstructors())
            {
                result = CallConstructor(constructorInfo, bannedTypes);
                if (result != null)
                    break;
            }

            return result;
        }

        private object CallConstructor(ConstructorInfo constructor, List<Type> bannedTypes)
        {
            ParameterInfo[] parametersInfo = constructor.GetParameters();
            object[] parameters = new object[parametersInfo.Length];
            int i = 0;
            foreach (var parameterInfo in parametersInfo)
            {
                object result = null;
                List<Type> curr = new List<Type>(bannedTypes);
                Type type = DependencyValidator.getFromDependency(parameterInfo.ParameterType, dependencies);
                Type[] genericArgs = null;

                if (type == null && parameterInfo.ParameterType.IsGenericType)
                {
                    genericArgs = parameterInfo.ParameterType.GenericTypeArguments;
                    type = DependencyValidator.getFromDependency(parameterInfo.ParameterType.GetGenericTypeDefinition(),
                        dependencies);
                }

                if (type == null || bannedTypes.Contains(type))
                    return null;

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
                    result = CallConstructor(constructorInfo, curr);
                    if (result != null)
                        break;
                }

                if (result == null)
                    return null;

                parameters[i] = result;
                i++;
            }

            return constructor.Invoke(parameters);
        }
    }
}