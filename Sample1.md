# 📄 **Example: Underpaid Trip**

This scenario demonstrates how the old reimbursement method **underpaid** for a trip, while the new state‑based model produces the **correct**, policy‑compliant amount.

***

# **1. Input Data**

**JWT:**  
`722887-202602-6`

**Store:**  
`10996302` `5220\9256`

**Rep:**  
`249260`

## 📘 **TravelItems.csv**

**travel\_id:**  
`722887-202602-6-10996302-249260-20260123`

**travel\_dt:**  
`1/23/2026`

**travel\_distance:**  
`57.96`  
*(legacy-reported distance for comparison only)*

**deduct\_miles:**  
`0`  
*(no deduction applied for this trip)*

**actual\_amount (old system):**  
`17.39`  
*(amount paid under the old flat $0.30/mile reimbursement system)*

***

## 📘 **TravelItemDetails.csv**

**Start GPS:**  
`41.1643975, -87.2706004`  
➡ *Indiana (IN)*

**End GPS:**  
`41.483936, -87.7419604`  
➡ *Illinois (IL)*

**Reported travel leg distance:**  
`57.96`  
*(raw GPS distance; new engine relies on Google’s drivable route instead)*

***

# **2. Old Method — Flat $0.30 for All Miles**

Under the old model:

    57.96 miles × $0.30 = $17.39

Problems with the old approach:

*   ❌ Did not consider state boundaries
*   ❌ Underpaid when high‑pay states (IL, CA, MA) were involved
*   ❌ No understanding of actual driving route
*   ❌ Not audit‑defensible (“why $17.39?”)

In this example, **18 miles occurred in Illinois**, which should have been paid at **$0.70**, not **$0.30**.

***

# **3. New Method — State-Based, Google-Verified Route**

The improved calculation uses:

*   **Google Directions API** → real drivable route
*   **Polyline decoding** → step‑by‑step path
*   **Reverse‑geocoding** → which state each segment belongs to
*   **Correct reimbursement rates** per state
*   **Deduction rules** (high‑pay states first) if applicable

Google shows the user traveled:

*   \~40 miles in **Indiana**
*   \~18 miles in **Illinois**

***

# **4. State Split + Reimbursement Calculation**

### Based on Google route:

| State | Miles       | Rate  | Final Miles | Reimbursement |
| ----- | ----------- | ----- | ----------- | ------------- |
| IN    | 39.94362430 | $0.30 | 39.94362430 | $11.98308729  |
| IL    | 18.00959015 | $0.70 | 18.00959015 | $12.60671311  |

### Correct adjusted amount:

    $11.98308729 + $12.60671311 = $24.58980040

This corrects an **underpayment** of:

    $24.5898 - $17.39 = $7.20

***

# **5. Output Files**

## 📄 AllStateMileageOutput.csv

    travel_id,travel_dt,State,Rate,Miles,Deducted,Final_Mile,Reimbursement
    722887...,1/23/2026,IN,0.3,39.94362429712997,0,39.94362429712997,11.983087289138991
    722887...,1/23/2026,IL,0.7,18.00959015221706,0,18.00959015221706,12.606713106551942

## 📄 TravelSummaryComparison.csv

    travel_id,travel_dt,travel_distance,actual_amount,MilesByState,adjusted_amount
    722887...,1/23/2026,57.96,17.39,57.95321445,24.58980040

***

# **6. Why the New Method Is More Accurate**

### ✔ Uses Google’s real route

Not straight-line estimation.

### ✔ Splits miles by state

18 miles in IL shouldn’t be paid at a low rate.

### ✔ Applies correct reimbursement rates

*   IN: $0.30
*   IL: $0.70

### ✔ Fair and transparent

Easy to defend in audits or employee disputes.

### Total API Calls: 116

***