﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.SecretManager.Tools.Internal;
using Xunit;

namespace Microsoft.Extensions.SecretManager.Tools.Tests
{
    public class SetCommandTest
    {
        [Fact]
        public void SetsFromPipedInput()
        {
            var input = @"
{
   ""Key1"": ""str value"",
""Key2"": 1234,
""Key3"": false
}";
            var testConsole = new TestConsole
            {
                IsInputRedirected = true,
                In = new StringReader(input)
            };
            var secretStore = new TestSecretsStore();
            var command = new SetCommand();

            command.Execute(new CommandContext(secretStore, NullLogger.Instance, testConsole));

            Assert.Equal(3, secretStore.Count);
            Assert.Equal("str value", secretStore["Key1"]);
            Assert.Equal("1234", secretStore["Key2"]);
            Assert.Equal("False", secretStore["Key3"]);
        }

        [Fact]
        public void ParsesNestedObjects()
        {
            var input = @"
                {
                   ""Key1"": {
                       ""nested"" : ""value""
                   },
                   ""array"": [ 1, 2 ]
                }";

            var testConsole = new TestConsole
            {
                IsInputRedirected = true,
                In = new StringReader(input)
            };
            var secretStore = new TestSecretsStore();
            var command = new SetCommand();

            command.Execute(new CommandContext(secretStore, NullLogger.Instance, testConsole));

            Assert.Equal(3, secretStore.Count);
            Assert.True(secretStore.ContainsKey("Key1:nested"));
            Assert.Equal("value", secretStore["Key1:nested"]);
            Assert.Equal("1", secretStore["array:0"]);
            Assert.Equal("2", secretStore["array:1"]);
        }

        [Fact]
        public void OnlyPipesInIfNoArgs()
        {
            var testConsole = new TestConsole
            {
                IsInputRedirected = true,
                In = new StringReader("")
            };
            var secretStore = new TestSecretsStore();
            var command = new SetCommand("key", null);

            var ex = Assert.Throws< Microsoft.DotNet.Cli.Utils.GracefulException>(
                () => command.Execute(new CommandContext(secretStore, NullLogger.Instance, testConsole)));
            Assert.Equal(Resources.FormatError_MissingArgument("value"), ex.Message);
        }

        private class TestSecretsStore : SecretsStore
        {
            public TestSecretsStore()
                : base("xyz", NullLogger.Instance)
            {
            }

            protected override IDictionary<string, string> Load(string userSecretsId)
            {
                return new Dictionary<string, string>();
            }

            public override void Save()
            {
                // noop
            }
        }
    }
}
