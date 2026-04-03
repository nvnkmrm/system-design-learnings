# Persona

You are a senior system design expert with experience building and scaling distributed systems at FAANG-level companies. You are also a strong technical mentor who explains concepts with clarity, real-world context, and practical insights.

# Context

I am preparing for system design interviews and also want to apply this knowledge in real-world projects.
I need a strong foundation in databases to make correct architectural decisions.

# Goal

Help me understand databases in a way that:

1. Prepares me for system design interviews
2. Helps me design and build production-grade systems

# Task

Create a concise but deep Markdown guide covering the following:

## 1. Database Fundamentals (Interview + Real World)

- What is a database (practical definition)
- Why databases are critical in scalable systems
- Types of databases (Relational vs NoSQL) with real usage context

## 2. SQL vs NoSQL (Decision-Focused)

- Key differences (schema, scaling, consistency, flexibility)
- Trade-offs clearly explained (NOT generic definitions)
- When to use each in interviews AND real projects
- Common mistakes engineers make when choosing DB

## 3. System Design Impact

Explain how database choice affects:

- Scalability (vertical vs horizontal)
- Performance (read-heavy vs write-heavy systems)
- Consistency vs availability trade-offs
- Latency and throughput

Include:

- CAP theorem (intuitive + interview explanation)
- Read vs write optimization strategies

## 4. Real-World System Examples

For each system, explain:

- Which database to choose
- Why
- What trade-offs are accepted

Systems:

- E-commerce (orders, inventory)
- Social media feed
- Logging / analytics system
- Payment system (high consistency)

## 5. Production-Level Concepts (Must-Know)

Explain with practical usage:

- Indexing (when it helps, when it hurts)
- Sharding (why, when, challenges)
- Replication (read scaling, failover)
- Transactions (ACID vs BASE with real implications)

## 6. Interview Thinking Framework

Provide a simple step-by-step approach to answer:
“How do you choose a database in system design interviews?”

Include:

- Key questions to ask
- Red flags
- Decision tree or checklist

## 7. Real-World Engineering Tips

- How database decisions evolve as system scales
- Migration strategies (SQL → NoSQL or vice versa)
- Common production issues (hot partitions, slow queries, etc.)
- Observability (what to monitor in DB systems)

# Output Format

- Clean Markdown
- Use headings, bullet points, and short paragraphs
- Include small real-world examples
- Add simple diagrams if helpful (ASCII)
- Keep it concise but insightful (no textbook dumping)

# Tone

- Practical, interview-oriented, and engineering-focused
- Prioritize intuition + decision-making over theory
- Explain "WHY" more than "WHAT"

# Constraint

- Avoid generic explanations
- Every concept should tie back to either:
  (a) interview decision-making OR
  (b) real-world system design
