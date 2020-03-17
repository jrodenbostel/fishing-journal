using System;
using System.Net;
using System.Threading;
using FishingJournal.Controllers;
using FishingJournal.Data;
using FishingJournal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FishJournalTest.Controllers
{
    public class JournalEntryControllerTest
    {
        [Fact]
        public void CreatePageShouldRender()
        {
            //Arrange
            var controller = new JournalEntryController(null);

            //Act
            var response = controller.Create();

            //Assert
            Assert.NotNull(response);
        }
        
        [Fact]
        public void CreatePostShouldCreate()
        {
            //Arrange
            var journalEntry = new JournalEntry
            {
                Latitude = "12",
                Longitude = "-12",
                LocationOverride = "Geneva",
                WeatherSummary = "Sunny",
                Date = new DateTime(),
                Notes = "caught lots of fish"
            };
            var contextOptions = new DbContextOptions<DefaultContext>();
            var mockContext = new Mock<DefaultContext>(contextOptions);

            mockContext.Setup(x => x.Add(It.IsAny<JournalEntry>()));
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
            
            var controller = new JournalEntryController(mockContext.Object);
            
            //Act
            var response = controller.Create(journalEntry).Result;

            //Assert
            Assert.NotNull(response);
            var viewResult = Assert.IsType<RedirectToActionResult>(response);
            Assert.Equal("Index", viewResult.ActionName);
            mockContext.Verify(x => x.Add(journalEntry));
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }
    }
}