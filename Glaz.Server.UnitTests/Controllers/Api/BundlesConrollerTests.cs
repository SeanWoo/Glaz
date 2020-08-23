using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Glaz.Server.Controllers.Api;
using Glaz.Server.Data;
using Glaz.Server.Data.Enums;
using Glaz.Server.Entities;
using Glaz.Server.UnitTests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Glaz.Server.UnitTests.Controllers.Api
{
    public sealed class BundlesConrollerTests : IDisposable
    {
        private const string TestTargetId = "1";
        private const string TestTargetName = "Test target";
        
        private readonly Guid _testOrderId = Guid.NewGuid();
        private readonly ApplicationDbContext _context;

        public BundlesConrollerTests()
        {
            _context = new ApplicationDbContext(GetInMemoryDbContextOptions());
            _context.Orders.Add(GetTestOrder());
            _context.SaveChanges();
        }
        private DbContextOptions<ApplicationDbContext> GetInMemoryDbContextOptions()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
            return builder.Options;
        }
        private Order GetTestOrder()
        {
            var order = new Order
            {
                Id = _testOrderId,
                Account = new GlazAccount { Id = Guid.NewGuid().ToString() },
                Label = "Test order",
                State = OrderState.Verifying,
                Attachments = new List<Attachment>()
            };
            order.Attachments.Add(GetTestTargetAttachment());
            order.Attachments.Add(CreateTestBundleByPlatform(AttachmentPlatform.Android));
            return order;
        }
        private Attachment GetTestTargetAttachment()
        {
            var target = new Attachment
            {
                Id = Guid.NewGuid(),
                Label = "Test target",
                Type = AttachmentType.Target,
                OrderId = _testOrderId
            };

            var vuforiaDetails = new VuforiaDetails
            {
                Id = Guid.NewGuid(),
                AttachmentId = target.Id,
                TargetId = TestTargetId,
                Rating = 5
            };
            target.VuforiaDetails = vuforiaDetails;
            return target;
        }
        private Attachment CreateTestBundleByPlatform(AttachmentPlatform platform)
        {
            var bundle = new Attachment
            {
                Id = Guid.NewGuid(),
                OrderId = _testOrderId,
                Platform = platform,
                Type = AttachmentType.Bundle,
                Label = "Android_Bundle"
            };
            string path = platform switch
            {
                AttachmentPlatform.Android => "Attachments/Bundles/Android_Bundle.bin",
                AttachmentPlatform.Ios => "Attachments/Bundles/Ios_Bundle.bin",
                AttachmentPlatform.None => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
            bundle.Path = path;
            return bundle;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetByTargetId

        [Fact]
        public async Task GetByTargetId_NotExistingPlatform_ReturnsBadRequestResponse()
        {
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetId("123", "Windows");

            Assert.IsType<BadRequestObjectResult>(actual);
        }

        [Fact]
        public async Task GetByTargetId_NotExistingTargetId_ReturnsNotFoundResponse()
        {
            string platform = AttachmentPlatform.Android.ToString();
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetId("123", platform);

            Assert.IsType<NotFoundObjectResult>(actual);
        }
        
        [Fact]
        public async Task GetByTargetId_NotExistingTargetBundle_ReturnsNotFoundResponse()
        {
            string platform = AttachmentPlatform.Ios.ToString();
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetId(TestTargetId, platform);

            Assert.IsType<NotFoundObjectResult>(actual);
        }
        
        [Fact]
        public async Task GetByTargetId_ValidTargetAndBundle_ReturnsNotFoundResponse()
        {
            string notExistingPlatform = AttachmentPlatform.Android.ToString();
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetId(TestTargetId, notExistingPlatform);

            Assert.IsType<FileContentResult>(actual);
        }
        
        #endregion

        #region GetByTargetName

        [Fact]
        public async Task GetByTargetName_NotExistingPlatform_ReturnsBadRequestResponse()
        {
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetName("123", "Windows");

            Assert.IsType<BadRequestObjectResult>(actual);
        }

        [Fact]
        public async Task GetByTargetName_NotExistingTargetName_ReturnsNotFoundResponse()
        {
            string platform = AttachmentPlatform.Android.ToString();
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetName("123", platform);

            Assert.IsType<NotFoundObjectResult>(actual);
        }
        
        [Fact]
        public async Task GetByTargetName_NotExistingTargetBundle_ReturnsNotFoundResponse()
        {
            string platform = AttachmentPlatform.Ios.ToString();
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetName(TestTargetName, platform);

            Assert.IsType<NotFoundObjectResult>(actual);
        }
        
        [Fact]
        public async Task GetByTargetName_ValidTargetAndBundle_ReturnsNotFoundResponse()
        {
            string notExistingPlatform = AttachmentPlatform.Android.ToString();
            var controller = new BundlesController(_context, new FakeWebHostEnvironment());

            var actual = await controller.GetByTargetName(TestTargetName, notExistingPlatform);

            Assert.IsType<FileContentResult>(actual);
        }

        #endregion
    }
}