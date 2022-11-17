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

        public string AngleFormat { get; set; } = "Degrees";

        public double[] OrientationMatrix { get; set; } 

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

        public void OnEulerAnglesChanged(CustomChangeEventArgs e)
        {
            if(double.TryParse(e.Value.ToString(), out var value))
            {
                switch (e.Sender)
                {
                    case "Rx": this.Rx = value; break;
                    case "Ry": this.Ry = value; break;
                    case "Rz": this.Rz = value; break;
                }
            }

            var eulerAngles = new double[] { this.Rx, this.Ry, this.Rz };
            this.OrientationMatrix = eulerAngles.ToRotationMatrix(Utilities.AngleFormat.Degrees);

            for(int i = 0; i< this.OrientationMatrix.Length; i++)
            {
                this.ValueSet.ActualValue[i] = this.OrientationMatrix[i].ToString();
            }

            this.StateHasChanged();
        }

        public void OnMatrixChanged(int changedIndex, ChangeEventArgs e)
        {
            //Recalculate Rx,Ry,Rz
        }

        public void OnAngleFormatChanged(ChangeEventArgs e)
        {
            this.AngleFormat = e.Value as string;
        }
    }
}
