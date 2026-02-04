# 📄 **Example: Overpaid Trip**

This scenario demonstrates how the old reimbursement method **overpaid** for a trip, while the new state‑based model produces the **correct**, policy‑compliant amount.

***

# **1. Input Data**

**JWT:**  
`722887-202603-6`

**Store:**  
`10989500` `5220\7962`

**Rep:**  
`234105`

## 📘 **TravelItems.csv**

**travel\_id:**  
`722887-202603-6-10989500-234105-20260116`

**travel\_dt:**  
`1/16/2026`

**travel\_distance:**  
`377.87` miles  
*(legacy system’s reported distance — used only for comparison)*

**deduct\_miles:**  
`30`  
*(deduction applied once per travel\_id — taken from high‑pay states first)*

**actual\_amount (old system):**  
`264.51`  
*(amount paid under the old flat‑rate reimbursement method)*

***

## 📘 **TravelItemDetails.csv**

**Start GPS:**  
`38.43405302, -82.1117655`  
→ *West Virginia (WV)*

**End GPS:**  
`37.16291864, -88.68534293`  
→ *Kentucky / Illinois border region*

**Reported travel leg distance:**  
`407.87` miles  
*(used only for informational comparison — the engine relies on Google’s actual mapped route)*

***

# 2. **Old Method — Flat Rate $0.30 for ENTIRE TRIP**

Old logic:

    377.87 miles × $0.30 = $113.36     ❌ WRONG

But the employee was paid:

    actual_amount = $264.51     ❌ VERY WRONG

Why so wrong?

The old system:

*   Didn’t calculate by state
*   Didn’t route through real roads
*   Didn’t account for deduction correctly
*   Frequently double counted mileage
*   Sometimes used *distance reported by the phone*, not real route distance

In this case, the employee was **overpaid by more than double**.

***

# 3. **New Engine — Based on Actual Google Route**

Google Directions shows the route goes through:

1.  **West Virginia**
2.  **Kentucky**
3.  **Very small portion of Illinois**  
    (less than 1 mile)

The state mileage breakdown is:

| State | Miles        | Rate     | Deducted     | Final     | Reimbursement |
| ----- | ------------ | -------- | ------------ | --------- | ------------- |
| WV    | 30.29138     | 0.30     | 29.44727     | 0.844118  | $0.2532355    |
| KY    | 370.52464    | 0.30     | 0            | 370.52464 | $111.15739    |
| IL    | **0.552734** | **0.70** | **0.552734** | **0**     | $0            |

Even though the rep’s **home state is IL**, Google shows the **route itself touches Illinois for less than one mile**, and due to company policy:

*   **Deduction applies to high‑pay states first**
*   All IL miles are deducted to zero
*   The Illinois portion gets **no reimbursement**

***

# 4. **Deduction Logic (Correct Implementation)**

Deduction: **30 miles**

Apply in this order:

1.  **IL (high‑pay)**
    *   IL raw miles: **0.5527**
    *   Take full 0.5527
    *   Remaining deduction: 29.4473

2.  **WV (next highest remaining miles)**
    *   WV miles: 30.2913
    *   Take 29.4473
    *   Remaining deduction: 0

3.  **KY**
    *   No deduction applied (deduction is already exhausted)

This yields:

| State | Deduction | Remaining |
| ----- | --------- | --------- |
| IL    | 0.5527    | 0         |
| WV    | 29.4473   | 0.844118  |
| KY    | 0         | 370.52464 |

***

# 5. **New Accurate Total**

### Raw miles after deduction:

    WV:  0.844118  
    KY: 370.524636  
    IL: 0  

### Reimbursement:

    WV: 0.844118 × $0.30 = $0.2532355
    KY: 370.524636 × $0.30 = $111.15739097
    IL: 0 × $0.70 = $0
    -------------------------------------
    Adjusted amount = $111.41062649879068

***

# 6. **Comparison: Old vs New**

| Method                     | Amount      | Correct?     |
| -------------------------- | ----------- | ------------ |
| Employee was paid          | $264.51     | ❌ overpaid   |
| Flat $0.30 method          | $113.36     | ❌ inaccurate |
| **New state‑based engine** | **$111.41** | ✔ correct    |

### Overpayment detected:

    $264.51 - $111.41 = $153.10 overpaid

This difference is significant — exactly why the state‑based reimbursement logic was implemented.

***

# 7. **Why the New Method is More Accurate**

### ✔ Uses the actual driving route from Google Maps

Not straight lines, not approximations.

### ✔ Splits mileage by *actual* states traveled

So IL receives only 0.55 miles → not incorrectly inflated.

### ✔ Applies company policy correctly

*   High‑pay states deducted first
*   Low‑pay states reimbursed at correct rate

### ✔ Prevents overpayment

Especially in long multi‑state routes where only a tiny sliver enters a high‑pay state.

### ✔ Transparent and audit‑friendly

If a rep challenges reimbursement:

> “This calculation is based on Google Maps’ recommended driving route for your coordinates.”

### Total API Calls: 78

***