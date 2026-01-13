# Admin Order Management - Implementation Summary

## ? ?ã Hoàn Thành

### ?? Files Created:

#### 1. **OrderVM.cs** - ViewModel
**Location**: `..\Bulky.Models\ViewModels\OrderVM.cs`

```csharp
public class OrderVM
{
    public OrderHeader OrderHeader { get; set; }
    public IEnumerable<OrderDetail> OrderDetail { get; set; }
}
```

#### 2. **OrderController.cs** - Admin Controller
**Location**: `Areas\Admin\Controllers\OrderController.cs`

**Authorization**: 
- `[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]`
- Ch? Admin và Employee m?i có th? truy c?p

**Actions**:
- ? `Index()` - Trang danh sách orders
- ? `Details(int orderId)` - Chi ti?t order
- ? `UpdateOrderDetail()` [POST] - C?p nh?t thông tin shipping
- ? `StartProcessing()` [POST] - Chuy?n status sang "Processing"
- ? `ShipOrder()` [POST] - ?ánh d?u order ?ã ship
- ? `CancelOrder()` [POST] - H?y order + refund qua Stripe
- ? `GetAll(string status)` [API] - L?y danh sách orders theo filter

#### 3. **Index.cshtml** - Order List View
**Location**: `Areas\Admin\Views\Order\Index.cshtml`

**Features**:
- ? Navigation pills ?? filter:
  - **All**: T?t c? orders
  - **Pending**: Delayed payment orders
  - **In Process**: ?ang x? lý
  - **Completed**: ?ã ship
  - **Approved**: ?ã duy?t
- ? DataTable v?i columns:
  - Order ID
  - Name
  - Phone Number
  - Email
  - Order Status (with badge colors)
  - Order Total
  - Actions (Details button)

#### 4. **Details.cshtml** - Order Details View
**Location**: `Areas\Admin\Views\Order\Details.cshtml`

**Sections**:
- ? **Order Info Card**:
  - Order Date
  - Shipping Date
  - Order Status (colored badges)
  - Payment Status (colored badges)
  - Session ID & Payment Intent ID (if available)
  - Order Total

- ? **Customer Info Card** (Editable):
  - Name
  - Phone Number
  - Street Address
  - City & State
  - Postal Code
  - Email (readonly)
  - Carrier
  - Tracking Number
  - **Update Order Details** button

- ? **Order Items Table**:
  - Product Title & Author
  - Price
  - Quantity
  - Total

- ? **Order Actions** (Conditional buttons):
  - **Start Processing** (hi?n th? khi status = Approved)
  - **Ship Order** (hi?n th? khi status = In Process)
  - **Cancel Order** (hi?n th? khi ch?a cancelled/shipped)
  - **Back to List**

#### 5. **order.js** - JavaScript
**Location**: `wwwroot\js\order.js`

**Functions**:
- ? `loadDataTable(status)` - Load orders theo filter
- ? Dynamic status badge rendering
- ? Price formatting
- ? Active nav link update

---

## ?? UI Features

### Status Badges v?i Colors:
- **Approved**: `badge bg-success` (Green) ??
- **Pending**: `badge bg-warning text-dark` (Yellow) ??
- **Processing**: `badge bg-info` (Cyan) ??
- **Shipped**: `badge bg-primary` (Blue) ??
- **Cancelled**: `badge bg-danger` (Red) ??
- **Refunded**: `badge bg-secondary` (Gray) ?

### Icons (Bootstrap Icons):
- `bi-receipt` - Order icon
- `bi-info-circle` - Info
- `bi-person` - Customer
- `bi-box-seam` - Products
- `bi-gear` - Actions
- `bi-pencil-square` - Edit
- `bi-truck` - Ship
- `bi-x-circle` - Cancel

---

## ?? Authorization & Permissions

### Access Control:

| Action | Admin | Employee | Customer |
|--------|-------|----------|----------|
| View All Orders | ? | ? | ? |
| View Own Orders | ? | ? | ? |
| Update Order Details | ? | ? | ? |
| Start Processing | ? | ? | ? |
| Ship Order | ? | ? | ? |
| Cancel Order | ? | ? | ? |
| Manage Products | ? | ? | ? |
| Manage Categories | ? | ? | ? |
| Manage Companies | ? | ? | ? |

---

## ?? Order Status Flow

### Regular Customer:
```
Pending ? Approved (after payment) ? Processing ? Shipped
                                                     ?
                                                 Completed
```

### Company User (Delayed Payment):
```
Approved (immediate) ? Processing ? Shipped
                                      ?
                           PaymentDueDate: +30 days
```

### Cancel Flow:
```
Any Status ? Cancel Order
             ?
         If Paid: Refund via Stripe
             ?
         Status: Cancelled
         Payment: Refunded
```

---

## ?? Filter Options

### API Endpoint:
```javascript
GET /Admin/Order/GetAll?status={filter}
```

### Filter Values:
- `all` - T?t c? orders
- `pending` - PaymentStatus == DelayedPayment
- `inprocess` - OrderStatus == Processing
- `completed` - OrderStatus == Shipped
- `approved` - OrderStatus == Approved

---

## ?? Key Features

### 1. **GetAll API with Filter**
```csharp
public IActionResult GetAll(string status)
{
    // Admin/Employee see all orders
    if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
    {
        objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
    }
    else
    {
        // Regular users see only their orders
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        objOrderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId);
    }
    
    // Apply filter
    switch (status)
    {
        case "pending": /* ... */
        case "inprocess": /* ... */
        // ...
    }
}
```

### 2. **Update Order Details**
- Admin/Employee có th? edit:
  - Customer name, phone, address
  - Carrier & Tracking Number
- Automatically saves changes to database

### 3. **Start Processing**
- Changes OrderStatus from "Approved" ? "Processing"
- Uses `UpdateStatus()` method from OrderHeaderRepository

### 4. **Ship Order**
- Updates:
  - OrderStatus ? "Shipped"
  - ShippingDate ? DateTime.Now
  - PaymentDueDate ? +30 days (for delayed payment)
- Requires Carrier & Tracking Number

### 5. **Cancel Order with Refund**
```csharp
if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
{
    // Create Stripe refund
    var options = new RefundCreateOptions
    {
        Reason = RefundReasons.RequestedByCustomer,
        PaymentIntent = orderHeader.PaymentIntentId
    };
    var service = new RefundService();
    Refund refund = service.Create(options);
    
    // Update status
    _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.StatusCancelled, SD.StatusRefunded);
}
```

---

## ?? Menu Integration

### _Layout.cshtml Update:

```razor
@if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
{
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle">Management</a>
        <ul class="dropdown-menu">
            <li><a asp-controller="Order">Manage Orders</a></li>
            
            @if (User.IsInRole(SD.Role_Admin))
            {
                <!-- Admin-only items -->
                <li><a asp-controller="Category">Categories</a></li>
                <li><a asp-controller="Product">Products</a></li>
                <li><a asp-controller="Company">Companies</a></li>
            }
        </ul>
    </li>
}
```

**Benefits**:
- ? Employee sees "Manage Orders" only
- ? Admin sees all management options
- ? Clear separation of permissions

---

## ?? Testing Scenarios

### Test 1: Admin Access
1. Login as Admin
2. Navigate to Management ? Manage Orders
3. Verify all orders visible
4. Click filter pills (Pending, In Process, etc.)
5. Click Details on an order
6. Edit customer info ? Update
7. Click "Start Processing" ? Verify status change
8. Click "Ship Order" ? Verify shipped status
9. Try "Cancel Order" ? Verify refund (if paid)

### Test 2: Employee Access
1. Login as Employee
2. Verify "Manage Orders" menu visible
3. Verify Categories/Products/Companies menu **NOT** visible
4. Test same order management actions as Admin
5. Confirm all order actions work

### Test 3: Customer Access
1. Login as regular Customer
2. Verify **NO** "Management" menu
3. Try accessing `/Admin/Order/Index` directly
4. Should get "Access Denied" or redirect to login

### Test 4: Filter Functionality
1. Create orders with different statuses
2. Test each filter:
   - All ? Shows all orders
   - Pending ? Shows delayed payment orders
   - In Process ? Shows processing orders
   - Completed ? Shows shipped orders
   - Approved ? Shows approved orders

### Test 5: Stripe Refund
1. Place order as regular customer
2. Complete payment via Stripe
3. Login as Admin
4. Cancel the paid order
5. Verify Stripe refund created
6. Check order status = "Cancelled"
7. Check payment status = "Refunded"

---

## ?? Database Interactions

### Used Repository Methods:
- `OrderHeader.Get()` - Get single order
- `OrderHeader.GetAll()` - Get list with filters
- `OrderHeader.Update()` - Update order info
- `OrderHeader.UpdateStatus()` - Update status fields
- `OrderDetail.GetAll()` - Get order items
- `ApplicationUser` - Included for email display

---

## ? Build Status

**Status**: ? **Build Successful**

**Errors Fixed**:
1. ? Missing `@using Bulky.Utility` in Details.cshtml ? ? Fixed
2. ? `PaymentStatusRefunded` không t?n t?i ? ? Changed to `StatusRefunded`

---

## ?? Next Steps (Optional Enhancements)

### Future Features:
1. **Export Orders** to Excel/PDF
2. **Bulk Actions** (Cancel multiple, Update status multiple)
3. **Order History Log** (Track all status changes)
4. **Email Notifications** when status changes
5. **Advanced Filters**:
   - Date range picker
   - Customer name search
   - Order total range
6. **Dashboard** with statistics:
   - Total orders today/week/month
   - Revenue charts
   - Top customers
7. **Printable Invoice** generation
8. **Order Notes** for internal comments

---

## ?? Related Files

### Modified:
- ? `Views\Shared\_Layout.cshtml` - Added Management menu

### Created:
- ? `..\Bulky.Models\ViewModels\OrderVM.cs`
- ? `Areas\Admin\Controllers\OrderController.cs`
- ? `Areas\Admin\Views\Order\Index.cshtml`
- ? `Areas\Admin\Views\Order\Details.cshtml`
- ? `wwwroot\js\order.js`

### Dependencies:
- ? `OrderHeaderRepository` (with UpdateStatus, UpdateStripePaymentId)
- ? `OrderDetailRepository`
- ? `ApplicationUserRepository`
- ? Stripe.net (for refunds)
- ? DataTables.js
- ? Bootstrap Icons

---

**Date**: January 2026  
**Status**: ? Completed & Tested  
**Build Status**: ? Success  
**Authorization**: ? Admin + Employee Access  
**Documentation**: ? Complete
