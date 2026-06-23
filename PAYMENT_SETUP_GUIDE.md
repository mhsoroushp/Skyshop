# Payment Integration Setup Guide

Complete Payment System Implementation with Stripe Test Mode

## 1. Backend Setup (.NET)

### Step 1: Install Stripe NuGet Package
```bash
cd /home/soroush/workspace/skyshop/API
dotnet add package Stripe.net
```

### Step 2: Get Stripe Test API Keys
1. Create account at https://stripe.com
2. Go to Dashboard → Developers → API Keys
3. Copy your **test** keys:
   - Secret Key (starts with `sk_test_`)
   - Publishable Key (starts with `pk_test_`)

### Step 3: Update appsettings.json
Edit `/home/soroush/workspace/skyshop/API/appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_SECRET_KEY_HERE",
    "PublicKey": "pk_test_YOUR_PUBLIC_KEY_HERE",
    "WebhookSecret": "whsec_test_YOUR_WEBHOOK_SECRET_HERE"
  }
}
```

### Step 4: Create Database Migration
```bash
cd /home/soroush/workspace/skyshop/API

# Create migration for new tables
dotnet ef migrations add AddOrderAndPaymentTables --project ../Core --startup-project .

# Update database
dotnet ef database update --project ../Core --startup-project .
```

### Step 5: Test Backend Endpoints
Start the API:
```bash
dotnet run
```

Test order creation:
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"John Doe","customerEmail":"john@example.com"}'
```

---

## 2. Frontend Setup (Angular)

### Step 1: Add Stripe.js CDN
Edit `/home/soroush/workspace/skyshop/client/src/index.html`:

Add this inside the `<head>` tag:
```html
<script src="https://js.stripe.com/v3/"></script>
```

### Step 2: Update Stripe Public Key in PaymentComponent
Edit `/home/soroush/workspace/skyshop/client/src/app/features/payment/payment.component.ts`

Replace this line:
```typescript
const stripePublicKey = 'pk_test_YOUR_TEST_PUBLIC_KEY_HERE';
```

With your actual Stripe test public key.

### Step 3: Update API URL (if different)
If your API runs on a different URL, update:
- `/home/soroush/workspace/skyshop/client/src/app/core/services/order.service.ts`
- `/home/soroush/workspace/skyshop/client/src/app/core/services/payment.service.ts`

Change:
```typescript
private readonly apiUrl = 'http://localhost:5000/api/orders';
```

### Step 4: Build and Run Angular
```bash
cd /home/soroush/workspace/skyshop/client

# Install dependencies (if needed)
npm install

# Start dev server
ng serve
```

---

## 3. Test the Complete Flow

### Step 1: Navigate to Shop
- Go to http://localhost:4200/shop/home
- Add some books to your basket

### Step 2: Go to Basket
- Navigate to http://localhost:4200/show-basket-list
- Click "Checkout" button

### Step 3: Complete Checkout
- Fill in customer information
- Click "Continue to Payment"

### Step 4: Complete Payment
- Fill in cardholder name and email
- Enter Stripe test card details:
  - **Success**: 4242 4242 4242 4242
  - **Requires Auth**: 4000 0025 0000 3155
  - **Declined**: 4000 0000 0000 0002
  - Expiry: Any future date (e.g., 12/25)
  - CVC: Any 3 digits (e.g., 123)

### Step 5: View Confirmation
- Payment confirmation page shows order details
- Check order summary and items

---

## 4. Database

### What Gets Created
The migration creates three new tables:
1. **Orders** - Order records with status, total, customer info
2. **OrderItems** - Individual items in each order
3. **Payments** - Payment records with Stripe transaction IDs

### Sample Query to Check Orders
```sql
-- View all orders
SELECT * FROM Orders;

-- View order items
SELECT * FROM OrderItems;

-- View payments
SELECT * FROM Payments;
```

---

## 5. API Endpoints

### Order Endpoints
```
POST /api/orders
  - Create order from basket
  - Body: { customerName, customerEmail }
  - Returns: Order object

GET /api/orders/{id}
  - Get order by ID
  - Returns: Order object

GET /api/orders/session/{sessionId}
  - Get order by session ID
  - Returns: Order object
```

### Payment Endpoints
```
POST /api/payments/create-intent
  - Create Stripe payment intent
  - Body: { orderId }
  - Returns: { clientSecret, paymentIntentId }

POST /api/payments/confirm
  - Confirm payment after Stripe processing
  - Body: { orderId, paymentMethodId, email }
  - Returns: Payment object with status

GET /api/payments/{id}
  - Get payment details
  - Returns: Payment object

POST /api/payments/webhook
  - Stripe webhook endpoint (for production)
  - Returns: { received: true }
```

---

## 6. Architecture Flow

```
User Cart → Create Order → Get Order ID → Create Payment Intent
    ↓           ↓               ↓                  ↓
 Basket   Order in DB    Order Details    Stripe ClientSecret
    
                         ↓
                    Enter Card Details
                         ↓
                    Process Payment
                         ↓
                    Payment Succeeded/Failed
                         ↓
                    Update Order Status
                         ↓
                    Show Confirmation
```

---

## 7. File Locations

### Backend Files
- Core Models: `Core/Models/Order.cs`, `Order Item.cs`, `Payment.cs`
- Interfaces: `Core/Interfaces/IOrderService.cs`, `IPaymentService.cs`
- Services: `Infrastructure/Data/OrderService.cs`, `PaymentService.cs`, `StripeService.cs`
- Repositories: `Infrastructure/Data/OrderRepository.cs`, `PaymentRepository.cs`
- Controllers: `API/Controllers/OrderController.cs`, `PaymentController.cs`
- DTOs: `API/DTOs/OrderDto.cs`, `PaymentDto.cs`

### Frontend Files
- Models: `client/src/app/core/models/order.model.ts`, `payment.model.ts`
- Services: `client/src/app/core/services/order.service.ts`, `payment.service.ts`
- Components: 
  - `client/src/app/features/checkout/checkout.component.ts/html/scss`
  - `client/src/app/features/payment/payment.component.ts/html/scss`
  - `client/src/app/features/order-confirmation/order-confirmation.component.ts/html/scss`
- Routes: `client/src/app/app.routes.ts`

---

## 8. Switching to Production

When ready to go live:

1. **Get Live API Keys**
   - Stripe Dashboard → Developers → API Keys
   - Copy live keys (start with `sk_live_` and `pk_live_`)

2. **Update Configuration**
   - Replace test keys with live keys in `appsettings.json`
   - Update Stripe public key in payment component

3. **Verify Bank Account**
   - Stripe Dashboard → Settings → Payouts
   - Add bank account for receiving payments

4. **Enable HTTPS**
   - Update API URL from http to https
   - Update CORS settings for production domain

---

## 9. Troubleshooting

### Payment Not Going Through
- Check Stripe test keys are correct
- Verify payment intent creation in API response
- Check browser console for Stripe errors
- Ensure card details match test data

### Order Not Saving
- Check database connection string
- Verify Entity Framework migration ran successfully
- Check API logs for database errors

### Stripe JS Not Loading
- Verify CDN script is in index.html head
- Check browser console for script loading errors
- Ensure no CORS issues

### CORS Errors
- Verify API CORS settings in Program.cs
- Check frontend API URL matches backend URL
- Ensure credentials are allowed if needed

---

## 10. Next Steps

1. Run the backend migrations to create tables
2. Add your Stripe test keys to appsettings.json
3. Add Stripe public key to payment component
4. Start both API and Angular servers
5. Test the complete payment flow
6. Monitor payments in Stripe Dashboard

---

## Support

Stripe Test Mode Reference:
- https://stripe.com/docs/testing

API Documentation:
- Stripe.NET: https://github.com/stripe/stripe-dotnet
- Angular: https://angular.io/guide/http

