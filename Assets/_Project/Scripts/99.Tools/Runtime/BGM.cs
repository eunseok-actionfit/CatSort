using System;
using UnityEngine;


namespace _99.Tools.Runtime
{
    public class BGM : MonoBehaviour
    {

        private void Awake()
        {
            
            DontDestroyOnLoad(this.gameObject);
        }
    }
}


