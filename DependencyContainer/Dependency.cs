using System;
using System.Collections.Generic;

namespace DependencyContainer
{
    public class Dependency
    {
        public bool isSingleton { get; set; }
        public KeyValuePair<Type, Type> _valuePair;

        public Dependency(KeyValuePair<Type,Type> valuePair, bool isSingleton)
        {
            this.isSingleton = isSingleton;
            _valuePair = valuePair;
        }
    }
}