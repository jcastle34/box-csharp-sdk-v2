using System;
using System.IO;
using System.Reflection;
using BoxApi.V2.Authentication.OAuth2;
using BoxApi.V2.Model;
using BoxApi.V2.Model.Enum;
using NUnit.Framework;
using File = BoxApi.V2.Model.File;

namespace BoxApi.V2.Tests.Harness
{
    public class BoxApiTestHarness
    {
        protected const string RootId = "0";
        protected readonly Action<Error> AbortOnFailure = (error) => { throw new BoxException(error); };
        protected readonly BoxManager Client;
        protected readonly string CollaboratingUser;

        protected BoxApiTestHarness()
        {
            RefreshAccessToken();
            TestConfigInfo testInfo = TestConfigInfo.Get();
            Client = new BoxManager(testInfo.AccessToken, null, BoxManagerOptions.RetryRequestOnceWhenHttp500Received);
            CollaboratingUser = testInfo.CollaboratingUserId;
            MaxWaitInSeconds = 20;
        }

        private void RefreshAccessToken()
        {
            var testConfigInfo = TestConfigInfo.Get();
            var authenticator = new TokenProvider(testConfigInfo.ClientId, testConfigInfo.ClientSecret);
            var refreshAccessToken = authenticator.RefreshAccessToken(testConfigInfo.RefreshToken);
            TestConfigInfo.Update(refreshAccessToken);
        }

        protected int MaxWaitInSeconds { get; set; }

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

        protected File PostImageFile(string fileName)
        {
            const string resource = "BoxApi.V2.Tests.Resources.corgilobster.jpg";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                return Client.CreateFile(Folder.Root, fileName, stream);
            }
        }
    }
}