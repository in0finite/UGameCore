using NUnit.Framework;
using UGameCore.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace UGameCore.Tests
{
    public class SpanCharBuilderTests : TestBase
    {
        readonly char[] CharBuffer = new char[1024 * 1024];

        readonly string[] ExampleStrings = new[]
        {
            "Lorem ipsum \n 1234 abcd LOREM IPSUM \n 1234 ABCD",
            "~!@#$%^&*()_+{}:\"|<>?|",
            "`1234567890-=[];'\\,./\\",
            "short",
            "SHORT",
            "",
            " ",
            "1",
            "12",
            "1111",
            "1234",
            "a",
            "ab",
            "aaAA",
            "abcd",
            new string(Enumerable.Range(1, 100).Select(n => (char)n).ToArray()),
            new string(Enumerable.Range(1, 10000).Select(n => (char)n).ToArray()),
        };

        readonly string[] ReplacementStrings = new[]
        {
            "a",
            "ab",
            "abCD",
            "Lorem ipsum",
            "LORem",
            "IPSum",
            "1",
            "1111",
            "12",
            "1234",
            " ",
            "\n",
            "<",
            "short",
            "sHOrt",
            "non-existent",
            "@",
            new string(Enumerable.Range(1, 100).Select(n => (char)n).ToArray()),
            new string(Enumerable.Range(1, 10000).Select(n => (char)n).ToArray()),
        };

        readonly bool[] IgnoreCases = new bool[]
        {
            true,
            false,
        };


        [Test]
        public void Replace()
        {
            ulong numComparisons = 0;

            foreach (bool ignoreCase in IgnoreCases)
            {
                foreach (string exampleStr in ExampleStrings)
                {
                    for (int i = 0; i < ReplacementStrings.Length; i++)
                    {
                        for (int j = 0; j < ReplacementStrings.Length; j++)
                        {
                            string stringToReplace = ReplacementStrings[i];
                            string newString = ReplacementStrings[j];

                            int capacity = stringToReplace.Length >= newString.Length
                                ? exampleStr.Length // limit capacity to String length
                                : CharBuffer.Length;

                            SpanCharBuilder sb = new(CharBuffer.AsSpan(0, capacity));
                            sb.WriteString(exampleStr);

                            // replace

                            sb.Replace(stringToReplace, newString, ignoreCase);

                            string replacedString = exampleStr.Replace(
                                stringToReplace, newString, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

                            Assert.AreEqual(replacedString, sb.AsString);

                            // replace part of string

                            int partIndex = exampleStr.Length / 4;
                            int partLength = exampleStr.Length / 2;
                            TestReplacePart(exampleStr, stringToReplace, newString, ref sb, ignoreCase, partIndex, partLength);

                            partIndex = exampleStr.Length / 2;
                            partLength = exampleStr.Length - partIndex;
                            TestReplacePart(exampleStr, stringToReplace, newString, ref sb, ignoreCase, partIndex, partLength);

                            partIndex = exampleStr.Length / 4;
                            partLength = exampleStr.Length - partIndex;
                            TestReplacePart(exampleStr, stringToReplace, newString, ref sb, ignoreCase, partIndex, partLength);

                            partIndex = 0;
                            partLength = exampleStr.Length / 2;
                            TestReplacePart(exampleStr, stringToReplace, newString, ref sb, ignoreCase, partIndex, partLength);

                            // -----

                            numComparisons++;
                        }
                    }
                }
            }

            Debug.Log($"numComparisons: {numComparisons}");
        }

        void TestReplacePart(
            string exampleStr,
            string stringToReplace,
            string newString,
            ref SpanCharBuilder sb,
            bool ignoreCase,
            int partIndex,
            int partLength)
        {
            sb.Clear();
            sb.WriteString(exampleStr);

            sb.Replace(stringToReplace, newString, partIndex, partLength, ignoreCase);

            string exampleStrPart = exampleStr.Substring(partIndex, partLength);
            string replacedStringPart = exampleStrPart.Replace(
                stringToReplace, newString, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

            Assert.AreEqual(
                exampleStr.Substring(0, partIndex) + replacedStringPart + exampleStr.Substring(partIndex + partLength),
                sb.AsString);
        }

        [Test]
        public void ReplaceOverflowNonFull()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                SpanCharBuilder sb = new(CharBuffer.AsSpan(0, 10));
                sb.WriteString("12345");
                Assert.Less(sb.Length, sb.Capacity);
                sb.Replace("12345", "1234567890_", true);
            });
        }

        [Test]
        public void ReplaceOverflowAlreadyFull()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                SpanCharBuilder sb = new(CharBuffer.AsSpan(0, 5));
                sb.WriteString("12345");
                Assert.AreEqual(sb.Length, sb.Capacity);
                sb.Replace("12345", "1234567890_", true);
            });
        }

        [Test]
        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 500)]
        [TestCase(0, 11)]
        [TestCase(10, 1)]
        [TestCase(5, 6)]
        public void ReplaceArgumentsOutOfRange(int index, int count)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                SpanCharBuilder sb = new(CharBuffer.AsSpan(0, 10));
                sb.WriteString("1234567890");
                Assert.AreEqual(sb.Length, sb.Capacity);
                sb.Replace("a", "b", index, count, true);
            });

            Assert.AreEqual("index", ex.ParamName);
            if (index < 0 || count < 0)
                Assert.AreEqual("Negative\r\nParameter name: index", ex.Message);
        }
    }
}
