namespace VisualGrep.Models
{
    public class FileLine
    {
        internal int index;

        public int Number => this.index + 1;

        public string Text { get; set; } = string.Empty;
    }
}
