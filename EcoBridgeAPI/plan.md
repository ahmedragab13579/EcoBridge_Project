# Technical PRD: Developer 5 (Admin Dashboard & Statistics)

## 1. Module Overview
[cite_start]This module is dedicated to providing the system administrator with high-level oversight and real-time data visualization of the EcoBridge platform's performance[cite: 49, 51].
* **Namespace Structure**: 
    * [cite_start]`EcoBridgeAPI.Services`: For statistics aggregation and LINQ queries.
    * [cite_start]`EcoBridgeAPI.Controllers`: For secure, Admin-only API endpoints[cite: 27, 29].

---

## 2. Functional Requirements (Admin Analytics)
[cite_start]The Admin Dashboard must provide APIs to return structured data for the following Key Performance Indicators (KPIs)[cite: 51, 63]:
* [cite_start]**Total Donations Count**: A summary of all food donation requests submitted by Donors[cite: 51].
* [cite_start]**Completed Deliveries**: Total count of donations with `Status == 3` (Delivered)[cite: 47, 51].
* [cite_start]**Active Volunteers**: Count of all registered accounts in the `Volunteers` table[cite: 51].
* [cite_start]**User Breakdown**: Total number of registered Donors, Charities, and Volunteers linked via the Identity system[cite: 10, 63].

---

## 3. Data & Security Logic (Identity Integrated)
* [cite_start]**Admin Profile**: Linked to the `Account` (inheriting from `IdentityUser<int>`) via a 1:1 relationship using `AccountId`[cite: 125, 128].
* [cite_start]**Authorization**: All endpoints in this module must be strictly protected using `[Authorize(Roles = "Admin")]`[cite: 27, 63].
* [cite_start]**Data Retrieval**: Use efficient Async LINQ queries (e.g., `CountAsync`) through the `EcoBridgeDbContext` to avoid performance bottlenecks[cite: 15, 130].

---

## 4. GitHub Milestone: Admin Core Features
### Milestone: Admin Dashboard MVP
* **Issue #1: Admin Statistics API**: Create an `AdminStatsDTO` and a controller endpoint to return total counts for donations, users, and successful deliveries.
* [cite_start]**Issue #2: Delivery Lifecycle Summary**: Implement logic to categorize donations by status (Pending, Accepted, Picked Up, Delivered, Cancelled)[cite: 47].
* [cite_start]**Issue #3: User Activity Overview**: Provide data on the distribution of different user roles within the system[cite: 10].

---

## 5. LLM Implementation Prompt (For Dev 5 Core Tasks)

> **Context**: I am Developer 5 for "EcoBridge" using ASP.NET Core 8 and Identity.
> **Task**: Generate the backend logic for the Admin Statistics module.
> 
> **Requirements**:
> 1. **AdminStatsDTO**: Create a DTO containing `TotalDonations`, `TotalVolunteers`, `CompletedDeliveries`, and `ActiveDonors`.
> 2. **StatisticsService**: Implement a service in the `Data` folder using `EcoBridgeDbContext` (which inherits from `IdentityDbContext`) to fetch these counts asynchronously.
> 3. **AdminController**: Create a controller with a `GET` method `/api/admin/stats`. 
> 4. **Security**: Apply `[Authorize(Roles = "Admin")]` and ensure it uses JWT role enforcement.
> 
> **Focus**: Provide only the Service, Controller, and DTO code. No deployment configuration needed.