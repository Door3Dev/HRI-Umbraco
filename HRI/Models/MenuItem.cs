using System.Collections.Generic;

namespace HRI.Models
{
    /// <summary>
    /// Menu item (header or footer)
    /// </summary>
    public class MenuItem
    {
        public string Label { get; set; }
        public bool ShouldOpenInTheNewTab { get; set; }
        public string Url { get; set; }
        public string LinkType { get; set; }
        public string FileUrl { get; set; }

        public IList<MenuItem> Items = new List<MenuItem>();
    }
}