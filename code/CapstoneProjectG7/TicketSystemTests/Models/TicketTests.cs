using TicketSystemWeb.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class TicketTests

    // Setup
    // dotnet add package coverlet.collector
    // dotnet tool install -g dotnet-reportgenerator-globaltool

    // dotnet test --collect:"xplat code coverage" --settings coverlet.runsettings
    // add specific filename ex. 6299b382-8b19-4328-be79-2faf3349a1f0.
    // reportgenerator -reports:"TestResults\6299b382-8b19-4328-be79-2faf3349a1f0\coverage.cobertura.xml" -targetdir:"coverageresults" -reporttypes:html
    {
        [Test]
        public void Ticket_Should_Have_Valid_Default_Values()
        {
            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Sample Ticket",
                Description = "This is a test ticket",
                Status = "Open",
                CreatedBy = "JohnDoe"
            };

            Assert.That(ticket.TicketId, Is.EqualTo(1));
            Assert.That(ticket.Title, Is.EqualTo("Sample Ticket"));
            Assert.That(ticket.Description, Is.EqualTo("This is a test ticket"));
            Assert.That(ticket.Status, Is.EqualTo("Open"));
            Assert.That(ticket.CreatedBy, Is.EqualTo("JohnDoe"));
        }
    }
}
