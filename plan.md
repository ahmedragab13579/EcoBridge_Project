# Technical PRD: EcoBridge Backend Data Layer

## 1. Project Architecture Overview
[cite_start]The project follows a clean structure focusing on separating business logic (Domains) from data access (Data)[cite: 15, 63].
* **Namespace Structure**: 
    * [cite_start]`EcoBridge.Domains.Models`: For all database entities[cite: 70, 91, 120].
    * [cite_start]`EcoBridge.Domains.Enums`: For status and role definitions[cite: 113, 114, 190].
    * [cite_start]`EcoBridge.Data`: For the `DbContext` and Fluent API configurations[cite: 15, 130].

---

## 2. Domain Models Specification (Domains/Models)

### A. Core Identity: Account
* [cite_start]**Id**: `int` (Primary Key)[cite: 71, 78].
* [cite_start]**FullName**: `nvarchar(150)`, Required[cite: 72, 79].
* [cite_start]**Email**: `nvarchar(255)`, Required[cite: 73, 80].
* [cite_start]**Phone**: `nvarchar(20)`, Required[cite: 74, 81].
* [cite_start]**PasswordHash**: `nvarchar(max)`, Required[cite: 75, 82].
* [cite_start]**RoleId**: `tinyint`, Required (Maps to UserRole Enum)[cite: 76, 83].
* [cite_start]**CreatedAt**: `datetime2`, Required[cite: 77, 117].

### B. Specialized Roles (1:1 Relationship with Account)
[cite_start]All following entities use `AccountId` as both Primary Key and Foreign Key[cite: 85, 121, 123, 128]:
* [cite_start]**Donor**: Includes `UserType` (tinyint) and `OrganizationName` (nvarchar(150), Nullable)[cite: 84, 86, 87].
* [cite_start]**Charity**: Includes `OrganizationName` (nvarchar(150)) and `RegistrationNumber` (nvarchar(50))[cite: 120, 111, 115].
* [cite_start]**Volunteer**: Includes `VehicleDetails` (nvarchar(100), Nullable)[cite: 122, 124, 127].
* [cite_start]**Admin**: Minimal table linked to Account[cite: 125, 128].

### C. Transactions: Donation
* [cite_start]**Id**: `int` (Primary Key)[cite: 92, 93].
* [cite_start]**DonorId**: `int`, Required (FK to Donor)[cite: 94, 95].
* [cite_start]**CharityId**: `int`, Nullable (FK to Charity)[cite: 96, 97].
* [cite_start]**VolunteerId**: `int`, Nullable (FK to Volunteer)[cite: 98, 99].
* [cite_start]**FoodType**: `nvarchar(100)`, Required[cite: 100, 101].
* [cite_start]**Quantity**: `nvarchar(50)`, Required[cite: 102, 103].
* [cite_start]**ExpiryDate**: `datetime2`, Required[cite: 104, 105].
* [cite_start]**PickupLocation**: `nvarchar(255)`, Required[cite: 106, 107].
* [cite_start]**ImageUrl**: `nvarchar(max)`, Nullable[cite: 109, 110].
* [cite_start]**Status**: `tinyint`, Required (Maps to DonationStatus Enum)[cite: 113, 114].
* [cite_start]**CreatedAt**: `datetime2`, Required[cite: 118, 119].

---

## 3. Enums Definition (Domains/Enums)
* [cite_start]**UserRole**: `Admin = 1`, `Donor = 2`, `Charity = 3`, `Volunteer = 4`[cite: 16, 23].
* [cite_start]**DonationStatus**: `Pending = 0`, `Accepted = 1`, `PickedUp = 2`, `Delivered = 3`, `Cancelled = 4`[cite: 47].

---

## 4. LLM Implementation Prompt

> [cite_start]**Context**: I am building the Data Layer for "EcoBridge" using ASP.NET Core 8 and EF Core[cite: 15].
> **Task**: Generate the following C# files based on the PRD:
> 
> 1. **Enums**: Create `UserRole` and `DonationStatus` in the `Domains.Enums` namespace.
> 2. **Models**: Create all Entity classes in `Domains.Models`. [cite_start]Use `Data Annotations` for basic constraints[cite: 130].
> 3. **DbContext**: Create `EcoBridgeDbContext` in the `Data` folder.
> 4. **Fluent API**: Inside `OnModelCreating`, explicitly define:
>    [cite_start]- The **One-to-One** mapping between `Account` and (`Donor`, `Charity`, `Volunteer`, `Admin`) using `AccountId`[cite: 176, 182, 187].
>    [cite_start]- The **Foreign Key** relationships for the `Donations` table[cite: 154, 159, 160].
>    [cite_start]- Proper SQL types (e.g., `tinyint` for Enums, `nvarchar` lengths)[cite: 143, 145, 149].
> 
> **Goal**: The code should be ready to run migrations immediately.