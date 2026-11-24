# Reportes: diseño, vista y generación de PDF

Programación III · Sesión asíncrona · 28 de noviembre de 2026

## Resultado esperado

El taller comprende la construcción de un reporte de productos activos con existencias menores a un límite elegido, agrupados por categoría. El trabajo se realiza sobre Northwind mediante ADO.NET y Blazor. El resultado incluye una vista del reporte y un archivo PDF guardado mediante la impresión del navegador.

La guía abarca la selección de datos, la agrupación, los conteos y la presentación. La actividad individual consiste en ampliar el reporte con un filtro de categoría combinado con el límite de existencias. La explicación de resultados deberá justificar la selección de productos y la correspondencia entre la vista y el PDF.

**Modalidad:** individual y asíncrona.  
**Tiempo estimado:** 120 minutos con el entorno operativo. La instalación de herramientas y el repaso de conceptos previos requieren tiempo adicional. 

## Índice

1. [Preparación y comprobación del entorno](#comenzar)
2. [Parte 1: selección y comprobación de datos](#parte-1)
3. [Parte 2: presentación y salida en PDF](#parte-2)
4. [Actividad individual](#actividad)
5. [Entrega y criterios](#entrega)
6. [Resolución de dificultades](#ayuda)
7. [Referencias](#referencias)

<a id="comenzar"></a>
## Preparación y comprobación del entorno

Se requiere .NET 10, Visual Studio con desarrollo web o una terminal, MySQL 8.0.16 o posterior, la adaptación de Northwind para MySQL utilizada en el curso y Edge o Chrome.

El proyecto de partida se obtiene del repositorio [jaime-oteima/northwind-reportes](https://github.com/jaime-oteima/northwind-reportes). El repositorio contiene una única aplicación, `NorthwindReportes`, y la carpeta `base-datos`. Se trabajará sobre una copia local de esa aplicación durante todo el taller. El punto de partida incluye la conexión y el listado de productos; el reporte y su ampliación se incorporan siguiendo esta guía.

Si la base Northwind del curso ya está disponible, no es necesario importarla nuevamente. Para una instalación nueva, el procedimiento se encuentra en `base-datos/README.md`. La importación se realiza una sola vez en una instancia de práctica que no contenga otra Northwind con trabajos. Ante un error, se deberá revisar qué tablas se crearon antes de repetir el procedimiento.

La solución se abre mediante `NorthwindReportes.slnx`. La comprobación de los SDK instalados y la restauración de paquetes se realizan desde una terminal ubicada en la carpeta `NorthwindReportes`, donde se encuentra `NorthwindReportes.csproj`:

```powershell
dotnet --list-sdks
dotnet restore
```

La conexión de desarrollo se debe configurar fuera del código. Los marcadores de servidor, puerto, usuario y contraseña deberán sustituirse por los datos de la instalación local:

```powershell
dotnet user-secrets set "ConnectionStrings:Northwind" "Server=localhost;Port=3306;Database=northwind;User ID=TU_USUARIO;Password=TU_CLAVE;"
dotnet run --launch-profile http
```

Las contraseñas, las capturas de la cadena de conexión y los archivos de secretos no deberán publicarse en GitHub. Los secretos de desarrollo se guardan separados del proyecto, pero no están cifrados y no constituyen un almacén para producción.

La aplicación estará disponible en `http://localhost:5183`. En la página Productos deberá comprobarse la lectura de datos. La muestra original contiene 77 productos. Si la base fue modificada, el total deberá contrastarse con `SELECT COUNT(*) FROM northwind.Products;`. Un error de conexión no equivale a cero productos.

La aplicación se detiene con Ctrl+C en la terminal desde la que se ejecutó.

<a id="parte-1"></a>
## Parte 1: selección y comprobación de datos

### 1. Propósito y contenido del reporte

Una revisión de inventario requiere responder la siguiente pregunta: **¿qué productos activos tienen existencias inferiores a un límite elegido, agrupados por categoría?** El límite constituye un criterio de revisión para este ejercicio, no una regla real de reposición de Northwind.

Un reporte selecciona y organiza información para responder una pregunta. En este caso incluye título, criterio aplicado, encabezados de categoría, ID, nombre y existencias de cada producto, conteo por categoría y total general. El ID permite contrastar cada fila con la base.

En Northwind, `Discontinued = 0` identifica los productos considerados activos en el reporte. `UnitsInStock` representa las existencias. El límite es estricto: se utiliza `<`, no `<=`. Con un límite de 10, un producto con exactamente 10 unidades queda fuera. Un producto activo con 0 unidades puede aparecer cuando el límite es mayor que 0.

### 2. Recorrido de la consulta y espera asíncrona

La revisión inicial comprende `Modelos/Producto.cs`, `Datos/ProductosAdo.cs`, `Program.cs` y `Components/Pages/Productos.razor`. El modelo representa un producto; la clase de datos consulta la base; el componente presenta el resultado.

En `Program.cs` se encuentra el registro de la clase de acceso a datos:

```csharp
builder.Services.AddScoped<NorthwindReportes.Datos.ProductosAdo>();
```

El registro permite que Blazor proporcione un objeto de esa clase al componente. La recepción del objeto se declara con:

```razor
@inject NorthwindReportes.Datos.ProductosAdo Datos
```

`NorthwindReportes.Datos.ProductosAdo` es el tipo del servicio y `Datos` es el nombre de la propiedad utilizada para invocar sus métodos. `AddScoped` conserva el objeto dentro del ámbito administrado por Blazor. La clase no mantiene una conexión abierta permanente: cada consulta crea y libera su conexión.

#### Introducción a `async` y `await`

Una consulta puede tardar en responder. En este proyecto se utiliza programación asíncrona para esperar operaciones de acceso a datos sin mantener bloqueado el recurso de ejecución durante esa espera. El método que necesita los datos continúa cuando la operación termina.

`async` es un modificador que permite utilizar `await` dentro de un método. `await` es un operador que permite esperar la finalización de una operación asíncrona y obtener su resultado, si lo tiene. Si la operación sigue pendiente, el método se suspende en ese punto; si ya terminó, continúa sin esa suspensión. Agregar `async` no crea por sí solo otro hilo ni hace más rápida una consulta.

Una solicitud de documentos a un archivo permite ilustrar la diferencia entre una operación y su resultado: el comprobante representa la solicitud, mientras que los documentos son el resultado esperado. De manera semejante, `Task` representa una operación y permite conocer su finalización o fallo. `Task<List<Producto>>` representa una operación cuyo resultado será una lista si termina correctamente. La operación puede estar ya completada cuando se recibe el `Task`.

Dentro de un método asíncrono, el recorrido puede escribirse en dos instrucciones:

```csharp
Task<List<Producto>> consulta = Datos.ListarAsync();
List<Producto> productos = await consulta;
```

`consulta` representa la operación; `productos` contiene la lista recibida. La llamada inicia el recorrido. Si la operación está pendiente, `await` suspende ese método y permite que el recurso de ejecución atienda otro trabajo. Cuando la operación termina, el método continúa; si falla, el error llega al `catch`. La espera asíncrona no acelera la consulta ni convierte un `Task` en un hilo.

La forma abreviada es `productos = await Datos.ListarAsync();`. El modificador `async` permite utilizar `await` en el método. Un método que devuelve la lista utiliza `Task<List<Producto>>`; uno que actualiza el estado del componente sin devolver un valor utiliza `Task`. En este recorrido no se utiliza `.Result` para sustituir la espera asíncrona.

La directiva `@rendermode @(new InteractiveServerRenderMode(prerender: false))` activa la interacción desde el servidor y desactiva la representación inicial estática de esa página, para evitar dos ejecuciones de su carga inicial. Esta configuración no modifica la consulta SQL.

**Comprobación.** En el componente Productos deberán identificarse la llamada, el `await`, la lista que recibe el resultado y el bloque que informa un fallo. La explicación deberá distinguir por qué `consulta` no puede recorrerse como si ya fuera la lista de productos.

#### Actividad de comprensión: propósito de `async` y `await`

Como parte del taller, se deberá consultar la documentación de Microsoft sobre [programación asíncrona con async y await](https://learn.microsoft.com/es-es/dotnet/csharp/asynchronous-programming/). Con apoyo de esa lectura y del código del proyecto, se deberá elaborar una explicación en palabras propias que responda:

1. ¿Qué es `async` y qué permite hacer dentro de un método?
2. ¿Qué es `await` y qué ocurre antes, durante y después de esperar una consulta?
3. ¿Para qué se utilizan en este proyecto? 
4. ¿Qué diferencia existe entre `Task<List<Producto>>` y `List<Producto>`?

La explicación deberá señalar un método del proyecto declarado con `async` y una instrucción que utilice `await`. A partir de ese ejemplo, deberá describirse qué operación se espera, qué resultado se obtiene y qué instrucción puede continuar después. La definición general deberá acompañarse de esta aplicación al código; no será suficiente copiar las definiciones de la documentación.

Esta explicación se incluirá en el documento de entrega, en un apartado titulado **Uso de async y await**.

### 3. Incorporación de la consulta del reporte

Con la aplicación detenida, se deberá agregar el siguiente método dentro de `ProductosAdo`, antes de la llave que cierra la clase. Los métodos existentes deberán conservarse. Después de incorporarlo, se deberá compilar el proyecto.

```csharp
public async Task<List<Producto>> ConsultarReporteAsync(int limite)
    {
        if (limite < 0 || limite > 32767)
        {
            throw new ArgumentOutOfRangeException(nameof(limite));
        }
        const string sql = """
            SELECT p.ProductID, p.ProductName, p.UnitsInStock,
                   p.CategoryID, c.CategoryName, p.Discontinued
            FROM Products AS p
            LEFT JOIN Categories AS c ON p.CategoryID = c.CategoryID
            WHERE p.Discontinued = 0 AND p.UnitsInStock < @limite
            ORDER BY c.CategoryName, p.CategoryID,
                     p.UnitsInStock, p.ProductName, p.ProductID;
            """;
        using MySqlConnection conexion = new MySqlConnection(cadenaConexion);
        using MySqlCommand comando = new MySqlCommand(sql, conexion);
        comando.Parameters.Add("@limite", MySqlDbType.Int32).Value = limite;
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
```

`LEFT JOIN` conserva un producto aunque no tenga categoría. `CategoriaId` es `int?`, que permite un identificador ausente. La lectura existente comprueba `IsDBNull` antes de obtenerlo y conserva «Sin categoría» como nombre visible. Un `INNER JOIN` excluiría esos productos.

El SQL selecciona productos activos con existencias inferiores al parámetro. `@limite` recibe un valor tipado; la entrada no se concatena al texto SQL. `using` libera lector, comando y conexión incluso si ocurre un error. `CommandBehavior.Default` selecciona la sobrecarga del proveedor que devuelve el lector MySQL utilizado en este ejemplo.

El orden organiza primero las categorías y después las existencias, el nombre y el ID de producto. La identidad de una categoría es su ID: dos categorías con el mismo nombre siguen siendo distintas.

**Comprobación del límite.** Se deberá identificar un producto activo y contrastar qué ocurre cuando sus existencias coinciden exactamente con el límite. La explicación deberá indicar cómo cambiaría el resultado al utilizar `<=`. Para el reporte se conserva `<`.

<a id="parte-2"></a>
## Parte 2: presentación y salida en PDF

### 4. Construcción de la vista agrupada

Se deberá crear `Components/Pages/ReporteInventario.razor` con el siguiente contenido. La página Productos se conserva como comprobación del acceso a datos.

```razor
@page "/reporte"
@rendermode @(new InteractiveServerRenderMode(prerender: false))
@using NorthwindReportes.Modelos
@inject NorthwindReportes.Datos.ProductosAdo Datos

<PageTitle>Reporte de inventario</PageTitle>
<div class="no-imprimir">
    <h1>Preparar reporte</h1>
    <p>Productos activos agrupados por categoría.</p>
    <label class="form-label" for="limite">Límite de existencias</label>
    <input id="limite" class="form-control mb-3" type="text" inputmode="numeric" @bind="limiteTexto" disabled="@cargando" />
    <button class="btn btn-primary mb-3" @onclick="GenerarAsync" disabled="@cargando">Generar reporte</button>
    @if (cargando) { <p role="status">Generando reporte...</p> }
    @if (error != "") { <p class="alert alert-danger" role="alert">@error</p> }
    @if (!generado && !cargando && error == "") { <p>Ingresa un límite entero entre 0 y 32767 y genera el reporte.</p> }
</div>
@if (generado)
{
    <section class="reporte" aria-label="Resultado del reporte">
        <h1>Productos activos con existencias bajas</h1>
        <p>Existencias inferiores a @limiteAplicado. Categorías: todas.</p>
        <p>El límite es un criterio de revisión, no una regla de reposición.</p>
        @if (productos.Count == 0) { <p>No hay productos que cumplan los criterios.</p> }
        @foreach (var grupo in productos.GroupBy(producto => producto.CategoriaId))
        {
            <section class="grupo">
                <h2>@grupo.First().Categoria</h2>
                <table class="table table-striped">
                    <thead><tr><th scope="col">ID</th><th scope="col">Producto</th><th scope="col">Existencias</th></tr></thead>
                    <tbody>@foreach (Producto producto in grupo)
                    {
                        <tr><td>@producto.Id</td><td>@producto.Nombre</td><td>@producto.Existencias</td></tr>
                    }</tbody>
                </table>
                <p>Productos en esta categoría: @grupo.Count()</p>
            </section>
        }
        <p><strong>Total de productos: @productos.Count</strong></p>
    </section>
}
else
{
    <p class="solo-imprimir">No hay un reporte generado. Regresa a la aplicación, revisa los datos y genera el reporte antes de imprimir.</p>
}

@code {
    private string limiteTexto = "10";
    private int limiteAplicado;
    private bool cargando;
    private bool generado;
    private string error = "";
    private List<Producto> productos = new List<Producto>();

    private async Task GenerarAsync()
    {
        generado = false;
        productos.Clear();
        error = "";
        if (!int.TryParse(limiteTexto, out int limite) || limite < 0 || limite > 32767)
        {
            error = "Escribe un número entero entre 0 y 32767.";
            return;
        }
        cargando = true;
        try
        {
            productos = await Datos.ConsultarReporteAsync(limite);
            limiteAplicado = limite;
            generado = true;
        }
        catch (Exception)
        {
            error = "No se pudo generar el reporte. Revisa la conexión y vuelve a intentar. Este fallo no significa que haya cero coincidencias.";
        }
        finally { cargando = false; }
    }
}
```

El enlace a la nueva página se incorpora en `Components/Layout/NavMenu.razor`, dentro de `nav`:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="reporte">Reporte</NavLink>
</div>
```

El botón llama a `GenerarAsync`. El método retira el resultado anterior y valida que la entrada sea un entero de 0 a 32767. Después espera la consulta, guarda el límite aplicado y habilita la salida. `finally` retira el estado de carga tanto si la consulta termina correctamente como si falla.

`limiteTexto` representa el valor que se está editando; `limiteAplicado` describe los datos recibidos. Si la entrada cambia de 10 a 20 sin generar nuevamente, el encabezado conserva 10 porque los datos todavía corresponden a ese límite.

`productos.GroupBy(producto => producto.CategoriaId)` forma grupos por ID de categoría. La expresión después de `=>` selecciona la clave de cada producto. El primer `foreach` recorre los grupos y el segundo recorre los productos de cada grupo. Cada grupo contiene al menos un producto, por lo que `First()` permite obtener su nombre de categoría. `Count()` cuenta los productos del grupo; `productos.Count` cuenta todos los productos seleccionados. Agrupar reúne por clave; ordenar establece la secuencia de presentación.

**Contraste con SQL.** Se deberá ejecutar el proyecto, abrir Reporte y generar una salida con límite 10. Los IDs y el conteo deberán contrastarse con la siguiente consulta en el cliente SQL:

```sql
SELECT ProductID, ProductName, CategoryID, UnitsInStock
FROM northwind.Products
WHERE Discontinued = 0 AND UnitsInStock < 10
ORDER BY CategoryID, UnitsInStock, ProductName, ProductID;
```

El orden de categorías puede diferir porque la vista las ordena por nombre. La comparación deberá centrarse en los IDs seleccionados y los conteos. También deberá comprobarse el límite 0: en esta muestra las existencias no son negativas, por lo que corresponde un mensaje sin coincidencias y total 0. Este resultado deberá distinguirse de un fallo de conexión.

### 5. Preparación de la impresión y guardado del PDF

Se deberá crear `wwwroot/impresion.css` con estas reglas:

```css
.solo-imprimir { display: none; }
@media print {
    @page { size: letter; margin: 16mm; }
    .sidebar, .top-row, .no-imprimir, #blazor-error-ui, blazor-reconnect-modal { display: none !important; }
    .page, main, article.content { display: block !important; width: 100% !important; padding: 0 !important; margin: 0 !important; }
    .solo-imprimir { display: block; }
    .reporte { color: #000; }
    .reporte h1 { font-size: 20pt; }
    .reporte h2 { font-size: 15pt; break-after: avoid; }
    .reporte table { width: 100%; font-size: 10pt; }
    .reporte thead { display: table-header-group; }
    .reporte tr { break-inside: avoid; }
    .grupo > p { break-before: avoid; }
}
```

En `Components/App.razor`, dentro de `head`, se agrega el enlace después de los otros estilos:

```razor
<link rel="stylesheet" href="@Assets["impresion.css"]" />
```

`@media print` aplica las reglas al imprimir: oculta menú y controles y conserva el reporte. `@page` define márgenes y tamaño de referencia. `break-inside: avoid` evita dividir una fila; no se aplica al grupo completo, porque una categoría grande puede necesitar varias páginas. `thead` permite repetir los encabezados de tabla cuando la tabla continúa en otra página.

Después de generar el reporte, la impresión se abre con Ctrl+P y se selecciona Guardar como PDF. Antes de guardar deberán revisarse el tamaño de papel, los márgenes y la vista previa. Los encabezados y pies añadidos por el navegador se controlan desde su diálogo. El PDF se produce a partir del HTML mostrado, sin una biblioteca de generación en el servidor.

El archivo guardado deberá abrirse y revisarse. Deberá conservar título, límite aplicado, categorías, filas, conteos y total, sin imprimir el menú ni los controles. También deberá probarse un límite mayor para obtener varias páginas y comprobar que las categorías puedan continuar sin perder filas. El archivo PDF forma parte de la entrega; una captura de pantalla no lo sustituye.

<a id="actividad"></a>
## Actividad individual: selección de categoría

La ampliación consiste en agregar una selección de **todas las categorías**, **una categoría específica** o **Sin categoría**, combinada con el límite de existencias. Las opciones deberán corresponder a las categorías reales de la base y utilizar su ID como identidad.

La solución deberá cumplir los siguientes requisitos:

- Combinar producto activo, existencias inferiores al límite y categoría seleccionada cuando corresponda.
- Conservar agrupación, columnas y conteos correctos.
- Mostrar en la vista y en el PDF el límite y la categoría realmente aplicados.
- Distinguir «todas» de «sin categoría»; esta última corresponde a `CategoryID IS NULL`.
- Mostrar un mensaje comprensible y total 0 cuando no existan coincidencias.
- Mantener validación, parámetros SQL y manejo de errores.

La construcción comprende la representación de las tres opciones, la obtención del catálogo de categorías, la adaptación de la consulta y sus parámetros, la incorporación del selector y el registro de la selección aplicada junto con el límite. Cambiar únicamente el número inicial de existencias no constituye la ampliación solicitada.

Se deberán realizar tres comprobaciones: todas las categorías, una categoría específica y cero coincidencias. En cada caso deberán registrarse los valores utilizados, el resultado esperado según la base y el resultado observado. El límite 0 permite obtener un caso vacío si la base conserva existencias no negativas. Si no hay productos sin categoría, esa opción puede devolver cero; no es necesario modificar la base para fabricar resultados.

| Caso | Límite y categoría | Resultado esperado e IDs o conteo | Resultado observado | ¿Coincide? |
|---|---|---|---|---|
| Todas las categorías | Por registrar | Por registrar | Por registrar | Por registrar |
| Una categoría | Por registrar | Por registrar | Por registrar | Por registrar |
| Sin coincidencias | Por registrar | Por registrar | Por registrar | Por registrar |

**Ampliación opcional.** Se podrá explicar cómo distinguir dos categorías con el mismo nombre y cómo comprobar una categoría que continúa en otra página, utilizando los recursos del taller.

<a id="entrega"></a>
## Entrega y criterios

La entrega será individual, en Moodle, **el martes 1 de diciembre de 2026**. 

<a id="entregables"></a>
### Entregables

La entrega deberá incluir:

- **Proyecto publicado en GitHub.** La aplicación completa deberá publicarse en un repositorio de la cuenta de GitHub del estudiante, con el reporte y la ampliación del filtro de categoría, además de las instrucciones de ejecución y configuración de la conexión local. El enlace deberá incluirse en el documento del trabajo y permitir el acceso para su revisión. No deberán publicarse contraseñas ni archivos de secretos.
- **Archivo PDF del reporte.** Deberá corresponder a un caso con resultados generado desde la aplicación ampliada. Deberá mostrar el límite y la categoría aplicados, los productos seleccionados, los conteos por categoría y el total general, de forma coherente con la vista.
- **Documento del trabajo.** Deberá estar redactado en palabras propias y organizado en los siguientes apartados:
  - **Identificación.** Nombre del estudiante, asignatura y título del taller.
  - **Enlace al repositorio de GitHub.** URL completa del repositorio propio del estudiante donde se encuentra publicado el proyecto terminado, incluida la ampliación del filtro de categoría. El enlace deberá ser accesible para su revisión.
  - **Propósito y selección de datos.** Pregunta que responde el reporte, condición utilizada para identificar productos activos.
  - **Implementación del filtro de categoría.** Descripción de los archivos y métodos modificados, cómo se combina la categoría seleccionada con el límite de existencias y cómo se distinguen las opciones «Todas las categorías», una categoría específica y «Sin categoría».
  - **Uso de async y await.** Explicación de qué es cada uno, para qué se utilizan en el proyecto y qué diferencia existe entre `Task<List<Producto>>` y `List<Producto>`. Deberán identificarse un método declarado con `async` y una instrucción con `await`, indicando qué operación se espera, qué resultado se obtiene y qué instrucción continúa después.
  - **Correspondencia entre la vista y el PDF.** Explicación de cómo se comprobó que el encabezado refleja los filtros aplicados y que el PDF conserva los productos seleccionados, los conteos por categoría y el total general de la vista.

### Criterios de evaluación

La revisión se realizará con los siguientes criterios:

| Criterio | Evidencia esperada |
|---|---|
| Selección correcta | Activos, límite estricto y categoría combinados; parámetros SQL. |
| Organización comprensible | Categorías identificadas, detalle legible y conteos de productos coherentes. |
| Consistencia de salida | Vista y PDF conservan criterios, filas y conteos. |
| Explicación del propósito | El documento identifica la pregunta del reporte y la condición utilizada para seleccionar productos activos. |
| Comprensión de la asincronía | Explicación en palabras propias de qué son async y await, su propósito y su aplicación en un método y una espera del proyecto. |
| Ejecución | Proyecto y configuración explicados; sin contraseñas publicadas. |

Antes de la entrega deberán comprobarse la apertura del PDF y el acceso al enlace. El repositorio no deberá incluir `bin`, `obj`, `.vs`, claves ni archivos de secretos.

<a id="ayuda"></a>
## Resolución de dificultades

| Situación | Revisión correspondiente |
|---|---|
| No compila para .NET 10 | Disponibilidad del SDK y carga de desarrollo web de Visual Studio. Se conserva el framework del proyecto. |
| Paquete no restaurado | Ejecución de `dotnet restore` con conexión a NuGet. Se conserva MySql.Data como proveedor. |
| Acceso denegado o servidor inaccesible | Servidor, puerto, servicio MySQL, permisos y configuración local. Las contraseñas no se incluyen al solicitar ayuda. |
| Tabla o columna no encontrada | Adaptación MySQL y mayúsculas de Products y Categories. |
| Puerto ocupado | Comprobación de que no haya una ejecución anterior de la aplicación utilizando el mismo puerto. |
| Clic sin efecto | Directivas `@rendermode` y `@onclick`, y conexión de la página con el servidor. |
| PDF incluye menú | Enlace a impresion.css y coincidencia del nombre del archivo. |
| Total inesperado | Condición de activo, límite estricto, categoría, datos de la base y diferencia entre productos y unidades. |


<a id="referencias"></a>
## Referencias

- [Estructura de Blazor Web App, .NET 10](https://learn.microsoft.com/en-us/aspnet/core/blazor/project-structure?view=aspnetcore-10.0).
- [Tipos de retorno asíncrono en C#](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-return-types).
- [Programación asíncrona con async y await](https://learn.microsoft.com/es-es/dotnet/csharp/asynchronous-programming/).
- [Prerenderizado en Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/prerender?view=aspnetcore-10.0).
- [Métodos asíncronos de MySQL Connector/NET](https://dev.mysql.com/doc/connector-net/en/connector-net-async-methods.html).
- [Estilos de impresión con CSS](https://developer.mozilla.org/en-US/docs/Web/CSS/Guides/Media_queries/Printing).
