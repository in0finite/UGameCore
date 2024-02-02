using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using UGameCore.Utilities;
using System;

namespace UGameCore.Tests
{
    public class ConfigVarTests : TestBase
    {
        [Test]
        public void Sizes()
        {
            Assert.AreEqual(Marshal.SizeOf<Union8>(), 8);
            Assert.AreEqual(Marshal.SizeOf<Union16>(), 16);
        }

        [Test, LoadSceneOnce]
        [TestCase("1")]
        [TestCase("a")]
        [TestCase("ValidName")]
        [TestCase("underslash_")]
        [TestCase("HyphenMinus-")]
        public void ValidNames(string configVarName)
        {
            Debug.Log($"testing valid config var name: {configVarName}");

            var cvarManager = GetSingleObject<CVarManager>();

            cvarManager.RegisterConfigVar(new IntConfigVar { SerializationName = configVarName });

            Assert.IsTrue(cvarManager.ConfigVars.ContainsKey(configVarName));
        }

        [Test, LoadSceneOnce]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(".")]
        [TestCase(",")]
        [TestCase("#")]
        [TestCase("?")]
        [TestCase("/")]
        [TestCase("\\")]
        [TestCase("a b")]
        [TestCase("a\n")]
        public void BadNames(string configVarName)
        {
            Debug.Log($"testing bad config var name: {configVarName}");

            var cvarManager = GetSingleObject<CVarManager>();

            Assert.Throws<ArgumentException>(() => cvarManager.RegisterConfigVar(new IntConfigVar { SerializationName = configVarName }));
        }
    }
}
