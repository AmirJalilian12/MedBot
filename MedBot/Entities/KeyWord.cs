namespace MedBot.Entities
{
    public class KeyWord
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public ICollection<NewsKeyWord> NewsKeyWords { get; set; }
    }
}
