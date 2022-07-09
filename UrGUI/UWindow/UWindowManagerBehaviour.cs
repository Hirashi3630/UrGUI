using System;
using UnityEngine;
using static UrGUI.Utils.Logger;

namespace UrGUI.UWindow
{
    internal class UWindowManagerBehaviour : MonoBehaviour
    {
        // private bool _isDestroying = false;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            
        }

        private void OnEnable()
        {
            // enable manager drawing 
            
        }

        // private void OnDisable()
        // {
        //     disable manager drawing
        //     if (!_isDestroying)
        //         war("Don't disable UrGUI ManagerBehaviour manually! If you want to stop/pause drawing use Manager.IsDrawing property!");
        // }

        // private void OnDestroy()
        // {
        //     _isDestroying = true;
        //     UWindowManager.OnBehaviourDestroy();
        // }

        private void OnGUI()
        {
            UWindowManager.OnBehaviourGUI();
        }
    }
}