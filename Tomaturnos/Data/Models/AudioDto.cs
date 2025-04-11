namespace TomaTurnos.Data.Models
{
    public class AudioDto(int? id, string nombre)
    {
        public int? Id { get; set; } = id;
        public string Nombre { get; set; } = nombre;
        public byte[]? Content { get; set; }
    }
}