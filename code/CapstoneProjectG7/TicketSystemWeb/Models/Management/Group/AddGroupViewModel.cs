﻿using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.ViewModels
{
    public class AddGroupViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? SelectedManagerId { get; set; }
        public List<Employee> AllEmployees { get; set; } = new();
        public List<string> SelectedEmployeeIds { get; set; } = new();
    }
}
