using System;
using System.Collections;
using System.IO;
using UnityEngine;
using static UrGUI.Utils.Logger;

namespace UrGUI.UWindow
{
    internal class UWindowManagerBehaviour : MonoBehaviour
    {
        // private bool _isDestroying = false;
        public static UWindowManagerBehaviour Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
                
            Instance = this;
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

        public void LoadDefaultSkin()
        {
            StartCoroutine(m_LoadDefaultSkin());
        }
        
        private IEnumerator m_LoadDefaultSkin()
        {
            // get skin asset-bundle from bundled resources
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            byte[] ba = default; // bytes
            int nOfB = 0; // number of read bytes
            using (Stream resFilestream = a.GetManifestResourceStream("UrGUI.Skins.main"))
            {
                if (resFilestream == null)
                {
                    Debug.LogWarning("Couldn't load default skin! Filestream is null");
                    yield break;
                }
                
                ba = new byte[resFilestream.Length];
                nOfB = resFilestream.Read(ba, 0, ba.Length);
            }
            
            if (ba.Length != nOfB)
                Debug.LogError("Stream didn't ready all of available bytes!");
            
            var assetBundleReq = AssetBundle.LoadFromMemoryAsync(ba);
            
            // yield return request;

            while (!assetBundleReq.isDone)
            {
                log($"Loading skin asset-bundle from memory... {(assetBundleReq.progress * 100f):0.##}%");
                yield return null;
            }
            log("AssetBundle loaded!");

            var skinReq = assetBundleReq.assetBundle.LoadAssetAsync<GUISkin>("main");
            while (!skinReq.isDone)
            {
                log($"Loading skin from asset-bundle... {(skinReq.progress * 100f):0.##}%");
                yield return null;
            }
            log("Skin loaded!");

            UWindowManager.DefaultSkin = (GUISkin)skinReq.asset;

            UWindowManager.LoadGlobalSkin_internal(UWindowManager.DefaultSkin);
            
            yield return null;
        }
    }
}