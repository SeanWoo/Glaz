using Glaz.Server.Controllers.Api;
using Glaz.Server.UnitTests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Glaz.Server.UnitTests.Controllers.Api
{
    public class StreamControllerTests
    {
        [Fact]
        public void GetVideoStream_NullFileName_ReturnsBadRequestResponse()
        {
            var fakeWebHostEnvironment = new FakeWebHostEnvironment();
            var controller = new StreamController(fakeWebHostEnvironment);

            var actual = controller.GetVideoStream(null);

            Assert.IsType<BadRequestResult>(actual);
        }
        
        [Fact]
        public void GetVideoStream_EmptyFileName_ReturnsBadRequestResponse()
        {
            var fakeWebHostEnvironment = new FakeWebHostEnvironment();
            var controller = new StreamController(fakeWebHostEnvironment);

            var actual = controller.GetVideoStream(string.Empty);

            Assert.IsType<BadRequestResult>(actual);
        }
        
        [Fact]
        public void GetVideoStream_WhitespaceFileName_ReturnsBadRequestResponse()
        {
            var fakeWebHostEnvironment = new FakeWebHostEnvironment();
            var controller = new StreamController(fakeWebHostEnvironment);

            var actual = controller.GetVideoStream("    ");

            Assert.IsType<BadRequestResult>(actual);
        }
        
        [Fact]
        public void GetVideoStream_NotExistingFileName_ReturnsBadRequestResponse()
        {
            var fakeWebHostEnvironment = new FakeWebHostEnvironment();
            var controller = new StreamController(fakeWebHostEnvironment);

            var actual = controller.GetVideoStream("abc");

            Assert.IsType<BadRequestResult>(actual);
        }
        
        [Fact]
        public void GetVideoStream_ExistingFileName_ReturnsFileStream()
        {
            var fakeWebHostEnvironment = new FakeWebHostEnvironment();
            var controller = new StreamController(fakeWebHostEnvironment);

            var actual = controller.GetVideoStream("test");

            Assert.IsType<PhysicalFileResult>(actual);
        }
    }
}