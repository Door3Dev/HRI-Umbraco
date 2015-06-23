using System.Collections.Generic;

namespace HRI.Models
{
    public class MemberCenterMenuItem
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public IEnumerable<MemberCenterMenuItem> Items;
    }
}