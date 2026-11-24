using MySql.Data.MySqlClient;
using NorthwindReportes.Modelos;

namespace NorthwindReportes.Datos;

public class ProductosAdo
{
    private readonly string cadenaConexion;

    public ProductosAdo(IConfiguration configuracion)
    {
        cadenaConexion = configuracion.GetConnectionString("Northwind") ?? "";
    }

    public async Task<List<Producto>> ListarAsync()
    {
        const string sql = """
            SELECT p.ProductID, p.ProductName, p.UnitsInStock,
                   p.CategoryID, c.CategoryName, p.Discontinued
            FROM Products AS p
            LEFT JOIN Categories AS c ON p.CategoryID = c.CategoryID
            ORDER BY p.ProductID;
            """;
        using MySqlConnection conexion = new MySqlConnection(cadenaConexion);
        using MySqlCommand comando = new MySqlCommand(sql, conexion);
        await conexion.OpenAsync();
        using MySqlDataReader lector = await comando.ExecuteReaderAsync(System.Data.CommandBehavior.Default);
        List<Producto> productos = new List<Producto>();
        while (await lector.ReadAsync())
        {
            Producto producto = LeerProducto(lector);
            productos.Add(producto);
        }
        return productos;
    }

    private static Producto LeerProducto(MySqlDataReader lector)
    {
        Producto producto = new Producto();
        producto.Id = lector.GetInt32("ProductID");
        producto.Nombre = lector.GetString("ProductName");
        producto.Existencias = lector.GetInt32("UnitsInStock");
        producto.Activo = lector.GetInt32("Discontinued") == 0;
        if (!lector.IsDBNull(lector.GetOrdinal("CategoryID")))
        {
            producto.CategoriaId = lector.GetInt32("CategoryID");
        }
        if (!lector.IsDBNull(lector.GetOrdinal("CategoryName")))
        {
            producto.Categoria = lector.GetString("CategoryName");
        }
        return producto;
    }
}

