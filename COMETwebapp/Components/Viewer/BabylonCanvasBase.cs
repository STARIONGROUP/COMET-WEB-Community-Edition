namespace COMETwebapp.Componentes.Viewer
{
    using CDP4Common.EngineeringModelData;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    /// <summary>
    /// Support class for the <see cref="BabylonCanvas"/>
    /// </summary>
    public class BabylonCanvasBase : ComponentBase
    {
        private bool isMouseDown = false;


        /// <summary>
        /// Property to inject the JSRuntime and allow C#-JS interop
        /// </summary>
        [Inject] 
        IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Injected property to get acess to <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        ISessionAnchor SessionAnchor { get; set; }

        [JSInvokable]
        public static string GetGUID() => Guid.NewGuid().ToString();

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                JSInterop.JsRuntime = JsRuntime;
                Scene.Init();
                InitializeElements();

                AddWorldAxes();

                Scene.AddPrimitive(new Cube(5, 10, 15), Color.Yellow);
                Scene.AddPrimitive(new Torus(70, 50, 0, 20, 10), Color.Blue);

                Cube cube = new Cube(-50, 70, 10, 20, 20, 20);
                cube.SetRotation(1.0, 0.2, 0.1);
                Scene.AddPrimitive(cube, Color.Red);

                CustomPrimitive cp = new CustomPrimitive("./Assets/obj/", "RX2_CUSTOM_BODYKIT.obj");
                Scene.AddPrimitive(cp);
            }
        }

        /// <summary>
        /// Initialize the elements in the scene based on the elements of the iteration
        /// </summary>
        private void InitializeElements()
        {
            var iteration = this.SessionAnchor.OpenIteration;
            var elementUsages = iteration?.Element.SelectMany(x => x.ContainedElement).ToList();

            if(elementUsages != null)
            {
                foreach (var elementUsage in elementUsages)
                {
                    CreateShapeBasedOnElementUsage(elementUsage);
                }
            }
        }

        /// <summary>
        /// Creates a shape for the scene based on the element usage
        /// </summary>
        /// <param name="elementUsage">The element usage used for creating the shape</param>
        private void CreateShapeBasedOnElementUsage(ElementUsage elementUsage)
        {
            //elementUsage.ParameterOverride.Find(x=>x.p)
            if (elementUsage.ParameterOverride.Count > 0)
            {

            }
        }

        /// <summary>
        /// Canvas on mouse down event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseDown(MouseEventArgs e)
        {

        }

        /// <summary>
        /// Canvas on mouse up event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseUp(MouseEventArgs e)
        {

        }

        /// <summary>
        /// Canvas on click event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnClick(MouseEventArgs e)
        {

        }

        /// <summary>
        /// Canvas on mouse move event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseMove(MouseEventArgs e) 
        { 

        }

        /// <summary>
        /// Canvas on mouse wheel event
        /// </summary>
        /// <param name="e">the mouse wheel args of the event</param>
        public void OnMouseWheel(WheelEventArgs e)
        {
            Scene.SetInfoPanelVisibility(false);
        }

        /// <summary>
        /// Canvas on key down event
        /// </summary>
        /// <param name="e">the keyboard args of the event</param>
        public void OnKeyDown(KeyboardEventArgs e)
        {

        }

        /// <summary>
        /// Canvas on key up event
        /// </summary>
        /// <param name="e">the keyboard args of the event</param>
        public void OnKeyUp(KeyboardEventArgs e)
        {

        }

        /// <summary>
        /// Create the world axes and adds them to the scene
        /// </summary>
        private void AddWorldAxes()
        {
            float size = 700;
            Line xAxis = new Line(-size, 0, 0, size, 0, 0);
            Scene.AddPrimitive(xAxis, Color.Red);

            Line yAxis = new Line(0, -size, 0, 0, size, 0);
            Scene.AddPrimitive(yAxis, Color.Green);

            Line zAxis = new Line(0, 0, -size, 0, 0, size);
            Scene.AddPrimitive(zAxis, Color.Blue);
        }
    }
}
