using System.Collections.Generic;

namespace DependencyContainer
{
    public class FlagsContainer
    {
        private Dictionary<string, bool> _dictionary;
        private string singletone = "singletone";

        public FlagsContainer()
        {
            _dictionary = new Dictionary<string, bool>();
        }

        public void setSingletoneFlague(bool flague)
        {
            _dictionary.Add(singletone,flague);
        }

        public bool getSingletoneFlague()
        {
            bool result;
            _dictionary.TryGetValue(singletone,out result);
            return result;
        }
    }
}