using System;
using System.Collections.Generic;

namespace DependencyContainer
{
    public class DependencyConfig
    {
        private List<Dependency> _dependencies { get; }

        DependencyConfig()
        {
            _dependencies = new List<Dependency>();
        }

        public void registrateClass<T1, T2>()
        {
            Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(typeof(T1), typeof(T2)), false);
            if (!_dependencies.Exists(x =>
                x._valuePair.Key.Equals(dependency._valuePair.Key) &&
                x._valuePair.Value.Equals(dependency._valuePair.Value)))
            {
                _dependencies.Add(dependency);
            }
        }

        public void registrateSingletoneClass<T1, T2>()
        {
            Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(typeof(T1), typeof(T2)), true);
            if (!_dependencies.Exists(depend =>
                depend._valuePair.Key.Equals(dependency._valuePair.Key) &&
                depend._valuePair.Value.Equals(dependency._valuePair.Value)))
            {
                _dependencies.Add(dependency);
            }
        }
    }
}