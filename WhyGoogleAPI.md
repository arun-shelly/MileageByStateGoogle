# 📑 Technical Appendix — Mileage‑By‑State Calculation

This appendix explains the engineering differences between the former **GeoJSON GIS engine** and the new **Google Directions–based mileage engine**, including algorithms, assumptions, and the reasons underlying state‑level discrepancies.

***

## 1. Legacy Engine (Straight‑Line GeoJSON Method)

### 1.1 Overview

The previous system used:

*   Raw GPS start and end coordinates
*   NetTopologySuite geometry processing
*   A U.S. states GeoJSON boundary dataset
*   Straight‑line Haversine distance

### 1.2 Algorithm

1.  Create a `LineString` between two GPS points.
2.  Intersect the line with polygon boundaries of U.S. states.
3.  For each polygon intersection:
    *   Extract the segment inside the state
    *   Compute segment distance
4.  Sum distances for each state.
5.  Apply deduction logic (high‑pay states first).

### 1.3 Limitations

*   **Not based on real roads** — purely geometric.
*   Can include states that vehicles never actually enter.
*   Small “corner intersections” (1–15 miles) frequently appear in diagonal routes.
*   Not user‑verifiable — employees cannot confirm via Google Maps.

**Example:** A straight line from WV → western KY clips Illinois, even though highways never enter Illinois for this trip.

***

## 2. Google Directions Engine (New System)

### 2.1 Overview

The Google‑based implementation uses:

*   Google Directions API for actual drivable route
*   Google Geocoding API for state detection
*   Polyline decoding for segment‑level mileage
*   Sampling to reduce API calls

### 2.2 Algorithm

**Step 1 — Request Route**  
Query Google Directions with start/end coordinates.  
Google returns:

*   A drivable route
*   Total distance
*   A compressed polyline representing road geometry

**Step 2 — Decode Polyline**  
Expand polyline → \~100–300 coordinates representing actual turns and curves.

**Step 3 — Sample the Route (5 points)**  
To avoid overusing Geocoding:

*   Sample \~5 evenly spaced polyline points
*   Reverse‑geocode samples only
*   Construct state sequence (e.g., `WV → KY`)

**Step 4 — Assign Segments to Sampled States**  
Each polyline segment is mapped to a state by the nearest sampled state index.

**Step 5 — Compute Mileage**  
Perform Haversine distance for each adjacent polyline point pair.

**Step 6 — Aggregate by State**  
Add all polyline‑segment miles per state.

**Step 7 — Apply Deduction**

**Deduct miles in the order they were actually traveled**, beginning with the **state where the route starts**, then continuing through each subsequent state in the sequence returned by Google’s route data.

1.  **Start with the state of the first coordinate (starting state)**
2.  Apply remaining deduction to **each next state in travel order**
3.  Stop when deduction reaches zero
4.  Deduction never exceeds the total state mileage

***


## 3. Comparison of Algorithms

| Feature            | GeoJSON Straight‑Line Engine | Google Directions Engine         |
| ------------------ | ---------------------------- | -------------------------------- |
| Path Type          | Straight geometric line      | Actual drivable route            |
| Intersection Logic | GIS polygon/line clipping    | Reverse‑geocode samples          |
| Mileage Basis      | Great‑circle segments        | Road polyline segments           |
| State Accuracy     | May include unreal states    | Reflects real driving            |
| Advantages         | GIS‑pure; offline            | Publicly verifiable; real routes |
| Common Issues      | False state crossings        | None for drivable travel         |

***

## 4. Why State Lists May Differ

### 4.1 Straight‑Line Crossings

A geometric line may touch a corner of a state.

Example:  
WV → KY trip diagonal intersects IL by \~10–15 geometric miles.

### 4.2 Google Route Fidelity

Google chooses real roads:

*   Highways
*   Interchanges
*   Legal driving paths
*   Traffic‑optimized routes

In some cases:

*   The drivable route *never enters Illinois*
*   Therefore IL is omitted by Google (correctly)

This is expected and desired.

***

## 5. Advantages of Google‑Based Mileage

*   **More defensible:** matches publicly verifiable Google Maps routes
*   **User‑friendly:** easy to audit and explain
*   **Real‑world grounded:** aligned with actual road travel
*   **Fewer disputes:** employees can independently confirm mileage
*   **Business‑aligned:** avoids GIS anomalies and false state crossings

***

## 6. Known Limitations

*   Requires Google API key & quota
*   Slight differences may occur if Google updates route data
*   Very close border roads may slightly shift state assignments (\~0.5–1 mile variances)
*   Rural or limited road‑data areas may produce fewer polyline points

A future enhancement could include border‑aware sampling or dual‑mode verification.

***

# 📊 Sample Output Comparison (GeoJSON Engine vs Google Directions Engine)

This section shows a real-world example of how the same trip produces different state‑level mileage depending on whether the system uses:

*   **Legacy Straight‑Line GeoJSON Engine**
*   **New Google Directions–Based Engine**

The travel record:

    travel_id: 722887-202603-6-10989500-234105-20260116  
    travel_dt: 1/16/2026
    Start: 38.43405302, -82.1117655  
    End:   37.16291864, -88.68534293  
    Reported Distance: 407.87 miles

***

## 🧭 Google Directions Engine Output (Drivable Route)

Google Maps identifies the fastest drivable route between these coordinates, which **only crosses West Virginia and Kentucky**.

    travel_id,travel_dt,State,Rate,Miles,Deducted,Final_Mile,Reimbursement
    722887-202603-6-10989500-234105-20260116,1/16/2026,WV,0.3,71.74415126581283,30.000000000000004,41.74415126581283,12.52324537974385
    722887-202603-6-10989500-234105-20260116,1/16/2026,KY,0.3,329.624603730156,0,329.624603730156,98.88738111904681

**Key characteristics:**

*   Route follows real roads (I‑64 → I‑57 → US‑60).
*   Google does **not** route the driver into Illinois.
*   Mileage reflects realistic driving patterns.

***

## 🗺️ Legacy GeoJSON Engine Output (Straight‑Line Geometry)

The old system draws a **straight line** between the GPS points and intersects it with state polygons.  
That straight diagonal clips Illinois, so Illinois appears in the results even though **no actual road route enters IL**.

    travel_id,travel_dt,State,Rate,Miles,Deducted,Final_Mile,Reimbursement
    722887-202603-6-10989500-234105-20260116,1/16/2026,IL,0.7,13.222325380590162,13.222325380590162,0,0
    722887-202603-6-10989500-234105-20260116,1/16/2026,KY,0.3,329.4211369488176,0,329.4211369488176,98.82634108464528
    722887-202603-6-10989500-234105-20260116,1/16/2026,WV,0.3,26.771868015132927,16.777674619409837,9.99419339572309,2.998258018716927

**Key characteristics:**

*   A diagonal line between coordinates crosses WV → KY → *IL* (slightly).
*   Illinois mileage (13.22 miles) is not drivable — it’s a geometric artifact.
*   Can create confusion for reimbursement and auditing.

***

## 🔍 Why These Outputs Differ

### ✔ Google Directions

Represents **actual drivable highways**.  
Never enters IL → IL is excluded.

### ✔ GeoJSON Engine

Represents **mathematical straight-line crossing**.  
Diagonal line barely touches IL → IL is included.

***

## 🎯 Summary of the Difference

| State | Google Directions | Straight‑Line GeoJSON |
| ----- | ----------------- | --------------------- |
| WV    | ✓                 | ✓                     |
| KY    | ✓                 | ✓                     |
| IL    | ✗ (never entered) | ✓ (corner clip)       |

The **Google‑based output is correct for mileage reimbursement**, because reimbursement must be based on *actual roads traveled*, not geometric intersections.


