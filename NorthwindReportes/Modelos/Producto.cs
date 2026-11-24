namespace NorthwindReportes.Modelos;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int Existencias { get; set; }
    public int? CategoriaId { get; set; }
    public string Categoria { get; set; } = "Sin categoría";
    public bool Activo { get; set; }
}
