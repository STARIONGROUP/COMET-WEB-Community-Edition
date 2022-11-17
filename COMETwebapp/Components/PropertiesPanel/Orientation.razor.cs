namespace COMETwebapp.Components.PropertiesPanel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using Microsoft.AspNetCore.Components;
    using System.Threading.Tasks;

    public partial class Orientation
    {

        [Parameter]
        public IValueSet? ValueSet { get; set; }

        [Parameter]
        public CompoundParameterType? OrientationParameterType { get; set; }

        [Parameter]
        public DetailsComponentBase? DetailsComponent { get; set; }

        public string AngleFormat { get; set; } = "Degrees";

        public double[]? OrientationMatrix { get; set; } 

        public double Rx { get; set; }
        public double Ry { get; set; }
        public double Rz { get; set; }


        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                this.OrientationMatrix = this.ValueSet.ParseIValueToRotationMatrix();
                var eulerAngles = this.OrientationMatrix.ToEulerAngles(Utilities.AngleFormat.Degrees);
                this.Rx = eulerAngles[0];
                this.Ry = eulerAngles[1];
                this.Rz = eulerAngles[2];
                this.StateHasChanged();
            }
        }

        public void OnEulerAnglesChanged(string sender, ChangeEventArgs e)
        {
            var type = e.Value.GetType();
            var valueText = e.Value as string;

            if(double.TryParse(valueText, out var value))
            {
                switch (sender)
                {
                    case "Rx": this.Rx = value; break;
                    case "Ry": this.Ry = value; break;
                    case "Rz": this.Rz = value; break;
                }
            }

            var eulerAngles = new double[] { this.Rx, this.Ry, this.Rz };

            Enum.TryParse<Utilities.AngleFormat>(this.AngleFormat, out var angleFormat);
            this.OrientationMatrix = eulerAngles.ToRotationMatrix(angleFormat);

            for(int i = 0; i< this.OrientationMatrix.Length; i++)
            {
                this.DetailsComponent.OnParameterValueChange(i, new ChangeEventArgs() { Value = this.OrientationMatrix[i].ToString() });
            }

            this.StateHasChanged();
        }

        public void OnAngleFormatChanged(ChangeEventArgs e)
        {
            this.AngleFormat = e.Value as string;
        }
    }
}
