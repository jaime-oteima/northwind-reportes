-- Ejecutar después de importar, sin modificar datos.
USE `northwind`;

UNION ALL SELECT 'OrderDetails', 279, COUNT(*), COUNT(*)-279 FROM OrderDetails;

-- No deben aparecer líneas sin pedido o producto ni pedidos sin líneas.
SELECT COUNT(*) AS DetallesSinRelacion
FROM OrderDetails d
LEFT JOIN Orders o ON o.OrderID=d.OrderID
LEFT JOIN Products p ON p.ProductID=d.ProductID
WHERE o.OrderID IS NULL OR p.ProductID IS NULL;

SELECT COUNT(*) AS PedidosSinDetalle
FROM Orders o LEFT JOIN OrderDetails d ON d.OrderID=o.OrderID
WHERE d.OrderID IS NULL;

-- Comprueba una consulta útil para el catálogo con sus relaciones.
SELECT p.ProductID, p.ProductName, c.CategoryName, s.CompanyName AS Supplier,
       p.UnitPrice, p.UnitsInStock, p.Discontinued
FROM Products p
LEFT JOIN Categories c ON c.CategoryID=p.CategoryID
LEFT JOIN Suppliers s ON s.SupplierID=p.SupplierID
ORDER BY p.ProductID
LIMIT 10;
