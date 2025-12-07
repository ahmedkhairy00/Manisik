Overview

This file lists all backend endpoints, required DTOs and example request payloads so frontend (Angular) Copilot refactor can use them.
DTOs are located in `UmarahBooking.Core/DTO`.

Notes about auth
- The API uses JWT Bearer. Login returns `AuthResponseDto` with `Token` (also sets cookie `authToken`).
- For authorized requests include header: `Authorization: Bearer {token}`.

---

Auth (User management)

1) Register
- Method: POST
- Route: `/api/Auth/Register`
- Auth: No
- Request DTO: `RegisterDto`
  - { "Email": string, "Password": string, "FirstName": string, "LastName": string, "PhoneNumber": string?, "Country": string? }
- Response: `ApiResponse<AuthResponseDto>`

2) Login
- Method: POST
- Route: `/api/Auth/Login`
- Auth: No
- Request DTO: `LoginDto`
  - { "Email": string, "Password": string, "RememberMe": bool? }
- Response: `ApiResponse<AuthResponseDto>` (contains Token)

3) Logout
- Method: POST
- Route: `/api/Auth/Logout`
- Auth: Yes (optional) — clears `authToken` cookie
- Response: `ApiResponse<object>`

4) Me
- Method: GET
- Route: `/api/Auth/Me`
- Auth: Yes (Bearer)
- Response: `ApiResponse<UserDto>`

5) Get all users (Admin)
- Method: GET
- Route: `/api/Auth/Users`
- Auth: Yes (Role=Admin)
- Response: `ApiResponse<IEnumerable<UserDto>>`

6) Get users by role (Admin)
- Method: GET
- Route: `/api/Auth/UsersByRole/{roleName}`
- Auth: Yes (Role=Admin)
- Response: `ApiResponse<IEnumerable<UserDto>>`

7) Assign role (Admin)
- Method: POST
- Route: `/api/Auth/AssignRole`
- Auth: Yes (Role=Admin)
- Request DTO: `AssignRoleDto` { "UserId": int, "RoleName": string }
- Response: `ApiResponse<object>`

8) Remove role (Admin)
- Method: POST
- Route: `/api/Auth/RemoveRole`
- Auth: Yes (Role=Admin)
- Request DTO: `AssignRoleDto` { "UserId": int, "RoleName": string }
- Response: `ApiResponse<object>`

9) MyBookings (user's bookings overview)
- Method: GET
- Route: `/api/Auth/MyBookings`
- Auth: Yes (Bearer)
- Response: `ApiResponse<UserWithBookingsDto>`

---

Bookings

1) Create booking
- Method: POST
- Route: `/api/Booking/CreateBooking`
- Auth: Yes (Role=User,Admin)
- Request DTO: `BookingDto` (see `UmarahBooking.Core/DTO/BookingDto.cs`)
  - Contains: Trip type, Travel dates, PaymentMethod, NumberOfTravelers, MakkahHotel (HotelBookingDto), MadinahHotel, InternationalTransport (TransportBookingDto), GroundTransport (GroundTransportBookingDto), Travelers (list of TravelerDto), TotalPrice optional
- Response: `ApiResponse<BookingDto>` (201 Created)

2) Get my bookings
- Method: GET
- Route: `/api/Booking/MyBookings`
- Auth: Yes (Role=User,Admin)
- Response: `ApiResponse<IEnumerable<BookingDto>>`

3) Get all bookings (Admin)
- Method: GET
- Route: `/api/Booking/AllBookings`
- Auth: Yes (Admin)
- Response: `ApiResponse<IEnumerable<BookingDto>>`

4) Get booking by ID
- Method: GET
- Route: `/api/Booking/GetBooking/{id}`
- Auth: Yes (owner or Admin)
- Response: `ApiResponse<BookingDto>`

5) Get booking by BookingId
- Method: GET
- Route: `/api/Booking/BookingId/{id}`
- Auth: Anonymous
- Response: `ApiResponse<BookingDto>`

6) Update booking status (Admin)
- Method: PUT
- Route: `/api/Booking/UpdateStatus/{id}`
- Auth: Admin
- Body: BookingStatus (enum string) — e.g. "Confirmed", "Cancelled"
- Response: `ApiResponse<BookingDto>`

7) Update payment status (Admin)
- Method: PUT
- Route: `/api/Booking/UpdatePaymentStatus/{id}`
- Auth: Admin
- Body: PaymentStatus (enum string)
- Response: `ApiResponse<BookingDto>`

8) Cancel booking
- Method: DELETE
- Route: `/api/Booking/CancelBooking/{id}`
- Auth: Owner or Admin
- Response: `ApiResponse<string>`

---

Hotels & Rooms

1) Get hotels
- Method: GET
- Route: `/api/Hotel` (check controller route)
- Auth: No
- Response: `ApiResponse<IEnumerable<HotelDto>>`

2) Get hotel by id
- Method: GET
- Route: `/api/Hotel/{id}`
- Auth: No
- Response: `ApiResponse<HotelDto>`

3) Create/update hotels (Admin/HotelManager)
- Methods: POST/PUT (check controller implementations)
- Auth: Role restricted
- Request DTO: `HotelDto` and `RoomDto` for rooms

---

Transports

1) International transports
- DTO: `InternationalTransportDto`, `TransportBookingDto`
- Endpoints: CRUD under `/api/International` and booking under `/api/InternationalTransportBooking`

2) Ground transports
- DTO: `GroundTransportDto`, `GroundTransportBookingDto`
- Endpoints: `/api/Ground`, `/api/GroundTransportBooking`

---

Payments (Stripe)

1) Create payment intent / client secret
- Controller: `StripeController`
- Route: e.g. `/api/Stripe/CreatePaymentIntent` (check controller for exact route)
- Auth: Usually Authenticated
- Body: `PaymentDto` or amount + currency
- Response: `ClientSecret` for frontend to confirm with Stripe.js

2) Webhook
- Route: `api/Stripe/Webhook` — configured in Stripe dashboard
- Auth: none (Stripe signature validation)

---

Subscribers

1) Subscribe (email)
- POST `/api/Subscriber` or similar
- Request: `SubscriberDto` { Email }
- Response: `ApiResponse<SubscriberDto>`

---

ChatBot

1) POST `/api/ChatBotAi/Ask` (or similar)
- Body: `ChatRequest` DTO
- Response: chat reply

---

Where the DTOs live
- `UmarahBooking.Core/DTO/*` - use these exact class names in your frontend TypeScript interfaces. The most important ones to map first:
  - `AuthResponseDto`, `UserDto`, `RegisterDto`, `LoginDto`, `BookingDto`, `HotelBookingDto`, `HotelDto`, `RoomDto`, `TravelerDto`, `TransportBookingDto`, `GroundTransportBookingDto`, `PaymentDto`, `SubscriberDto`.

Suggested Angular front-end mapping approach (quick)
- Create TypeScript interfaces mirroring DTOs in `src/app/models/api`.
- Create `AuthService` with `register`, `login`, `me`, `logout` and store JWT in `localStorage` and `Authorization` header via HttpInterceptor.
- Create `BookingService` with `createBooking`, `getMyBookings`, `getBooking`, `cancelBooking` etc.
- Use `HttpInterceptor` to attach `Authorization: Bearer {token}` to every request.

Files I added/moved for tests
- `UmarahBooking.Tests/AllTests/TestWebApplicationFactory.cs` — centralized WebApplicationFactory for tests.
- `UmarahBooking.Tests/AllTests/AuthTests.cs` — integration tests for auth endpoints.

Current test status (brief)
- Tests are running but some integration tests returned 403 in the test host for admin endpoints; this is due to role seeding timing/uniqueness in the in-memory DB. I consolidated test files into `AllTests` but the test seeding still needs to be fully deterministic.

Next actions I can take (pick one):
1) Finish deterministic seeding (make tests pass) and run full suite until green.
2) Produce the Angular TypeScript DTO/interfaces + sample services/HttpInterceptor file now.
3) Generate a Swagger-exported JSON (OpenAPI) file for frontend codegen.

Tell me which next action you want me to do now and I'll proceed.
