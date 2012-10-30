﻿using System;
using System.Linq;
using BoxApi.V2.Model;
using NUnit.Framework;

namespace BoxApi.V2.Tests
{
    [TestFixture]
    public class EventTestsSync : BoxApiTestHarness
    {
        [Test]
        public void CanGetCurrentStreamPosition()
        {
            var latestPosition = Client.GetCurrentStreamPosition();
            Assert.That(latestPosition, Is.GreaterThan(0));
        }

        [Test]
        public void CanGetEventForUser()
        {
            var latestPosition = Client.GetCurrentStreamPosition();
            var testFolder = Client.CreateFolder(RootId, TestItemName());
            try
            {
                Event events = Client.GetUserEvents(latestPosition, StreamType.All, 100);
                Assert.That(events.ChunkSize, Is.EqualTo(1));
                Assert.That(events.Entries.Count, Is.EqualTo(1));
                var entry = events.Entries.Single();
                Assert.That(entry.Source.Id, Is.EqualTo(testFolder.Id));
                Assert.That(entry.Source.CreatedAt, Is.EqualTo(testFolder.CreatedAt));
            }
            catch (Exception)
            {
                Client.Delete(testFolder);
            }
        }

        [Test]
        public void NonHierarchyEventsCanBeExcluded()
        {
            var testFile = Client.CreateFile(RootId, TestItemName());
            var latestPosition = Client.GetCurrentStreamPosition();
            Client.CreateComment(testFile, "comment!");
            try
            {
                Event events = Client.GetUserEvents(latestPosition, StreamType.TreeChanges, 100);
                Assert.That(events.ChunkSize, Is.EqualTo(0));
                Assert.That(events.Entries.Count, Is.EqualTo(0));
            }
            catch (Exception)
            {
                Client.Delete(testFile);
            }
        }
    }
}