using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public class SimpleAutoDestroy : MonoBehaviour
    {
        public float Delay = 0;

        private void Start()
        {
            if (Delay > 0)
                DestroySelf(Delay);
        }

        public void DestroySelf(float delay)
        {
            transform.SetParent(null);
            Destroy(gameObject, delay);
        }
        public void DestroySelf_WO_Parent(float delay)
        {
            Destroy(gameObject, delay);
        }
        IEnumerator Routine(float delay)
        {
            yield return new WaitForSeconds(delay);
            transform.SetParent(null);
            Destroy(gameObject);
        }
    }

}