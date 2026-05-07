# Distributed System And Consistency by Anshika Mishra

A collection of independent computers that appears to it's user as a single coherent system.

Passing message rather than sharing memory.

## Why they matter ?

- Scalability - Horizontal and vertical, adding notes
- Geo Proximity - Lower latency by serving request by nearest server
- Resilience - No single point of failure
- Cost Efficiency - No super computer, use commodity hardware

### Real World Example,

**Netflix**

Microservice architecture + CDN

**AWS**

- S3 - Now it is strongly consistent.
- ECS, Lambda

## Key Challenges

- **Partial Failures** - Unexpected failures leads to indeterminate state.
- **Latency & Timing** - Tail latency and ordering events
- **Network Partitions** - Choose between availability and consistency
- **State Management** - Keeping the replicas in sync
- **Idempotency** - Safe to execute multiple times without any side effect
- **Observability** - Correlated logs required, distributed tracing.
- **Load & Pressure**
  - Back Pressure - Slowing down producers when consumers are overwhelmed.
  - Load Shedding - Dropping request when capacity is reached to save the system.
  - Throttling - Limiting rates per user/service.
  - Cascading Failures - One service failure taking down others via timeouts.

## CAP Theorem

### Consistency

Every read receives the most recent write or an error. In distributed systems this implies linearizability (atomic consistency).

### Availability

Every request we are sending should guarantee a response.

### Partition Tolerance

The system continues to operate despite an arbitrary number of message being dropped or delayed by the network.

In CAP Theorem, partition tolerance is mandatory.

## Consistency Models

### Strong Consistency (CP System)

Linearizability

Operation appears instantaneous and globally ordered in real time. Once write completes, all subsequent reads see it.

Trade off - System may be unavailable in case of failures.

#### Best For

- Banking systems

### Causal Consistency

All the related events are ordered and all the unrelated events are not ordered.

Cause and effect.

Preserves the order of causally related events (e.g replies follow posts). Unrelated operations can be seen in difference orders.

Better availability than strong consistency.

#### Best For

Social feeds and comments
Collaborative editing.

### Eventual Consistency (AP System)

Highest availability, lowest latency.

If new updates are made, all replicas will eventually converge to the same state. No guarantees on how long it takes.

#### Best For

Shopping carts
Like/View comments

## Consensus: Paxos vs Raft

### Paxos (The Academic Standard)

Proven, minimal core; theoretically elegant but notoriously difficult to implement correctly

Any node can propose, If majority nodes vote for it, that will be taken as a decision.

### Raft (Understandable Consensus)

Node choose the leader and follow the leader decisions.

Designed for clarity; separates leader election from log replication. Strong leader model.

## Reliable Design Patterns

### Leader Election

One node becomes the leader all other nodes will follow it.

### Quorums Read/Writer

Node comes to decision if enough number of nodes are agree to it.

### Write-Ahead Log

Persists state changes to a durable append only log before applying them, ensuring atomicity and recovery after crashes.

### Replication

Redundancy strategies (Multi Leader Follower, Geo replication) to increase availability and reduce latency for read heavy loads.

### Idempotency

Designing operation so that repeating them produces the same result as a single execution.

### Sagas

Way of handling transactions

Instead of making single big transaction, we are breaking into local transactions.

Each service has to define what it will be if transaction failure and roll back strategies and how it will inform the other services to roll back it's own sagas.

### Circuit breakers

No point of calling failing services again and again. We can skip the call and retry after sometime. That's what circuit breaker does. It breaks the flow and eventually, it starts calling it.

Prevents cascading failures by detecting repeated errors and temporarily blocking the requests to the failing service.

### Dead Letter Queue

Captures messages that cannot be processed successfully after retries, allowing for manual inspection and debugging.

## Real System: Design Choices in Practice

### Google Spanner

Globally distributed database.

**Model:** Strongly consistent system (CP)

**Key Tech:** TrueTime API using atomic clocks/GPS

Achieves global synchronous replication and distributed transactions with strict ordering.

### etcd

Control center for kubernates

**Model:** Strong consistent (CP)
**Key Tech** Raft consensus

The brain of Kubernates. Prioritizes consistency over availability to ensure cluster state correctness.

### Apache Kafka

Distributed streaming platform

**Model:** Turnable/Log based
**Key Tech** ISR (In Sync Replicas)

Supports idempotent producers and transactional writes for "exactly-once" stream processing.

### Amazon S3

**Model:** Strong consistency
**Key Tech** Distributed caching logic

Since 2020, S3 delivers strong read after write consistency for all operations, simplifying app design.

### DynamoDB

**Model:** Tunable (AP/CP)
**Key Tech** Leaderless Replication

Default to eventual consistency for speed; offers optional strongly consistent read at 2X cost.

### Cassandra

**Model:** Wide column / AP
**Key Tech** Tunable consistency

Masterless architecture. You configure $R + W > N$ per query to trade latency for consistency.

### MongoDB

**Model:** Document / CP
**Key Tech** Replica Sets

Single leader by default (strong consistency). Write concern levels allow turning for durability vs speed.

### The Shift

Modern systems are moving away from "pure AP" chaos. The trend is toward strong defaults (S3, Spanner) with optional relaxation for specific performance needs.

## Key Takeaways

- Consistency is a Product Choice: Align technical guarantees with user expectations, not just database defaults.

- CAP Reality: Partition happen. You must deliberately choose between blocking (CP) or serving stale data (AP)

- Toolbox Approach: Use Raft/Paxos for control planes (config/locks) and eventual consistency for high scale data.

- Resilience Patterns: Implement Idempotency, retries with backoff, and circuit breaker to handle partial failures.

## Decision Checklist

### What are the invariants?

Is double spending fatal (Strong) or just annoying (Eventual) ?

### What is the latency budget?

Can you afford cross region round trips for every write?

### What is the failure model?

How must the system behave when a region goes dark?

### Read vs . Write Ratio?

Heavy reads favor replications/caching (Eventual).
