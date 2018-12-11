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
            _classGenerator = new ClassGenerator(config._dependencies);
        }

        public T resolve<T>()
        {
            foreach (var dependency in allDependencies)
            {
                if (dependency._valuePair.Key == typeof(T))
                {
                    
                }           
            }
        }
    }
}