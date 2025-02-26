using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests : IDisposable
    {
        private TicketDBContext _context;
        private Mock<ILogger<HomeController>> _loggerMock;
        private HomeController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TicketDBContext(options);

            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test]
        public void Index_ReturnsViewWithTickets()
        {
            var result = _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = (ViewResult)result;
            Assert.That(viewResult.Model, Is.TypeOf<List<Ticket>>());
        }

        [Test]
        public async Task AddTicket_ValidModel_AddsTicketAndRedirects()
        {
            var ticket = new Ticket { Title = "New Ticket", Description = "Test description" };

            var result = _controller.AddTicket(ticket);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;
            Assert.That(redirect.ActionName, Is.EqualTo("Index"));

            var tickets = await _context.Tickets.ToListAsync();
            Assert.That(tickets.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddTicket_InvalidModel_ReturnsViewWithTickets()
        {
            var ticket = new Ticket();
            _controller.ModelState.AddModelError("Error", "Invalid ticket data");

            var result = _controller.AddTicket(ticket);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = (ViewResult)result;
            Assert.That(viewResult.Model, Is.TypeOf<List<Ticket>>());
        }
    }
}
