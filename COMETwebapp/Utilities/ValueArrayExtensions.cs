namespace COMETwebapp.Utilities
{
    using CDP4Common.Types;

    public static class ValueArrayExtensions
    {
        public static bool ContainsSameValues<T>(this ValueArray<T> valueArray, ValueArray<T> comparison) where T : class
        {
            if (valueArray.Count != comparison.Count)
            {
                return false;
            }

            for (int i = 0; i < valueArray.Count; i++)
            {
                if (valueArray[i] != comparison[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
