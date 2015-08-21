using System.Collections.Generic;

namespace HRI.Models
{
    public class SiteMapItem
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }

        public IList<SiteMapItem> Children;
    }
}