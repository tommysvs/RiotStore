# Bitácora de decisiones técnicas

## 1. Arquitectura event-driven con Kafka

**Decisión:** Usar Apache Kafka como message broker central para desacoplar el procesamiento de órdenes del API.

**Justificación:**

* Separación de responsabilidades: El API solo recibe y publica eventos, el Consumer los procesa
* Escalabilidad horizontal: El Consumer puede correr en múltiples instancias sin conflictos
* Resiliencia: Si el Consumer falla, los eventos quedan en Kafka esperando ser procesados
* Desempeño: El API no bloquea esperando confirmación de procesamiento

**Trade-offs:**

* Complejidad añadida: Requiere infraestructura adicional (Kafka, Zookeeper)
* Consistencia eventual: Las órdenes no se procesan inmediatamente, hay latencia
* Debugging más difícil: Problemas distribuidos son más complejos de diagnosticar
* Costo operacional: Mantenimiento y monitoreo de un cluster Kafka adicional

**Alternativas consideradas:**

* Background Jobs (Hangfire): Más simple pero menos escalable
* Llamadas síncronas directas: Más simple pero acoplado y menos resiliente

---

## 2. Separación en proyectos físicos (API vs Consumer)

**Decisión:** Dividir en dos proyectos ejecutables: `RiotStore.API` y `RiotStore.Consumer`.

**Justificación:**

* Ciclos de vida independientes
* Escalado diferenciado
* Deploys independientes
* Monitoreo separado

**Trade-offs:**

* Dos procesos para mantener
* Sincronización de esquema
* Debugging más engorroso
* Dependencias duplicadas

**Alternativas consideradas:**

* Todo en un solo proyecto
* Múltiples Workers especializados

---

## 3. Entity Framework Core con PostgreSQL

**Decisión:** Usar EF Core con PostgreSQL para persistencia.

**Justificación:**

* ORM maduro
* Migrations automáticas
* Type-safety en C#
* PostgreSQL: ACID y alta concurrencia

**Trade-offs:**

* Overhead de performance
* Curva de aprendizaje
* Queries complejas en LINQ
* Lock-in relacional

**Alternativas consideradas:**

* Dapper / ADO.NET
* MongoDB
* SQLite

---

## 4. Patrón Repository

**Decisión:** Implementar repositories sobre EF Core.

**Justificación:**

* Abstracción
* Testabilidad
* Consistencia
* Mantenibilidad

**Trade-offs:**

* Boilerplate
* Overhead de abstracción
* Posible sobre-ingeniería
* EF ya abstrae bastante

**Alternativas consideradas:**

* DbContext directo
* Unit of Work
* CQRS

---

## 5. Proyecto compartido (Shared)

**Decisión:** Crear `RiotStore.Shared` para eventos y DTOs.

**Justificación:**

* Contrato único
* Versionado centralizado
* DRY
* Validación consistente

**Trade-offs:**

* Acoplamiento
* Cambios afectan ambos proyectos
* Versionado complejo
* Riesgo de dependencias circulares

**Alternativas consideradas:**

* Duplicar tipos
* JSON genérico

---

## 6. Procesamiento asincrónico en Consumer

**Decisión:** Usar `BackgroundService` con async/await.

**Justificación:**

* Non-blocking
* Eficiencia de recursos
* Manejo de cancelación
* Alto throughput

**Trade-offs:**

* Debugging complejo
* Riesgo de deadlocks
* Manejo de errores difícil
* Testing más complicado

**Alternativas consideradas:**

* Procesamiento síncrono
* TPL Dataflow
* Ejecutores personalizados

---

## 7. Validación de stock con optimistic locking

**Decisión:** Control de concurrencia optimista.

**Justificación:**

* Sin locks explícitos
* Detecta conflictos
* ACID
* Simple implementación

**Trade-offs:**

* Reintentos necesarios
* Posible starvation
* Manejo de errores complejo
* No ideal para alta contención

**Alternativas consideradas:**

* Pessimistic locking
* Locks distribuidos (Redis)
* Cola serial
* Overselling

---

## 8. Segmentación de demanda

**Decisión:** Simular segmentos (20% alta, 50% media, 30% baja).

**Justificación:**

* Mayor realismo
* Testing de edge cases
* Análisis de comportamiento
* Simulación de picos

**Trade-offs:**

* Complejidad adicional
* Distribución arbitraria
* Difícil parametrización
* Posible irrealismo

**Alternativas consideradas:**

* Distribución uniforme
* Configurable en appsettings
* Machine learning

---

## 9. Dashboard con polling

**Decisión:** Polling cada 5–10 segundos.

**Justificación:**

* Simple
* Compatible
* Stateless
* Fail-safe

**Trade-offs:**

* Latencia
* Carga en servidor
* Overhead de red
* UX menos fluida

**Alternativas consideradas:**

* WebSockets
* SignalR
* SSE
* GraphQL subscriptions

---

## 10. Soft-delete

**Decisión:** Usar `IsActive` en lugar de eliminar.

**Justificación:**

* Auditoría
* Recuperación
* Reportes históricos
* Compliance

**Trade-offs:**

* Queries más complejas
* Rendimiento degradado
* Ambigüedad en FK
* Crecimiento de datos

**Alternativas consideradas:**

* Hard-delete
* Temporal tables
* Archivado

---

## 11. IDs autoincrementales

**Decisión:** Usar `SERIAL` en PostgreSQL.

**Justificación:**

* Simple
* Eficiente
* Estándar
* Legible

**Trade-offs:**

* No distribuido
* Predecible
* Difícil renumeración
* Límite teórico

**Alternativas consideradas:**

* UUID
* Snowflake / NanoID
* Hash

---

## 12. Configuración en appsettings.json

**Decisión:** Centralizar configuración fuera del código.

**Justificación:**

* Seguridad
* Flexibilidad
* Soporte por entorno
* Buenas prácticas .NET

**Trade-offs:**

* Desincronización entre entornos
* Manejo de secretos
* Complejidad en CI/CD
* Riesgo de commits accidentales

**Alternativas consideradas:**

* Variables de entorno
* User Secrets
* Hardcoded (descartado)

---

## 13. Worker Service vs Console App

**Decisión:** Usar template Worker Service.

**Justificación:**

* Logging integrado
* Dependency Injection
* Configuración integrada
* Lifecycle management
* Preparado para servicio

**Trade-offs:**

* Más boilerplate
* Más dependencias
* Curva de aprendizaje
* Overkill para scripts simples

**Alternativas consideradas:**

* Console App
* Windows Service
* Hangfire

---

## 14. Particiones en Kafka

**Decisión:** 3 particiones, replication factor = 1.

**Justificación:**

* Paralelismo
* Balance adecuado
* Escalabilidad
* Flexibilidad

**Trade-offs:**

* Sin tolerancia a fallos
* Rebalancing
* Orden no global
* Coordinación adicional

**Alternativas consideradas:**

* 1 partición
* Muchas particiones
* Replication factor 3
* Múltiples tópicos

---

## 15. Auto offset reset = earliest

**Decisión:** Procesar desde el inicio si no hay offset.

**Justificación:**

* Útil en desarrollo
* Recuperación
* Evita pérdida de datos

**Trade-offs:**

* Peligroso en producción
* Latencia inicial
* Duplicados
* Necesidad de idempotencia

**Alternativas consideradas:**

* Latest
* None
* Offset en BD

---

## 16. Auto commit habilitado

**Decisión:** Usar auto-commit de offsets.

**Justificación:**

* Simplicidad
* Menos código
* Buen rendimiento

**Trade-offs:**

* Pérdida de mensajes
* Duplicados
* No exactamente-una-vez
* Menor control

**Alternativas consideradas:**

* Manual commit
* Transacciones Kafka
* Idempotencia