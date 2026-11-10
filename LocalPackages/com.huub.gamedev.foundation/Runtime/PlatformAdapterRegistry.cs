using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDev.Foundation
{
    public static class PlatformAdapterRegistry
    {
        private static readonly List<Func<IPlatformAdapter>> Factories = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Factories.Clear();
        public static void Register(Func<IPlatformAdapter> factory) => Factories.Add(factory);
        public static void Install(PlatformServices services)
        {
            foreach (var factory in Factories) services.Register(factory());
        }
    }
}
