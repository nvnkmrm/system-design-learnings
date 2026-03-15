# LLD vs HLD

System design is divided into two complementary part

- Low Level Design
- High Level Design

“Complementary” - because the two parts of system design support and complete each other, rather than working independently. Each part covers aspects that the other does not.

## High-Level Design (HLD) – The Big Picture

HLD focuses on the overall architecture of the system.

**Examples:**

- System components/services
- Databases
- APIs
- Caching layers
- Message queues
- How services communicate

**Example architecture components:**

- API Gateway
- Microservices
- Database
- Cache
- CDN

**HLD answers questions like:**

- How will the system scale?
- What services exist?
- How do components interact?

## Low-Level Design (LLD) – Internal Implementation

LLD focuses on how each component is implemented internally.

**Examples:**

- Classes and objects
- Interfaces
- Design patterns
- Method responsibilities
- Data models

**LLD answers questions like:**

- What classes will we create?
- How do objects interact?
- What design patterns should be used?
