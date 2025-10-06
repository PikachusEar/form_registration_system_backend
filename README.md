#  Database Quick Reference Card

##  Standard Update Workflow

```bash
# Step 1: Make changes
vim Models/Registration.cs
vim DTOs/RegistrationDtos.cs

# Step 2: Create migration
dotnet ef migrations add DescriptiveName

# Step 3: Apply to database
dotnet ef database update

# Step 4: Verify
docker exec -it ap_registration_db psql -U apuser -d ap_registration
\d "Registrations"
\q

# Step 5: Test
dotnet run

# Step 6: Commit
git add Migrations/ Models/ DTOs/
git commit -m "Update: description"
```

---

##  Migration Commands

| Command | Purpose |
|---------|---------|
| `dotnet ef migrations add <Name>` | Create new migration |
| `dotnet ef migrations list` | List all migrations |
| `dotnet ef migrations remove` | Delete last migration (not applied) |
| `dotnet ef database update` | Apply all pending migrations |
| `dotnet ef database update <Name>` | Revert to specific migration |
| `dotnet ef database update 0` | Revert all migrations |

---

##  Docker Commands

| Command | Purpose |
|---------|---------|
| `docker-compose ps` | Check database status |
| `docker-compose up -d` | Start database |
| `docker-compose down` | Stop database |
| `docker-compose down -v` | Stop & delete all data ⚠️ |
| `docker-compose restart` | Restart database |
| `docker-compose logs postgres` | View logs |

---

## 💾 Database Connection

```bash
# Connect to database
docker exec -it ap_registration_db psql -U apuser -d ap_registration

# Alternative (from host)
psql -h localhost -p 5432 -U apuser -d ap_registration
```

**Connection String:**
```
Host=localhost;Port=5432;Database=ap_registration;Username=apuser;Password=YourSecurePassword123!
```

---

##  PostgreSQL Commands (in psql)

### Basic Navigation
| Command | Description |
|---------|-------------|
| `\l` | List databases |
| `\dt` | List tables |
| `\d "TableName"` | Table structure |
| `\d+ "TableName"` | Detailed structure |
| `\du` | List users |
| `\q` | Quit |

### Useful Queries
```sql
-- View table structure
\d "Registrations"

-- Count records
SELECT COUNT(*) FROM "Registrations";

-- View recent registrations
SELECT * FROM "Registrations" 
ORDER BY "CreatedAt" DESC LIMIT 5;

-- Check migration history
SELECT * FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";

-- View by payment status
SELECT "PaymentStatus", COUNT(*) 
FROM "Registrations" 
GROUP BY "PaymentStatus";
```

---

##  Troubleshooting

| Problem | Solution |
|---------|----------|
| "Connection refused" | `docker-compose ps` (check if running) |
| "Port already in use" | Change port in `docker-compose.yml` |
| "Migration failed" | `dotnet ef migrations remove` → fix → try again |
| "Table already exists" | Drop database → recreate → migrate |
| "Can't find migration" | `dotnet ef migrations list` |

---

##  Common Scenarios

### Undo Last Migration (Not Applied)
```bash
dotnet ef migrations remove
# Fix your model
dotnet ef migrations add FixedVersion
dotnet ef database update
```

### Undo Applied Migration
```bash
# Revert to previous
dotnet ef database update PreviousMigrationName

# Remove bad migration
dotnet ef migrations remove

# Create corrected migration
dotnet ef migrations add CorrectedVersion
dotnet ef database update
```

### Complete Reset ⚠️
```bash
# Delete migrations
rm -rf Migrations/

# Drop & recreate database
docker exec -it ap_registration_db psql -U apuser -d postgres \
  -c "DROP DATABASE ap_registration;"
docker exec -it ap_registration_db psql -U apuser -d postgres \
  -c "CREATE DATABASE ap_registration;"

# Fresh start
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## ✅ Pre-Deployment Checklist

- [ ] Migration created with descriptive name
- [ ] Migration reviewed for correctness
- [ ] Migration applied successfully
- [ ] Database schema verified
- [ ] API runs without errors
- [ ] Swagger tests pass
- [ ] Frontend updated (if needed)
- [ ] Changes committed to Git
- [ ] Backup created (production only)

---

##  Quick Help

**Docker not running?**
```bash
open -a Docker  # Start Docker Desktop on macOS
```

**Check everything is healthy:**
```bash
docker-compose ps
dotnet --version
dotnet ef --version
```

**View API logs:**
```bash
dotnet run --verbosity detailed
```

---
**Project:** AP Exam Registration System  
**Database:** PostgreSQL 16