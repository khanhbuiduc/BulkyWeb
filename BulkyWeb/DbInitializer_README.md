# Database Initializer

## Mô t?
DbInitializer t? ??ng kh?i t?o database khi application kh?i ??ng l?n ??u tiên.

## Ch?c n?ng

### 1. T? ??ng Apply Migrations
- Ki?m tra các migrations ch?a ???c apply
- T? ??ng apply các migrations pending
- Log thông tin v? quá trình migration

### 2. T?o Roles
T? ??ng t?o 4 roles n?u ch?a t?n t?i:
- **Customer**: Khách hàng thông th??ng
- **Company**: Công ty (B2B customer)
- **Employee**: Nhân viên
- **Admin**: Qu?n tr? viên

### 3. T?o Admin User M?c ??nh
T? ??ng t?o admin user v?i thông tin:
- **Email**: admin@bulkybook.com
- **Password**: Admin@123
- **Role**: Admin
- **Thông tin khác**: ???c ?i?n s?n ?? test

## Setup trong Program.cs

```csharp
// ??ng ký service
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// Kh?i t?o database sau khi build app
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    dbInitializer.Initialize();
}
```

## Cách s? d?ng

### L?n ??u ch?y application:
1. Application s? t? ??ng:
   - Apply t?t c? migrations
   - T?o các roles
   - T?o admin user m?c ??nh

2. Login v?i tài kho?n admin:
   - Email: `admin@bulkybook.com`
   - Password: `Admin@123`

### Các l?n ch?y ti?p theo:
- DbInitializer s? ki?m tra và ch? apply migrations m?i (n?u có)
- Không t?o l?i roles ho?c admin user n?u ?ã t?n t?i

## Logging
DbInitializer ghi log thông tin v?:
- Migrations ???c apply
- Roles ???c t?o
- Admin user ???c t?o
- Các l?i x?y ra (n?u có)

## Security Notes
?? **Quan tr?ng**: Nên ??i password c?a admin user ngay sau khi setup xong!

## Files
- `Bulky.DataAccess/DbInitializer/IDbInitializer.cs` - Interface
- `Bulky.DataAccess/DbInitializer/DbInitializer.cs` - Implementation
- `Program.cs` - Setup và kh?i t?o
