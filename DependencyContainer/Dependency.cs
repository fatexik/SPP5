using System;
using System.Collections.Generic;

namespace DependencyContainer
{
    public class Dependency
    {
        internal KeyValuePair<Type, Type> KeyValueTypePair;
        internal bool isSingleton;

        internal Dependency(KeyValuePair<Type, Type> keyValueTypePair, bool isSingleton)
        {
            KeyValueTypePair = keyValueTypePair;
            this.isSingleton = isSingleton;
        }
    }
}