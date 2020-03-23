using System;
using System.Security.Claims;
using System.Threading;
using FishingJournal.Controllers;
using FishingJournal.Data;
using FishingJournal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
            var controller = new JournalEntryController(null, null);

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
            var user = new User
            {
                Email = "test@test.com"
            };
            var contextOptions = new DbContextOptions<DefaultContext>();
            var mockContext = new Mock<DefaultContext>(contextOptions);
            var mockIdentityContext = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            var mockUserStore = new Mock<UserStore<User>>(mockIdentityContext.Object, null);
             var mockUserManager =
                new Mock<UserManager<User>>(mockUserStore.Object, null, null, null, null, null, null, null, null);

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockContext.Setup(x => x.Add(It.IsAny<JournalEntry>()));
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));

            var controller = new JournalEntryController(mockContext.Object, mockUserManager.Object);

            //Act
            var response = controller.Create(journalEntry).Result;

            //Assert
            Assert.NotNull(response);
            var viewResult = Assert.IsType<RedirectToActionResult>(response);
            mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            mockContext.Verify(x => x.Add(journalEntry), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal("Index", viewResult.ActionName);
        }
    }
}