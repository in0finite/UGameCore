using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UGameCore.Tests
{
    public class LoadSceneOnceAttribute : NUnitAttribute, IOuterUnityTestAction
    {
        string m_scenePath;
        internal static bool LoadedScene = false;


        public IEnumerator BeforeTest(ITest test)
        {
            if (LoadedScene)
                yield break;

            if (!Application.isPlaying)
                throw new System.InvalidOperationException("This should only execute in Play mode");

            m_scenePath = SceneUtility.GetScenePathByBuildIndex(0);
            Debug.Log($"Loading scene: {m_scenePath}");
            var asyncOp = SceneManager.LoadSceneAsync(m_scenePath, LoadSceneMode.Single);
            while (!asyncOp.isDone)
                yield return null;

            // wait for scripts to initialize
            yield return null;
            yield return null;

            LoadedScene = true;
        }

        public IEnumerator AfterTest(ITest test)
        {
            yield break;
        }
    }
}
