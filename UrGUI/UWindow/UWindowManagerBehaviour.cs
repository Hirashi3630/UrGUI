using System;
using UnityEngine;

namespace UrGUI.UWindow
{
    internal class UWindowManagerBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            throw new NotImplementedException();
        }

        private void Start()
        {
            throw new NotImplementedException();
        }

        private void OnEnable()
        {
            // enable manager drawing 
        }

        private void OnDisable()
        {
            // disable manager drawing 
        }

        // private void OnDestroy()
        // {
        //     UWindowManager.OnBehaviourDestroy();
        // }

        private void OnGUI()
        {
            UWindowManager.OnBehaviourGUI();
        }
    }
}