using System;
using System.Reflection;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Region : Layer
        {
        }
    }
}
