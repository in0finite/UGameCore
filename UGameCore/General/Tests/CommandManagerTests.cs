using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UGameCore.Tests
{
    public class CommandManagerTests : TestBase
    {
        [Test]

        [TestCase("1234", new string[] { "1234" })]
        [TestCase("1 2", new string[] { "1", "2" })]
        [TestCase("1 2 3 4", new string[] { "1", "2", "3", "4" })]
        [TestCase("'1' '2 3' ' 4 '", new string[] { "1", "2 3", " 4 " })]

        // single argument in quotes
        [TestCase("\"a\"", new string[] { "a" })]
        [TestCase("\"\"", new string[] { "" })]
        [TestCase("\" a \"", new string[] { " a " })]
        [TestCase("\" a b c \"", new string[] { " a b c " })]

        [TestCase("abcd\"abc\"abc", new string[] { "abcd\"abc\"abc" })]
        [TestCase("abcd\" abcd\" abcd", new string[] { "abcd\"", "abcd\"", "abcd" })]
        [TestCase("abcd \"abcd\"abcd", new string[] { "abcd", "abcd\"abcd" })]
        [TestCase("abcd \"abcd \"abcd", new string[] { "abcd", "abcd \"abcd" })]
        [TestCase("abcd \"a  |", new string[] { "abcd", "a  |" })]
        [TestCase("abcd \"\"", new string[] { "abcd", "" })]
        [TestCase("abcd \" \"", new string[] { "abcd", " " })]

        // multiple argument separators together
        [TestCase("abcd  1234", new string[] { "abcd", "1234" })]
        [TestCase("  abcd  \"\"  1234  ", new string[] { "abcd", "", "1234" })]
        [TestCase("  abcd  \"12\"  1234  ", new string[] { "abcd", "12", "1234" })]

        // multiple argument separators at end, in quotes
        [TestCase("abcd \"1234  ", new string[] { "abcd", "1234" })]
        [TestCase("\"1234  ", new string[] { "1234" })]

        [TestCase("abc\n", new string[] { "abc" })]
        [TestCase("abc\n\n", new string[] { "abc" })]
        [TestCase("abc;", new string[] { "abc" })]
        [TestCase("abc;;", new string[] { "abc" })]
        [TestCase("abc;\n", new string[] { "abc" })]

        [TestCase("\nabc", new string[] { "abc" })]
        [TestCase("\n\nabc", new string[] { "abc" })]
        [TestCase(";abc", new string[] { "abc" })]
        [TestCase(";;abc", new string[] { "abc" })]
        [TestCase(";\nabc", new string[] { "abc" })]
        public void SingleCommand(string command, string[] expectedArguments)
        {
            string[] args = CommandManager.SplitSingleCommandIntoArguments(command);
            Assert.AreEqual(expectedArguments, args);
        }

        [Test]
        [TestCase("abc;abc")]
        [TestCase("abc;abc;")]
        [TestCase("abc\nabc")]
        [TestCase("abc\nabc;")]
        [TestCase(";abc;abc")]
        [TestCase(";abc;abc;")]
        public void SingleCommand_GivenMultipleCommands(string command)
        {
            var ex = Assert.Throws<ArgumentException>(() => CommandManager.SplitSingleCommandIntoArguments(command));
            Assert.AreEqual($"Found multiple commands ({2}) while trying to split arguments of single command", ex.Message);
        }

        [Test]

        [TestCase("abc;123", new string[] { "abc" }, new string[] { "123" })]
        [TestCase("abc;123;", new string[] { "abc" }, new string[] { "123" })]
        [TestCase("abc\n123", new string[] { "abc" }, new string[] { "123" })]
        [TestCase("abc\n123;", new string[] { "abc" }, new string[] { "123" })]
        [TestCase(";abc;123", new string[] { "abc" }, new string[] { "123" })]
        [TestCase(";abc;123;", new string[] { "abc" }, new string[] { "123" })]

        // multiple arguments per command
        [TestCase("a b ; 1 2", new string[] { "a", "b" }, new string[] { "1", "2" })]
        [TestCase("a b c ; 1 2 3", new string[] { "a", "b", "c" }, new string[] { "1", "2", "3" })]
        [TestCase("a b \n 1 2", new string[] { "a", "b" }, new string[] { "1", "2" })]
        [TestCase("a b c \n 1 2 3", new string[] { "a", "b", "c" }, new string[] { "1", "2", "3" })]

        [TestCase(";;;;a;;;;1;;;;2;;;;", new string[] { "a" }, new string[] { "1" }, new string[] { "2" })]
        [TestCase("\na\n1\n2\n", new string[] { "a" }, new string[] { "1" }, new string[] { "2" })]
        [TestCase(";\na;\n1;\n2;\n", new string[] { "a" }, new string[] { "1" }, new string[] { "2" })]
        [TestCase("\n;a\n;1\n;2\n;", new string[] { "a" }, new string[] { "1" }, new string[] { "2" })]

        // command separators in quotes
        [TestCase("';abc\n';123", new string[] { ";abc\n" }, new string[] { "123" })]
        [TestCase("'\na b c;'\n1 2 3", new string[] { "\na b c;" }, new string[] { "1", "2", "3" })]

        // command separator (;) after quotes
        [TestCase("'a';b", new string[] { "a" }, new string[] { "b" })]
        [TestCase("'a';'b'", new string[] { "a" }, new string[] { "b" })]
        [TestCase("'a';'b", new string[] { "a" }, new string[] { "b" })]
        [TestCase("'a';'b';", new string[] { "a" }, new string[] { "b" })]
        [TestCase("'a';;'b';;", new string[] { "a" }, new string[] { "b" })]

        // command separator (;) before quotes
        [TestCase(";'a';b", new string[] { "a" }, new string[] { "b" })]
        [TestCase(";'a';'b'", new string[] { "a" }, new string[] { "b" })]
        [TestCase(";'a';'b", new string[] { "a" }, new string[] { "b" })]
        [TestCase(";'a';'b';", new string[] { "a" }, new string[] { "b" })]
        [TestCase(";'a';;'b';;", new string[] { "a" }, new string[] { "b" })]

        public void MultipleCommands(string command, string[] expectedArguments0, string[] expectedArguments1, string[] expectedArguments2 = null)
        {
            List<string[]> commands = CommandManager.SplitMultipleCommandsIntoArguments(command);
            Assert.AreEqual(expectedArguments0, commands[0]);
            Assert.AreEqual(expectedArguments1, commands[1]);
            if (expectedArguments2 != null)
                Assert.AreEqual(expectedArguments2, commands[2]);
        }

        [Test, LoadSceneOnce]

        [TestCase("echo abc;echo 123", "abc", "123")]
        [TestCase("echo abc;echo 123;", "abc", "123")]
        [TestCase("echo abc;echo 123;;", "abc", "123")]
        [TestCase(";echo abc;echo 123", "abc", "123")]
        [TestCase(";;echo abc;echo 123", "abc", "123")]
        [TestCase("echo abc\necho 123", "abc", "123")]
        [TestCase("echo abc\necho 123\n", "abc", "123")]
        [TestCase("\necho abc\necho 123", "abc", "123")]
        [TestCase("\necho abc\necho 123\n", "abc", "123")]

        // quotes
        [TestCase("echo \"abc\"; echo \"123\"", "abc", "123")]
        [TestCase("echo 'abc'; echo '123'", "abc", "123")]

        // command separators in quotes
        [TestCase("echo ';abc\n';echo '\n123;'", ";abc\n", "\n123;")]

        // command separator (;) after quotes
        [TestCase("'echo';echo", "", "")]
        [TestCase("'echo';'echo'", "", "")]
        [TestCase("'echo';'echo", "", "")]
        [TestCase("'echo';'echo';", "", "")]
        [TestCase("'echo';;'echo';;", "", "")]

        // command separator (;) before quotes
        [TestCase(";'echo';echo", "", "")]
        [TestCase(";'echo';'echo'", "", "")]
        [TestCase(";'echo';'echo", "", "")]
        [TestCase(";'echo';'echo';", "", "")]
        [TestCase(";'echo';;'echo';;", "", "")]

        // `echo` command eats whitespaces between arguments
        [TestCase("echo  abc  123 ; echo  abc  123  ", "abc 123", "abc 123")]

        // empty `echo` command response
        [TestCase("echo;echo", "", "")]
        [TestCase("echo;echo;", "", "")]
        [TestCase(";echo;echo;", "", "")]
        [TestCase("echo ;echo ;", "", "")]

        // argument that has both separator and quotes - test esaping in CombineArguments()
        [TestCase("echo 'abc;\"123'; echo \"abc;'123\"", "abc;\"123", "abc;'123")]
        [TestCase("echo 'abc\n\"123'; echo \"abc\n'123\"", "abc\n\"123", "abc\n'123")]
        [TestCase("echo 'abc \"123'; echo \"abc '123\"", "abc \"123", "abc '123")]
        // multiple consecutive
        [TestCase("echo 'abc\"\"123'; echo \"abc''123\"", "abc\"\"123", "abc''123")]
        // multiple consecutive, all double quotes
        [TestCase("echo \"abc\"\"123\"; echo", "abc\"\"123", "")]
        // 3 consecutive, all double quotes
        [TestCase("echo \"abc\"\"\"123\"; echo", "abc\"\"\"123", "")]

        public void ProcessMultipleCommands(string command, string expectedResponse0, string expectedResponse1)
        {
            CommandManager commandManager = GetSingleObject<CommandManager>();

            CommandManager.ProcessCommandResult[] results = commandManager.ProcessMultipleCommands(
                    new CommandManager.ProcessCommandContext { command = command, hasServerPermissions = true });

            Assert.AreEqual(expectedResponse0, results[0].response);
            Assert.AreEqual(expectedResponse1, results[1].response);
        }

        [Test, LoadSceneOnce]
        [TestCase("echo abc\\n;echo \\n123", "abc\n", "\n123")]
        [TestCase("echo abc\\t;echo \\t123", "abc\t", "\t123")]
        [TestCase("echo abc\\\\;echo \\\\123", "abc\\", "\\123")]
        [TestCase("echo abc\\';echo \\'123", "abc'", "'123")]
        [TestCase("echo abc\\\";echo \\\"123", "abc\"", "\"123")]
        public void ProcessMultipleCommands_UnescapingCharacters(
            string command, string expectedResponse0, string expectedResponse1)
        {
            ProcessMultipleCommands(command, expectedResponse0, expectedResponse1);
        }

        [Test, LoadSceneOnce]
        [TestCase("echo abc\\n\\n;echo \\n\\n123", "abc\n\n", "\n\n123")]
        [TestCase("echo abc\\t\\t;echo \\t\\t123", "abc\t\t", "\t\t123")]
        [TestCase("echo abc\\\\\\\\;echo \\\\\\\\123", "abc\\\\", "\\\\123")]
        [TestCase("echo abc\\'\\';echo \\'\\'123", "abc''", "''123")]
        [TestCase("echo abc\\\"\\\";echo \\\"\\\"123", "abc\"\"", "\"\"123")]
        public void ProcessMultipleCommands_UnescapingMultipleConsecutiveCharacters(
            string command, string expectedResponse0, string expectedResponse1)
        {
            ProcessMultipleCommands(command, expectedResponse0, expectedResponse1);
        }

        public void AutoComplete(
            string command, string expectedResponse, string[] possibleOptions)
        {
            CommandManager commandManager = GetSingleObject<CommandManager>();

            var possibleCompletions = new List<string>();

            commandManager.AutoCompleteCommand(
                new CommandManager.ProcessCommandContext { command = command, hasServerPermissions = true },
                out string outExactCompletion,
                possibleCompletions);

            Assert.AreEqual(expectedResponse, outExactCompletion);
            Assert.AreEqual(possibleCompletions, possibleOptions?.ToList());
        }

        [Test, LoadSceneOnce]

        [TestCase("echo; echo", null)]
        [TestCase("echo; ech", "echo; echo")]
        [TestCase("echo; ec", "echo; echo")]
        [TestCase("123; ech", "123; echo")]

        // with handler
        [TestCase("123; help echo", null)]
        [TestCase("123; help ech", "123; help echo")]
        [TestCase("123; help ec", "123; help echo")]

        [TestCase("123; 456; abc; ech", "123; 456; abc; echo")] // 4 commands

        [TestCase("123; 456; abc; help ech", "123; 456; abc; help echo")] // 4 commands with handler

        [TestCase("123; help \"ech\"", "123; help echo")] // quotes

        // non-existent commands
        [TestCase("123; non-existent", null)]
        [TestCase("123; help non-existent", null)]

        // multiple possible options
        [TestCase("123; ex", null, new string[] { "exit", "exec" })]
        [TestCase("123; help ex", null, new string[] { "exit", "exec" })]

        public void AutoCompleteMultipleCommands(
            string command, string expectedResponse, string[] possibleOptions = null)
        {
            AutoComplete(command, expectedResponse, possibleOptions ?? Array.Empty<string>());
        }

        [Test, LoadSceneOnce]

        [TestCase("echo", null)]
        [TestCase("ech", "echo")]
        [TestCase("ec", "echo")]

        [TestCase("echo; ", null)]
        [TestCase("  echo  ;  ", null)]
        [TestCase("  echo  ", null)]
        [TestCase("  \"echo\"  ", null)]

        // with handler
        [TestCase("help echo", null)]
        [TestCase("help ech", "help echo")]
        [TestCase("help ec", "help echo")]

        // non-existent commands
        [TestCase("non-existent", null)]
        [TestCase("help non-existent", null)]

        // multiple possible options
        [TestCase("ex", null, new string[] { "exit", "exec" })]
        [TestCase("help ex", null, new string[] { "exit", "exec" })]

        public void AutoCompleteSingleCommand(
            string command, string expectedResponse, string[] possibleOptions = null)
        {
            AutoComplete(command, expectedResponse, possibleOptions ?? Array.Empty<string>());
        }
    }
}
