using System;
using System.Collections.Generic;

namespace DependencyContainer
{
    public class DependenciesConfiguration
    {
        internal List<Dependency> Dependencies { get; }

        public DependenciesConfiguration()
        {
            Dependencies = new List<Dependency>();
        }

        public void Registrate<T1, T2>()
            where T1 : class
            where T2 : class
        {
            KeyValuePair<Type, Type> newKeyValuePair = new KeyValuePair<Type, Type>(typeof(T1), typeof(T2));
            Dependency dependency = new Dependency(newKeyValuePair, false);
            if (!Dependencies.Exists(depend => depend.KeyValueTypePair.Key == depend.KeyValueTypePair.Key
                                               && depend.KeyValueTypePair.Value == dependency.KeyValueTypePair.Value))
                Dependencies.Add(dependency);
        }

        public void RegistrateAsSingleton<T1, T2>()
            where T1 : class
            where T2 : class
        {
            KeyValuePair<Type, Type> newKeyValuePair = new KeyValuePair<Type, Type>(typeof(T1), typeof(T2));
            Dependency dependency = new Dependency(newKeyValuePair, true);
            if (!Dependencies.Exists(depend => depend.KeyValueTypePair.Key == dependency.KeyValueTypePair.Key
                                               && depend.KeyValueTypePair.Value == dependency.KeyValueTypePair.Value))
                Dependencies.Add(dependency);
        }

        public void Registrate(Type type1, Type type2)
        {
            if ((type1.IsClass || type1.IsInterface) && (type2.IsClass || type2.IsInterface))
            {
                KeyValuePair<Type, Type> newKeyValuePair = new KeyValuePair<Type, Type>(type1, type2);
                Dependency dependency = new Dependency(newKeyValuePair, true);
                if (!Dependencies.Exists(x =>
                    x.KeyValueTypePair.Key == dependency.KeyValueTypePair.Key &&
                    x.KeyValueTypePair.Value == dependency.KeyValueTypePair.Value))
                    Dependencies.Add(dependency);
            }
        }

        public void RegistrateAsSingleton(Type type1, Type type2)
        {
            if ((type1.IsClass || type1.IsInterface) && (type2.IsClass || type2.IsInterface))
            {
                KeyValuePair<Type, Type> newKeyValuePair = new KeyValuePair<Type, Type>(type1, type2);
                Dependency dependency = new Dependency(newKeyValuePair, true);
                if (!Dependencies.Exists(x =>
                    x.KeyValueTypePair.Key == dependency.KeyValueTypePair.Key &&
                    x.KeyValueTypePair.Value == dependency.KeyValueTypePair.Value))
                    Dependencies.Add(dependency);
            }
        }
    }
}