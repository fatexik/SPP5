using System;
using System.Collections.Generic;

namespace DependencyContainer
{
    public class DependencyProvider
    {
        private List<Dependency> allDependencies;
        private ClassGenerator _classGenerator;

        public DependencyProvider(DependencyConfig config)
        {
            allDependencies = new List<Dependency>(config._dependencies);
            _classGenerator = new ClassGenerator(allDependencies);
        }


        private Type GetLastType(Type type, bool isFirst = true)
        {
            foreach (Dependency dependency in allDependencies)
            {
                if (dependency._valuePair.Key == type)
                    return dependency._valuePair.Value == type
                        ? dependency._valuePair.Value
                        : GetLastType(dependency._valuePair.Value, false);
            }

            return isFirst ? null : type;
        }

        public T resolve<T>()
            where T : class
        {
            foreach (Dependency dependency in allDependencies)
            {
                if (dependency._valuePair.Key == typeof(T))
                {
                    Type usedType = GetLastType(dependency._valuePair.Value) ?? dependency._valuePair.Value;
                    return (T) _classGenerator.generate(usedType, dependency);
                }
            }

            if (typeof(T).IsGenericType)
            {
                foreach (Dependency dependency in allDependencies)
                {
                    if (dependency._valuePair.Key == typeof(T).GetGenericTypeDefinition())
                    {
                        Type usedType = GetLastType(dependency._valuePair.Value) ?? dependency._valuePair.Value;
                        Type generic =usedType.MakeGenericType(typeof(T).GenericTypeArguments);
                        try
                        {
                            return (T) _classGenerator.generate(generic,
                                new Dependency(new KeyValuePair<Type, Type>(typeof(T),
                                        dependency._valuePair.Value.MakeGenericType(typeof(T).GenericTypeArguments)),
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

        public IEnumerable<T> resolveAll<T>()
            where T : class
        {
            if (typeof(T).IsGenericTypeDefinition)
                return null;

            List<T> result = new List<T>();
            foreach (Dependency dependency in allDependencies)
            {
                if (dependency._valuePair.Key == typeof(T))
                {
                    result.Add((T) _classGenerator.generate(
                        GetLastType(dependency._valuePair.Value) ?? dependency._valuePair.Value, dependency));
                }
            }

            if (typeof(T).IsGenericType)
            {
                foreach (Dependency dependency in allDependencies)
                {
                    if (dependency._valuePair.Key == typeof(T).GetGenericTypeDefinition())
                    {
                        try
                        {
                            Type generic =
                                (GetLastType(dependency._valuePair.Value) ?? dependency._valuePair.Value)
                                .MakeGenericType(
                                    typeof(T).GenericTypeArguments);
                            result.Add((T) _classGenerator.generate(generic,
                                new Dependency(
                                    new KeyValuePair<Type, Type>(typeof(T),
                                        dependency._valuePair.Value.MakeGenericType(typeof(T).GenericTypeArguments)),
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
    }
}