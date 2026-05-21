namespace EmployeeAccessSystem.Models
{
    public class ReportCategory
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }

        public ReportCategory()
        {
            Name = string.Empty;
        }
    }
}