using System.Text;

namespace Bank.BL.Other
{
    public static class StringGenerator
    {
        public static string GenerateRandomString(int min, int max)
        {
            StringBuilder result = new(max);
            Random random = new(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < max; i++)
            {
                result.Append(random.Next(min, max));
            }

            return result.ToString();
        }
    }
}
