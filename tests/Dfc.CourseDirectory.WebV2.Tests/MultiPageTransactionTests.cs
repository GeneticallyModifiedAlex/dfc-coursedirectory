﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class MultiPageTransactionTests : TestBase
    {
        public MultiPageTransactionTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task RequestWithNoInstanceIdToStartsMptxAction_CreatesInstanceAndRedirects()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("MultiPageTransactionTests/starts?qp=value");

            // Assert
            Assert.Equal(HttpStatusCode.RedirectKeepVerb, response.StatusCode);
            Assert.StartsWith(
                "/MultiPageTransactionTests/starts?qp=value&ffiid=",
                response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task RequestWithInstanceIdToStartsMptxAction_DoesNotRedirect()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                MultiPageTransactionTestsController.FlowName,
                new Dictionary<string, object>());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/starts?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RequestWithNoInstanceId_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("MultiPageTransactionTests/second");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task StartsMptxActionWithCaptures_StoresCapturesInStore()
        {
            // Arrange

            // Act
            await HttpClient.GetAsync("MultiPageTransactionTests/starts?qp=value");

            // Assert
            Assert.Equal("value", Factory.MptxStateProvider.Instances.Single().Value.Items["qp"]);
        }

        [Fact]
        public async Task ModelBindingMptxInstanceContext_Succeeds()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                MultiPageTransactionTestsController.FlowName,
                new Dictionary<string, object>());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/modelbinds?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(instance.InstanceId, responseContent);
        }

        [Fact]
        public async Task ModelBindingMptxInstanceContextMismatchedType_ReturnsBadRequest()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                MultiPageTransactionTestsController.FlowName,
                new Dictionary<string, object>(),
                state: new MultiPageTransactionTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/modelbindswrongtype?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task StateUpdates_PersistedToStore()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                MultiPageTransactionTestsController.FlowName,
                new Dictionary<string, object>(),
                new MultiPageTransactionTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/updatesstate?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var state = Assert.IsType<MultiPageTransactionTestsFlowState>(
                Factory.MptxStateProvider.Instances.Single().Value.State);
            Assert.Equal(69, state.Bar);
        }

        [Fact]
        public async Task CompleteInstance_RemovesInstanceFromStore()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                MultiPageTransactionTestsController.FlowName,
                new Dictionary<string, object>());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/final?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.DoesNotContain(instance.InstanceId, Factory.MptxStateProvider.Instances.Keys);
        }
    }

    public class MultiPageTransactionTestsFlowState : IMptxState
    {
        public int Foo { get; set; }
        public int Bar { get; set; }
    }

    public class MultiPageTransactionTestsDifferentFlowState : IMptxState
    {
    }

    [Route("MultiPageTransactionTests")]
    public class MultiPageTransactionTestsController : Controller
    {
        public const string FlowName = "MultiPageTransactionTests";

        [StartsMptx(FlowName, capturesQueryParams: "qp")]
        [HttpGet("starts")]
        public IActionResult Starts() => Ok();

        [MptxAction(FlowName)]
        [HttpGet("second")]
        public IActionResult Second() => Ok();

        [MptxAction(FlowName)]
        [HttpGet("modelbinds")]
        public IActionResult ModelBinds(MptxInstanceContext<MultiPageTransactionTestsFlowState> flowState)
        {
            flowState.Update(s => s.Foo = 42);
            return Ok(flowState.InstanceId);
        }

        [MptxAction(FlowName)]
        [HttpGet("modelbindswrongtype")]
        public IActionResult ModelBindsWrongType(
            MptxInstanceContext<MultiPageTransactionTestsDifferentFlowState> flowState) => Ok();

        [MptxAction(FlowName)]
        [HttpGet("updatesstate")]
        public IActionResult UpdatesState(
            MptxInstanceContext<MultiPageTransactionTestsFlowState> flowState)
        {
            flowState.Update(s => s.Bar = 69);
            return Ok();
        }

        [MptxAction(FlowName)]
        [HttpGet("final")]
        public IActionResult Final(MptxInstanceContext<MultiPageTransactionTestsFlowState> flowState)
        {
            flowState.Complete();
            return Ok();
        }
    }
}