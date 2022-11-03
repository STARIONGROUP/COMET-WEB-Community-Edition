namespace COMETwebapp.Utilities
{
    public static class MatrixExtensions
    {
        public static double[] ToEulerAngles(this double[] rotMatrix)
        {
            double Rx = 0, Ry = 0, Rz = 0;
            if (rotMatrix[6]!= 1 && rotMatrix[6]!= -1)
            {
                Ry = -Math.Asin(rotMatrix[6]);
                Rx = Math.Atan2(rotMatrix[7]/Math.Cos(Ry), rotMatrix[8]/Math.Cos(Ry));
                Rz = Math.Atan2(rotMatrix[3] / Math.Cos(Ry), rotMatrix[0] / Math.Cos(Ry));
            }
            else
            {
                Rz = 0;
                if (rotMatrix[6] == -1)
                {
                    Ry = Math.PI / 2.0;
                    Rx = Ry + Math.Atan2(rotMatrix[1],rotMatrix[2]);
                }
                else
                {
                    Ry = -Math.PI / 2.0;
                    Rx = -Ry + Math.Atan2(-rotMatrix[1],-rotMatrix[2]);
                }
            }
            return new double[] { Rx, Ry, Rz };
        }

    }
}
