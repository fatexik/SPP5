using System;
using System.Security.Cryptography;

namespace DependencyContainer
{
    public class Dependency
    {
        internal Type declaration;
        internal Type value;
        public FlagsContainer _flagsContainer;


        internal Dependency(Type declaration, Type value, bool isSingleton)
        {
            this.declaration = declaration;
            this.value = value;
            _flagsContainer = new FlagsContainer();
            _flagsContainer.setSingletoneFlague(isSingleton);
        }

        public override int GetHashCode()
        {
            return 31 * declaration.GetHashCode() + value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Dependency);
        }

        public bool Equals(Dependency dependency)
        {
            return dependency != null && dependency.declaration.Equals(this.declaration) &&
                   dependency._flagsContainer.getSingletoneFlague() == this._flagsContainer.getSingletoneFlague() &&
                   dependency.value.Equals(this.value);
        }
    }
}