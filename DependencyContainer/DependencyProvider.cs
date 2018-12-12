using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencyContainer;

namespace DependencyContainer
{
    public class DependencyProvider
    {
        private DependencyValidator validator;
        private List<Dependency> dependencies;
        private Dictionary<KeyValuePair<Type, Type>, object> singletons;
        private object locker;

        public DependencyProvider(DependenciesConfiguration config)
        {
            locker = new object();
            singletons = new Dictionary<KeyValuePair<Type, Type>, object>();
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
                if (dependency.KeyValueTypePair.Key == typeof(T))
                {
                    return (T) Generate(GetIerarhyTop(dependency.KeyValueTypePair.Value) ?? dependency.KeyValueTypePair.Value, dependency);
                }
            }

            if (typeof(T).IsGenericType)
            {
                foreach (Dependency dependency in dependencies)
                {
                    if (dependency.KeyValueTypePair.Key == typeof(T).GetGenericTypeDefinition())
                    {
                        try
                        {
                            Type generic =
                                (GetIerarhyTop(dependency.KeyValueTypePair.Value) ?? dependency.KeyValueTypePair.Value).MakeGenericType(
                                    typeof(T).GenericTypeArguments);
                            return (T) Generate(generic,
                                new Dependency(
                                    new KeyValuePair<Type, Type>(typeof(T),
                                        dependency.KeyValueTypePair.Value.MakeGenericType(typeof(T).GenericTypeArguments)),
                                    dependency.isSingleton));
                        }
                        catch
                        {
                            return null;
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
                if (dependency.KeyValueTypePair.Key == typeof(T))
                {
                    result.Add((T) Generate(GetIerarhyTop(dependency.KeyValueTypePair.Value) ?? dependency.KeyValueTypePair.Value, dependency));
                }
            }

            if (typeof(T).IsGenericType)
            {
                foreach (Dependency dependency in dependencies)
                {
                    if (dependency.KeyValueTypePair.Key == typeof(T).GetGenericTypeDefinition())
                    {
                        try
                        {
                            Type generic =
                                (GetIerarhyTop(dependency.KeyValueTypePair.Value) ?? dependency.KeyValueTypePair.Value).MakeGenericType(
                                    typeof(T).GenericTypeArguments);
                            result.Add((T) Generate(generic,
                                new Dependency(
                                    new KeyValuePair<Type, Type>(typeof(T),
                                        dependency.KeyValueTypePair.Value.MakeGenericType(typeof(T).GenericTypeArguments)),
                                    dependency.isSingleton)));
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }

            return result;
        }

        private object Generate(Type typeForGeneration, Dependency dependency)
        {
            if (!dependency.isSingleton)
            {
                return Create(typeForGeneration);
            }
            if (dependency.isSingleton)
            {
                object result;

                lock (locker)
                {
                    if (singletons.Keys.ToList()
                        .Exists(x => x.Key == dependency.KeyValueTypePair.Key && x.Value == dependency.KeyValueTypePair.Value))
                    {
                        singletons.TryGetValue(dependency.KeyValueTypePair, out result);
                    }
                    else
                    {
                        result = Create(typeForGeneration);
                        singletons.Add(dependency.KeyValueTypePair, result);
                    }
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

        private Type GetIerarhyTop(Type type, bool isFirst = true)
        {
            foreach (Dependency dependency in dependencies)
            {
                if (dependency.KeyValueTypePair.Key == type)
                    return dependency.KeyValueTypePair.Value != type
                        ? GetIerarhyTop(dependency.KeyValueTypePair.Value, false)
                        : dependency.KeyValueTypePair.Value;
            }

            return isFirst ? null : type;
        }

        private object CallConstructor(ConstructorInfo constructor, List<Type> bannedTypes)
        {
            ParameterInfo[] parametersInfo = constructor.GetParameters();
            object[] parameters = new object[parametersInfo.Length];
            int i = 0;
            foreach (var parameterInfo in parametersInfo)
            {
                object result = null;
                List<Type> curr;
                Type type = GetIerarhyTop(parameterInfo.ParameterType);
                Type[] genericArgs = null;

                if (type == null && parameterInfo.ParameterType.IsGenericType)
                {
                    genericArgs = parameterInfo.ParameterType.GenericTypeArguments;
                    type = GetIerarhyTop(parameterInfo.ParameterType.GetGenericTypeDefinition());
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
                        return null;
                    }
                }

                foreach (ConstructorInfo constructorInfo in type.GetConstructors())
                {
                    curr = new List<Type>(bannedTypes);
                    curr.Add(type);
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