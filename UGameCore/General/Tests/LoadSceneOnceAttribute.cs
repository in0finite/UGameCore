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
        bool m_loadedScene = false;

        public IEnumerator BeforeTest(ITest test)
        {
            if (m_loadedScene)
                yield break;

            m_scenePath = SceneUtility.GetScenePathByBuildIndex(0);
            yield return new EnterPlayMode();
            Debug.Log($"Loading scene: {m_scenePath}");
            var asyncOp = SceneManager.LoadSceneAsync(m_scenePath, LoadSceneMode.Single);
            while (!asyncOp.isDone)
                yield return null;

            m_loadedScene = true;
        }

        public IEnumerator AfterTest(ITest test)
        {
            yield break;
        }
    }
}
