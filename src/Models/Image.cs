namespace Models
{
    public class Image
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }
    }
}
