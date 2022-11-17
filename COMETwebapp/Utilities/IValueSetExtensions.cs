namespace COMETwebapp.Utilities
{
    using CDP4Common.EngineeringModelData;

    public static class IValueSetExtensions
    {
        /// <summary>
        /// Parses an <see cref="IValueSet"/> to translations along main axes
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>An array of type [X,Y,Z]</returns>
        public static double[] ParseIValueToPosition(this IValueSet? valueSet)
        {
            double x = 0, y = 0, z = 0;

            if (valueSet is not null)
            {
                double.TryParse(valueSet.ActualValue[0], out x);
                double.TryParse(valueSet.ActualValue[1], out y);
                double.TryParse(valueSet.ActualValue[2], out z);
            }
            return new double[] { x, y, z };
        }

        /// <summary>
        /// Parses an <see cref="IValueSet"/> to rotation matrix
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>And array of type [Rx,Ry,Rz]</returns>
        public static double[] ParseIValueToRotationMatrix(this IValueSet? valueSet)
        {
            double[] rotMatrix = new double[9];

            if (valueSet is not null)
            {

                if (valueSet.ActualValue.Any(x => { return (x == "-" || x == string.Empty); }))
                {
                    rotMatrix[0] = rotMatrix[4] = rotMatrix[8] = 1.0;
                }
                else
                {
                    for (int i = 0; i < 9; i++)
                    {
                        rotMatrix[i] = double.Parse(valueSet.ActualValue[i]);
                    }
                }
            }

            return rotMatrix;
        }

        /// <summary>
        /// Parses an <see cref="IValueSet"/> to Euler Angles
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>And array of type [Rx,Ry,Rz]</returns>
        public static double[] ParseIValueToEulerAngles(this IValueSet? valueSet)
        {
            return valueSet.ParseIValueToRotationMatrix().ToEulerAngles();
        }


    }
}
