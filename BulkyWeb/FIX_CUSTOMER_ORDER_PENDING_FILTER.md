# Fix: Customer Order Filter "Pending" Issue

## ?? **Problem:**

Khi click filter **"Pending"** trong Customer Order list, không có orders nào hi?n th? m?c dù trong **"All"** có orders v?i status Pending.

---

## ?? **Root Cause:**

### **Logic C? (Sai):**
```csharp
case "pending":
    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
    break;
```

**V?n ??:**
- Ch? filter orders có `PaymentStatus == "ApprovedForDelayedPayment"` (Company users)
- **KHÔNG** filter orders có `OrderStatus == "Pending"` (Regular customers ch?a thanh toán)
- **KHÔNG** filter orders có `PaymentStatus == "Pending"`

---

## ? **Solution:**

### **Logic M?i (?úng):**
```csharp
case "pending":
    // Pending bao g?m: OrderStatus Pending HO?C PaymentStatus DelayedPayment HO?C PaymentStatus Pending
    objOrderHeaders = objOrderHeaders.Where(u => 
        u.OrderStatus == SD.StatusPending || 
        u.PaymentStatus == SD.PaymentStatusDelayedPayment ||
        u.PaymentStatus == SD.PaymentStatusPending);
    break;
```

**Gi?i thích:**
Filter "Pending" bây gi? bao g?m **T?T C?** các tr??ng h?p:

1. ? **OrderStatus = "Pending"**
   - Regular customer ?ã ??t hàng nh?ng ch?a thanh toán
   - Ch? Stripe payment

2. ? **PaymentStatus = "Pending"**
   - Order ?ang ch? x? lý thanh toán

3. ? **PaymentStatus = "ApprovedForDelayedPayment"**
   - Company user v?i thanh toán tr? ch?m
   - Order ?ã approved nh?ng payment ch?a xong

---

## ?? **Order Status Scenarios:**

### **Scenario 1: Regular Customer (Ch?a thanh toán)**
```
OrderStatus: "Pending"
PaymentStatus: "Pending"
```
? **Hi?n th? trong filter "Pending"** ?

### **Scenario 2: Company User (Delayed Payment)**
```
OrderStatus: "Approved"
PaymentStatus: "ApprovedForDelayedPayment"
```
? **Hi?n th? trong filter "Pending"** ?

### **Scenario 3: Regular Customer (?ã thanh toán)**
```
OrderStatus: "Approved"
PaymentStatus: "Approved"
```
? **KHÔNG hi?n th? trong filter "Pending"** ?
? Hi?n th? trong filter "Approved" ?

### **Scenario 4: Order ?ang x? lý**
```
OrderStatus: "Processing"
PaymentStatus: "Approved"
```
? **KHÔNG hi?n th? trong filter "Pending"** ?
? Hi?n th? trong filter "In Process" ?

---

## ?? **Filter Definitions:**

| Filter | Conditions | Use Cases |
|--------|------------|-----------|
| **All** | No filter | T?t c? orders |
| **Pending** | `OrderStatus == "Pending"` <br/>OR `PaymentStatus == "Pending"` <br/>OR `PaymentStatus == "DelayedPayment"` | - Regular customer ch?a thanh toán<br/>- Company delayed payment<br/>- Orders ch? x? lý |
| **Approved** | `OrderStatus == "Approved"` | Orders ?ã ???c duy?t |
| **In Process** | `OrderStatus == "Processing"` | Orders ?ang x? lý |
| **Completed** | `OrderStatus == "Shipped"` | Orders ?ã ship |

---

## ?? **Testing:**

### **Test Case 1: Regular Customer Order (Pending)**
1. Login as **Regular Customer**
2. Place order ? Complete Stripe payment ? **HO?C** cancel payment
3. If cancelled:
   - OrderStatus = "Pending"
   - PaymentStatus = "Pending"
4. Go to "My Orders"
5. Click **"All"** ? Verify order hi?n th?
6. Click **"Pending"** ? ? **Order ph?i hi?n th?**

### **Test Case 2: Company User Order (Delayed Payment)**
1. Login as **Company User**
2. Place order (skip Stripe)
3. Order created:
   - OrderStatus = "Approved"
   - PaymentStatus = "ApprovedForDelayedPayment"
4. Go to "My Orders"
5. Click **"All"** ? Verify order hi?n th?
6. Click **"Pending"** ? ? **Order ph?i hi?n th?** (vì delayed payment)

### **Test Case 3: Paid Order (NOT Pending)**
1. Login as **Regular Customer**
2. Place order ? Complete Stripe payment successfully
3. Order:
   - OrderStatus = "Approved"
   - PaymentStatus = "Approved"
4. Go to "My Orders"
5. Click **"All"** ? Verify order hi?n th?
6. Click **"Pending"** ? ? **Order KHÔNG hi?n th?** (correct!)
7. Click **"Approved"** ? ? **Order hi?n th?**

### **Test Case 4: In Process Order**
1. Admin marks order as "Processing"
2. Customer goes to "My Orders"
3. Click **"Pending"** ? ? **Order KHÔNG hi?n th?**
4. Click **"In Process"** ? ? **Order hi?n th?**

---

## ?? **Complete Filter Logic:**

```csharp
[HttpGet]
public IActionResult GetAll(string status)
{
    var claimsIdentity = (ClaimsIdentity)User.Identity;
    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

    IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader
        .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");

    switch (status)
    {
        case "pending":
            // Bao g?m: Pending orders, Delayed payment, ho?c Payment pending
            objOrderHeaders = objOrderHeaders.Where(u => 
                u.OrderStatus == SD.StatusPending || 
                u.PaymentStatus == SD.PaymentStatusDelayedPayment ||
                u.PaymentStatus == SD.PaymentStatusPending);
            break;
            
        case "inprocess":
            // Ch? orders ?ang x? lý
            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
            break;
            
        case "completed":
            // Ch? orders ?ã ship
            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
            break;
            
        case "approved":
            // Ch? orders ?ã approved
            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
            break;
            
        default:
            // All - không filter
            break;
    }

    return Json(new { data = objOrderHeaders });
}
```

---

## ?? **Same Fix for Admin:**

Admin OrderController c?ng có cùng issue. N?u c?n, apply cùng logic:

```csharp
case "pending":
    objOrderHeaders = objOrderHeaders.Where(u => 
        u.OrderStatus == SD.StatusPending || 
        u.PaymentStatus == SD.PaymentStatusDelayedPayment ||
        u.PaymentStatus == SD.PaymentStatusPending);
    break;
```

---

## ? **Build Status:**

**Status**: ? Build Successful

**Hot Reload**: Restart app ?? apply changes

---

## ?? **Related Constants (SD.cs):**

```csharp
// Order Statuses
public const string StatusPending = "Pending";
public const string StatusApproved = "Approved";
public const string StatusInProcess = "Processing";
public const string StatusShipped = "Shipped";
public const string StatusCancelled = "Cancelled";
public const string StatusRefunded = "Refunded";

// Payment Statuses
public const string PaymentStatusPending = "Pending";
public const string PaymentStatusApproved = "Approved";
public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
public const string PaymentStatusRejected = "Rejected";
```

---

## ?? **Result:**

Filter **"Pending"** bây gi? ho?t ??ng chính xác và hi?n th?:
- ? Regular customer orders ch?a thanh toán
- ? Company user orders v?i delayed payment
- ? B?t k? order nào có payment pending

---

**Date**: January 2026  
**Status**: ? Fixed & Tested  
**Issue**: Filter logic incomplete  
**Solution**: Include all pending scenarios
