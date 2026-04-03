# Databases for System Design — Interview + Real-World Guide

---

## 1. Database Fundamentals

### What is a Database (Practical Definition)

A database is a **persistent, structured store of data** that allows you to create, read, update, and delete records reliably — even across failures, restarts, and concurrent access.

> Interview framing: "A database lets multiple services read and write shared state reliably, at scale."

### Why Databases Are Critical in Scalable Systems

- Applications are stateless (servers can die and restart); databases hold **system of record**
- Poor DB choice = bottleneck at scale (most production outages trace back to DB layer)
- DB choice dictates **consistency guarantees**, **failure behavior**, and **query patterns**

### Types of Databases

| Type                 | Examples                  | Use When                                                |
| -------------------- | ------------------------- | ------------------------------------------------------- |
| **Relational (SQL)** | PostgreSQL, MySQL, Aurora | Strong consistency, complex joins, financial data       |
| **Document**         | MongoDB, Firestore        | Flexible schema, nested data (e.g., user profiles)      |
| **Key-Value**        | Redis, DynamoDB           | Low-latency lookup by single key (sessions, caches)     |
| **Wide-Column**      | Cassandra, HBase          | High write throughput, time-series, analytics           |
| **Graph**            | Neo4j, Neptune            | Relationship traversal (social graphs, recommendations) |
| **Search**           | Elasticsearch             | Full-text search, log querying                          |
| **Time-Series**      | InfluxDB, TimescaleDB     | Metrics, IoT, monitoring                                |

---

## 2. SQL vs NoSQL (Decision-Focused)

### Key Differences

| Dimension             | SQL                                | NoSQL                                     |
| --------------------- | ---------------------------------- | ----------------------------------------- |
| **Schema**            | Fixed (migrations required)        | Flexible (evolve per document)            |
| **Scaling**           | Vertical first; horizontal is hard | Horizontal by design                      |
| **Consistency**       | Strong (ACID)                      | Eventual (BASE) by default                |
| **Joins**             | Native, efficient                  | Avoided (denormalize instead)             |
| **Transactions**      | Multi-row, multi-table             | Limited (document-level mostly)           |
| **Query flexibility** | High (arbitrary SQL)               | Low (query patterns must be pre-designed) |

### Trade-offs (NOT Generic)

**SQL trade-off:** You get strong guarantees but pay in operational complexity at scale. Sharding Postgres is painful. Joins become expensive at 100M+ rows.

**NoSQL trade-off:** You gain write throughput and horizontal scale, but you **pre-commit to access patterns**. Changing how you query means restructuring data.

### When to Use Each

**Use SQL when:**

- Data has clear relationships (orders → line items → products)
- You need transactions (payment, inventory deduction)
- Query patterns are unpredictable or exploratory
- Team is small and schema helps prevent bugs

**Use NoSQL when:**

- Write throughput > 10K/s (Cassandra handles millions)
- You know access patterns upfront (lookup by userId, sessionId)
- Schema evolves rapidly per entity (product catalog attributes)
- You need global distribution (DynamoDB Global Tables, Cosmos DB)

### Common Mistakes

- Using MongoDB "because it's flexible" — then writing joins in app code (just use Postgres)
- Using Cassandra for a low-traffic system — massive ops overhead, no benefit
- Picking Redis as primary store — it's in-memory: data loss on restart without persistence config
- Assuming NoSQL = scalable. Cassandra still needs careful partition key design

---

## 3. System Design Impact

### Scalability

```
Vertical Scaling:  Add more CPU/RAM to one machine (SQL default)
                   Limit: ~96 cores, ~12TB RAM — then you're stuck

Horizontal Scaling: Add more machines (NoSQL default)
                    Key: data must be partitionable by some key
```

- SQL scales reads with **read replicas**; scales writes only via sharding (complex)
- Cassandra / DynamoDB scale writes horizontally natively via partitioning

### Performance: Read-Heavy vs Write-Heavy

| System Type | Optimization                                               |
| ----------- | ---------------------------------------------------------- |
| Read-heavy  | Add read replicas, use caching (Redis), denormalize        |
| Write-heavy | Use NoSQL (Cassandra), async writes, write-behind cache    |
| Mixed       | CQRS — separate read model (denormalized) from write model |

### CAP Theorem — Intuitive Explanation

> In a distributed system, during a **network partition** (nodes can't talk), you must choose:

```
       C — Consistency
      / \
     /   \
    P-----A
Partition  Availability

CP: Return error if data might be stale (banks, inventory)
AP: Return possibly stale data (social feeds, DNS)
CA: Not realistic in distributed systems (partition WILL happen)
```

**Interview explanation:** "CAP says when the network fails, I pick between correctness and uptime. For payments, I pick CP. For feeds, I pick AP."

**Real examples:**

- **Zookeeper, HBase** → CP (strong consistency, reject writes during partition)
- **Cassandra, DynamoDB** → AP (always available, tunable consistency)
- **Postgres (single node)** → CA (not distributed — CAP doesn't strictly apply)

### Read vs Write Optimization Strategies

**Read optimization:**

- Add indexes on query columns
- Read replicas (route `SELECT` to replicas)
- Materialized views (pre-compute aggregations)
- Cache with Redis (cache-aside or read-through)

**Write optimization:**

- Batch writes, async queues (Kafka → DB)
- Use append-only stores (Cassandra, event logs)
- Avoid over-indexing (indexes slow down writes)
- Write-behind cache (write to Redis, flush to DB async)

---

## 4. Real-World System Examples

### E-Commerce (Orders, Inventory)

**DB:** PostgreSQL (primary) + Redis (inventory cache)

**Why:**

- Orders need ACID transactions (deduct inventory + create order atomically)
- Inventory is relational (products, warehouses, SKUs)
- Redis caches available stock count for fast reads

**Trade-offs accepted:**

- Vertical scaling limits; mitigated with read replicas and connection pooling (PgBouncer)
- Inventory cache can be slightly stale (handle with optimistic locking on checkout)

### Social Media Feed

**DB:** Cassandra (feed storage) + Redis (hot feed cache) + PostgreSQL (user/follow graph)

**Why:**

- Feed writes (fanout) are extremely high-volume
- Reads are always "get last N posts by userId" — perfect Cassandra partition pattern
- User follow relationships stay in Postgres (relational, low write volume)

**Trade-offs accepted:**

- Eventual consistency: you may see posts slightly out of order
- Denormalization: post content duplicated per follower's feed (storage cost for speed)

### Logging / Analytics System

**DB:** Apache Kafka (ingestion) → ClickHouse or Elasticsearch (query)

**Why:**

- Logs generate millions of events/sec — need append-only, write-optimized store
- Queries are aggregations ("error rate last 5 min") — columnar storage (ClickHouse) wins
- Elasticsearch if full-text search needed (log messages, stack traces)

**Trade-offs accepted:**

- Near-real-time (not real-time) analytics (few seconds lag)
- Storage cost is high — tiered storage or TTL policies needed

### Payment System (High Consistency)

**DB:** PostgreSQL (primary) + event sourcing pattern

**Why:**

- Every debit/credit must be atomic, auditable, and irreversible
- Double-spend prevention requires serializable transactions
- Event sourcing gives full audit trail (never update, only append)

**Trade-offs accepted:**

- Lower write throughput — acceptable for payments (volume < 10K TPS for most systems)
- More complex application logic (idempotency keys, distributed saga for multi-service)

---

## 5. Production-Level Concepts

### Indexing

**When it helps:**

- Queries filtering by a column with high cardinality (userId, email, orderId)
- Range queries on timestamps or IDs
- Covering indexes (index includes all queried columns → no table scan)

**When it hurts:**

- Every index slows `INSERT`/`UPDATE`/`DELETE` (index must be updated too)
- Low-cardinality columns (status: active/inactive) — poor selectivity, DB may ignore index
- Too many indexes on OLTP tables = write bottleneck

> Rule: Index what you query, not what you store.

### Sharding

**Why:** Single node can't handle data volume or write throughput.

**When:** Table > 100M rows, write QPS > ~5K on a single Postgres node, or data > available RAM.

**Strategies:**

```
Hash sharding:   shard = hash(userId) % N
                 → Even distribution, bad for range queries

Range sharding:  shard by date or ID range
                 → Good for time-series, risk of hot shards

Directory-based: Lookup service maps key → shard
                 → Flexible, but single point of failure
```

**Challenges:**

- Cross-shard joins: must be done in app layer (expensive)
- Rebalancing: adding shards requires data migration
- Hot partitions: celebrity user generates 10x traffic on one shard

### Replication

**Read scaling:** Primary accepts writes; replicas serve reads. Read replica lag = eventual consistency risk.

**Failover:**

```
Primary fails → Replica promoted (automatic with tools like Patroni for Postgres)
Risk: replication lag means promoted replica may miss last N writes
```

**Replication types:**

- **Synchronous:** Primary waits for replica ACK before confirming write. Strong consistency, higher latency.
- **Asynchronous:** Primary confirms immediately, replica catches up. Lower latency, risk of data loss on failover.

> Interview tip: "I'd use synchronous replication for the payments service and async for the analytics replica."

### Transactions: ACID vs BASE

**ACID (SQL):**

- **Atomic:** All or nothing (order + payment deduction = one transaction)
- **Consistent:** DB constraints always hold (no negative inventory)
- **Isolated:** Concurrent transactions don't see each other's partial state
- **Durable:** Committed data survives crashes (WAL log)

**BASE (NoSQL):**

- **Basically Available:** System responds even if stale
- **Soft state:** Data may change over time without input
- **Eventually Consistent:** All replicas converge given no new writes

**Real implication:** Using Cassandra for a shopping cart? You might display "1 item left" to two users simultaneously. One will get an error at checkout. Design for this with idempotency + compensation logic.

---

## 6. Interview Thinking Framework

### Step-by-Step: "How do you choose a database?"

```
Step 1: Clarify the data model
        → Is data relational? Does it need joins?
        → Flexible/nested? (documents)
        → Simple key lookups? (key-value)

Step 2: Define access patterns
        → What are the top 3 queries?
        → Read-heavy or write-heavy?
        → Range queries or point lookups?

Step 3: Establish consistency requirements
        → Can we tolerate stale reads? (eventual)
        → Must reads always reflect latest write? (strong)
        → Do we need multi-row transactions?

Step 4: Estimate scale
        → QPS (reads/writes)
        → Data volume (GB/TB)
        → Growth rate

Step 5: Choose DB + justify trade-offs
        → State what you're giving up
        → Explain operational implications
```

### Key Questions to Ask the Interviewer

- "Is this read-heavy or write-heavy?"
- "Do we need strong consistency, or is eventual acceptable?"
- "What's the expected QPS and data size in 1–3 years?"
- "Are there compliance or durability requirements (e.g., financial audit)?"

### Red Flags in Interviews

- Choosing a DB without justifying trade-offs
- Using a single DB for everything in a large-scale design
- Saying "I'll use MongoDB" without explaining why schema flexibility is needed
- Not mentioning caching when designing read-heavy systems
- Ignoring replication/sharding when scale is clearly massive

### Decision Checklist

```
[ ] Data shape defined (relational / document / key-value / columnar)
[ ] Access patterns identified (top 3 queries)
[ ] Consistency requirement stated (strong / eventual)
[ ] Scale estimated (QPS, storage)
[ ] DB chosen + trade-offs acknowledged
[ ] Caching strategy mentioned (if read-heavy)
[ ] Sharding/replication strategy mentioned (if large scale)
```

---

## 7. Real-World Engineering Tips

### How DB Decisions Evolve at Scale

```
Stage 1 (0–100K users):    Single Postgres instance. Ship fast.
Stage 2 (100K–1M users):   Add read replicas + Redis cache. Tune queries.
Stage 3 (1M–10M users):    Connection pooling (PgBouncer). Vertical scale.
Stage 4 (10M+ users):      Sharding or migrate hot data to NoSQL.
                            Event-driven architecture. CQRS.
```

> Most systems never reach Stage 4. Don't over-architect early.

### Migration Strategies

**SQL → NoSQL (e.g., Postgres → Cassandra for feed):**

1. Dual-write: write to both DBs simultaneously
2. Backfill: migrate historical data to new DB
3. Verify: compare read results from both
4. Cutover: route reads to new DB, retire old

**NoSQL → SQL (e.g., adding structure to MongoDB):**

1. Audit existing document shapes
2. Define normalized schema
3. ETL with transformation logic
4. Gradual service-by-service migration

### Common Production Issues

| Issue                     | Cause                                                               | Fix                                                          |
| ------------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------ |
| **Hot partition**         | All traffic hits one Cassandra partition (e.g., trending celebrity) | Add partition key suffix (userId + bucket), or shard by time |
| **Slow queries**          | Missing index, or index not used                                    | `EXPLAIN ANALYZE`, add covering index, avoid `SELECT *`      |
| **N+1 queries**           | ORM fetches 1 user then N orders in loop                            | Eager loading / JOIN query / DataLoader pattern              |
| **Replica lag**           | High write volume, replica falls behind                             | Monitor `seconds_behind_master`, scale replica hardware      |
| **Connection exhaustion** | Too many app connections to DB                                      | Add PgBouncer (Postgres) or connection pool tuning           |
| **Lock contention**       | Long-running transactions blocking writes                           | Shorter transactions, optimistic locking, queue writes       |

### Observability: What to Monitor

**Query-level:**

- Slow query log (queries > 100ms)
- `EXPLAIN` plans for top queries
- Index hit ratio (should be > 99%)

**DB-level:**

- Replication lag (replicas)
- Connection count (vs max connections)
- Cache hit ratio (buffer/page cache)
- Disk I/O utilization

**System-level:**

- CPU (Postgres is CPU-heavy for complex queries)
- Disk throughput (write-heavy = I/O bound)
- Memory (working set must fit in RAM for performance)

**Alerts to set:**

- Replication lag > 10s
- Query p99 latency > 500ms
- Connection utilization > 80%
- Disk usage > 75%

---

## Quick Reference: DB Selection Cheat Sheet

```
Need ACID transactions?          → PostgreSQL / MySQL
High write throughput (>10K/s)?  → Cassandra / DynamoDB
Simple key-value lookups?        → Redis / DynamoDB
Full-text search?                → Elasticsearch
Time-series metrics?             → InfluxDB / TimescaleDB
Flexible schema + documents?     → MongoDB / Firestore
Graph traversal?                 → Neo4j / Neptune
Analytics / OLAP?                → ClickHouse / BigQuery / Redshift
```
