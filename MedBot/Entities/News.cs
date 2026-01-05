namespace MedBot.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public int MessageId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public ICollection<NewsKeyWord> NewsKeyWords { get; set; }
    }
}
