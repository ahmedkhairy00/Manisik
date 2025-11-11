INSERT INTO [dbo].[Auths]
(FullName, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
VALUES
('John Smith', 'john', 'JOHN', 'john@example.com', 'JOHN@EXAMPLE.COM', 0, NULL, NEWID(), NEWID(), 0, 0, 0, 0),
('Emily Johnson', 'emily', 'EMILY', 'emily@example.com', 'EMILY@EXAMPLE.COM', 0, NULL, NEWID(), NEWID(), 0, 0, 0, 0),
('Michael Brown', 'michael', 'MICHAEL', 'michael@example.com', 'MICHAEL@EXAMPLE.COM', 0, NULL, NEWID(), NEWID(), 0, 0, 0, 0),
('Sarah Davis', 'sarah', 'SARAH', 'sarah@example.com', 'SARAH@EXAMPLE.COM', 0, NULL, NEWID(), NEWID(), 0, 0, 0, 0),
('Robert Wilson', 'robert', 'ROBERT', 'robert@example.com', 'ROBERT@EXAMPLE.COM', 0, NULL, NEWID(), NEWID(), 0, 0, 0, 0),
('Olivia Taylor', 'olivia', 'OLIVIA', 'olivia@example.com', 'OLIVIA@EXAMPLE.COM', 0, NULL, NEWID(), NEWID(), 0, 0, 0, 0);

INSERT INTO [dbo].[Hotels] (Name, DistanceFromHaram, PricePerNight, City)
VALUES
('Hilton Makkah', 200, 350.00, 'Makkah'),
('Anwar Al Madinah', 300, 280.00, 'Madinah'),
('Swissotel Al Maqam', 250, 320.00, 'Makkah'),
('Dar Al Iman Intercontinental', 400, 250.00, 'Madinah'),
('Conrad Makkah', 150, 400.00, 'Makkah'),
('Pullman Zamzam', 350, 270.00, 'Makkah');

INSERT INTO [dbo].[Transports] (VehicleType, Price, ProviderName)
VALUES
('Bus', 150.00, 'Saudi Transport Co.'),
('Private Car', 300.00, 'Elite Transport'),
('Van', 200.00, 'Comfort Travel'),
('SUV', 350.00, 'Luxury Ride'),
('Mini Bus', 180.00, 'Al Haramain Transit'),
('Sedan', 250.00, 'City Movers');

INSERT INTO [dbo].[UmrahBookings]
(TripType, FullName, NationalId, Email, Phone, TravelMode, DepartureAirport, Airline, ShipName, StartDate, EndDate, DepartureDate, ArrivalDate, TravelPrice, AuthId, TransportId, PaymentProvider, PaymentProviderId, PaymentStatus, IsPaid, PaymentCapturedAt)
VALUES
('Standard', 'John Smith', 'A123456789', 'john@example.com', '0501234567', 'Air', 'JED', 'Saudia', NULL, '2025-12-01', '2025-12-10', '2025-12-01', '2025-12-10', 2500.00, 1, 1, 'Stripe', 'pi_001', 'Paid', 1, GETDATE()),
('Premium', 'Emily Johnson', 'B987654321', 'emily@example.com', '0502345678', 'Air', 'MED', 'FlyNas', NULL, '2025-11-15', '2025-11-25', '2025-11-15', '2025-11-25', 3200.00, 2, 2, 'PayPal', 'pp_002', 'Paid', 1, GETDATE()),
('Economy', 'Michael Brown', 'C112233445', 'michael@example.com', '0503456789', 'Air', 'JED', 'Emirates', NULL, '2025-11-20', '2025-11-30', '2025-11-20', '2025-11-30', 2000.00, 3, 3, 'Stripe', 'pi_003', 'Pending', 0, NULL),
('VIP', 'Sarah Davis', 'D998877665', 'sarah@example.com', '0504567890', 'Air', 'MED', 'Qatar Airways', NULL, '2025-12-05', '2025-12-15', '2025-12-05', '2025-12-15', 4000.00, 4, 4, 'PayPal', 'pp_004', 'Paid', 1, GETDATE()),
('Standard', 'Robert Wilson', 'E554433221', 'robert@example.com', '0505678901', 'Air', 'JED', 'Etihad', NULL, '2025-11-18', '2025-11-27', '2025-11-18', '2025-11-27', 2300.00, 5, 5, 'Stripe', 'pi_005', 'Failed', 0, NULL),
('Deluxe', 'Olivia Taylor', 'F665544332', 'olivia@example.com', '0506789012', 'Air', 'MED', 'FlyDubai', NULL, '2025-11-22', '2025-11-29', '2025-11-22', '2025-11-29', 3100.00, 6, 6, 'PayPal', 'pp_006', 'Paid', 1, GETDATE());

INSERT INTO [dbo].[UmrahBookingHotels] (UmrahBookingId, HotelId, CheckIn, CheckOut)
VALUES
(1, 1, '2025-12-01', '2025-12-05'),
(2, 2, '2025-11-15', '2025-11-20'),
(3, 3, '2025-11-20', '2025-11-25'),
(4, 4, '2025-12-05', '2025-12-10'),
(5, 5, '2025-11-18', '2025-11-22'),
(6, 6, '2025-11-22', '2025-11-27');


INSERT INTO [dbo].[Rooms] (Type, Capacity, PricePerNight, HotelId, ImgsUrl, IsAvailable)
VALUES
('Single', 1, 350.00, 1, 'room1.jpg', 1),
('Double', 2, 400.00, 2, 'room2.jpg', 1),
('Suite', 3, 600.00, 3, 'room3.jpg', 1),
('Family', 4, 700.00, 4, 'room4.jpg', 1),
('Twin', 2, 380.00, 5, 'room5.jpg', 1),
('Deluxe', 2, 500.00, 6, 'room6.jpg', 1);

INSERT INTO [dbo].[PaymentEvents] (Provider, EventId, ProcessedAt, BookingId)
VALUES
('Stripe', 'evt_001', GETDATE(), 1),
('PayPal', 'evt_002', GETDATE(), 2),
('Stripe', 'evt_003', GETDATE(), 3),
('PayPal', 'evt_004', GETDATE(), 4),
('Stripe', 'evt_005', GETDATE(), 5),
('PayPal', 'evt_006', GETDATE(), 6);


