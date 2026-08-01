namespace Host.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ProducesErrorAttribute : Attribute
    {
        public string ErrorCode { get; }

        public ProducesErrorAttribute(string errorCode)
        {
            ErrorCode = errorCode;
        }
    }
}
