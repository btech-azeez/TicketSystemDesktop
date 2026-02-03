# 🎫 Customer Support Ticket System

A professional, full-stack enterprise solution for managing customer support requests. This project features a modern,
premium WPF desktop application powered by a robust ASP.NET Core Web API and a MySQL database.

---

## 🚀 Project Overview

The **Ticket System** is designed to streamline communication between customers and support teams. It provides a structured workflow for ticket creation, 
assignment, and resolution, ensuring that no request goes unanswered.

### Key Capabilities:
- **Role-Based Access**: Specialized interfaces for regular users and system administrators.
- **Real-Time Tracking**: Monitor ticket status (Open → In Progress → Closed) with a detailed activity timeline.
- **Communication Hub**: Threaded comments for better collaboration on issues.
- **Audit Logging**: Comprehensive history tracking for every status change and assignment.

---

 Tech Stack

| Component | Technology | Version |
| :--- | :--- | :--- |
| **Frontend** | WPF (Windows Presentation Foundation) | .NET 6.0 |
| **Backend API** | ASP.NET Core Web API | .NET 6.0 |
| **Database** | MySQL Server | 8.0+ |
| **Styling** | Custom XAML Design System | Modern Premium |
| **API Client** | HttpClient with JSON | RESTful |

---

## ⚙️ Local Setup Guide

Follow these steps to get the system running on your local machine:

### 1. Prerequisites
- **Visual Studio 2022** (.NET Desktop & ASP.NET workloads).
- **MySQL Server** (Running on port 3306).
- **.NET 6.0 SDK**.

### 2. Database Configuration
1. Open your MySQL client (e.g., MySQL Workbench).
2. Execute the script found in `Database/01_CreateDatabase.sql`.
3. Verify the connection credentials in `Backend/TicketSystemAPI/appsettings.json`:
   ```json
   "Server=localhost;Database=TicketSystemDB;User=root;Password=AbdulAzeez@12345;Port=3306;"
   ```

### 3. Launch the Backend API
1. Open `TicketSystem.sln` in Visual Studio.
2. Set **TicketSystemAPI** as the Startup Project.
3. Press `F5`. The API will be available at `http://localhost:5000`.
4. Swagger documentation will open automatically at `http://localhost:5000/swagger`.

### 4. Launch the Desktop App
1. Right-click the **TicketSystemDesktop** project and select **Set as Startup Project**.
2. Press `F5`.
3. Login using the default credentials:
   - **Admin**: `admin` / `admin123`
   - **User**: `john` / `john123`

---

## 🎨 Design Decisions & Assumptions

### 1. Premium UI Overhaul
We transitioned from a basic "generic" look to a **Premium Modern Design**. 
- **Decision**: Used a Violet/Indigo color palette to evoke professional reliability.
- **Decision**: Implemented a **Frameless Login Window** with custom drag-and-drop logic for a high-fidelity application feel.
- **Decision**: Added a 12px `CornerRadius` and `DropShadow` effects to UI elements to follow modern "Card-based" design principles.

### 2. Decoupled Architecture
The system uses a strictly decoupled architecture where the Frontend has zero knowledge of the database.
- **Decision**: All communication happens over a secured REST API. This allows for potential future web or mobile clients to plug into the same backend.

### 3. Technical Fixes & Assumptions
- **Assumption**: Plain-text passwords are used for demonstration. In a production environment, Argon2 or BCrypt hashing would be implemented.
- **BaseAddress Logic**: A critical decision was made to ensure `HttpClient` URIs are constructed with predictable trailing slashes (e.g., `api/`) to prevent environment-specific routing errors.
- **Markup Optimization**: DataGrids are configured with `AutoGenerateColumns="False"` and specific `MinWidth` constraints to maintain high readability across different screen resolutions.

---


---

*Developed with ❤️ as a complete Full-Stack Solution.*
