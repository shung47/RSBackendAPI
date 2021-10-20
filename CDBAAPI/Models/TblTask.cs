using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTask
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string Region { get; set; }
        public string Department { get; set; }
        public string Summary { get; set; }
        public string ReferenceNumber { get; set; }
        public bool IsDeleted { get; set; }
    }
}
