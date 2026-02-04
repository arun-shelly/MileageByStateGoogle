
# 📄 Google API Cost Analysis  

### (Directions API + Geocoding API)

This document provides a dedicated overview of **Google Maps Platform charges** for the APIs used by the mileage engine:

- **Directions API**
- **Geocoding API (Reverse Geocoding)**

It breaks down **pricing**, **cost per travel record**, **monthly cost estimates**.

---

# 1. Google Maps APIs Used

Our mileage engine uses two Google APIs:

### ✔ **Google Directions API**
Used to:
- Fetch the actual drivable route
- Get the polyline path
- Ensure state calculations match real roads

### ✔ **Google Geocoding API (Reverse Geocoding)**
Used to:
- Determine which U.S. state each sampled point belongs to
- Tag route segments to states

---

# 2. Official Google Pricing (USD)

Pricing follows Google Maps Platform Pay‑As‑You‑Go.

## **2.1 Directions API**
| Price | Unit |
|-------|--------|
| **$5.00** | per **1,000 requests** |

We make **1 request per travel leg**.

---

## **2.2 Geocoding API (Reverse Geocoding)**

| Price | Unit |
|-------|--------|
| **$5.00** | per **1,000 requests** |

We make **multiple requests per travel leg**, but optimized down to ~20 per leg with sampling.

---

# 3. Cost Per Travel ID (Actual)

Based on current implementation:

### ✔ 1 Directions API call  
```

Cost = $0.005

```

### ✔ ~20 Reverse Geocode calls  
```

20 × $0.005 = $0.10

```

### 🎉 **Total cost per travel record ≈ $0.105**
(About **10.5 cents per travel_id**)

---

# 4. Monthly Cost Estimates

Assuming average usage:

| Travel IDs per Month | Approx Cost |
|----------------------|-------------|
| **500** | ~$52 |
| **1,000** | ~$105 |
| **2,000** | ~$210 |
| **5,000** | ~$525 |
| **10,000** | ~$1,050 |

---

# 5. Google Free Monthly Credit

Google provides **$200 free credits per month** for Maps APIs.

Based on our cost per record:

```

$200 / $0.105 ≈ \~1,900 travel IDs free per month

```

### ✔ If your system processes fewer than ~1,900 trips/month → **Effectively free**
### ✔ Above that amount → Pay only for excess usage

---

# 6. Cost Reduction Optimizations (Already Implemented)

Originally, reverse‑geocoding each polyline point would require:

```

200–600 geocode calls per leg

```

This would cost:
```

\~$2.00 per travel\_id

```

At scale (e.g., 5,000 travel IDs/month):
```

5,000 × $2.00 = $10,000/month

```

But we introduced **dynamic sampling**, reducing calls by ~90%.

### ✔ New geocode calls per leg: ~20  
### ✔ Cost per travel_id: ~$0.105  
### ✔ Savings: **~95% reduction in Google billing**

---

# 7. Annual Cost Example (With Optimization)

Assume:
- 4,000 travel IDs per month
- Cost per ID = $0.105

```

Monthly = 4,000 × $0.105 = $420
Annual = $420 × 12 = $5,040

```

With Google’s first 1,900 IDs free:

```

Monthly cost = (4,000 – 1,900) × $0.105
\= 2,100 × $0.105
\= $220.50/month

Annual = \~$2,646

```

---

# 8. Key Takeaways

### ✔ Google API usage is predictable and inexpensive  
(~10 cents per travel record)

### ✔ First ~1,900 trips per month are free  

### ✔ Total annual cost stays relatively low compared to payroll, operations, or overpayment risk  

### ✔ Cost scales linearly with number of trips processed  

---

# Conclusion

Google Maps Platform provides a **cost‑effective**, **highly accurate**, and **defensible** foundation for route‑based mileage reimbursement.  

At roughly **10 cents per travel_id**, and with the first ~1,900 IDs free per month, the total operational cost is low—especially when compared to the financial risk of using inaccurate mileage calculations.

This investment ensures:

- Accurate state mileage  
- Policy‑correct reimbursement  
- Reduced overpayments  
- Transparent auditability  

A small API cost provides **large accuracy and compliance benefits**.

