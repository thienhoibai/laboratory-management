# ✅ Fixed: DbContext Registration Conflict

## 🐛 Problem

**Error:**
```
System.AggregateException: 'Some services are not able to be constructed 
(Error while validating the service descriptor 
'ServiceType: Microsoft.EntityFrameworkCore.Internal.IDbContextPool`1[TestOrder.Infrastructure.Data.TestOrderDBContext] 
Lifetime: Singleton ImplementationType: Microsoft.EntityFrameworkCore.Internal.DbContextPool`1
```

**Root Cause:**
Đăng ký **2 lifetime khác nhau** cho cùng một DbContext:

```csharp
// ❌ CONFLICT: 2 registrations
builder.Services.AddDbContext<TestOrderDBContext>(...)              // Scoped
builder.Services.AddPooledDbContextFactory<TestOrderDBContext>(...) // Singleton
```

---

## ✅ Solution

Chỉ dùng **1 registration** với `AddPooledDbContextFactory` + `AddScoped` resolver:

```csharp
// ✅ CORRECT: Single source of truth
builder.Services.AddPooledDbContextFactory<TestOrderDBContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Scoped DbContext resolver cho Controllers/Services
builder.Services.AddScoped<TestOrderDBContext>(sp => 
{
    var factory = sp.GetRequiredService<IDbContextFactory<TestOrderDBContext>>();
    return factory.CreateDbContext();
});
```

---

## 📊 How it works

### Before (Conflict):
```
AddDbContext           → TestOrderDBContext (Scoped)
AddPooledDbContextFactory → IDbContextFactory<TestOrderDBContext> (Singleton)
                        → IDbContextPool (Singleton)
❌ Conflict: 2 cách tạo DbContext
```

### After (Fixed):
```
AddPooledDbContextFactory → IDbContextFactory<TestOrderDBContext> (Singleton)
                         → IDbContextPool (Singleton)
AddScoped resolver       → TestOrderDBContext (Scoped, created from Factory)
✅ Single source: DbContext luôn được tạo từ Factory
```

---

## 🎯 Benefits

### 1. **No Conflict**
- Chỉ 1 cách đăng ký DbContext
- Không còn validation error

### 2. **Performance**
- DbContext pooling giảm overhead tạo context mới
- Reuse connections

### 3. **Background Services**
- `IDbContextFactory<T>` dùng cho `IHostedService` (CSV Worker)
- `TestOrderDBContext` inject trực tiếp vào Controllers/Services

---

## 🔧 Usage

### Controllers/Services (Inject DbContext trực tiếp):
```csharp
public class BookingController : ControllerBase
{
    private readonly TestOrderDBContext _db;
    
    public BookingController(TestOrderDBContext db)
    {
        _db = db; // ✅ Works
    }
}
```

### Background Services (Inject Factory):
```csharp
public class CsvIngestWorker : BackgroundService
{
    private readonly IDbContextFactory<TestOrderDBContext> _factory;
    
    public CsvIngestWorker(IDbContextFactory<TestOrderDBContext> factory)
    {
        _factory = factory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var db = await _factory.CreateDbContextAsync(ct); // ✅ Works
        // ...
    }
}
```

---

## ✅ Verification

### Build:
```bash
dotnet build
# ✅ Build successful
```

### Run:
```bash
dotnet run
# ✅ No more validation errors
```

---

## 📝 Related Files

- `TestOrder.Presentation/Program.cs` - Fixed registration
- `TestOrder.Presentation/Workers/CsvIngestWorker.cs` - Uses `IDbContextFactory<T>`

---

## 🚀 Test

```bash
cd TestOrder.Presentation
dotnet run

# Check
curl http://localhost:5001/api/bookings
# Should work without errors
```

---

**Status:** ✅ Fixed  
**Build:** ✅ Success  
**Impact:** High (Critical fix)
