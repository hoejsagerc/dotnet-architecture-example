-- This creates the Products table
CREATE TABLE IF NOT EXISTS "Products" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT NOT NULL,
    "Price" DECIMAL(18, 2) NOT NULL,
    "ImageUrl" VARCHAR(2048) NOT NULL,
    "Quantity" INT NOT NULL
);

-- adding indexes for common query pattern name column
CREATE INDEX IF NOT EXISTS "IX_Products_Name" ON "Products" ("Name");