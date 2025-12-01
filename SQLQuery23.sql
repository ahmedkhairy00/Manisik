INSERT INTO [dbo].[Hotels] (Name, HotelCity, Address, StarRating, DistanceToHaram, Description, ImageUrl, IsActive, CreatedAt)
VALUES
('Hilton Makkah', 0, 'Al Haram Rd', 5, 200, 'A luxurious 5-star hotel with direct views of the Holy Haram.', '/images/hotels/hilton-makkah.jpg', 1, GETDATE()),
('Anwar Al Madinah', 1, 'Prince Mohammed St', 4, 300, 'Elegant hotel near the Prophet’s Mosque with modern rooms.', '/images/hotels/anwar-al-madinah.jpg', 1, GETDATE()),
('Swissotel Al Maqam', 0, 'Abraj Al Bait', 4, 250, 'Located in the Abraj Al Bait complex with direct access to Masjid Al Haram.', '/images/hotels/swissotel-al-maqam.jpg', 1, GETDATE()),
('Dar Al Iman Intercontinental', 0, 'King Fahd Rd', 3, 400, 'Luxury hotel facing the Prophet’s Mosque.', '/images/hotels/dar-al-iman.jpeg', 1, GETDATE()),
('Conrad Makkah', 1, 'King Abdulaziz Rd', 5, 150, '5-star hotel with luxury suites and direct Haram views.', '/images/hotels/conrad-makkah.jpg', 1, GETDATE()),
('Pullman Zamzam', 1, 'Abraj Al Bait Towers', 4, 350, 'Modern rooms with Haram views, close to shopping and dining.', '/images/hotels/pullman-zamzam.jpg', 1, GETDATE());

INSERT INTO [dbo].[HotelRooms] 
(RoomType, Capacity, PricePerNight, AvailableRooms, IsActive, HotelId)
VALUES
(0, 2, 350, 20, 1, 7),
(1, 4, 280, 15, 1, 8),
(2, 3, 320, 25, 1, 9),
(3, 5, 250, 10, 1, 10),
(0, 2, 400, 5, 1, 11),
(0, 3, 270, 12, 1, 12);

--new 
INSERT INTO [dbo].[HotelRooms] 
(RoomType, Capacity, PricePerNight, AvailableRooms, IsActive, HotelId)
VALUES
-- Hotel 7
(0, 1, 200, 10, 1, 7), -- Single
(1, 2, 300, 8, 1, 7),  -- Double
(2, 3, 500, 5, 1, 7),  -- Suite
(3, 5, 700, 3, 1, 7),  -- Family

-- Hotel 8
(0, 1, 180, 12, 1, 8),
(1, 2, 280, 10, 1, 8),
(2, 3, 450, 6, 1, 8),
(3, 5, 650, 4, 1, 8),

-- Hotel 9
(0, 1, 220, 15, 1, 9),
(1, 2, 320, 10, 1, 9),
(2, 3, 520, 7, 1, 9),
(3, 5, 750, 4, 1, 9),

-- Hotel 10
(0, 1, 210, 9, 1, 10),
(1, 2, 310, 7, 1, 10),
(2, 3, 480, 5, 1, 10),
(3, 5, 700, 3, 1, 10),

-- Hotel 11
(0, 1, 230, 6, 1, 11),
(1, 2, 330, 5, 1, 11),
(2, 3, 550, 4, 1, 11),
(3, 5, 780, 2, 1, 11),

-- Hotel 12
(0, 1, 190, 10, 1, 12),
(1, 2, 290, 8, 1, 12),
(2, 3, 470, 5, 1, 12),
(3, 5, 690, 3, 1, 12);


INSERT INTO [dbo].[GroundTransports]
([ServiceName],[InternalTransportType],[PricePerPerson],[Description],[Capacity],[IsActive])
VALUES
('PrivateCar',0,130,'PrivateCar',1,1),
('SharedBus',1,40,'SharedBus',20,1),
('Taxi',2,80,'Taxi',3,1),
('PrivateCar',0,130,'PrivateCar',1,1),
('Taxi',1,80,'Taxi',2,1),
('SharedBus',2,40,'SharedBus',30,1);
/*
INSERT INTO [dbo].[InternationalTransports] 
(TransportType, CarrierName, DepartureAirport, ArrivalAirport, DepartureDate, ArrivalDate, Price, AvailableSeats, FlightNumber, IsActive)
VALUES
(0, 'Saudia', 0, 0, '2025-12-20', '2025-12-20', 500, 150, 'SV101', 1),
(0, 'EgyptAir', 1, 1, '2025-12-21', '2025-12-21', 700, 200, 'MS202', 1),
(0, 'Flynas', 2, 0, '2025-12-22', '2025-12-22', 600, 180, 'XY303', 1),
(0, 'Flyadeal', 3, 1, '2025-12-23', '2025-12-23', 650, 170, 'FA404', 1),
(0, 'AirCairo', 4, 2, '2025-12-24', '2025-12-24', 400, 160, 'AC505', 1),
(0, 'NileAir', 5, 0, '2025-12-25', '2025-12-25', 800, 200, 'NP606', 1);*/
 
delete  from  [dbo].[InternationalTransports];
delete  from  [dbo].[BookingInternationalTransports];
INSERT INTO [dbo].[InternationalTransports] 
(TransportType, CarrierName, DepartureAirport, ArrivalAirport, DepartureDate, ArrivalDate, ReturnDate, Price, AvailableSeats, FlightNumber, IsActive, Duration, Stops, FlightClass, rate, review)
VALUES
(0, 'Saudia', 0, 0, '2025-12-20', '2025-12-21', '2025-12-27', 500, 150, 'SV101', 1, '5 h 40 m', 0, 0, 4, 120),
(0, 'EgyptAir', 1, 1, '2025-12-21', '2025-12-22', '2025-12-28', 700, 200, 'MS202', 1, '2 h 00 m', 0, 0, 5, 180),
(0, 'Flynas', 2, 0, '2025-12-22', '2025-12-23', '2025-12-30', 600, 180, 'XY303', 1, '4 h 30 m', 0, 1, 3, 95),
(0, 'Flyadeal', 3, 1, '2025-12-23', '2025-12-24', '2025-12-29', 650, 170, 'FA404', 1, '7 h 30 m', 0, 1, 4, 140),
(0, 'AirCairo', 4, 2, '2025-12-24', '2025-12-25', '2026-01-02', 400, 160, 'AC505', 1, '8 h 30 m', 0, 2, 2, 60),
(0, 'NileAir', 5, 0, '2025-12-25', '2025-12-26', '2026-01-03', 800, 200, 'NP606', 1, '2 h 30 m', 0, 2, 5, 200),
(1, 'RedSeaFerries', 6, 0, '2025-12-22', '2025-12-23', '2025-12-30', 300, 100, 'RSF001', 1, '12 h 00 m', 1, 0, 4.8, 500),
(1, 'MediterraneanLines', 7, 0, '2025-12-23', '2025-12-24', '2025-12-29', 350, 120, 'ML002', 1, '15 h 30 m', 0, 3, 5, 750),
(1, 'NileCruises', 8, 1, '2025-12-24', '2025-12-25', '2026-01-02', 400, 80, 'NC003', 1, '20 h 00 m', 0, 0, 5, 600);

delete from [dbo].[GroundTransports];
INSERT INTO [dbo].[GroundTransports]
(ServiceName, InternalTransportType, PricePerPerson, Description, Capacity, IsActive,Duration,Route,rate)
VALUES
-- Service 1
('SAPTCO (Public Bus)', 
 0, 
 25.00,
 ' AC, Comfortable seats, WiFi',
 50,
 1,
 ' 1h 30m',
 ' Jeddah - Makkah',
 4),

('Al Khalij Bus Services', 
 0, 
 35.00,
 'AC, Reclining seats, Refreshments, Prayer stops',
 50,
 1,
 '4h 30m',
 'Makkah - Madinah',
 4),

('Haramain High-Speed Train', 
 1, 
 60.00,
 'AC, WiFi, Comfortable seats',
 300,
 1,
 '45 min',
 'Jeddah - Makkah',
 4),

('Haramain Express Train', 
 1, 
 80.00,
 ' AC, WiFi, Refreshments, Spacious seats',
 350,
 1,
 '2h 00m',
 'Makkah - Madinah',
 4),

('Blacklane Chauffeur Service',
 3,
 180.00,
 'Luxury sedan, AC, Professional driver',
 3,
 1,
 '1h 20m',
 'Jeddah - Makkah',
 4),

('UberX', 
 2, 
 90.00,
 'AC, 4 seats',
 4,
 1,
 '1h 20m',
 'Jeddah - Makkah',
 4),

('Careem Go', 
 2, 
 95.00,
 ' AC, 4 seats, Water',
 4,
 1,
 '4h 10m',
 'Makkah - Madinah',
 4),

('Elite Chauffeur Saudi Arabia',
 3,
 220.00,
 'Luxury private SUV , Leather seats, AC, Professional chauffeur',
 4,
 1,
 '4h 10m',
 'Makkah - Madinah',
 4),

('Makkah Limousine',
 3,
 150.00,
 'Standard private car , AC, Comfortable seats, 4 passengers',
 4,
 1,
 '1h 30m',
 'Jeddah - Makkah',
 4),

('Saudi VIP Taxi',
 3,
 130.00,
 'Private taxi service , AC, 4 seats, Driver included',
 4,
 1,
 '4h 00m',
 'Madinah - Makkah',
 4);
 



INSERT INTO [dbo].[Bookings]
(BookingNumber, BookingStatus, TripType, TotalPrice, BookingDate, TravelStartDate, TravelEndDate, NumberOfTravelers, PaymentStatus, PaymentMethod, PaymentIntentId, PaymentDate, CreatedAt, UpdatedAt, UserId)
VALUES
('BKG001', 0, 0, 1000, GETDATE(), '2025-12-20', '2025-12-25', 2, 1, 0, 'PI001', GETDATE(), GETDATE(), GETDATE(), 1),
('BKG002', 1, 1, 1200, GETDATE(), '2025-12-22', '2025-12-28', 3, 1, 1, 'PI002', GETDATE(), GETDATE(), GETDATE(),1),
('BKG003', 0, 0, 900,  GETDATE(), '2025-12-24', '2025-12-30', 1, 1, 0, 'PI003', GETDATE(), GETDATE(), GETDATE(), 1),
('BKG004', 2, 1, 1500, GETDATE(), '2025-12-26', '2026-01-01', 4, 1, 1, 'PI004', GETDATE(), GETDATE(), GETDATE(), 1),
('BKG005', 1, 0, 1100, GETDATE(), '2025-12-28', '2026-01-02', 2, 1, 0, 'PI005', GETDATE(), GETDATE(), GETDATE(), 1),
('BKG006', 0, 1, 1300, GETDATE(), '2025-12-30', '2026-01-05', 3, 1, 1, 'PI006', GETDATE(), GETDATE(), GETDATE(), 1);

INSERT INTO [dbo].[BookingGroundTransports] (BookingId, GroundTransportId, ServiceDate, PickupLocation, DropoffLocation, NumberOfPassengers, TotalPrice)
VALUES
(1, 1, '2025-12-20', 'Hilton Makkah', 'Haram', 2, 200),
(2, 2, '2025-12-21', 'Anwar Al Madinah', 'Haram', 3, 150),
(3, 3, '2025-12-22', 'Swissotel Al Maqam', 'Haram', 4, 300),
(4, 4, '2025-12-23', 'Dar Al Iman', 'Haram', 1, 100),
(5, 5, '2025-12-24', 'Conrad Makkah', 'Haram', 2, 220),
(6, 6, '2025-12-25', 'Pullman Zamzam', 'Haram', 3, 260);


INSERT INTO [dbo].[BookingInternationalTransports] (BookingId, InternationalTransportId, NumberOfSeats, TotalPrice)
VALUES
(1, 1, 2, 2000),
(2, 2, 3, 2500),
(3, 3, 4, 3000),
(4, 4, 1, 1800),
(5, 5, 2, 2200),
(6, 6, 3, 2600);

INSERT INTO [dbo].[Travelers] (BookingId, FirstName, LastName, DateOfBirth, PassportNumber, PassportExpiryDate, Nationality, Gender, PhoneNumber, Email, EmergencyContactName, EmergencyContactPhone, IsMainTraveler, CreatedAt)
VALUES
(1, 'Ahmed', 'Ali', '1990-01-01', 'P001', '2030-01-01', 0, 0, '01000000001', 'ahmed1@mail.com', 'Mohamed Ali', '01000000002', 1, GETDATE()),
(2, 'Sara', 'Hassan', '1992-02-02', 'P002', '2032-02-02', 1, 1, '01000000003', 'sara@mail.com', 'Laila Hassan', '01000000004', 1, GETDATE()),
(3, 'Omar', 'Khalid', '1985-03-03', 'P003', '2035-03-03', 0, 0, '01000000005', 'omar@mail.com', 'Ali Khalid', '01000000006', 1, GETDATE()),
(4, 'Fatma', 'Youssef', '1995-04-04', 'P004', '2035-04-04', 1, 1, '01000000007', 'fatma@mail.com', 'Sara Youssef', '01000000008', 1, GETDATE()),
(5, 'Hassan', 'Ibrahim', '1988-05-05', 'P005', '2038-05-05', 0, 0, '01000000009', 'hassan@mail.com', 'Mohamed Ibrahim', '01000000010', 1, GETDATE()),
(6, 'Mona', 'Saeed', '1991-06-06', 'P006', '2031-06-06', 1, 1, '01000000011', 'mona@mail.com', 'Fatma Saeed', '01000000012', 1, GETDATE());


TRUNCATE TABLE [Payments];
DBCC CHECKIDENT ('Payments', RESEED, 0);

INSERT INTO [Payments] (BookingId, Currency, Amount, PaymentMethod, Status, PaymentIntentId, TransactionId, PayerEmail, PayerName, PaidAt, FailureReason, IdempotencyKey, CreatedAt, UpdatedAt)
VALUES
(1, 'USD', 1000, 0, 4, 'pi_001', 'txn_001', 'ahmed1@example.com', 'Ahmed Ali', GETDATE(), NULL, 'idempotency_001', GETDATE(), GETDATE()),
(2, 'USD', 1800, 1, 0, 'pi_002', 'txn_002', 'sara2@example.com', 'Sara Hassan', GETDATE(), NULL, 'idempotency_002', GETDATE(), GETDATE()),
(3, 'USD', 1200, 0, 1, 'pi_003', 'txn_003', 'omar3@example.com', 'Omar Mohamed', GETDATE(), NULL, 'idempotency_003', GETDATE(), GETDATE()),
(4, 'USD', 1500, 1, 4, 'pi_004', 'txn_004', 'nour4@example.com', 'Nour Ali', GETDATE(), NULL, 'idempotency_004', GETDATE(), GETDATE()),
(5, 'USD', 700, 0, 2, 'pi_005', 'txn_005', 'khaled5@example.com', 'Khaled Samir', GETDATE(), NULL, 'idempotency_005', GETDATE(), GETDATE()),
(6, 'USD', 2200, 1, 3, 'pi_006', 'txn_006', 'mona6@example.com', 'Mona Ahmed', GETDATE(), NULL, 'idempotency_006', GETDATE(), GETDATE());

-- =========================================
-- 10. PaymentEvents
-- =========================================
TRUNCATE TABLE [PaymentEvents];
DBCC CHECKIDENT ('PaymentEvents', RESEED, 0);

INSERT INTO [PaymentEvents] (PaymentId, Provider, EventId, ProcessedAt, Payload)
VALUES
(1, 'Stripe', 'evt_001', GETDATE(), '{}'),
(2, 'PayPal', 'evt_002', GETDATE(), '{}'),
(3, 'Stripe', 'evt_003', GETDATE(), '{}'),
(4, 'PayPal', 'evt_004', GETDATE(), '{}'),
(5, 'Stripe', 'evt_005', GETDATE(), '{}'),
(6, 'PayPal', 'evt_006', GETDATE(), '{}');


DELETE FROM [dbo].[PaymentEvents];
DELETE FROM [dbo].[Payments];
DELETE FROM [dbo].[BookingInternationalTransports];
DELETE FROM [dbo].[BookingGroundTransports];
DELETE FROM [dbo].[Travelers];
DELETE FROM [dbo].[Bookings];
DELETE FROM [dbo].[HotelRooms];
DELETE FROM [dbo].[GroundTransports];
DELETE FROM [dbo].[InternationalTransports];
DELETE FROM [dbo].[Hotels];

DBCC CHECKIDENT ('[dbo].[Hotels]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[HotelRooms]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[GroundTransports]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[InternationalTransports]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Bookings]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[BookingGroundTransports]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[BookingInternationalTransports]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Travelers]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Payments]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[PaymentEvents]', RESEED, 0);
