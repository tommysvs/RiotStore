# Riot Merch

## Descripción general

**Riot Merch** es una plataforma de e-commerce construida con **.NET 8** que integra un sistema de procesamiento de órdenes en tiempo real utilizando **Apache Kafka**. El proyecto simula compras masivas, procesa inventario de forma asincrónica y proporciona análisis detallados del comportamiento de compra.

### Características principales:
- Catálogo de productos con categorías
- Gestión de inventario con detección de overselling
- Procesamiento asincrónico de órdenes via Kafka
- Dashboard con estadísticas en tiempo real
- Simulador de compras para pruebas de carga
- Análisis de intentos de compra por categoría y segmento

---

## Requisitos previos

Antes de instalar el proyecto, asegúrate de tener instalado:

- **.NET 8 SDK** → [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 14+** → [Descargar](https://www.postgresql.org/download/)
- **Apache Kafka 3.0+** → [Descargar](https://kafka.apache.org/downloads)
- **Visual Studio 2022+**
- **Docker** → [Descargar](https://www.docker.com/products/docker-desktop)
- **Docker Compose** (incluido en el proyecto)
- **Git** → [Descargar](https://git-scm.com/)

---

## Instalación y configuración

### Clonar el repositorio

```bash
git clone https://github.com/tommysvs/RiotStore.git
cd RiotStore
```

### Configurar base de datos (postgresql)

```bash
# Crear base de datos
psql -U postgres -c "CREATE DATABASE riotstore;"

# Crear usuario (opcional)
psql -U postgres -c "CREATE USER riotstore WITH PASSWORD 'password123';"
psql -U postgres -c "ALTER DATABASE riotstore OWNER TO riotstore;"

```

### Configurar kafka

```bash
# Iniciar zookeeper (en una terminal)
bin/zookeeper-server-start.sh config/zookeeper.properties

# Iniciar kafka (en otra terminal)
bin/kafka-server-start.sh config/server.properties

# Crear el topic de órdenes
bin/kafka-topics.sh --create --topic order-events --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1
```

### Actualizar cadena de conexión

Editar `appsettings.json` en ambos proyectos:

**RiotStore.API/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=riotstore;User Id=riotstore;Password=password123;"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

**RiotStore.Consumer/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=riotstore;User Id=riotstore;Password=password123;"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

### Restaurar dependencias e iniciar

```bash
# Restaurar dependencias
dotnet restore

# Construir la solución
dotnet build

# Migrar base de datos (si aplica)
dotnet ef database update -p RiotStore.Infrastructure -s RiotStore.API

# Ejecutar api
cd RiotStore.API
dotnet run

# En otra terminal, ejecutar consumer
cd RiotStore.Consumer
dotnet run
```

### Verificar que todo funciona

- **API:** http://localhost:5000/swagger
- **Kafka:** Verificar logs del consumer en consola

---

## Estructura de proyectos

La solución está compuesta por **4 proyectos** interconectados:

---

## **1. RiotStore.Shared**
### Modelos y eventos compartidos

Este proyecto contiene los DTOs y eventos que se utilizan en toda la solución. Es la capa de comunicación entre API y Consumer.

### Estructura:
```
RiotStore.Shared/
├── Events/
│   └── OrderCreatedEvent.cs
│
└── Dtos/
    ├── GeneratorStatsDto.cs
    └── SimulatorMetricsDto.cs
```

### Modelos y clases

#### **OrderCreatedEvent**
Evento que se publica en Kafka cuando se crea una orden.

#### **OrderItemDto**
Representa un producto dentro de una orden.

#### **GeneratorStatsDto**
Estadísticas de generación de eventos de compra.

#### **SimulatorMetricsDto**
Estadísticas de simulación de lotes de compra.

---

## **2. RiotStore.Infrastructure**
### Acceso a datos y repositorios

Contiene la configuración de Entity Framework, modelos de base de datos y repositorios para acceso a datos.

### Estructura:
```
RiotStore.Infrastructure/
├── Data/
│   ├── RiotStoreDbContext.cs
└── Repositories/
    ├── Interfaces/
    │   ├── IProductRepository.cs
    │   ├── ICategoryRepository.cs
    │   ├── IStockRepository.cs
    │   └── IOrderRepository.cs
    └── Implementations/
        ├── ProductRepository.cs
        ├── CategoryRepository.cs
        ├── StockRepository.cs
        └── OrderRepository.cs
```

### RiotStoreDbContext
Contexto de Entity Framework que gestiona todas las entidades y relaciones.

### Repositorios

#### **IProductRepository** y **ProductRepository**

`GetAllProductsAsync()`  
Obtiene todos los productos activos con sus categorías.

`GetProductByIdAsync(int productId)`  
Obtiene un producto específico por ID.

`GetProductsByCategoryAsync(int categoryId)`  
Obtiene productos de una categoría específica.

#### **ICategoryRepository** y **CategoryRepository**

`GetAllCategoriesAsync()`  
Obtiene todas las categorías.

#### **IStockRepository** y **StockRepository**

`GetByProductIdAsync(int productId)`  
Obtiene balance de stock de un producto.

`GetAllAsync()`  
Obtiene todos los balances de stock.

`UpdateOrCreateAsync(int productId, int initialStock, int totalAttempts, int currentBalance)`  
Crea o actualiza balance de stock.

#### **IOrderRepository** y **OrderRepository**

`CreateOrderAsync(string fullName, string email, string address, string city, string state, string zipCode, List<OrderItemDto> items, string paymentMethod)`  
Crea una nueva orden con detalles.

---

## **3. RiotStore.API**
### Servicios web y endpoints

Servidor ASP.NET Core que expone endpoints REST para interactuar con la plataforma. Incluye controladores para productos, checkout, dashboard y simulación.

### Estructura:
```
RiotStore.API/
├── Controllers/
│   ├── ProductsController.cs
│   ├── CheckoutController.cs
│   ├── DashboardController.cs
│   └── SimulatorController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IKafkaProducerService.cs
│   │   ├── IDataGeneratorService.cs
│   │   └── ISimulatorService.cs
│   └── Implementations/
│       ├── KafkaProducerService.cs
│       ├── DataGeneratorService.cs
│       └── SimulatorService.cs
├── DTOs/
│   ├── CheckoutRequestDto.cs
│   ├── DashboardStatisticsDto.cs
│   └── StockBalanceDetailDto.cs
└── wwwroot/
    ├── index.html
    └── js/
```

### Servicios

#### **IKafkaProducerService** y **KafkaProducerService**

`SendOrderCreatedEventAsync(OrderCreatedEvent orderEvent)`  
Envía un evento de orden creada a Kafka.

**Nota:** Conecta a Kafka en el topic `order-events`. La configuración de bootstrap server viene de `appsettings.json` (Kafka:BootstrapServers).

#### **IDataGeneratorService** y **DataGeneratorService**

`GenerateSinglePurchaseAttemptAsync()`  
Genera un evento de compra individual aleatorio.

`GenerateBatchAsync(int count, string? targetProductCategory, bool simulatePeakHour)`  
Genera un lote de eventos de compra.

**Detalles de generación:**
- **Categorías con pesos de demanda:** Estatuas (30%), Coleccionables (35%), Ropa (25%), Peluches (10%)
- **Segmentos de cliente:** high-demand (20%), mid-demand (50%), low-demand (30%)
- **Cantidades:** high-demand (1-5), mid-demand (1-3), low-demand (1-2)
- **Peak hour multiplier:** Si `simulatePeakHour=true`, multiplica cantidades por 1.5

#### **ISimulatorService** y **simulatorservice**

`SimulatePurchaseAttemptAsync(int productId, string productName, int quantity)`  
Simula un intento de compra individual.

`SimulateBatchPurchaseAsync(List<(int, string, int)> purchases)`  
Simula múltiples intentos de compra.

`SimulateBatchWithMetricsAsync(int quantity, int batchCount)`  
Simula lotes y registra métricas de rendimiento.

###  Controladores y endpoints

#### **ProductsController** - base: `/api/products`

HTTP: **GET**  
`/api/products`  
Obtiene todos los productos con stock actual.

HTTP: **GET**  
`/api/products/categories`  
Obtiene todas las categorías.

HTTP: **GET**  
`/api/products/category/{categoryId}`  
Obtiene productos de una categoría.

HTTP: **GET**  
`/api/products/{productId}`  
Obtiene detalles de un producto específico.

---

#### **CheckoutController** - base: `/api/checkout`

HTTP: **POST**  
`/api/checkout`  
Procesa una orden de compra.

---

#### **DashboardController** - base: `/api/dashboard`

HTTP: **GET**  
`/api/dashboard/stock`  
Obtiene balance de stock de todos los productos.

HTTP: **GET**  
`/api/dashboard/stock/{productId}`  
Obtiene balance de stock de un producto.

HTTP: **GET**  
`/api/dashboard/statistics`  
Obtiene estadísticas globales de ventas.

HTTP: **GET**  
`/api/dashboard/benchmarks`  
Obtiene métricas de benchmarks de generación.

HTTP: **GET**  
`/api/dashboard/purchase-attempts/summary`  
Resumen de intentos de compra.

HTTP: **GET**  
`/api/dashboard/purchase-attempts/by-category`  
Intentos de compra agrupados por categoría.

---

#### **SimulatorController** - base: `/api/simulator`

HTTP: **POST**  
`/api/simulator/single`  
Envía un intento de compra individual.

HTTP: **POST**  
`/api/simulator/batch`  
Envía múltiples intentos de compra.

HTTP: **POST**  
`/api/simulator/batch-metrics`  
Simula lotes con métricas de rendimiento.

---

## **4. RiotStore.Consumer**
### Servicio worker para procesamiento de órdenes

Servicio Windows (.NET Worker Service) que corre continuamente escuchando eventos de Kafka y procesando órdenes en tiempo real.

### Estructura:
```
RiotStore.Consumer/
├── Workers/
│   └── OrderProcessingWorker.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IKafkaConsumerService.cs
│   │   └── IOrderProcessingService.cs
│   └── Implementations/
│       ├── KafkaConsumerService.cs
│       └── OrderProcessingService.cs
└── Program.cs
```

### Servicios

#### **IKafkaConsumerService** y **KafkaConsumerService**

`StartConsumingAsync(CancellationToken cancellationToken)`  
Inicia el consumo de eventos del topic `order-events`.

**Detalles de funcionamiento:**
- Se suscribe al topic `order-events` en Kafka
- Grupo de consumo: `riotstore-consumer-group`
- Auto offset reset: `Earliest` (procesa desde el inicio si es la primera vez)
- Auto commit: Habilitado

#### **IOrderProcessingService** y **orderprocessingservice**

`ProcessOrderAsync(OrderCreatedEvent orderEvent)`  
Procesa una orden completa con validación de stock.

**Flujo de procesamiento:**
1. Valida que el evento tenga items
2. Obtiene o crea el cliente en BD
3. Crea un registro de orden
4. Para cada item de la orden:
   - Valida stock disponible
   - Si hay stock: Deduce del inventario, registra compra exitosa
   - Si NO hay stock: Registra intento fallido (FAILED_OUT_OF_STOCK)
5. Actualiza balance de stock con nuevos totales

`ProcessOrderItemAsync(Order order, OrderItemDto item, OrderCreatedEvent orderEvent)`  
Procesa un item individual de la orden.

---

### OrderProcessingWorker (BackgroundService)

Inherita de `BackgroundService` y ejecuta el servicio Kafka Consumer de forma continua.

`ExecuteAsync(CancellationToken stoppingToken)`  
Método principal que inicia el consumo de eventos.

---

## Disclaimer

Este proyecto es una implementación independiente con fines educativos y de experimentación.

No está afiliado, asociado, autorizado ni respaldado por Riot Games, Inc. ni por Riot Merch (la tienda oficial). Todas las marcas, nombres comerciales y contenidos relacionados pertenecen a sus respectivos propietarios.

El uso de nombres o conceptos similares tiene únicamente fines demostrativos y no pretende infringir derechos de propiedad intelectual.