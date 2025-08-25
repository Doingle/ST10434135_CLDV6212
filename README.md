ST10434135_CLDV6212_PART1
Order Processing System for ABC Retail


📑 Contents:

🌍 Published Web App: 
📌 About the Project:
👨‍💻 Creator
✨ Features
🛠 Requirements
🚀 Getting Started + How to run the project locally
🌐 Recommended Browser


🌍 Published Web App: 

🔗 Access the live version here:
[PLACEHOLDER – Insert Deployed Web App URL]


📌 About the Project:

- This project implements an Order Processing System for ABC Retail as part of the CLDV6212 module.
- The system integrates with Azure Table Storage, Azure Blob Storage, and Azure Queue Storage to provide a scalable and reliable cloud-based retail solution.
- Users can manage Products, Customers, and Orders through a web interface, with full CRUD functionality and messaging support for order status updates.

👨‍💻 Creator:

- ST10434135
- Built as part of the CLDV6212 coursework.

✨ Features:

- Products
- Add, update, delete products
- Upload product images (stored in Azure Blob Storage)
- Track available stock
- Customers
- Manage customer information
- Store customer details securely in Azure Table Storage
- Orders
- Create new orders (validates stock availability)
- Automatically reduces stock when orders are placed
- Cancel orders and reallocate stock
- Delete orders with optional stock reallocation
- View all orders with customer and product details populated
- Azure Integration
- Table Storage → Stores all entities (Products, Customers, Orders)
- Blob Storage → Stores uploaded product images
- Queue Storage → Sends messages whenever order status changes (e.g. Created → Cancelled)
- UI/UX
- Web app built with ASP.NET Core MVC
- Simple and responsive interface
- Dropdowns for customer/product selection when creating orders

🛠 Requirements:

- .NET 8 SDK
- Visual Studio 2022
- An Azure Storage Account with:
  A Blob Container (for product images)
  A Table Storage with tables: Products, Customers, Orders
  A Queue (for order status messages)

🚀 Getting Started + How to run the project locally:

- Click the green code button by the project name on github
- choose the last option from the dropdown "download as .zip"
- once downloaded, extract all
- open the project using visual studio 2022 by right clicking the .sln (ST10434135_CLDV6212.sln) and choosing open with, then visual studio 2022
- wait for the project to load, then run it by pressing control + F5

🌐 Recommended Browser:

- The project is tested and optimized for Google Chrome.
- Other modern browsers may work, but Chrome is recommended for the best experience.
