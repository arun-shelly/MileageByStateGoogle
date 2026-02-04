# 📦 MileageByState Google Routing Engine

A .NET console application that computes **accurate state‑by‑state mileage** using **Google Maps Directions API** and **Reverse Geocoding**, then applies **state‑specific reimbursement rates** and **deduction rules**.


***

# 🚀 **What This Application Does**

This app processes two input CSVs:

1.  **TravelItems.csv**  
    Trip‑level details: travel\_id, date, total distance, deduction miles, actual paid amount, etc.

2.  **TravelItemDetails.csv**  
    Leg‑level GPS coordinates: start/end latitude & longitude.

Using this data, the app:

### ✔ Calls **Google Directions API**

To obtain the actual drivable route polyline for each travel leg.

### ✔ Splits the polyline into segments

Each segment is evaluated using **high‑resolution sampling** to determine which U.S. state it belongs to.

### ✔ Computes **mile-by-mile distance per state**

Using the Haversine formula.

### ✔ Applies **state reimbursement rates**

*   $0.70 for high‑pay states (CA, IL, MA)
*   $0.30 for all others

### ✔ Applies deduction miles

Deducts from **high‑pay states first**, then others, only once per travel\_id.

### ✔ Produces multiple output CSVs

*   `AllStateMileageOutput.csv`
*   `HighPayTravelOnly.csv`
*   `TravelSummaryComparison.csv`
*   `ApiCallStats.csv`

### ✔ Logs everything with Serilog

Including:

*   Duration per travel\_id
*   Detected states
*   Deduction steps
*   API usage statistics

***

# 📥 **Input Files (./Data/Input/)**

### `TravelItems.csv`

Contains one row per travel\_id:

    travel_id,job_no,wave_no,task_no,store_id,merch_no,rep_homestate,travel_dt,travel_distance,deduct_miles,actual_amount

### `TravelItemDetails.csv`

Contains one row per leg:

    travel_id,travel_dt,Start_latitude,Start_longitude,End_latitude,End_longitude,travel_distance

***

# 📤 **Output Files (./Data/Output/)**

### `AllStateMileageOutput.csv`

A row for each **state** visited for each travel\_id.

### `HighPayTravelOnly.csv`

Contains only rows where the state is **CA, IL, MA**.

### `TravelSummaryComparison.csv`

One row per travel\_id **that contains at least one high‑pay state**  
Includes full multi‑state reimbursements.

### `ApiCallStats.csv`

Shows:

*   Directions API calls
*   Geocode API calls
*   Total Google API calls
*   Final TOTAL row (`TOTAL-ALL-TRIPS`)

***

# 🔧 **Development Setup (Using .NET User Secrets)**

To avoid storing API keys in source code, the app uses **.NET User Secrets** during development.

### **1. Initialize user secrets**

    dotnet user-secrets init

### **2. Set your API key**

    dotnet user-secrets set "GApiKey" "your-google-api-key"

### **3. Verify**

    dotnet user-secrets list

***

# 🏭 **Production Setup (Using Environment Variables)**

On a Windows Server or production host, set the API key as a **system-level environment variable**.

### **PowerShell**

    setx GApiKey "your-google-api-key" /M

### **Verify (restart shell first)**

    echo $env:GApiKey

The application will automatically detect and use this environment variable in production.

***
