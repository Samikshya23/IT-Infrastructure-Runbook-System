using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class AccessControlDashboardModel
    {
        public int TotalUsers { get; set; }

        public int TotalRoles { get; set; }

        public int TotalMenus { get; set; }

        public int AssignedAccess { get; set; }

        public List<AccessControlUserModel> Users { get; set; }

        public AccessControlDashboardModel()
        {
            Users = new List<AccessControlUserModel>();
        }
    }
}