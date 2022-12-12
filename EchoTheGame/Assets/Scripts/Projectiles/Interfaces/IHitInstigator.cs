using Project.Echo.Projectiles.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Project.Echo.Projectiles.Interfaces
{
    public interface IHitInstigator
    {
        void HitPerformed(HitData hit);
}
}
