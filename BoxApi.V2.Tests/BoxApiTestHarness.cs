using System;
using System.IO;
using BoxApi.V2.Model;
using BoxApi.V2.Model.Enum;
using NUnit.Framework;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using File = BoxApi.V2.Model.File;

namespace BoxApi.V2.Tests
{
    public class BoxApiTestHarness
    {
        protected readonly BoxManager Client;
        protected readonly BoxManager UnauthenticatedClient;

        protected const string RootId = "0";
        protected readonly Action<Error> AbortOnFailure = (error) => { throw new BoxException(error); };
        protected readonly string CollaboratingUser;

        protected int MaxWaitInSeconds { get; set; }

        protected BoxApiTestHarness()
        {
            var testInfo = GetTestInfo();
            Client = new BoxManager(testInfo.AppKey, testInfo.AuthKey);
            UnauthenticatedClient = new BoxManager(testInfo.AppKey);
            CollaboratingUser = testInfo.CollaboratingUser;
            MaxWaitInSeconds = 10;
        }

        protected static string TestItemName()
        {
            return string.Format("test_{0}", DateTime.UtcNow.Ticks.ToString());
        }

        protected static void AssertFolderConstraints(Folder folder, string expectedName, string expectedParentId, string expectedId = null)
        {
            AssertEntityConstraints(folder, ResourceType.Folder, expectedName, expectedParentId, expectedId);
        }

        protected static void AssertFileConstraints(File file, string expectedName, string expectedParentId, string expectedId = null)
        {
            AssertEntityConstraints(file, ResourceType.File, expectedName, expectedParentId, expectedId);
        }

        private static void AssertEntityConstraints(File item, ResourceType expectedType, string expectedName, string expectedParentId, string expectedId)
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Type, Is.EqualTo(expectedType));
            Assert.That(item.Name, Is.EqualTo(expectedName));

            if (expectedParentId == null)
            {
                Assert.That(item.Parent, Is.Null);
            }
            else
            {
                Assert.That(item.Parent.Id, Is.EqualTo(expectedParentId));
            }

            if (expectedId != null)
            {
                Assert.That(item.Id, Is.EqualTo(expectedId));
            }
        }

        protected static void AssertSharedLink(SharedLink actual, SharedLink sharedLink)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Access, Is.EqualTo(sharedLink.Access));
            Assert.That(actual.UnsharedAt, Is.GreaterThan(DateTime.MinValue));
            Assert.That(actual.UnsharedAt, Is.LessThan(DateTime.MaxValue));
            Assert.That(actual.Permissions.CanDownload, Is.True);
            Assert.That(actual.Permissions.CanPreview, Is.True);
        }

        private static TestConfigInfo GetTestInfo()
        {
            TestConfigInfo testConfigInfo;
            var fi = new FileInfo(@"..\..\test_info.json");
            if (!fi.Exists)
            {
                testConfigInfo = ConfigureTestInfo(fi);
            }
            else
            {
                string content;
                using (var stream = fi.OpenText())
                {
                    content = stream.ReadToEnd();
                }
                try
                {
                    testConfigInfo = new JsonDeserializer().Deserialize<TestConfigInfo>(new RestResponse {Content = content});
                }
                catch
                {
                    testConfigInfo = ConfigureTestInfo(fi);
                }
            }
            return testConfigInfo;
        }

        private static TestConfigInfo ConfigureTestInfo(FileInfo fi)
        {
            TestConfigInfo testConfigInfo;
            var ui = new TestConfig();
            ui.ShowDialog();
            using (var stream = fi.CreateText())
            {
                testConfigInfo = new TestConfigInfo
                    {
                        AppKey = ui.AppKey,
                        AuthKey = ui.AuthKey,
                        TestEmail = ui.TestEmail,
                    };
                stream.Write(new JsonSerializer().Serialize(testConfigInfo));
            }
            return testConfigInfo;
        }

        public class TestConfigInfo
        {
            public string AppKey { get; set; }
            public string TestEmail { get; set; }
            public string AuthKey { get; set; }
            public string CollaboratingUser { get; set; }
        }
    }
}