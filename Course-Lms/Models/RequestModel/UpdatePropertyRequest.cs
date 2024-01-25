namespace Course_Lms.Models.RequestModel
{
    public class UpdatePropertyRequest
    {
        public int EntityId { get; set; }
        public string PropertyName { get; set; } // Adjust the type as needed
        public string NewValue { get; set; } // Adjust the type as needed

    }
}
