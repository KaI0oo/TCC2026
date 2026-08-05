namespace INTERFACE_POSTRATA.Validators
{
    public class ValidationResult<T>
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Value { get; set; }
    }
}
