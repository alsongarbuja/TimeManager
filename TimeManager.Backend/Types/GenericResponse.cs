namespace TimeManager.Backend.Types
{
    public class GenericResponse
    {
        public int StatusCode { get; set; }

        public required string Status { get; set; }
        
        public required string Message { get; set; }
    }
}
