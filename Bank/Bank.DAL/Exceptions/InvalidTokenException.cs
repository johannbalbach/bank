namespace Bank.DAL.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base("Oops! This token is already invalid")
        {
        }

        public InvalidTokenException(string message) : base(message)
        {

        }
    }
}
