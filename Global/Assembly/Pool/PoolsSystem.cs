using MadApper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BEH
{
    public class PoolsSystem
    {
        static SceneChangeSubscriber sceneChangeSubscriber;

        static Pool[] _allPools;
        static public Pool[] s_AllPools
        {
            get
            {
                if (_allPools == null || _allPools.Length == 0)
                {
                    _allPools = MADUtility.GetAllDerivedInstancesInAllAssemblies<Pool>().ToArray();
                }

                return _allPools;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            if (s_AllPools == null)
                return;

            foreach (var item in s_AllPools)
                item.Initialize();

            sceneChangeSubscriber = new SceneChangeSubscriber.Builder()
                .SetOnSceneChanged(SceneChanged)
                .SetOnSceneUnloaded(SceneUnloaded)              
                .Build();
        }

        private static void SceneChanged(string obj)
        {
            if (s_AllPools == null)
                return;

            foreach (var item in s_AllPools)
                item.OnSceneChanged(obj);
        }

        private static void SceneUnloaded(string obj)
        {
            if (_allPools == null)
                return;


            foreach (var item in _allPools)
                item.OnSceneUnloaded(obj);
        }       
    }

}