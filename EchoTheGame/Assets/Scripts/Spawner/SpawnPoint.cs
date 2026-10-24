using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Echo.Spawner
{
    public class SpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.6f, 0.91f, 0.89f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, 2f);
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }
}
