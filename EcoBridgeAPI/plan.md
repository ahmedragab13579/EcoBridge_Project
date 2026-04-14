# Context

I am building an ASP.NET Core Web API called "EcoBridge". The architecture uses a custom `Result<T>` pattern for all service responses, Entity Framework Core for data access, and JWT for authentication.

I need to implement the missing "Donation Creation and Management" module for the "Donor" role (Task assigned to Dev 2). The location will remain a simple string, so no map integration is needed.

# Existing Dependencies to utilize

- `EcoBridgeDbContext` (contains `Donations` DbSet).
- `Result<T>` class for responses: `Result.Success(value, message)` and `Result.Fail(value, message)`.
- Enums: `DonationStatus.Pending`
- DTOs (already created):
  - `CreateDonationDTO` (FoodType, Quantity, ExpiryDate, PickupLocation, IFormFile? Image)
  - `UpdateDonationDTO` (nullable fields of the above)

# Tasks Required

## Task 1: Cloudinary Image Service

Create a service to handle image uploads using the `CloudinaryDotNet` package.

1. Create `CloudinarySettings` class (CloudName, ApiKey, ApiSecret) to bind from appsettings.
2. Create `IPhotoService` with a method: `Task<string?> UploadImageAsync(IFormFile file)`.
3. Create `PhotoService` implementing the interface. Validate the file (size/extension if necessary) and return the secure URL string.

## Task 2: Update IDonationService & DonationService

Add the following methods to the existing `DonationService`.
**Business Rules:**

- Only the Donor who created the donation can Update or Delete it.
- Updates and Deletions are ONLY allowed if the `DonationStatus` is `Pending` (Value = 0).

1. **CreateDonation:**
   - Input: `int accountId`, `CreateDonationDTO dto`.
   - Logic: Find the `DonorId` using the `accountId`. Upload the image using `IPhotoService` if provided. Map DTO to `Donation` entity (Status = Pending, CreatedAt = DateTime.UtcNow). Save to DB.
   - Return: `Result<int>` (the new Donation ID).

2. **UpdateDonation:**
   - Input: `int donationId`, `int accountId`, `UpdateDonationDTO dto`.
   - Logic: Find donation. Verify it belongs to the donor associated with `accountId`. Verify status is `Pending`. Update provided fields. Upload new image if provided and update the `ImageUrl`. Save to DB.
   - Return: `Result<bool>`.

3. **DeleteDonation:**
   - Input: `int donationId`, `int accountId`.
   - Logic: Find donation. Verify it belongs to the donor associated with `accountId`. Verify status is `Pending`. Remove from DB. Save.
   - Return: `Result<bool>`.

## Task 3: Donation Controller Endpoints

Add the following endpoints to the existing `DonationController`:

- **POST `/api/donation`**: `[Authorize(Roles = "Donor")]`. Extracts AccountId from `User.FindFirst(ClaimTypes.NameIdentifier)`, calls `CreateDonation`.
- **PUT `/api/donation/{id}`**: `[Authorize(Roles = "Donor")]`. Extracts AccountId, calls `UpdateDonation`.
- **DELETE `/api/donation/{id}`**: `[Authorize(Roles = "Donor")]`. Extracts AccountId, calls `DeleteDonation`.

# Output Requirements

Please generate ONLY the C# code for:

1. `CloudinarySettings.cs`
2. `IPhotoService.cs` & `PhotoService.cs`
3. The new methods for `IDonationService.cs` & `DonationService.cs`
4. The new endpoints for `DonationController.cs`
   Do not provide explanations, just the clean, production-ready code matching the project's existing style.
