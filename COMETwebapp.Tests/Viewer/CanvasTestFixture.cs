

namespace COMETwebapp.Tests.Viewer
{
    using Bunit;
    using COMETwebapp.Components.CanvasComponent;
    using COMETwebapp.Interoperability;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CanvasTestFixture
    {
        private TestContext context;
        private BabylonCanvas canvas;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);
            this.context.Services.AddTransient<ISceneSettings, SceneSettings>();
            this.context.Services.AddTransient<IJSInterop, JSInterop>();

            var renderer = this.context.RenderComponent<BabylonCanvas>();
            this.canvas = renderer.Instance;
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeAdded()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);

            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectCanBeAdded()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddTemporarySceneObject(sceneObject);

            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatSceneObjectsCanBeCleared()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);
            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));

            await this.canvas.ClearSceneObjects();
            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectsCanBeCleared()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddTemporarySceneObject(sceneObject);
            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));

            await this.canvas.ClearTemporarySceneObjects();
            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeRetrievedByID()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);
            var retrieved = this.canvas.GetSceneObjectById(sceneObject.ID);
            Assert.That(retrieved, Is.Not.Null);
        }
    }
}
