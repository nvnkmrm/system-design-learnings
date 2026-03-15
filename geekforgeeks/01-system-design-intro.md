# Table of Contents

- [System Design](#system-design)
  - [Introduction](#introduction)
- [LLD vs HLD](#lld-vs-hld)
  - [High-Level Design (HLD) – The Big Picture](#high-level-design-hld-the-big-picture)
  - [Low-Level Design (LLD) – Internal Implementation](#low-level-design-lld-internal-implementation)

---

# System Design

## Introduction

- Process of designing **-->** architecture, component, interface **-->** meeting end user requirements.

### System Design Process

- Planning and structuring architecture of software system.
- User requirement **to** technical blueprint.
- System component **-->** data flow **-->** interaction between services.
- Well organized and efficient structure that meets intended purpose while considering factors,
  - Scalability
  - Maintainability
  - Performance

### Importance

- Build scalable, robust and efficient software application.
- Help architect solution that handle real world complexity.
- **Scalability and Reliability** - handle increased demand without failure.
- **Efficient Resource Management** - optimize resource allocation, responsive application.
- **Adaptability** - changing business needs.
- **Architectural Understanding** - Learning different architecture (microservice and monolithic), for build suitable application.

### System Design in SDLC

- Without system design one cannot jump straight to implementation or testing
- Vital step, serves as backbone for implementation

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
