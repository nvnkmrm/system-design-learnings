# System Design Components

## Table of Contents

- [Databases](#databases)
  - [SQL Databases](#sql-databases)
  - [NoSQL Databases](#nosql-databases)
  - [SQL vs NoSQL — When to Use What](#sql-vs-nosql--when-to-use-what)
- [Caches](#caches)
  - [What is Caching?](#what-is-caching)
  - [Redis](#redis)
  - [Memcached](#memcached)
  - [CDN (Content Delivery Network)](#cdn-content-delivery-network)
  - [Redis vs Memcached](#redis-vs-memcached)
- [APIs](#apis)
  - [What is an API?](#what-is-an-api)
  - [REST API](#rest-api)
  - [GraphQL](#graphql)
  - [gRPC](#grpc)
  - [REST vs GraphQL vs gRPC](#rest-vs-graphql-vs-grpc)
- [How These Components Work Together](#how-these-components-work-together)

---

## Databases

A **database** is a structured collection of data that is stored and managed so it can be easily accessed, updated, and queried. In system design, choosing the right database is one of the most critical decisions.

---

### SQL Databases

**SQL (Structured Query Language)** databases are **relational databases** that store data in tables with predefined schemas and relationships.

#### Key Characteristics

| Feature             | Detail                                              |
| ------------------- | --------------------------------------------------- |
| **Data Model**      | Tables with rows and columns                        |
| **Schema**          | Fixed/Predefined (schema-on-write)                  |
| **Query Language**  | SQL                                                 |
| **Relationships**   | Supports JOINs and foreign keys                     |
| **ACID Compliance** | Yes — Atomicity, Consistency, Isolation, Durability |
| **Scalability**     | Vertical (scale-up) primarily                       |

#### ACID Properties

- **Atomicity** — A transaction is all-or-nothing. Either all operations succeed or none do.
- **Consistency** — Data always transitions from one valid state to another.
- **Isolation** — Concurrent transactions don't interfere with each other.
- **Durability** — Once a transaction is committed, it persists even after a crash.

#### Popular SQL Databases

| Database                 | Best For                                     |
| ------------------------ | -------------------------------------------- |
| **PostgreSQL**           | Complex queries, JSONB support, open-source  |
| **MySQL**                | Web applications, read-heavy workloads       |
| **SQLite**               | Embedded systems, mobile apps, local storage |
| **Microsoft SQL Server** | Enterprise applications                      |
| **Oracle DB**            | Large enterprise, high-transaction systems   |

#### Use Cases

- E-commerce order management (orders, products, users with relationships)
- Banking systems (strict ACID compliance required)
- CRM and ERP systems
- Any system where data integrity and complex queries are critical

#### Strengths

- Strong consistency guarantees
- Powerful query capabilities (JOINs, aggregations, subqueries)
- Mature technology with robust tooling
- Well-understood transactional model

#### Weaknesses

- Horizontal scaling (sharding) is complex
- Schema changes can be expensive in large tables
- Not ideal for unstructured or rapidly evolving data

---

### NoSQL Databases

**NoSQL (Not Only SQL)** databases are non-relational databases designed for flexibility, scalability, and high performance with large volumes of distributed data.

#### Types of NoSQL Databases

##### 1. Document Stores

- Store data as JSON/BSON documents
- Each document can have a different structure
- **Examples:** MongoDB, CouchDB, Firestore
- **Use Case:** User profiles, product catalogs, content management

##### 2. Key-Value Stores

- Simplest NoSQL model — data stored as key-value pairs
- Extremely fast reads/writes
- **Examples:** Redis, DynamoDB, Riak
- **Use Case:** Session management, caching, shopping carts

##### 3. Wide-Column Stores

- Stores data in tables but with flexible column definitions per row
- Optimized for queries over large datasets
- **Examples:** Apache Cassandra, HBase, Google Bigtable
- **Use Case:** Time-series data, IoT telemetry, event logging

##### 4. Graph Databases

- Stores data as nodes and edges representing relationships
- Optimized for traversing complex relationships
- **Examples:** Neo4j, Amazon Neptune, ArangoDB
- **Use Case:** Social networks, fraud detection, recommendation engines

#### Key Characteristics

| Feature            | Detail                                                          |
| ------------------ | --------------------------------------------------------------- |
| **Data Model**     | Flexible (document, key-value, column, graph)                   |
| **Schema**         | Dynamic (schema-on-read)                                        |
| **Query Language** | Database-specific (no universal standard)                       |
| **Relationships**  | Typically avoided (denormalized data)                           |
| **CAP Theorem**    | Often trades Consistency for Availability + Partition Tolerance |
| **Scalability**    | Horizontal (scale-out) natively                                 |

#### BASE Properties (vs ACID in SQL)

- **Basically Available** — The system guarantees availability but may return stale data.
- **Soft State** — The state of the system may change over time without input.
- **Eventually Consistent** — The system will become consistent over time.

#### Use Cases

- Real-time applications (chat, gaming leaderboards)
- Big data and analytics pipelines
- Content delivery platforms (blogs, news, media)
- Systems with rapidly evolving schemas (startups, MVPs)

#### Strengths

- Horizontal scalability built-in
- High write throughput
- Flexible schemas adapt easily to change
- Optimized for specific access patterns

#### Weaknesses

- Eventual consistency can lead to stale reads
- Complex queries (multi-table joins) are harder
- Less mature tooling compared to SQL in some cases

---

### SQL vs NoSQL — When to Use What

| Criteria               | SQL                            | NoSQL                               |
| ---------------------- | ------------------------------ | ----------------------------------- |
| **Data structure**     | Structured, relational         | Unstructured, semi-structured       |
| **Schema flexibility** | Fixed                          | Flexible                            |
| **Transactions**       | Strong ACID support            | Limited (improving)                 |
| **Scalability**        | Vertical                       | Horizontal                          |
| **Query complexity**   | Complex JOINs and aggregations | Simple lookups or specific patterns |
| **Consistency**        | Strong                         | Eventual                            |
| **Examples**           | Banking, ERP, CRM              | Social media, IoT, real-time apps   |

> **Rule of Thumb:**
>
> - Use **SQL** when data integrity, complex relationships, and transactional safety matter.
> - Use **NoSQL** when you need scale, speed, and flexibility over strict consistency.

---

## Caches

### What is Caching?

**Caching** is the process of storing copies of frequently accessed data in a **fast, temporary storage layer** (the cache) so future requests can be served faster without hitting the primary data source (database, API, file system).

#### Why Caching Matters

- Reduces **database load** and query latency
- Improves **response times** dramatically (memory access ~100ns vs disk ~10ms)
- Enables systems to **scale** by offloading repetitive work
- Reduces **cost** by avoiding repeated expensive computations or network calls

#### Caching Strategies

| Strategy                       | Description                                                          | Use Case                    |
| ------------------------------ | -------------------------------------------------------------------- | --------------------------- |
| **Cache-Aside (Lazy Loading)** | App checks cache first; on miss, fetches from DB and populates cache | General purpose reads       |
| **Write-Through**              | Data written to cache and DB simultaneously                          | Strong consistency needed   |
| **Write-Behind (Write-Back)**  | Data written to cache first, DB updated asynchronously               | High write throughput       |
| **Read-Through**               | Cache sits in front of DB; auto-populates on miss                    | Transparent caching layer   |
| **Refresh-Ahead**              | Cache proactively refreshes data before it expires                   | Predictable access patterns |

#### Cache Eviction Policies

- **LRU (Least Recently Used)** — Evicts data that hasn't been accessed for the longest time.
- **LFU (Least Frequently Used)** — Evicts data that is accessed least often.
- **FIFO (First In First Out)** — Evicts the oldest cached item.
- **TTL (Time To Live)** — Each item has an expiry time; auto-evicted when expired.

---

### Redis

**Redis (Remote Dictionary Server)** is an open-source, in-memory data structure store used as a database, cache, message broker, and queue.

#### Key Characteristics

| Feature               | Detail                                                                               |
| --------------------- | ------------------------------------------------------------------------------------ |
| **Storage**           | In-memory (with optional disk persistence)                                           |
| **Data Structures**   | Strings, Hashes, Lists, Sets, Sorted Sets, Bitmaps, HyperLogLog, Streams, Geospatial |
| **Persistence**       | RDB snapshots and AOF (Append Only File) logs                                        |
| **Replication**       | Master-replica replication                                                           |
| **Clustering**        | Redis Cluster for horizontal scaling                                                 |
| **Pub/Sub**           | Built-in publish/subscribe messaging                                                 |
| **Atomic Operations** | Yes — single-threaded execution guarantees atomicity                                 |

#### Redis Data Structures and Use Cases

| Data Structure  | Use Case                                             |
| --------------- | ---------------------------------------------------- |
| **String**      | Simple caching, counters, session tokens             |
| **Hash**        | User profiles, product details (field-level updates) |
| **List**        | Message queues, activity feeds, recent items         |
| **Set**         | Unique visitors, tags, friend lists                  |
| **Sorted Set**  | Leaderboards, priority queues, rate limiting         |
| **Stream**      | Event sourcing, log aggregation, real-time feeds     |
| **HyperLogLog** | Approximate unique counts (page views, UV)           |
| **Geospatial**  | Location-based services, nearby search               |

#### Common Redis Use Cases

- **Session Storage** — Store user sessions with TTL for auto-expiry
- **Leaderboards** — Sorted Sets make real-time rankings trivial
- **Rate Limiting** — Increment counters with TTL to throttle requests
- **Pub/Sub Messaging** — Real-time notifications, chat systems
- **Distributed Locks** — `SETNX` for coordination between services
- **Job Queues** — Lists as FIFO queues for background workers
- **Full-Page Caching** — Cache rendered HTML or API responses

#### Redis Persistence Options

- **No Persistence** — Pure cache mode; data lost on restart (fastest)
- **RDB (Redis Database)** — Periodic point-in-time snapshots to disk
- **AOF (Append Only File)** — Logs every write operation; more durable, larger files
- **RDB + AOF** — Combined for both fast restarts and durability

#### Strengths

- Rich data structures beyond simple key-value
- Sub-millisecond latency
- Persistence options for durability
- Pub/Sub and Streams for messaging
- Lua scripting for atomic complex operations
- Cluster mode for horizontal scaling

#### Weaknesses

- Entire dataset must fit in memory (costly at scale)
- Single-threaded (though I/O is non-blocking)
- Cluster mode adds operational complexity

---

### Memcached

**Memcached** is a high-performance, distributed, in-memory **key-value caching system** designed for simplicity and speed.

#### Key Characteristics

| Feature             | Detail                                            |
| ------------------- | ------------------------------------------------- |
| **Storage**         | In-memory only (no persistence)                   |
| **Data Structures** | Only Strings (simple key-value)                   |
| **Persistence**     | None                                              |
| **Multi-threading** | Yes — uses multiple threads for higher throughput |
| **Clustering**      | Client-side sharding (no built-in cluster)        |
| **Protocol**        | Simple text/binary protocol                       |

#### Common Memcached Use Cases

- **Database query result caching** — Cache expensive SQL query results
- **Object caching** — Cache serialized objects (user data, product info)
- **Session caching** — Store session data for web applications
- **HTML fragment caching** — Cache rendered page fragments

#### Strengths

- Simple, proven, and battle-tested
- Multi-threaded architecture for high concurrency
- Very low overhead per connection
- Predictable performance for simple caching

#### Weaknesses

- No persistence — all data lost on restart
- Only supports simple string values
- No replication built-in
- Client is responsible for clustering/sharding logic
- No built-in pub/sub or advanced data structures

---

### CDN (Content Delivery Network)

A **CDN** is a geographically distributed network of servers (**edge nodes/PoPs — Points of Presence**) that cache and deliver content to users from the closest physical location.

#### How a CDN Works

```
User (India) --> CDN Edge Node (Mumbai) --> Cached Content Served
                                        --> Cache Miss --> Origin Server (US) --> Cache and Serve
```

1. User requests content (image, video, HTML, JS, CSS)
2. DNS resolves to the nearest CDN edge node
3. If cached (**cache hit**) → served directly from edge (fast!)
4. If not cached (**cache miss**) → fetched from origin server, cached at edge, then served

#### Types of CDN Caching

| Type                    | Description                                              |
| ----------------------- | -------------------------------------------------------- |
| **Static Content CDN**  | Caches unchanging assets: images, CSS, JS, fonts, videos |
| **Dynamic Content CDN** | Optimizes delivery of personalized/API responses         |
| **Streaming CDN**       | Optimized for video streaming (HLS, DASH)                |
| **Security CDN**        | DDoS protection, WAF, bot mitigation (e.g., Cloudflare)  |

#### What CDNs Cache

- Images, videos, audio files
- JavaScript and CSS bundles
- HTML pages (for static sites)
- API responses (with proper cache headers)
- Software downloads and packages

#### Popular CDN Providers

| Provider             | Notable For                          |
| -------------------- | ------------------------------------ |
| **Cloudflare**       | Security, DDoS protection, free tier |
| **AWS CloudFront**   | Tight AWS integration                |
| **Akamai**           | Enterprise scale, largest CDN        |
| **Fastly**           | Low-latency, real-time purging       |
| **Azure CDN**        | Microsoft Azure integration          |
| **Google Cloud CDN** | GCP integration, HTTP/2 & QUIC       |

#### CDN Use Cases

- Serving static website assets globally
- Video streaming platforms (Netflix, YouTube)
- Software update distribution
- API acceleration for global applications
- DDoS mitigation and WAF (Web Application Firewall)

#### Cache Control Headers (CDN Behavior)

- `Cache-Control: public, max-age=86400` — CDN caches for 24 hours
- `Cache-Control: private` — CDN must NOT cache (user-specific content)
- `Cache-Control: no-cache` — Must revalidate with origin on each request
- `ETag` / `Last-Modified` — Conditional requests for cache validation

#### Strengths

- Dramatically reduces latency for global users
- Offloads massive traffic from origin servers
- Improves availability (origin can go down; CDN still serves cached content)
- Built-in DDoS protection (with providers like Cloudflare)

#### Weaknesses

- Cache invalidation can be slow (TTL-based)
- Cost increases with data transfer volume
- Dynamic, personalized content is hard to cache
- Not suitable for real-time or frequently changing data

---

### Redis vs Memcached

| Feature               | Redis                                     | Memcached                       |
| --------------------- | ----------------------------------------- | ------------------------------- |
| **Data Structures**   | Rich (Strings, Hashes, Lists, Sets, etc.) | Simple key-value (strings only) |
| **Persistence**       | Yes (RDB + AOF)                           | No                              |
| **Replication**       | Yes (master-replica)                      | No                              |
| **Clustering**        | Yes (Redis Cluster)                       | Client-side only                |
| **Pub/Sub**           | Yes                                       | No                              |
| **Threads**           | Single-threaded (I/O multiplexed)         | Multi-threaded                  |
| **Memory efficiency** | Slightly higher overhead                  | More memory-efficient           |
| **Use case**          | Complex caching + messaging + queues      | Pure high-throughput caching    |

> **Rule of Thumb:**
>
> - Use **Redis** when you need persistence, pub/sub, complex data structures, or distributed locks.
> - Use **Memcached** when you need pure, simple, high-throughput caching with multiple CPU cores.

---

## APIs

### What is an API?

An **API (Application Programming Interface)** is a defined contract that allows two software components to communicate with each other. In system design, APIs define how clients (users, frontend apps, other services) interact with backend services.

#### API Design Principles

- **Consistency** — Predictable naming, behavior, and error formats
- **Idempotency** — Safe to retry (GET, PUT, DELETE should be idempotent)
- **Versioning** — `/v1/users` allows API evolution without breaking clients
- **Security** — Authentication (JWT, OAuth), rate limiting, input validation
- **Documentation** — OpenAPI/Swagger specs for discoverability

---

### REST API

**REST (Representational State Transfer)** is an architectural style for designing APIs using HTTP methods and stateless communication.

#### REST Principles

- **Stateless** — Each request contains all information needed; server holds no session state
- **Client-Server** — Clear separation between client and server concerns
- **Cacheable** — Responses can be cached to improve performance
- **Uniform Interface** — Consistent use of HTTP methods and URIs
- **Layered System** — Client doesn't know if it's talking to the actual server or a proxy

#### HTTP Methods in REST

| Method     | Operation             | Idempotent | Safe |
| ---------- | --------------------- | ---------- | ---- |
| **GET**    | Read/Retrieve         | Yes        | Yes  |
| **POST**   | Create                | No         | No   |
| **PUT**    | Replace/Update (full) | Yes        | No   |
| **PATCH**  | Partial Update        | No         | No   |
| **DELETE** | Delete                | Yes        | No   |

#### REST URL Design Best Practices

```
GET    /api/v1/users          → List all users
POST   /api/v1/users          → Create a new user
GET    /api/v1/users/{id}     → Get a specific user
PUT    /api/v1/users/{id}     → Replace a user
PATCH  /api/v1/users/{id}     → Partially update a user
DELETE /api/v1/users/{id}     → Delete a user
GET    /api/v1/users/{id}/orders  → Get orders for a user
```

#### HTTP Status Codes

| Code                        | Meaning                            |
| --------------------------- | ---------------------------------- |
| `200 OK`                    | Successful GET, PUT, PATCH         |
| `201 Created`               | Successful POST                    |
| `204 No Content`            | Successful DELETE                  |
| `400 Bad Request`           | Invalid input                      |
| `401 Unauthorized`          | Authentication required            |
| `403 Forbidden`             | Authenticated but lacks permission |
| `404 Not Found`             | Resource doesn't exist             |
| `429 Too Many Requests`     | Rate limit exceeded                |
| `500 Internal Server Error` | Server-side failure                |

#### Strengths of REST

- Simple, widely understood, uses standard HTTP
- Stateless — easy to scale horizontally
- Great tooling and language support
- Human-readable with JSON/XML

#### Weaknesses of REST

- **Over-fetching** — Response may include more data than needed
- **Under-fetching** — May need multiple requests to get related data
- No real-time support (requires polling or WebSockets separately)

---

### GraphQL

**GraphQL** is a query language for APIs and a runtime for executing queries, developed by Facebook (2015). It gives clients the power to request **exactly the data they need**.

#### Key Concepts

| Concept          | Description                                               |
| ---------------- | --------------------------------------------------------- |
| **Query**        | Read data (equivalent to GET)                             |
| **Mutation**     | Write/modify data (equivalent to POST/PUT/DELETE)         |
| **Subscription** | Real-time data via WebSocket                              |
| **Schema**       | Strongly typed contract defining all types and operations |
| **Resolver**     | Function that fetches data for each field                 |

#### GraphQL Example

```graphql
# Query — fetch only what you need
query {
  user(id: "123") {
    name
    email
    orders {
      id
      total
    }
  }
}

# Response
{
  "data": {
    "user": {
      "name": "Naveen",
      "email": "naveen@example.com",
      "orders": [
        { "id": "o1", "total": 250 }
      ]
    }
  }
}
```

#### Strengths of GraphQL

- No over-fetching or under-fetching — request exactly what you need
- Single endpoint (`/graphql`) for all operations
- Strongly typed schema acts as living documentation
- Excellent for complex, interconnected data (social graphs, dashboards)
- Real-time with Subscriptions

#### Weaknesses of GraphQL

- Steeper learning curve vs REST
- Complex queries can cause N+1 database query problem (mitigated with DataLoader)
- Harder to cache at HTTP level (all requests are POST to same endpoint)
- Overkill for simple CRUD APIs

---

### gRPC

**gRPC (Google Remote Procedure Call)** is a high-performance, open-source RPC framework that uses **Protocol Buffers (protobuf)** for serialization and **HTTP/2** for transport.

#### Key Concepts

| Concept                | Description                                                |
| ---------------------- | ---------------------------------------------------------- |
| **Protobuf**           | Binary serialization format — smaller and faster than JSON |
| **HTTP/2**             | Multiplexed, bidirectional streaming, header compression   |
| **Service Definition** | `.proto` file defines services, methods, and message types |
| **Code Generation**    | Stubs auto-generated for multiple languages                |
| **Streaming**          | Server-side, client-side, and bidirectional streaming      |

#### gRPC Communication Patterns

| Pattern                     | Description                                  |
| --------------------------- | -------------------------------------------- |
| **Unary**                   | Single request, single response (like REST)  |
| **Server Streaming**        | Single request, stream of responses          |
| **Client Streaming**        | Stream of requests, single response          |
| **Bidirectional Streaming** | Both client and server stream simultaneously |

#### gRPC `.proto` Example

```protobuf
syntax = "proto3";

service UserService {
  rpc GetUser(UserRequest) returns (UserResponse);
  rpc ListUsers(Empty) returns (stream UserResponse);
}

message UserRequest {
  string id = 1;
}

message UserResponse {
  string id = 1;
  string name = 2;
  string email = 3;
}
```

#### Strengths of gRPC

- Extremely fast — binary protobuf + HTTP/2 (up to 10x faster than REST/JSON)
- Strong typing via `.proto` schema contract
- Built-in code generation for many languages
- Native streaming support
- Ideal for internal microservice communication

#### Weaknesses of gRPC

- Binary format is not human-readable (harder to debug)
- Limited browser support (requires gRPC-Web proxy)
- Requires protobuf tooling — more setup than REST
- Not ideal for public-facing APIs

---

### REST vs GraphQL vs gRPC

| Feature                 | REST                | GraphQL                 | gRPC                     |
| ----------------------- | ------------------- | ----------------------- | ------------------------ |
| **Protocol**            | HTTP/1.1            | HTTP/1.1                | HTTP/2                   |
| **Data Format**         | JSON / XML          | JSON                    | Protobuf (binary)        |
| **Schema / Contract**   | OpenAPI (optional)  | Strongly typed schema   | `.proto` file (required) |
| **Over/Under Fetching** | Possible            | Eliminated              | N/A (method-based)       |
| **Real-time**           | Polling / WebSocket | Subscriptions           | Bidirectional streaming  |
| **Performance**         | Moderate            | Moderate                | High                     |
| **Browser Support**     | Full                | Full                    | Limited (gRPC-Web)       |
| **Best For**            | Public APIs, CRUD   | Complex queries, mobile | Internal microservices   |
| **Learning Curve**      | Low                 | Medium                  | High                     |

> **Rule of Thumb:**
>
> - Use **REST** for public APIs and standard CRUD operations.
> - Use **GraphQL** when clients need flexible, nested data queries (mobile apps, dashboards).
> - Use **gRPC** for high-performance internal microservice-to-microservice communication.

---

## How These Components Work Together

A typical production system uses all of these components together in a layered architecture:

```
                    ┌──────────────────────────────────┐
                    │           CDN                    │
                    │  (Static assets, edge caching)   │
                    └──────────────┬───────────────────┘
                                   │
                    ┌──────────────▼───────────────────┐
                    │        API Gateway / LB          │
                    │   (REST / GraphQL / gRPC)        │
                    └──────────────┬───────────────────┘
                                   │
             ┌─────────────────────▼──────────────────────┐
             │              Application Servers            │
             │         (Business Logic Services)           │
             └───────┬──────────────────────┬─────────────┘
                     │                      │
        ┌────────────▼──────┐    ┌──────────▼──────────┐
        │   Cache Layer     │    │    Database Layer    │
        │  Redis/Memcached  │    │   SQL + NoSQL        │
        │  (Hot data, TTL)  │    │   (Persistent data)  │
        └───────────────────┘    └─────────────────────┘
```

### Request Flow Example

1. **User requests a product page**
2. **CDN** checks if the static assets (images, JS, CSS) are cached at the edge → serves them directly
3. **API Gateway** receives the API request (`GET /api/v1/products/123`)
4. **Application Server** checks **Redis cache** for the product data
   - **Cache Hit** → returns cached data immediately (sub-ms)
   - **Cache Miss** → queries **SQL database** for product details, stores result in Redis with TTL
5. **Response** returned to the user

### Component Responsibilities Summary

| Component          | Role                                                  | Example Technology           |
| ------------------ | ----------------------------------------------------- | ---------------------------- |
| **SQL Database**   | Persistent, structured, transactional data            | PostgreSQL, MySQL            |
| **NoSQL Database** | Flexible, scalable, high-volume data                  | MongoDB, Cassandra, DynamoDB |
| **Redis**          | Fast caching, sessions, queues, pub/sub               | Redis                        |
| **Memcached**      | Simple, high-throughput key-value caching             | Memcached                    |
| **CDN**            | Edge caching of static/media content, DDoS protection | Cloudflare, CloudFront       |
| **REST API**       | Public-facing CRUD operations                         | Express.js, Django REST      |
| **GraphQL**        | Flexible data querying for clients                    | Apollo Server, Hasura        |
| **gRPC**           | Fast internal microservice communication              | gRPC, Envoy                  |
