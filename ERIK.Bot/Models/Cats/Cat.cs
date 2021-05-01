using System.Collections.Generic;

namespace ERIK.Bot.Models.Cats
{
    public class Cat
    {
        public List<object> breeds { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}