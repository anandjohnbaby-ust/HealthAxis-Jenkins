# 🏥 HealthAxis - Healthcare Management System

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/Angular-21-DD0031?style=for-the-badge&logo=angular" />
  <img src="https://img.shields.io/badge/Blazor-WebAssembly-512BD4?style=for-the-badge&logo=blazor" />
  <img src="https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver" />
  <img src="https://img.shields.io/badge/AWS-ECS-FF9900?style=for-the-badge&logo=amazonaws" />
  <img src="https://img.shields.io/badge/Jenkins-CI%2FCD-D24939?style=for-the-badge&logo=jenkins" />
  <img src="https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq" />
  <img src="https://img.shields.io/badge/Redis-Garnet-DC382D?style=for-the-badge&logo=redis" />
</p>

---

## 📖 Overview

**HealthAxis** is a modern, enterprise-grade Healthcare Management System built using **.NET 10**, **Angular 21**, and **Blazor WebAssembly**.

The application provides dedicated portals for **Patients**, **Doctors**, and **Administrators**, enabling seamless appointment management, health record management, authentication, reporting, and monitoring.

The project follows enterprise development practices including **REST APIs**, **JWT Authentication**, **Repository Pattern**, **Dependency Injection**, **CI/CD**, **Caching**, **Message Queues**, **Structured Logging**, and **Cloud Deployment**.

---

# 🚀 Features

## 👨‍⚕️ Patient Portal

- Patient Registration & Login
- JWT Authentication
- Book Appointments
- Cancel Appointments
- View Appointment History
- View Health Records
- Dashboard
- Responsive UI

---

## 🩺 Doctor Portal

- Secure Login
- Doctor Dashboard
- Today's Schedule
- Weekly Schedule
- Manage Patient Appointments
- Update Appointment Status
- Create Health Records
- View Patient History

---

## 👨‍💼 Admin Portal

Built using **Blazor WebAssembly Standalone**

- Secure Admin Login
- Doctor Management
- Patient Management
- Appointment Monitoring
- Reports & Analytics
- Dashboard

---

# 🏗️ Tech Stack

## Backend

- .NET 10 Web API
- ASP.NET Core Identity
- Entity Framework Core 10
- SQL Server
- AutoMapper
- JWT Authentication
- Global Exception Handler
- Dependency Injection
- Repository Pattern
- REST APIs
- Async Programming

---

## Frontend

### Angular 21

Used for:

- Patient Portal
- Doctor Portal

Technologies:

- Angular 21
- RxJS
- Angular Router
- Reactive Forms
- Route Guards
- HTTP Client

---

### Blazor WebAssembly Standalone

Used for:

- Admin Portal

---

# ⚙️ Enterprise Features

✅ JWT Authentication

✅ Role-Based Authorization

✅ Global Exception Handling

✅ Dependency Injection

✅ Repository Pattern

✅ AutoMapper

✅ Entity Framework Core

✅ RESTful APIs

✅ Asynchronous Programming

✅ DTO Mapping

✅ Validation

---

# 📨 Messaging

RabbitMQ is used for asynchronous communication.

Features:

- Event-Driven Architecture
- Background Processing
- Reliable Message Delivery
- Event Consumers
- Notification Processing

---

# ⚡ Caching

Redis Cache using **Microsoft Garnet Server**

Used for:

- Doctor Availability
- Frequently Accessed Data
- Performance Optimization
- Reduced Database Load

---

# 📊 Logging & Monitoring

## Serilog

- Structured Logging
- Centralized Logging
- Exception Logging

## Elasticsearch

- Stores Application Logs

## Kibana

Provides dashboards for:

- API Logs
- Errors
- Performance Metrics
- Request Monitoring

---

# 🔐 Security

- JWT Authentication
- ASP.NET Core Identity
- Role-Based Authorization
- Password Hashing
- Protected API Endpoints
- Authentication Middleware

### Roles

- Admin
- Doctor
- Patient

---

# ☁️ Cloud & DevOps

## AWS

- AWS ECS (Elastic Container Service)

---

## CI/CD

Implemented using **Jenkins**

Pipeline includes:

- Source Checkout
- Restore Packages
- Build Solution
- Run Tests
- SonarQube Analysis
- Publish
- Docker Image Build
- Deploy to AWS ECS

# 🗄️ Database

Database: **SQL Server**

Main Tables

- Users
- Patients
- Doctors
- Appointments
- Health Records
- Notifications

---

# 📡 API Highlights

- RESTful APIs
- CRUD Operations
- Pagination
- Filtering
- Validation
- DTO Mapping
- Async Operations
- Role-Based Authorization

---

# ⚡ Performance Optimizations

- Redis Caching
- Async/Await
- Repository Pattern
- Optimized SQL Queries
- Dependency Injection
- Structured Logging
- Background Processing

---

# 🔄 Application Workflow

```text
Patient / Doctor / Admin
           │
           ▼
      Angular / Blazor
           │
           ▼
      .NET 10 Web API
           │
 ┌─────────┼──────────┐
 │         │          │
 ▼         ▼          ▼
SQL     Redis      RabbitMQ
Server  Garnet     Messaging
 │                    │
 ▼                    ▼
Health Data     Background Jobs
```

---

# 🚀 CI/CD Pipeline

```text
Developer
    │
    ▼
GitHub Repository
    │
    ▼
Jenkins Pipeline
    │
    ├── Restore
    ├── Build
    ├── Test
    ├── SonarQube Analysis
    ├── Publish
    ├── Docker Build
    ├── Push Image
    └── Deploy to AWS ECS
```

---

# 🛠️ Prerequisites

Before running the project, install:

- .NET 10 SDK
- Node.js
- Angular CLI
- SQL Server
- RabbitMQ
- Microsoft Garnet Server
- Elasticsearch
- Kibana
- Docker Desktop
- Jenkins (Optional)

---

# ▶️ Running the Project

## Clone Repository

```bash
git clone https://github.com/<anandjohnbaby-ust>/HealthAxis.git

cd HealthAxis
```

---

## Backend

```bash
cd HealthAxis.API

dotnet restore

dotnet ef database update

dotnet run
```

---

## Angular

```bash
cd HealthAxis.Angular

npm install

ng serve
```

---

## Blazor Admin

```bash
cd HealthAxis.Admin

dotnet run
```

# 🛠️ Skills Demonstrated

- ASP.NET Core 10
- Angular 21
- Blazor WebAssembly
- Entity Framework Core
- SQL Server
- JWT Authentication
- ASP.NET Identity
- AutoMapper
- Repository Pattern
- Dependency Injection
- Global Exception Handling
- RabbitMQ
- Redis (Garnet)
- Serilog
- Elasticsearch
- Kibana
- Jenkins
- AWS ECS
- Git & GitHub
- REST API Design
- CI/CD
- Enterprise Application Architecture

---

# 👨‍💻 Author

**Anand John**

Full Stack .NET Developer

---

## ⭐ If you found this project helpful, consider giving it a star!