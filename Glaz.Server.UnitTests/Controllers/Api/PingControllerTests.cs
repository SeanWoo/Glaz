using Glaz.Server.Controllers.Api;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Glaz.Server.UnitTests.Controllers.Api
{
    public sealed class PingControllerTests
    {
        [Fact]
        public void Index_SimpleRequest_ReturnsOkResponse()
        {
            var controller = new PingController();

            var actual = controller.Index();
            
            Assert.IsType<OkResult>(actual);
        }
    }
}