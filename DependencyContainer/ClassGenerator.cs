using System;
using System.Collections.Generic;
using System.Reflection;

namespace DependencyContainer
{
    public class ClassGenerator
    {
        private List<Dependency> _dependencies;
        ClassGenerator(List<Dependency> dependencies)
        {
            _dependencies = dependencies;
        }
        
        public object generate(Type typeForGenerate, Dependency dependency){
            if (dependency.isSingleton)
            {
                return Create(typeForGenerate);
            }

            return null;
        }


        private object Create(Type type)
        {
            object result = null;
            List<Type> bannedTypes = new List<Type>();
            bannedTypes.Add(type);
            foreach (var constructorInfo in type.GetConstructors())
            {
                result = CallConstructor(constructorInfo,bannedTypes);
                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        private Type GetLastType(Type type, bool isFirst = true)
        {
            foreach (Dependency dependency in _dependencies)
            {
                if (dependency._valuePair.Key == type)
                    return dependency._valuePair.Value != type ? GetLastType(dependency._valuePair.Value, false) : dependency._valuePair.Value;
            }

            return isFirst ? null : type;
        }
        
        private object CallConstructor(ConstructorInfo constructorInfo, List<Type> bannedTypes)
        {
            ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
            object[] parameters = new object[parameterInfos.Length];
            int i = 0;
            foreach (var parameterInfo in parameterInfos)
            {
                object result = null;
                List<Type> current;
                Type type = GetLastType(parameterInfo.ParameterType);
                Type[] genericsArgs = null;

                if (type == null && parameterInfo.ParameterType.IsGenericType)
                {
                    genericsArgs = parameterInfo.ParameterType.GenericTypeArguments;
                    type = GetLastType(parameterInfo.ParameterType.GetGenericTypeDefinition());
                }
                
                if (type == null || bannedTypes.Contains(type))
                    return null;
                
                if (type.IsGenericTypeDefinition)
                {
                    if(genericsArgs.Length>0)
                    {
                        type = type.MakeGenericType(genericsArgs);
                    }
                    else
                    {
                        return null;
                    }
                }
                foreach (var constrInfo in type.GetConstructors())
                {
                    current = new List<Type>(bannedTypes);
                    current.Add(type);
                    result = CallConstructor(constrInfo, current);
                    if (result != null)
                        break;
                }
                
                if (result == null)
                    return null;

                parameters[i] = result;
                i++;
            }

            return constructorInfo.Invoke(parameters);
        }
    }
}