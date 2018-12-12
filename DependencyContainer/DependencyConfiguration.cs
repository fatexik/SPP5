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
        {
            Dependency dependency = new Dependency(typeof(T1),typeof(T2), false);
            if (!Dependencies.Exists(depend => depend.declaration == dependency.declaration
                                               && depend.value == dependency.value))
                Dependencies.Add(dependency);
        }

        public void RegistrateAsSingleton<T1, T2>()
        {
            Dependency dependency = new Dependency(typeof(T1),typeof(T2), true);
            if (!Dependencies.Exists(depend => depend.declaration == dependency.declaration
                                               && depend.value == dependency.value))
                Dependencies.Add(dependency);
        }

        public void Registrate(Type type1, Type type2)
        {
            if ((type1.IsClass || type1.IsInterface) && (type2.IsClass || type2.IsInterface))
            {
                Dependency dependency = new Dependency(type1,type2, true);
                if (!Dependencies.Exists(depend =>
                    depend.declaration == dependency.declaration &&
                    depend.value == dependency.value))
                    Dependencies.Add(dependency);
            }
        }

        public void RegistrateAsSingleton(Type type1, Type type2)
        {
            if ((type1.IsClass || type1.IsInterface) && (type2.IsClass || type2.IsInterface))
            {
                Dependency dependency = new Dependency(type1,type2, true);
                if (!Dependencies.Exists(depend =>
                    depend.declaration == dependency.declaration &&
                    depend.value == dependency.value))
                    Dependencies.Add(dependency);
            }
        }
    }
}