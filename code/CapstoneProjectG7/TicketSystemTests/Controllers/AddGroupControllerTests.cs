using System;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using TicketSystemWeb.Controllers;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class AddGroupControllerTests : IDisposable
    {
        private AddGroupController _controller;

        [SetUp]
        public void SetUp()
        {
            _controller = new AddGroupController();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
        }
    }
}
